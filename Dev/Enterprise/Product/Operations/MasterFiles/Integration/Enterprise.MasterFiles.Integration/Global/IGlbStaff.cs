using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Integration
{
	public interface IGlbStaff : IBusiness, IUser
	{
		ZString GS_NextOfKin { get; set; }
		new ZGuid PK { get; }
		ZString GS_FullName { get; set; }
		ZString GS_Code { get; set; }
		ZString GS_Title { get; set; }
		ZString GS_EmailAddress { get; set; }
		ZString GS_LoginName { get; set; }
		ZString GS_WorkingLanguage { get; set; }
		ZBool GS_IsActive { get; set; }
		ZGuid GS_ActiveDirectoryObjectGuid { get; set; }
		ZString GS_UserAddress1 { get; set; }
		ZString GS_City { get; set; }
		ZString GS_FriendlyName { get; set; }
		ZString GS_State { get; set; }
		ZString GS_Postcode { get; set; }
		ZString GS_WorkPhone { get; set; }
		ZString GS_FaxNum { get; set; }
		ZString GS_HomePhone { get; set; }
		ZString GS_MobilePhone { get; set; }
		ZString GS_Pager { get; set; }
		ZBool GS_ChangePasswordAtNextLogin { get; set; }
		ZString StaffConfirmPassword { get; set; }
		ZString StaffPlainTextPassword { get; set; }
		ZDateTime GS_EmploymentDate { get; set; }
		ZDateTime GS_DepartureDate { get; set; }
		IBusinessObjectCollection Groups { get; }
		IBusinessObjectCollection Capabilities { get; }
		ZBool GS_IsController { get; set; }
		ZBool GS_IsSystemAccount { get; set; }
		ZBool GS_IsRobot { get; set; }
		ZString GS_ExternalId { get; set; }
	}
}
