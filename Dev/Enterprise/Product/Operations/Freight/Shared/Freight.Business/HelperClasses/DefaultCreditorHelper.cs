using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Business
{
	public static class DefaultCreditorHelper
	{
		public enum CreditorType
		{
			ExportConsol,
			ImportConsol,
			ForwardingConsol,
			ExportPenalty,
			ImportPenalty,
			PickupTransport,
			DeliveryTransport,
			DomesticConsol,
			CrossTradeConsol
		}

		public class OrgRelatedPartyFilter
		{
			public ZGuid OrgAddress { get; set; }
			public ZGuid OrgPartyAddress { get; set; }
			public CreditorType CreditorType { get; set; }
			public ZString UNLOCO { get; set; }
			public ZString TransportMode { get; set; }
			public ZString ContainerMode { get; set; }
			public ZString PaymentType { get; set; }
		}

		public static ZGuid GetCreditorAddress(OrgRelatedPartyFilter orgRelatedPartyFilter, BusinessObjectFactory factory)
		{
			var isExportConsol = orgRelatedPartyFilter.CreditorType == CreditorType.ExportConsol;
			var isImportConsol = orgRelatedPartyFilter.CreditorType == CreditorType.ImportConsol;
			var isDomesticConsol = orgRelatedPartyFilter.CreditorType == CreditorType.DomesticConsol;
			var isCrossTradeConsol = orgRelatedPartyFilter.CreditorType == CreditorType.CrossTradeConsol;

			var result = GetRelatedParties(orgRelatedPartyFilter, factory);

			if (result.Any())
			{
				var orgRelatedParty = SortRelatedParties(result).First().PR_OH_RelatedParty;
				var newCreditor = factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, orgRelatedParty));

				return newCreditor.MainAddress.PK;
			}
			else if (isExportConsol || isImportConsol || isDomesticConsol || isCrossTradeConsol)
			{
				if (!orgRelatedPartyFilter.OrgPartyAddress.IsEmpty)
				{
					var orgHeader = GetOrgHeaderFromAddress(orgRelatedPartyFilter.OrgPartyAddress, factory);

					if (orgHeader?.OH_IsCreditor ?? false)
					{
						return orgRelatedPartyFilter.OrgPartyAddress;
					}
				}
			}

			return ZGuid.Empty;
		}

		public static ZGuid GetCreditorOrgHeaderPK(OrgRelatedPartyFilter orgRelatedPartyFilter, BusinessObjectFactory factory)
		{
			var result = GetRelatedParties(orgRelatedPartyFilter, factory);

			if (result.Any())
			{
				var orgRelatedParty = SortRelatedParties(result).First().PR_OH_RelatedParty;
				var newCreditor = factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, orgRelatedParty));
				return newCreditor.PK;
			}

			return ZGuid.Empty;
		}

		public static ZGuid GetDefaultCreditor(
			IImportExport importExport,
			OrgHeader carrier,
			string transportMode,
			string containerMode,
			string origin,
			string destination,
			BusinessObjectFactory factory)
		{
			if (carrier is null || importExport is null)
			{
				return ZGuid.Empty;
			}

			if (!importExport.IsUnknown())
			{
				var isExportOrDomestic = importExport.IsDomestic() || importExport.IsExport();
				var isImportOrCrosstrade = importExport.IsImport() || importExport.IsCrossTrade();
				var relatedParty = GetCreditorOrgHeaderFromOrgRelatedParties(
					carrier,
					new ()
					{
						TransportMode = transportMode,
						ContainerMode = containerMode,
						CreditorType =
							isExportOrDomestic ? CreditorType.ExportConsol :
							isImportOrCrosstrade ? CreditorType.ImportConsol :
							throw new InvalidOperationException("Unsupported JobDirection when defaulting Creditor"),
						UNLOCO =
							isExportOrDomestic ? origin :
							isImportOrCrosstrade ? destination :
							throw new InvalidOperationException("Unsupported JobDirection when defaulting Creditor"),
					},
					factory);

				if (relatedParty is not null)
				{
					return relatedParty.PK;
				}
			}

			if (carrier.OH_IsCreditor)
			{
				return carrier.PK;
			}

			return ZGuid.Empty;
		}

		public static OrgHeader GetCreditorOrgHeaderFromOrgRelatedParties(OrgHeader org, OrgRelatedPartyFilter orgRelatedPartyFilter, BusinessObjectFactory factory)
		{
			if (org == null)
			{
				return null;
			}

			var directionValues = GetDirectionValues(orgRelatedPartyFilter);
			var transports = new List<string> { Constants.TransportModes.All, orgRelatedPartyFilter.TransportMode };
			var containerModes = new List<string> { ZString.Empty, orgRelatedPartyFilter.ContainerMode };
			List<string> locationFilter = null;

			if (!string.IsNullOrWhiteSpace(orgRelatedPartyFilter.UNLOCO) && orgRelatedPartyFilter.UNLOCO.Length > 1)
			{
				locationFilter = new () { ZString.Empty, orgRelatedPartyFilter.UNLOCO.Left(2), orgRelatedPartyFilter.UNLOCO };
			}

			var matchingRelatedParties = org.AllRelatedParties.Cast<OrgRelatedParty>()
				.Where(o =>
					o.PR_PartyType == RelatedPartyTypeList.Codes.ServiceProviderCreditor
					&& directionValues.Contains(o.CalculatedDirection)
					&& transports.Contains(o.PR_FreightTransportMode)
					&& containerModes.Contains(o.PR_FreightContainerMode)
					&& (o.PR_GC == ZGuid.Empty || o.PR_GC == GlbCompany.CurrentCompany.PK)
					&& (locationFilter?.Contains(o.PR_Location) ?? true))
				.ToList();

			if (matchingRelatedParties.Count > 0)
			{
				var payableRelatedParties = GetPayableRelatedPartiesInCurrentCompany(matchingRelatedParties, factory);

				if (payableRelatedParties.Count > 0)
				{
					var orgRelatedParty = SortRelatedParties(payableRelatedParties).First().PR_OH_RelatedParty;
					return factory.LoadTop1<OrgHeader>(new (OrgHeaderSchema.PK, orgRelatedParty));
				}
			}

			return null;
		}

		static List<string> GetDirectionValues(OrgRelatedPartyFilter orgRelatedPartyFilter)
		{
			var directionValues = new List<string> { RelatedPartyDirectionList.Codes.PickupAndDelivery };

			switch (orgRelatedPartyFilter.CreditorType)
			{
				case CreditorType.ExportConsol:
				case CreditorType.ExportPenalty:
				case CreditorType.PickupTransport:
					directionValues.Add(RelatedPartyDirectionList.Codes.Pickup);
					break;
				case CreditorType.ImportConsol:
				case CreditorType.ImportPenalty:
				case CreditorType.DeliveryTransport:
				case CreditorType.CrossTradeConsol:
					directionValues.Add(RelatedPartyDirectionList.Codes.Delivery);
					break;
				case CreditorType.DomesticConsol:
					directionValues.Add(RelatedPartyDirectionList.Codes.Pickup);
					directionValues.Add(RelatedPartyDirectionList.Codes.Delivery);
					break;
				case CreditorType.ForwardingConsol:
					if (orgRelatedPartyFilter.PaymentType == Constants.PaymentType.Prepaid)
					{
						directionValues.Add(RelatedPartyDirectionList.Codes.Pickup);
					}
					else if (orgRelatedPartyFilter.PaymentType == Constants.PaymentType.Collect)
					{
						directionValues.Add(RelatedPartyDirectionList.Codes.Delivery);
					}
					break;
			}

			return directionValues;
		}

