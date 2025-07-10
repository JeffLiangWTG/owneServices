using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Security.Cryptography.X509Certificates;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.UY.Manifest.Business
{
	public class GlbCompanyCredential : GlbExternalPasswordWithCertificate, IxTMessageAttributeProvider
	{
		public GlbCompanyCredential(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			GP_PasswordType = PasswordTypesList.Codes.UTB;
		}

		#region Overrided properties

		[MaxLength(8)]
		public override ZString GP_UserID
		{
			get { return base.GP_UserID; }
			set
			{
				var oldValue = GP_UserID;
				base.GP_UserID = value;
				if (!IsCopying && oldValue != GP_UserID)
				{
					CalculateGP_CertificatePassphraseStatus();
				}
			}
		}

		protected override bool GP_UserID_ReadOnly => false;

		[MaxLength(15)]
		public override ZString CurrentDecryptedPassword
		{
			get { return base.CurrentDecryptedPassword; }
			set
			{
				var oldValue = CurrentDecryptedPassword;
				base.CurrentDecryptedPassword = value;
				if (!IsCopying && oldValue != CurrentDecryptedPassword)
				{
					CalculateGP_CertificatePassphraseStatus();
				}
			}
		}

		public override ZBlob GP_Certificate
		{
			get => base.GP_Certificate;
			set
			{
				var oldValue = GP_Certificate;
				base.GP_Certificate = value;
				if (!IsCopying && oldValue != GP_Certificate)
				{
					CalculateGP_CertificatePassphraseStatus();
				}
			}
		}

		[MaxLength(30)]
		public override ZString CurrentDecryptedCertificatePassphrase
		{
			get => base.CurrentDecryptedCertificatePassphrase;
			set
			{
				var oldValue = CurrentDecryptedCertificatePassphrase;
				base.CurrentDecryptedCertificatePassphrase = value;
				if (!IsCopying && oldValue != CurrentDecryptedCertificatePassphrase)
				{
					CalculateGP_CertificatePassphraseStatus();
				}
			}
		}

		[ReadOnly(true)]
		public override ZString GP_PasswordStatus { get => base.GP_PasswordStatus; set => base.GP_PasswordStatus = value; }

		[ReadOnly(true)]
		public override ZString GP_StatusReason { get => base.GP_StatusReason; set => base.GP_StatusReason = value; }

		#endregion

		#region Password status

		public ZString PasswordStatus => Lookups.PasswordStatusList.GetDescriptionFromCode(base.GP_PasswordStatus) ?? base.GP_PasswordStatus;

		protected override void ChangeStatusWhenUpdateCurrentPassword(ZString oldValue)
		{
			var newValue = GP_CurrentPassword;
			if (oldValue != newValue)
			{
				GP_PasswordStatus = GetCredentialStatus();
			}
		}

		protected override bool ShouldSendCredential() => false;

		#endregion

		protected override void ClearDataDefaultedFromCertificate()
		{
			GP_ExpiryDate = ZDateTime.Empty;
		}

		protected override void DefaultDataFromCertificate(X509Certificate2 certificate)
		{
			GP_ExpiryDate = certificate.NotAfter;
		}

		protected override GlbExternalPasswordValidation GetNewValidation()
		{
			return new GlbCompanyCredentialValidation(this);
		}

		public new GlbCompanyCredentialValidation Validation => (GlbCompanyCredentialValidation)GetNewValidation();

		public Dictionary<string, string> GetMessageAttrDictionary()
		{
			var t = (IxTMessageAttributeProvider)this;
			return t.GetXTMsgAttrProviderForGlbExternalPasswordSendingHttpUserAndPassword(true);
		}
	}
}
