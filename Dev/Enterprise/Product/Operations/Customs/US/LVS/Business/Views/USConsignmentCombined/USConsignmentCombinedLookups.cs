//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSConsignmentCombinedLookups
//
//    This class should be used for overriding collections in AutoUSConsignmentCombinedLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.LVS.Business
{
	public class USConsignmentCombinedLookups : AutoUSConsignmentCombinedLookups
	{
		public USConsignmentCombinedLookups(AutoUSConsignmentCombined parent)
			: base(parent)
		{
		}

		public GlbBranchCollection Branches => new GlbBranchCollection(Factory);

		public OrgHeaderCollection Importers => new OrgHeaderCollection(Factory);

		public OrgHeaderCollection Clients => new OrgHeaderCollection(Factory);

		public ConsigneeCollection Consignees => new ConsigneeCollection(Factory);

		public OrgAddressCollection ConsigneeAddresses => new OrgAddressCollection(Factory);

		public ConsignorCollection Sellers => new ConsignorCollection(Factory);

		public OrgAddressCollection SellerAddresses => new OrgAddressCollection(Factory);

		public ImportMessageStatusList MessageStatuses => Factory.GetCachedValue<ImportMessageStatusList>();

		public CRLReleaseStatusList ReleaseStatuses => Factory.GetCachedValue<CRLReleaseStatusList>();

		public static CodeDescriptionPairList GetTransportModes(BusinessObjectFactory factory) => CusUSLVClearanceLookups.GetULH_TransportModeList(factory);
		public CodeDescriptionPairList TransportModes => CusUSLVClearanceLookups.GetULH_TransportModeList(Factory);

		public CodeDescriptionPairList JobTypes => GetJobTypes(Factory);

		public static CodeDescriptionPairList GetJobTypes(BusinessObjectFactory factory) =>
			factory.GetCachedValue("USLVConsignmentJobTypeList", () => new USConsignmentCombinedJobTypes());

		public ZZRefCusCodeListCombinedCollection PortOfLoadingList => ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, ZDateTime.Today);

		public ZZRefCusCodeListCombinedCollection PortOfEntryList => ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today);

		public ZZRefCusCodeListCombinedCollection PortOfDischargeList => ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today);
	}
}