#if DEBUG
		internal
#endif
		static List<OrgRelatedParty> GetRelatedParties(OrgRelatedPartyFilter orgRelatedPartyFilter, BusinessObjectFactory factory)
		{
			var directionValues = GetDirectionValues(orgRelatedPartyFilter);

			// getting list of org related parties from Org address -> OrgHeader -> Org Related Party
			var oaSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.PK);
			oaSubQuery.AddToFilter(OrgAddressSchema.OA_IsActive, true);
			oaSubQuery.AddToFilter(OrgAddressSchema.PK, orgRelatedPartyFilter.OrgAddress);

			var orgHeaderSQ = new ZDBOnlySubQuery(typeof(OrgHeader), OrgHeaderSchema.PK);
			orgHeaderSQ.AddToFilter(OrgHeaderSchema.OH_IsActive, true);
			orgHeaderSQ.AddSubQuery(OrgHeaderSchema.PK, OrgAddressSchema.OA_OH, oaSubQuery, JoinCondition.And);

			var query = new ZDBOnlyQuery(typeof(OrgRelatedParty));
			query.AddToFilter(OrgRelatedPartySchema.PR_PartyType, RelatedPartyTypeList.Codes.ServiceProviderCreditor);
			query.AddToFilter(OrgRelatedPartySchema.PR_FreightDirection, directionValues);
			query.AddToFilter(OrgRelatedPartySchema.PR_FreightTransportMode, new List<string> { Constants.TransportModes.All, orgRelatedPartyFilter.TransportMode });
			query.AddToFilter(OrgRelatedPartySchema.PR_FreightContainerMode, new List<string> { ZString.Empty, orgRelatedPartyFilter.ContainerMode });

			var companyQuery = new ZQuery(OrgRelatedPartySchema.PR_GC, GlbCompany.CurrentCompany.PK);
			companyQuery.AddToFilter(JoinCondition.Or, OrgRelatedPartySchema.PR_GC, null);
			query.AddToFilter(companyQuery);

			if (string.IsNullOrWhiteSpace(orgRelatedPartyFilter.UNLOCO))
			{
				query.AddToFilter(OrgRelatedPartySchema.PR_Location, ZString.Empty);
			}
			else if (orgRelatedPartyFilter.UNLOCO.Length > 1)
			{
				query.AddToFilter(OrgRelatedPartySchema.PR_Location, new List<string> { ZString.Empty, orgRelatedPartyFilter.UNLOCO.Left(2), orgRelatedPartyFilter.UNLOCO });
			}

			query.AddSubQuery(OrgRelatedPartySchema.PR_OH_Parent, OrgHeaderSchema.PK, orgHeaderSQ, JoinCondition.And);

			var result = factory.Load<OrgRelatedParty>(query).ToList();

			if (result.Any())
			{
				return GetPayableRelatedPartiesInCurrentCompany(result, factory);
			}

			return result;
		}

		public static OrgHeader GetOrgHeaderFromAddress(ZGuid address, BusinessObjectFactory factory)
		{
			var sq = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.PK);
			sq.AddToFilter(OrgAddressSchema.OA_IsActive, true);
			sq.AddToFilter(OrgAddressSchema.PK, address);

			var dbquery = new ZDBOnlyQuery(typeof(OrgHeader));
			dbquery.AddSubQuery(OrgHeaderSchema.PK, OrgAddressSchema.OA_OH, sq, JoinCondition.And);

			return factory.Load<OrgHeader>(dbquery).FirstOrDefault();
		}

		static List<OrgRelatedParty> GetPayableRelatedPartiesInCurrentCompany(List<OrgRelatedParty> result, BusinessObjectFactory factory)
		{
			var payableOrgs = GetPayableOrganisationInCurrentCompany(result.Select(rp => rp.PR_OH_RelatedParty), factory);
			return result.Where(rp => payableOrgs.Contains(rp.PR_OH_RelatedParty)).ToList();
		}

		static List<ZGuid> GetPayableOrganisationInCurrentCompany(IEnumerable<ZGuid> orgs, BusinessObjectFactory factory)
		{
			var zQuery = new ZDBOnlyQuery(typeof(OrgCompanyData));
			zQuery.AddToFilter(OrgCompanyDataSchema.OB_GC, GlbCompany.CurrentCompany.PK);
			zQuery.AddToFilter(OrgCompanyDataSchema.OB_IsCreditor, true);
			zQuery.AddToFilter(OrgCompanyDataSchema.OB_OH, orgs);

			return factory.Load<OrgCompanyData>(zQuery).Select(o => o.OB_OH).ToList();
		}

#if DEBUG
		internal
#endif
		static IOrderedEnumerable<OrgRelatedParty> SortRelatedParties(List<OrgRelatedParty> relatedParties) =>
			relatedParties.OrderByDescending(x => x.PR_GC.IsEmpty ? 0 : 1)
				.ThenByDescending(x => x.PR_Location.Length)
				.ThenByDescending(x => string.IsNullOrWhiteSpace(x.PR_FreightContainerMode) ? 0 : 1)
				.ThenByDescending(x => x.PR_FreightTransportMode == Constants.TransportModes.All ? 0 : 1)
				.ThenByDescending(x => GetFreightDirectionOrder(x.PR_FreightDirection));

		static int GetFreightDirectionOrder(string freightDirection) =>
			freightDirection switch
			{
				RelatedPartyDirectionList.Codes.Pickup => 2,
				RelatedPartyDirectionList.Codes.Delivery => 1,
				_ => 0,
			};
	}
}
