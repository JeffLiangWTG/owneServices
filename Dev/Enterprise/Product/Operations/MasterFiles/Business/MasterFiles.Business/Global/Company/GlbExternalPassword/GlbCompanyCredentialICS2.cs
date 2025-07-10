using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business.Customs.XmlCredential;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class GlbCompanyCredentialICS2 : GlbExternalPasswordWithCertificate
	{
		public GlbCompanyCredentialICS2(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			GP_PasswordType = PasswordTypesList.Codes.IC2;
		}

		[ResourceStringData("{4951AE1D-C2A9-450D-A0EF-1D9BB316DF17}", Caption = "Reporter EORI")]
		public override ZString GP_MailBoxID { get => base.GP_MailBoxID; set => base.GP_MailBoxID = value; }

		#region Password status

		[ReadOnly(true)]
		public override ZString GP_PasswordStatus { get => base.GP_PasswordStatus; set => base.GP_PasswordStatus = value; }

		public ZString PasswordStatus => Lookups.PasswordStatusList.GetDescriptionFromCode(base.GP_PasswordStatus) ?? base.GP_PasswordStatus;

		protected override void ChangeStatusWhenUpdateCurrentPassword(ZString oldValue)
		{
			if (oldValue != GP_CurrentPassword)
			{
				GP_PasswordStatus = GetCredentialStatus();
			}
		}

		#endregion

		#region Override

		public override object CreateCredentialData()
		{
			return new Group()
			{
				Type = GP_PasswordType,
				Status = GetCredentialStatus(),
				Reference = GP_MailBoxID,
				Items = CreateCredentialItems()
			};
		}

		protected override object[] CreateCredentialItems()
		{
			var item = new Item() { Name = Constants.ItemTypes.Platform, Value = PasswordTypesList.Codes.IC2 };

			if (!IsDeleted && !GP_Certificate.IsEmpty && !CurrentDecryptedCertificatePassphrase.IsEmpty)
			{
				var certificate = CredentialSender.CreateCertificate(GP_Certificate, CurrentDecryptedCertificatePassphrase);
				return new object[] { item, certificate };
			}
			else
			{
				return new object[] { item };
			}
		}

		protected override ZPropertyInfo[] CredentialApplicableInfos()
		{
			return new ZPropertyInfo[] { GP_CertificatePassPhraseInfo, GP_MailBoxIDInfo, GP_CertificateInfo };
		}

		public override CredentialRecipient CredentialRecipient => CredentialRecipient.DirectxT;

		public override ZString ConfigurationName => "EUICS2Credential";

		protected override string InterchangeTypeForSending => Constants.IdentifierList.IC2;

		protected override void RegisterConfigurationForSending()
		{
			var credentials = Factory.Load<GlbCompanyCredentialICS2>(new ZQuery(GlbExternalPasswordSchema.GP_PasswordType, PasswordTypesList.Codes.IC2));

			foreach (var credential in credentials.OrderBy(c => c.Company.GC_Code))
			{
				ExternalPasswordConfigurationToSender.RegisterForSending(Factory, ConfigurationName, credential.Company, credential.Group, credential.Staff ?? credential.OriginalStaff, InterchangeTypeForSending, CredentialRecipient);
			}
		}

		#endregion

		#region Private Key Password

		public override ZString CurrentDecryptedCertificatePassphrase
		{
			get => base.CurrentDecryptedCertificatePassphrase;
			set
			{
				var oldValue = CurrentDecryptedCertificatePassphrase;
				base.CurrentDecryptedCertificatePassphrase = value;
				if (oldValue != CurrentDecryptedCertificatePassphrase)
				{
					CalculateGP_CertificatePassphraseStatus();
				}
			}
		}

		#endregion

		protected override GlbExternalPasswordValidation GetNewValidation() => new GlbCompanyCredentialICS2Validation(this);
	}
}
