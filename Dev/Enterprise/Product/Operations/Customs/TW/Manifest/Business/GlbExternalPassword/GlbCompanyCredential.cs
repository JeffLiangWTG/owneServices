using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Manifest.Business
{
	public class GlbCompanyCredential : TW.Business.GlbExternalPasswordBase_TW
	{
		public GlbCompanyCredential(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			GP_PasswordType = PasswordTypesList.Codes.TVF;
		}

		public new GlbCompanyCredentialLookups Lookups => (GlbCompanyCredentialLookups)base.Lookups;
		protected override GlbExternalPasswordLookups GetNewLookups()
		{
			return new GlbCompanyCredentialLookups(this);
		}

		public new GlbCompanyCredentialValidation Validation => (GlbCompanyCredentialValidation)GetNewValidation();
		protected override GlbExternalPasswordValidation GetNewValidation()
		{
			return new GlbCompanyCredentialValidation(this);
		}

		[MaxLength(35)]
		[ResourceStringData("D9EE8745-FBBE-47BF-BB7E-DFBB5622A14C", Caption = "Mailbox")]
		public override ZString GP_MailBoxID { get => base.GP_MailBoxID; set => base.GP_MailBoxID = value; }

		[MaxLength(35)]
		[ResourceStringData("59F189B3-4B4C-46AA-847E-192B3E722ED6", Caption = "Mailbox Password")]
		public override ZString CurrentDecryptedPassword { get => base.CurrentDecryptedPassword; set => base.CurrentDecryptedPassword = value; }

		[MaxLength(35)]
		[ResourceStringData("45BA985C-E3BF-4E0B-BF93-DC8C3169FE1B", Caption = "Sender ID")]
		public override ZString GP_UserID { get => base.GP_UserID; set => base.GP_UserID = value; }

		[MaxLength(35)]
		[ResourceStringData("1D20C6B9-5FB8-4F17-9C6F-82573647A2DB", Caption = "Certificate Password")]
		public override ZString CurrentDecryptedCertificatePassphrase { get => base.CurrentDecryptedCertificatePassphrase; set => base.CurrentDecryptedCertificatePassphrase = value; }

		[ReadOnly(true)]
		[ResourceStringData("310999DB-BD70-481E-A76D-DF504C99DDFD", Caption = "Status")]
		public override ZString GP_PasswordStatus { get => base.GP_PasswordStatus; set => base.GP_PasswordStatus = value; }

		[ReadOnly(true)]
		[ResourceStringData("C874AFC9-86A0-44D3-8809-8364FC7B3B12", Caption = "Status Reason")]
		public override ZString GP_StatusReason { get => base.GP_StatusReason; set => base.GP_StatusReason = value; }

		public bool IsEmpty => CertificateStatus == CertificateEmpty
			&& GP_MailBoxID.IsEmpty
			&& CurrentDecryptedPassword.IsEmpty
			&& GP_UserID.IsEmpty
			&& CurrentDecryptedCertificatePassphrase.IsEmpty;
	}
}
