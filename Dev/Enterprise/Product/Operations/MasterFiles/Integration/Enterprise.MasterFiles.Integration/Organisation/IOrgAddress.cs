using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Integration
{
	public interface IOrgAddress : IBusiness
	{
		ZGuid PK { get; }
		ZString Address { get; }
		ZString AddressDetailed { get; }
		ZString AddressDetailedOnSingleLine { get; }
		ZString AddressFull { get; }
		ZString AddressFullFormatted { get; }
		IOrgHeader Header { get; }
		ZGuid OrganisationPK { get; }
		ZPropertyInfo UsageCommentInfo { get; }

		ZString OA_AccessPoint { get; set; }
		ZString OA_Address1 { get; set; }
		ZString OA_Address2 { get; set; }
		ZString OA_AIREquipmentNeeded { get; set; }
		ZString OA_City { get; set; }
		ZString OA_Code { get; set; }
		ZString OA_CommunicationRequired { get; set; }
		ZString OA_CompanyNameOverride { get; set; }
		ZString OA_ContainerHandling { get; set; }
		ZDateTime OA_DeliverFromTimeOnly { get; set; }
		ZDateTime OA_DeliverToTimeOnly { get; set; }
		ZString OA_Dock_Height { get; set; }
		ZBool OA_DockLeveler { get; set; }
		ZDateTime OA_DoNotAttendFrom { get; set; }
		ZDateTime OA_DoNotAttendTo { get; set; }
		ZString OA_Email { get; set; }
		ZString OA_Fax { get; set; }
		ZString OA_Fax_Formatted { get; set; }
		ZString OA_FCLEquipmentNeeded { get; set; }
		ZBool OA_ForkLift { get; set; }
		ZGeography OA_GeoLocation { get; set; }
		ZBool OA_IsActive { get; set; }
		ZString OA_LabourRequired { get; set; }
		ZString OA_Language { get; set; }
		ZDecimal OA_Latitude { get; set; }
		ZString OA_LCLEquipmentNeeded { get; set; }
		ZString OA_LoadingUnloadingConstraints { get; set; }
		ZDecimal OA_Longitude { get; set; }
		ZString OA_Mobile { get; set; }
		ZString OA_Mobile_Formatted { get; set; }
		ZGuid OA_OH { get; set; }
		ZString OA_OtherWarehouseFacilities { get; set; }
		ZBool OA_PalletJack { get; set; }
		ZString OA_Phone { get; set; }
		ZString OA_Phone_Formatted { get; set; }
		ZDateTime OA_PickupFromTimeOnly { get; set; }
		ZDateTime OA_PickupToTimeOnly { get; set; }
		ZString OA_PostCode { get; set; }
		ZString OA_RL_NKRelatedPortCode { get; set; }
		ZString OA_State { get; set; }
		ZString OA_ValidationStatus { get; set; }
		ZString ValidationStatus { get; set; }
		ZString OA_RN_NKCountryCode { get; set; }
	}
}
