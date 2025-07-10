using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace Enterprise.CryptoUtilities
{
	internal static class X509Certificate2Extensions
	{
		public static bool IsIssuedBy(this X509Certificate2 certificate, X509Certificate2 issuer)
		{
			if (issuer.Subject != certificate.Issuer)
			{
				// If passed issuer is a root CA, and certificate was issued by an intermediate CA then chain still can be built,
				// but it would not mean that passed certificate is an issuer certificate.
				return false;
			}

			using (var ch = new X509Chain())
			{
				ch.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
				ch.ChainPolicy.VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority | X509VerificationFlags.IgnoreNotTimeValid;
				ch.ChainPolicy.ExtraStore.Add(issuer);

				if (!ch.Build(certificate))
				{
					return false;
				}

				// Making sure that issuer certificate was included into the chain.
				// We may have another issuer certificate in the chain with different public key, which could come from windows certificate store.
				// We accept it only if it has the same public key. That is what old CertificateTrusted method did.
				byte[] issuerPublicKey = issuer.PublicKey.EncodedKeyValue.RawData;
				return ch.ChainElements.Cast<X509ChainElement>().Any(c => c.Certificate.PublicKey.EncodedKeyValue.RawData.SequenceEqual(issuerPublicKey));
			}
		}
	}
}
