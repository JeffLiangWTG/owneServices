using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Integration
{
	public interface IOrgContact : IBusiness
	{
		ZGuid PK { get; }
		ZString OC_AttachmentType { get; set; }
		ZDateTime OC_Birthday { get; set; }
		ZString OC_ContactName { get; set; }
		ZString OC_ContactSource { get; set; }
		ZDateTime OC_DetailsVerified { get; set; }
		ZString OC_Email { get; set; }
		ZString OC_Fax { get; set; }
		ZString OC_Gender { get; set; }
		ZString OC_HomePhone { get; set; }
		ZBool OC_IsActive { get; set; }
		ZString OC_JobCategory { get; set; }
		ZString OC_Language { get; set; }
		ZString OC_Mobile { get; set; }
		ZString OC_NotifyMode { get; set; }
		ZGuid OC_OA_OrgAddress { get; set; }
		ZGuid OC_OH { get; set; }
		ZGuid OC_OH_AddressOverride { get; set; }
		ZString OC_OtherPhone { get; set; }
		ZString OC_Pager { get; set; }
		ZString OC_PersonalInfo { get; set; }
		ZString OC_Phone { get; set; }
		ZString OC_PhoneExtension { get; set; }
		ZBlob OC_ProfilePhoto { get; set; }
		ZString OC_RN_NKNationality { get; set; }
		ZString OC_Salutation { get; set; }
		ZString OC_Title { get; set; }
		ZBool OC_WebAccessEnabled { get; set; }
		ZDateTime OC_WebContractSignedDate { get; set; }
		ZDateTime OC_YearJoinedCompany { get; set; }
		ZDateTime OC_YearJoinedIndustry { get; set; }
	}
}
