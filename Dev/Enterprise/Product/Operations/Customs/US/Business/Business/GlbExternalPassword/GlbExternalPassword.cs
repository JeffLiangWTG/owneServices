using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public abstract class GlbExternalPassword : MasterFiles.Business.GlbExternalPassword
	{
		protected GlbExternalPassword(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Properties

		#region GP_MailBoxID

		[List(nameof(Lookups) + "." + nameof(GlbExternalPassword.Lookups.InsuranceAgents))]
		public override ZString GP_MailBoxID
		{
			get => base.GP_MailBoxID;
			set => base.GP_MailBoxID = value;
		}

		#endregion

		#region GP_MailBoxIDDescription

		public ZString MailBoxIDDescription => Lookups.InsuranceAgents.GetDescriptionFromCode(GP_MailBoxID) ?? GP_MailBoxID;

		#endregion

		#region GP_Certificate

		[MaxLength(32)]
		public override ZString CurrentDecryptedCertificatePassphrase
		{
			get => base.CurrentDecryptedCertificatePassphrase;
			set => base.CurrentDecryptedCertificatePassphrase = value;
		}

		#endregion

		#region GP_PasswordStatus

		public ZString PasswordStatus => Lookups.PasswordStatusList.GetDescriptionFromCode(base.GP_PasswordStatus) ?? base.GP_PasswordStatus;

		#endregion

		#endregion

		public new static readonly GlbExternalPasswordTypeDecider TypeDecider = new GlbExternalPasswordTypeDecider();

		public new GlbExternalPasswordLookups Lookups => (GlbExternalPasswordLookups)base.Lookups;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			GP_PasswordType = MasterFiles.Business.PasswordTypesList.Codes.EBD;
			GP_PasswordStatus = MasterFiles.Business.PasswordStatusList.Codes.Valid;
		}

		protected override bool ShouldSendCredential()
		{
			return false;
		}

		protected override bool ShouldSendDeleteCredential()
		{
			return false;
		}
	}
}
