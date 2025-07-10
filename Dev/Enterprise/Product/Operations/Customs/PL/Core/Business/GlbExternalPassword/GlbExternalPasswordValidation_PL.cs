using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.PL.Business;

public class GlbExternalPasswordValidation_PL(GlbExternalPassword_PL parent) : GlbExternalPasswordWithCertificateValidation(parent)
{
	const int OnlineRevocationVerificationTimeout = 1_000;

	protected new GlbExternalPassword_PL Parent => (GlbExternalPassword_PL)base.Parent;

	protected override void CheckGP_ExpiryDate()
	{
		base.CheckGP_ExpiryDate();
		var expiryDate = Parent.GP_ExpiryDate;
		if (expiryDate >= ZDateTime.Now && expiryDate < ZDateTime.Now.AddMonths(1))
		{
			Parent.GP_ExpiryDateInfo.AddWarning(Res.GetString("FBEA6543-283C-4EAA-84ED-7831EE0C093F", "This certificate will expire soon – the expiry date is within one month."));
		}
	}

	protected override void AddGP_ExpiryDateExpiredNotification()
	{
		Parent.GP_ExpiryDateInfo.AddError(Res.GetString("2B0D7A89-7C66-448B-B8F2-AD84F2919A8A", "This certificate is not valid – the expiry date is in the past."));
	}

	protected override void CheckGP_IssueDateIsValidZDateTimeRange() { }
	protected override void CheckGP_ExpiryDateIsValidZDateTimeRange() { }

	protected override bool IsCurrentDecryptedCertificatePassphraseMandatory => Parent.GP_Certificate != null && !Parent.GP_Certificate.IsEmpty;
	protected override bool IsCertificateMandatory => false;

	protected override void CheckGP_Certificate()
	{
		base.CheckGP_Certificate();

		if (!Parent.GP_MailBoxID.IsEmpty && Parent.GP_Certificate.IsEmpty)
		{
			Parent.GP_CertificateInfo.AddMessageError(Res.GetString("754FB462-40C6-445E-A218-9B4C7C8924E5", "A certificate is required for sending electronic declarations to PUESC."));
		}

		try
		{
			var certificateChainErrors = CertificateValidationHelper.GetInvalidCertificateChainErrors(Parent.GP_Certificate, Parent.CurrentDecryptedCertificatePassphrase, ConfigureChainPolicyWithNoRevocationCheck);
			if (!string.IsNullOrEmpty(certificateChainErrors))
			{
				Parent.GP_CertificateInfo.AddWarning(certificateChainErrors);
			}
		}
		catch (CryptographicException)
		{
		}
	}

	protected override void CheckCurrentDecryptedCertificatePassphrase()
	{
		base.CheckCurrentDecryptedCertificatePassphrase();

		if (Parent.GP_PasswordStatus == PasswordStatusList.Codes.Invalid)
		{
			if (Parent.GP_Certificate != null && !Parent.GP_Certificate.IsEmpty)
			{
				Parent.CurrentDecryptedCertificatePassphraseInfo.AddError(Res.GetString("0FC7214C-F6DB-4035-AC94-573FEEACD2BA", "This password is not valid for specified certificate."));
			}
		}
	}

	protected override void CheckCurrentDecryptedPassword()
	{
		base.CheckCurrentDecryptedPassword();

		if (!Parent.GP_MailBoxID.IsEmpty && Parent.CurrentDecryptedPassword.IsEmpty)
		{
			Parent.CurrentDecryptedPasswordInfo.AddError(Res.GetString("A7ECAE1B-A556-4601-823D-B494B9580E3A", "The password is required for the login."));
		}
	}

	static void ConfigureChainPolicyWithNoRevocationCheck(X509ChainPolicy policy)
	{
		policy.VerificationTime = ZDateTime.Now.ToDateTime();
		policy.UrlRetrievalTimeout = TimeSpan.FromMilliseconds(OnlineRevocationVerificationTimeout);
		policy.VerificationFlags = Globals.IsTest || Globals.IsDebugMode ? X509VerificationFlags.AllowUnknownCertificateAuthority : X509VerificationFlags.NoFlag;
	}
}
