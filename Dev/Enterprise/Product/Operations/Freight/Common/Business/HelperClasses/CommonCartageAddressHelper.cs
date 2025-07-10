using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Common.Business
{
	[SuppressStaticMethodsAreLocatedOnCorrectClassMessage]
	public class CommonCartageAddressHelper
	{
		#region NEW!

		#region GetCartageAddress

		public static JobDocAddress GetCartagePickupAddress(IDocAddresses cartage, CommonCartageLegType legType)
		{
			return GetCartageAddress(cartage, legType?.FromOrg);
		}

		public static JobDocAddress GetCartageWaitPointAddress(IDocAddresses cartage, CommonCartageLegType legType)
		{
			return GetCartageAddress(cartage, legType?.WaitPointOrg);
		}

		public static JobDocAddress GetCartageDeliveryAddress(IDocAddresses cartage, CommonCartageLegType legType)
		{
			return GetCartageAddress(cartage, legType?.ToOrg);
		}

		static JobDocAddress GetCartageAddress(IDocAddresses cartage, CommonCartageOrg orgType)
		{
			JobDocAddress result = null;

			if (orgType != null)
			{
				var sequence = orgType.CommonCartageType
					.CommonCartageOrganisations
					.TakeWhile(cartageOrgType => cartageOrgType.PK != orgType.PK)
					.Count(cartageOrgType => cartageOrgType.E5_OrgType == orgType.E5_OrgType);

				var docAddressType = GetDocAddressTypeFromOrgType(orgType.E5_OrgType);
				if (docAddressType != DocAddressType.None)
				{
					result = cartage.DocAddresses.FindByDocAddressType(docAddressType, sequence);
				}
			}

			return result;
		}

		static DocAddressType GetDocAddressTypeFromOrgType(string orgType)
		{
			switch (orgType)
			{
				case LocalCartageJobOrgTypeList.Codes.CFS:
					return DocAddressType.LocalCartageCFS;
				case LocalCartageJobOrgTypeList.Codes.CTO:
					return DocAddressType.LocalCartageCTO;
				case LocalCartageJobOrgTypeList.Codes.CNE:
					return DocAddressType.LocalCartageImporter;
				case LocalCartageJobOrgTypeList.Codes.CNR:
					return DocAddressType.LocalCartageExporter;
				case LocalCartageJobOrgTypeList.Codes.CYD:
					return DocAddressType.LocalCartageYard;
				case LocalCartageJobOrgTypeList.Codes.SRV:
					return DocAddressType.LocalCartageService;
				case LocalCartageJobOrgTypeList.Codes.WHS:
					return DocAddressType.LocalCartageWarehouse;
				case LocalCartageJobOrgTypeList.Codes.MSC:
					return DocAddressType.LocalCartageMSC;
				default:
					return DocAddressType.None;
			}
		}

		#endregion

		#endregion

		protected CommonCartageAddressHelper(string cartageType, SortedList addressOrgTypes)
		{
			this.CartageType = cartageType;
			this.AddressOrgTypes = new CommonCartageOrgIndexer(addressOrgTypes);
		}

		public readonly string CartageType;
		public readonly CommonCartageOrgIndexer AddressOrgTypes;

		#region Org Type Properties

		public CommonCartageOrg Address1OrgType
		{
			get { return AddressOrgTypes[0]; }
		}

		public CommonCartageOrg Address2OrgType
		{
			get { return AddressOrgTypes[1]; }
		}

		public CommonCartageOrg Address3OrgType
		{
			get { return AddressOrgTypes[2]; }
		}

		public CommonCartageOrg Address4OrgType
		{
			get { return AddressOrgTypes[3]; }
		}

		#endregion

		#region Relevant Address Type

		public static DocAddressType GetRelevantDocAddressType(CommonCartageLegType booking)
		{
			var pickupDocAddType = CommonCartageAddressHelper.GetCartageDocAddressTypeFromOrgType(booking.OrgTypeFromCode);
			var waitDocAddType = CommonCartageAddressHelper.GetCartageDocAddressTypeFromOrgType(booking.OrgTypeWaitCode);
			var deliveryDocAddType = CommonCartageAddressHelper.GetCartageDocAddressTypeFromOrgType(booking.OrgTypeToCode);

			return GetRelevantDocAddressType(new List<DocAddressType> { pickupDocAddType, waitDocAddType, deliveryDocAddType });
		}

		public static DocAddressType GetRelevantDocAddressType(List<DocAddressType> docAddressTypes)
		{
			var result = DocAddressType.None;

			if (docAddressTypes.Contains(DocAddressType.LocalCartageImporter))
			{
				result = DocAddressType.LocalCartageImporter;
			}
			else if (docAddressTypes.Contains(DocAddressType.LocalCartageExporter))
			{
				result = DocAddressType.LocalCartageExporter;
			}
			else if (docAddressTypes.Contains(DocAddressType.LocalCartageCFS))
			{
				result = DocAddressType.LocalCartageCFS;
			}

			return result;
		}

		#endregion

		#region Static Methods

		public static CommonCartageAddressHelper ByJobType(CommonCartageType jobType)
		{
			SortedList orgTypes = new SortedList();
			string jobTypeCode = "";

			if (jobType != null)
			{
				jobType.Factory.AddFetchHint(LocalCartageJobOrgSchema.E5_E3, jobType.PK);

				jobTypeCode = jobType.E3_JobType;
				for (int i = 0; i < jobType.AllCartageLegTypes.Count; i++)
				{
					AddByLegType(jobType.AllCartageLegTypes[i], orgTypes, true);
				}
			}

			return new CommonCartageAddressHelper(jobTypeCode, orgTypes);
		}

		static void AddByLegType(CommonCartageLegType legType, SortedList orgTypes, bool onlyAddValid)
		{
			AddOrgType(legType.FromOrg, orgTypes, onlyAddValid);
			AddOrgType(legType.WaitPointOrg, orgTypes, onlyAddValid);
			AddOrgType(legType.ToOrg, orgTypes, onlyAddValid);
		}

		static void AddOrgType(CommonCartageOrg orgType, SortedList orgTypes, bool onlyAddValid)
		{
			if (!onlyAddValid || (orgType != null && !ContainsOrgTypeCode(orgType, orgTypes)))
			{
				orgTypes.Add(orgTypes.Count, orgType);
			}
		}

		static bool ContainsOrgTypeCode(CommonCartageOrg orgType, SortedList orgTypes)
		{
			bool result = false;
			foreach (CommonCartageOrg orgTypeInList in orgTypes.Values)
			{
				ZGuid orgTypeInListPK = orgTypeInList != null ? orgTypeInList.PK : ZGuid.Empty;
				ZGuid orgTypePK = orgType != null ? orgType.PK : ZGuid.Empty;

				if (orgTypeInListPK == orgTypePK)
				{
					result = true;
					break;
				}
			}
			return result;
		}

		public static ZString GetOrgTypeFromCartageDocAddressType(DocAddressType docAddressType)
		{
			switch (docAddressType)
			{
				case DocAddressType.LocalCartageCFS:
					return LocalCartageJobOrgTypeList.Codes.CFS;
				case DocAddressType.LocalCartageCTO:
					return LocalCartageJobOrgTypeList.Codes.CTO;
				case DocAddressType.LocalCartageImporter:
					return LocalCartageJobOrgTypeList.Codes.CNE;
				case DocAddressType.LocalCartageExporter:
					return LocalCartageJobOrgTypeList.Codes.CNR;
				case DocAddressType.LocalCartageYard:
					return LocalCartageJobOrgTypeList.Codes.CYD;
				case DocAddressType.LocalCartageService:
					return LocalCartageJobOrgTypeList.Codes.SRV;
				case DocAddressType.LocalCartageWarehouse:
					return LocalCartageJobOrgTypeList.Codes.WHS;
				case DocAddressType.LocalCartageMSC:
					return LocalCartageJobOrgTypeList.Codes.MSC;
				case DocAddressType.BookingPartyDocumentaryAddress:
					return DocAddressTypes.Codes.BookingPartyDocumentaryAddress;
				case DocAddressType.ClientRequestedBillingParty:
					return DocAddressTypes.Codes.ClientRequestedBillingParty;
				case DocAddressType.NonPersistent:
				case DocAddressType.None:
					return "";
				default:
					throw new NotSupportedException(docAddressType.ToString() + " needs to be added to GetOrgTypeFromCartageDocAddressType");
			}
		}

		public static ZString GetOrgTypeDescriptionFromCartageDocAddressType(DocAddressType docAddressType)
		{
			switch (docAddressType)
			{
				case DocAddressType.LocalCartageCFS:
					return LocalCartageJobOrgTypeList.Descriptions.CFS;
				case DocAddressType.LocalCartageCTO:
					return LocalCartageJobOrgTypeList.Descriptions.CTO;
				case DocAddressType.LocalCartageImporter:
					return LocalCartageJobOrgTypeList.Descriptions.CNE;
				case DocAddressType.LocalCartageExporter:
					return LocalCartageJobOrgTypeList.Descriptions.CNR;
				case DocAddressType.LocalCartageYard:
					return LocalCartageJobOrgTypeList.Descriptions.CYD;
				case DocAddressType.LocalCartageService:
					return LocalCartageJobOrgTypeList.Descriptions.SRV;
				case DocAddressType.LocalCartageWarehouse:
					return LocalCartageJobOrgTypeList.Descriptions.WHS;
				case DocAddressType.LocalCartageMSC:
					return LocalCartageJobOrgTypeList.Descriptions.MSC;
				case DocAddressType.BookingPartyDocumentaryAddress:
					return DocAddressTypes.Descriptions.BookingPartyDocumentaryAddress;
				case DocAddressType.ClientRequestedBillingParty:
					return DocAddressTypes.Descriptions.ClientRequestedBillingParty;
				case DocAddressType.NonPersistent:
				case DocAddressType.None:
					return "";
				default:
					throw new NotSupportedException(docAddressType.ToString() + " needs to be added to GetOrgTypeDescriptionFromCartageDocAddressType");
			}
		}

		public static DocAddressType GetCartageDocAddressTypeFromOrgType(ZString orgType)
		{
			switch (orgType)
			{
				case LocalCartageJobOrgTypeList.Codes.CFS:
					return DocAddressType.LocalCartageCFS;
				case LocalCartageJobOrgTypeList.Codes.CTO:
					return DocAddressType.LocalCartageCTO;
				case LocalCartageJobOrgTypeList.Codes.CNE:
					return DocAddressType.LocalCartageImporter;
				case LocalCartageJobOrgTypeList.Codes.CNR:
					return DocAddressType.LocalCartageExporter;
				case LocalCartageJobOrgTypeList.Codes.CYD:
					return DocAddressType.LocalCartageYard;
				case LocalCartageJobOrgTypeList.Codes.SRV:
					return DocAddressType.LocalCartageService;
				case LocalCartageJobOrgTypeList.Codes.WHS:
					return DocAddressType.LocalCartageWarehouse;
				case LocalCartageJobOrgTypeList.Codes.MSC:
					return DocAddressType.LocalCartageMSC;
				case DocAddressTypes.Codes.BookingPartyDocumentaryAddress:
					return DocAddressType.BookingPartyDocumentaryAddress;
				case DocAddressTypes.Codes.ClientRequestedBillingParty:
					return DocAddressType.ClientRequestedBillingParty;
				case "":
					return DocAddressType.None;
				default:
					throw new NotSupportedException(orgType + " needs to be added to GetCartageDocAddressTypeFromOrgType");
			}
		}

		#endregion

		#region Indexers

		#region CommonCartageOrgIndexer

		public class CommonCartageOrgIndexer
		{
			internal CommonCartageOrgIndexer(SortedList collection)
			{
				this.collection = collection;
			}

			public CommonCartageOrg this[int index]
			{
				get
				{
					CommonCartageOrg result = null;

					if (collection != null && index >= 0 && index < collection.Count && collection[index] != null)
					{
						result = (CommonCartageOrg)collection[index];
					}

					return result;
				}
			}

			public int Count
			{
				get { return collection.Count; }
			}

			public bool Contains(string orgTypeCode)
			{
				bool result = false;

				foreach (CommonCartageOrg orgType in collection)
				{
					if (orgType.E5_OrgType == orgTypeCode)
					{
						result = true;
						break;
					}
				}

				return result;
			}

			readonly SortedList collection;
		}

		#endregion

		#endregion
	}
}
