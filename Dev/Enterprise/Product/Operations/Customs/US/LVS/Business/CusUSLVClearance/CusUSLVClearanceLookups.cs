//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusUSLVClearanceLookups
//
//    This class should be used for overriding collections in AutoCusUSLVClearanceLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using CargoWise.Types;
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
	public class CusUSLVClearanceLookups : AutoCusUSLVClearanceLookups
	{
		public CusUSLVClearanceLookups(AutoCusUSLVClearance parent) : base(parent)
		{
		}

		public CodeDescriptionPairList ULH_TransportModeList => GetULH_TransportModeList(Factory);

		public static CodeDescriptionPairList GetULH_TransportModeList(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("USLVTransportTypeList", () =>
			{
				var result = new CodeDescriptionPairList();
				result.AddPair(TransportTypeList.Codes.Sea, TransportTypeList.Descriptions.Sea);
				result.AddPair(TransportTypeList.Codes.Rail, TransportTypeList.Descriptions.Rail);
				result.AddPair(TransportTypeList.Codes.Road, TransportTypeList.Descriptions.Road);
				result.AddPair(TransportTypeList.Codes.Air, TransportTypeList.Descriptions.Air);
				result.AddPair(TransportTypeList.Codes.Mail, TransportTypeList.Descriptions.Mail);
				result.AddPair(TransportTypeList.Codes.Truck, TransportTypeList.Descriptions.Truck);
				return result;
			});
		}

		public CodeDescriptionPairList ULH_IORTypeList => GetULH_IORTypeList(Factory);

		public static CodeDescriptionPairList GetULH_IORTypeList(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("USLVRegistrationTypeList", () =>
			{
				var result = new CodeDescriptionPairList();
				var codeLists = new OrgCodeLists();
				var cusCodes = codeLists.CustomsCodes_List(Core.Constants.CountryCodes.UnitedStates);
				result.AddPair(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, cusCodes.GetDescriptionFromCode(OrgCusCode.USACodeTypes.EmployerIdentificationNumber));
				result.AddPair(OrgCusCode.USACodeTypes.CBPAssignedNumber, cusCodes.GetDescriptionFromCode(OrgCusCode.USACodeTypes.CBPAssignedNumber));
				result.AddPair(OrgCusCode.USACodeTypes.SocialSecurityNumber, cusCodes.GetDescriptionFromCode(OrgCusCode.USACodeTypes.SocialSecurityNumber));
				return result;
			});
		}

		public CodeDescriptionPairList ULH_ContainerModeList => GetULH_ContainerModeList(Factory);

		public static CodeDescriptionPairList GetULH_ContainerModeList(BusinessObjectFactory factory) => factory.GetCachedValue<ContainerModeList>();

		public USCarrierCombinedCollection ULH_MasterBillIssuerSCACList => new USCarrierCombinedCollection(Factory);

		public USCarrierCombinedCollection ULH_CarrierSCACList => new USCarrierCombinedCollection(Factory);

		public RefVesselCollection Vessels => new RefVesselCollection(Factory);

		public RefUNLOCOCollection ULH_RL_NKPortOfLoadingList => new RefUNLOCOCollection(Factory);

		public RefUNLOCOCollection ULH_RL_NKPortOfDischargeList => new RefUNLOCOCollection(Factory);

		public ZZRefCusCodeListCombinedCollection ULH_US_NKLocationOfGoodsList => ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, ZDateTime.Today);

		public ZZRefCusCodeListCombinedCollection ULH_US_NKCentralizedExamSiteList => ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, ZDateTime.Today);

		public ZZRefCusCodeListCombinedCollection ULH_PortOfLoadingList
		{
			get
			{
				ZZRefCusCodeListCombinedCollection result = null;
				var parent = (CusUSLVClearance)Parent;
				if (parent.ULH_PortOfLoadingIsDropEdit)
				{
					result = parent.PortOfLadingRefLocoMappings as ZZRefCusCodeListCombinedCollection;
				}
				return result ?? ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, ZDateTime.Today);
			}
		}

		public ZZRefCusCodeListCombinedCollection ULH_PortOfEntryList => ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today);

		public ZZRefCusCodeListCombinedCollection ULH_PortOfDischargeList
		{
			get
			{
				ZZRefCusCodeListCombinedCollection result = null;
				var parent = (CusUSLVClearance)Parent;
				if (parent.ULH_PortOfDischargeIsDropEdit)
				{
					result = parent.PortOfDischargeRefLocoMappings as ZZRefCusCodeListCombinedCollection;
				}
				return result ?? ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today);
			}
		}

		public ZZRefCusCodeListCombinedCollection ULH_PreparerDistrictPortList => ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today);
	}
}
