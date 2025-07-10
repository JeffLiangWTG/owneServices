using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Accounting;

namespace Enterprise.MasterFiles.Business
{
	public class EInvoicingCertificateCredentialValidation : GlbExternalPasswordValidation
	{
		public EInvoicingCertificateCredentialValidation(EInvoicingCertificateCredential parent)
			: base(parent)
		{
		}

		protected new EInvoicingCertificateCredential Parent
		{
			get { return (EInvoicingCertificateCredential)base.Parent; }
		}

		protected override void CheckGP_IssueDate()
		{
			base.CheckGP_IssueDate();

			var today = ZDateTime.Today;
			if (!Parent.IsInDatabase && Parent.GP_IssueDate > today)
			{
				Parent.GP_IssueDateInfo.AddWarning(Res.GetString("16a676fb-23f4-4e38-8d5b-4636808fd400", "Certificate is not yet valid"));
			}
		}

		protected override void CheckGP_ExpiryDate()
		{
			base.CheckGP_ExpiryDate();

			var today = ZDateTime.Today;
			if (Parent.GP_ExpiryDate < today)
			{
				if (Parent.IsInDatabase)
				{
					Parent.GP_ExpiryDateInfo.AddWarning(Res.GetString("4f9bfd36-0df4-419b-9bb6-ac94df82c349", "The certificate has already expired, please renew"));
				}
				else
				{
					Parent.GP_ExpiryDateInfo.AddError(Res.GetString("29418eb2-69ed-4c23-a236-a130ad0b4b99", "Certificate is expired"));
				}
			}
			else if (Parent.CredentialSettings is IEInvoicingCertificateCredentialSettings credSettings && Parent.GP_ExpiryDate <= today.AddDays(credSettings.ExpiryWarningDays))
			{
				Parent.GP_ExpiryDateInfo.AddWarning(Res.GetString("c1ff66fa-8e16-4790-99c2-fdbad151a0c0", "Certificate will expire soon, please renew"));
			}
		}

		protected override void CheckCurrentDecryptedCertificatePassphrase()
		{
			base.CheckCurrentDecryptedCertificatePassphrase();

			MandatoryValidation.CheckEntered(Parent.CurrentDecryptedCertificatePassphraseInfo, Res.GetString("84becba5-32ca-45e0-aad3-78ba6a0fe40a", "Pass Phrase"));

			if (Parent.IsSupportCertificateDataBase64EncodedTwice && Parent.CurrentDecryptedCertificatePassphrase.Length > 0)
			{
				try
				{
					var passPhraseBytes = Convert.FromBase64String(Parent.CurrentDecryptedCertificatePassphrase);
					if (passPhraseBytes != null && passPhraseBytes.Length != 32)  //As today, only SA support base64 twice encoded certificate, and it expects pass phrase to be exactly 32 bytes, this code needs to be revisited if we have new countries with different requirement
					{
						Parent.CurrentDecryptedCertificatePassphraseInfo.AddWarning(Res.GetString("aaaf80fb-a2e5-45cf-a103-f6617297a2f1", "Unexpected length of Pass Phrase. Please confirm this has been entered correctly."));
					}
				}
				catch (FormatException)
				{
					Parent.CurrentDecryptedCertificatePassphraseInfo.AddWarning(Res.GetString("f6de6e3b-985f-44d4-82a7-911f4ed39a02", "Invalid Base64 encoded string has been entered. Please confirm this has been entered correctly."));
				}
			}
		}

		protected override void CheckGP_PasswordStatus()
		{
			base.CheckGP_PasswordStatus();

			if (!Parent.IsInDatabase)
			{
				if (Parent.GP_Certificate == null || Parent.GP_Certificate.Length == 0)
				{
					Parent.GP_PasswordStatusInfo.AddError(Res.GetString("f4f8dfda-ca74-4fc5-ac58-30b28128a109", "Certificate must be loaded."));
				}
				else if (string.IsNullOrEmpty(Parent.CurrentDecryptedCertificatePassphrase) && Parent.GP_PasswordStatus == PasswordStatusList.Codes.Invalid)
				{
					Parent.GP_PasswordStatusInfo.AddError(Res.GetString("e2c6c802-b139-4ebe-883b-f63ce8988339", "Certificate cannot be read because the pass phrase is blank and the certificate file requires a pass phrase."));
				}
				else if (!string.IsNullOrEmpty(Parent.CurrentDecryptedCertificatePassphrase) && Parent.GP_PasswordStatus == PasswordStatusList.Codes.Invalid)
				{
					Parent.GP_PasswordStatusInfo.AddError(Res.GetString("88a899da-ba6e-49ac-a045-2821878e34aa", "Certificate cannot be read because the certificate file is corrupt or the pass phrase is incorrect."));
				}
				else
				{
					if (string.IsNullOrEmpty(Parent.SubjectNameCommonName) || string.IsNullOrEmpty(Parent.IssuerNameCommonName))
					{
						if (string.IsNullOrEmpty(Parent.SubjectNameCommonName))
						{
							Parent.GP_PasswordStatusInfo.AddError(Res.GetString("80cdf7ea-5c1f-412c-b1a7-281536f5945a", "Certificate has not been issued correctly because the Subject field is missing a Common Name."));
						}
						else if (string.IsNullOrEmpty(Parent.IssuerNameCommonName))
						{
							Parent.GP_PasswordStatusInfo.AddError(Res.GetString("e143a2b8-a0e1-4928-a713-9bd48d17966d", "Certificate has not been issued correctly because the Issuer field is missing a Common Name."));
						}
					}
					else if (Parent.SubjectNameCommonName == Parent.IssuerNameCommonName)
					{
						Parent.GP_PasswordStatusInfo.AddError(Res.GetString("bb1033c6-b0c7-4c03-8d28-de8cf2ae1a50", "Certificate has not been issued correctly because the certificate is self-signed."));
					}
				}
			}
		}
	}
}
