using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DeniedPartyScreening.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterData.Business
{
	public static class RelatedOrgPartyScreeningStatusHelper
	{
		public static RelatedOrgPartyScreeningStatusCollection GetRelatedOrgPartyScreeningStatusCollection(BusinessObjectFactory factory, ScreeningParty[] screeningParties, params ZGuid[] relatedJobPKs)
		{
			var collection = new RelatedOrgPartyScreeningStatusCollection(factory, new ZQuery() { IsNoResultQuery = true });

			if (factory != null && screeningParties != null && screeningParties.Any())
			{
				var orgWithDescriptionDictionary = new Dictionary<ZGuid, OrgHeaderWithDescriptions>();
				var overrideAddressWithDescriptionDictionary = new Dictionary<ZGuid, ZString>();
				var vesselWithDescriptionDictionary = new Dictionary<ZGuid, ZString>();

				FillScreeningStatusDescriptions(screeningParties, orgWithDescriptionDictionary, overrideAddressWithDescriptionDictionary, vesselWithDescriptionDictionary);

				if (orgWithDescriptionDictionary.Count > 0 || overrideAddressWithDescriptionDictionary.Count > 0 || vesselWithDescriptionDictionary.Count > 0 || relatedJobPKs?.Length > 0)
				{
					var parentPKs = orgWithDescriptionDictionary.Select(u => u.Key).Concat(overrideAddressWithDescriptionDictionary.Select(u => u.Key)).Concat(vesselWithDescriptionDictionary.Select(u => u.Key));
					if (relatedJobPKs != null && relatedJobPKs.Length > 0)
					{
						parentPKs = parentPKs.Concat(relatedJobPKs);
					}

					var query = new ZQuery(StmEntityScreeningLogSchema.PJ_ParentID, parentPKs);
					query.AddToFilter(new ZQuery(StmEntityScreeningLogSchema.PJ_Status, SQLComparisonOperator.NotEqual, DeniedPartyConstants.LogsScreeningStatus.ComplianceRiskSnapshot));
					collection = new RelatedOrgPartyScreeningStatusCollection(factory, query);

					foreach (var status in collection)
					{
						status.RelatedOrganization = GetRelatedOrganization(status.PJ_ParentID, status.PJ_ParentTableCode, orgWithDescriptionDictionary, overrideAddressWithDescriptionDictionary, vesselWithDescriptionDictionary);
					}
				}
			}

			return collection;
		}

		static void FillScreeningStatusDescriptions(ScreeningParty[] screeningParties, Dictionary<ZGuid, OrgHeaderWithDescriptions> orgWithDescriptionDictionary, Dictionary<ZGuid, ZString> overrideAddressWithDescriptionDictionary, Dictionary<ZGuid, ZString> vesselWithDescriptionDictionary)
		{
			foreach (var party in screeningParties)
			{
				OrgHeader header = null;
				ZString description = string.Empty;

				if (party.Header != null)
				{
					header = party.Header;
					description = party.Description;
				}
				else if (party.DocAddress != null && !party.DocAddress.IsEmpty)
				{
					description = ScreeningParty.GetPartyDescriptionWithCountryCodeIfParentIsDeclaration(party.Parent, party.DocAddress.AddressDescription);

					if (party.DocAddress.HasRealAddress)
					{
						header = party.DocAddress.Address?.Header;
					}
					else
					{
						AddOverrideAddressToDic(overrideAddressWithDescriptionDictionary, description, party.DocAddress.PK);
					}
				}
				else if (party.Vessel != null)
				{
					description = string.Format(CultureInfo.InvariantCulture, "{0}({1})", party.Vessel.RV_Code, party.Description);
					AddVesselToDic(vesselWithDescriptionDictionary, party.Vessel.PK, description);
				}
				else if (party.NotLinkedVessel != null)
				{
					description = string.Format(CultureInfo.InvariantCulture, "{0}({1})", party.NotLinkedVessel.Code, party.Description);
					AddVesselToDic(vesselWithDescriptionDictionary, (party.NotLinkedVessel as BusinessObject).PK, description);
				}

				if (header != null)
				{
					AddOrgToDic(orgWithDescriptionDictionary, header, description);
				}
			}
		}

		class OrgHeaderWithDescriptions
		{
			public OrgHeaderWithDescriptions(OrgHeader header)
			{
				Header = header;
				Descriptions = new List<string>();
			}

			public OrgHeader Header { get; }
			public List<string> Descriptions { get; }
		}

		static ZString GetRelatedOrganization(ZGuid parentPK, string parentTableCode, Dictionary<ZGuid, OrgHeaderWithDescriptions> orgWithDescDic, Dictionary<ZGuid, ZString> overrideAddressWithDescDic, Dictionary<ZGuid, ZString> vesselWithDescDic)
		{
			ZString result;

			if (parentTableCode == OrgHeaderSchema.Constants.Prefix && orgWithDescDic.TryGetValue(parentPK, out var orgWithDesc))
			{
				result = string.Format(CultureInfo.InvariantCulture, "{0}({1})", orgWithDesc.Header.OH_Code, string.Join("|", orgWithDesc.Descriptions));
			}
			else if (parentTableCode == JobDocAddressSchema.Constants.Prefix && overrideAddressWithDescDic.TryGetValue(parentPK, out var overrideAddressDescription))
			{
				result = Res.GetString("99EB269E-7762-4A9B-9C42-17544025F432", "Override Address({0})", overrideAddressDescription);
			}
			else if (vesselWithDescDic.TryGetValue(parentPK, out var vesselWithDesc))
			{
				result = string.Format(CultureInfo.InvariantCulture, vesselWithDesc);
			}
			else
			{
				result = ZString.Empty;
			}

			return result;
		}

		static void AddOrgToDic(Dictionary<ZGuid, OrgHeaderWithDescriptions> orgWithDescriptionDictionary, OrgHeader header, ZString description)
		{
			if (orgWithDescriptionDictionary.TryGetValue(header.PK, out var orgWithDescription))
			{
				if (!orgWithDescription.Descriptions.Contains(description))
				{
					orgWithDescription.Descriptions.Add(description);
				}
			}
			else
			{
				orgWithDescription = new OrgHeaderWithDescriptions(header);
				orgWithDescription.Descriptions.Add(description);
				orgWithDescriptionDictionary.Add(header.PK, orgWithDescription);
			}
		}

		static void AddOverrideAddressToDic(Dictionary<ZGuid, ZString> overrideAddressWithDescriptionDictionary, ZString description, ZGuid addressPK)
		{
			if (!overrideAddressWithDescriptionDictionary.TryGetValue(addressPK, out var overrideAddressWithDescriptions))
			{
				overrideAddressWithDescriptions = description;
				overrideAddressWithDescriptionDictionary.Add(addressPK, overrideAddressWithDescriptions);
			}
		}

		static void AddVesselToDic(Dictionary<ZGuid, ZString> vesselWithDescriptionDictionary, ZGuid vesselPK, ZString description)
		{
			if (!vesselWithDescriptionDictionary.TryGetValue(vesselPK, out var vesselWithDescription))
			{
				vesselWithDescription = description;
				vesselWithDescriptionDictionary.Add(vesselPK, vesselWithDescription);
			}
		}
	}
}
