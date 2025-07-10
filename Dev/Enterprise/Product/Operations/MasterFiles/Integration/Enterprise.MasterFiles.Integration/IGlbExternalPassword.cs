using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Integration
{
	public interface IGlbExternalPassword
	{
		BusinessObjectFactory Factory { get; }
		ZGuid PK { get; }
		ZString GP_UserID { get; set; }
		ZString GP_SystemLastEditUser { get; set; }
		ZDateTime GP_SystemLastEditTimeUtc { get; set; }
		ZString GP_SystemCreateUser { get; set; }
		ZDateTime GP_SystemCreateTimeUtc { get; set; }
		ZString GP_StatusReason { get; set; }
		ZString GP_PasswordType { get; set; }
		ZString GP_PasswordStatus { get; set; }
		ZString GP_NextPassword { get; set; }
		ZString GP_MailBoxID { get; set; }
		ZGuid GP_GS { get; set; }
		ZGuid GP_GG { get; set; }
		ZGuid GP_GC { get; set; }
		ZGuid GP_GB { get; set; }
		ZString GP_CurrentPassword { get; set; }
		ZString GP_CertificatePassPhrase { get; set; }
		ZBlob GP_Certificate { get; set; }
		ZString CurrentDecryptedPassword { get; set; }
		ZString NextDecryptedPassword { get; set; }
		ZString GP_PasswordStatusDescription { get; }
		ZString GP_PasswordTypeDescription { get; }
		ZDateTime GP_IssueDate { get; set; }
		ZDateTime GP_ExpiryDate { get; set; }
		bool IsDeleted { get; }
		bool IsInDatabase { get; }
	}
}
