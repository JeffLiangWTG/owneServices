using System;
using System.Security.Cryptography.X509Certificates;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.PL.Business;

public abstract class MessagePreSendingValidationBase
{
	static readonly TimeSpan UrlRetrievalTimeout = TimeSpan.FromSeconds(1);

	public void Validate(INotifications notifications) => ValidateCore(notifications);

	protected virtual void ValidateCore(INotifications notifications)
	{
		var currentUser = GlbStaff.CurrentUser;
		var certificate = currentUser.GetPLWrapper().PLBPassword;
		if (CurrentUserCredentialsAreIncomplete(certificate))
		{
			notifications.AddError(Res.GetString("3fb91cce-ef8b-4322-a057-f36f437073ad|MissingCredentials", "PUESC Credentials are not set. Please, ensure that 'PUESC login', 'Password', 'PLB Certificate' and 'Certificate Password' are configured for current Staff member."));
		}

		if (CurrentUserHaveExpiredCertificate(certificate))
		{
			notifications.AddError(Res.GetString("52218a26-cedb-4dc1-b027-58b4464ce7bc|ExpiredCertificate", "The certificate has expired, add a new certificate before sending customs message."));
		}

		if (TryGetCertificateChainErrors(certificate, out var chainErrors))
		{
			notifications.AddWarning(Res.GetString("e789cfea-4b97-4a25-b616-7d82d924b190|InvalidCertificateChain", "PUESC certificate chain is not valid:{0}{1}", System.Environment.NewLine, chainErrors));
		}
	}

	static bool CurrentUserCredentialsAreIncomplete(IGlbExternalPasswordWithCertificate certificate)
	{
		return certificate == null
				|| certificate.GP_MailBoxID.IsEmpty
				|| certificate.GP_Certificate.IsEmpty
				|| certificate.CurrentDecryptedCertificatePassphrase.IsEmpty;
	}

	static bool CurrentUserHaveExpiredCertificate(IGlbExternalPasswordWithCertificate certificate)
	{
		return certificate is { GP_Certificate.IsEmpty: false, GP_ExpiryDate.IsEmpty: false }
				&& certificate.GP_ExpiryDate.IsInThePast();
	}

	static bool TryGetCertificateChainErrors(IGlbExternalPasswordWithCertificate certificate, out string chainErrors)
	{
		chainErrors = null;

		if (certificate is null)
		{
			return false;
		}

		chainErrors = CertificateValidationHelper.GetInvalidCertificateChainErrors(certificate.GP_Certificate, certificate.CurrentDecryptedCertificatePassphrase, ConfigureChainPolicyWithNoRevocationCheck);
		return !string.IsNullOrEmpty(chainErrors);
	}

	static void ConfigureChainPolicyWithNoRevocationCheck(X509ChainPolicy policy)
	{
		policy.VerificationTime = ZDateTime.Now.ToDateTime();
		policy.UrlRetrievalTimeout = UrlRetrievalTimeout;
		policy.VerificationFlags = Globals.IsTest || Globals.IsDebugMode
			? X509VerificationFlags.AllowUnknownCertificateAuthority
			: X509VerificationFlags.NoFlag;
	}
}
