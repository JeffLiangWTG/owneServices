using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	public static class SampleCertificateHelper
	{
		public static X509Certificate2 CreateSampleRootCertificate(X500DistinguishedName distinguishedName, ZDateTime? issueDate = null, ZDateTime? expiryDate = null)
		{
			var issueDateAsDateTime = issueDate.GetValueOrDefault(ZDateTime.Today.AddYears(-1)).ToDateTime();
			var expiryDateAsDateTime = expiryDate.GetValueOrDefault(ZDateTime.Today.AddYears(5)).ToDateTime();

			using (var dsa = new ECDsaCng())
			{
				return new CertificateRequest(distinguishedName, dsa, HashAlgorithmName.SHA256)
					.Add(certificateRequest => new X509BasicConstraintsExtension(true, true, 0, true))
					.Add(certificateRequest => new X509SubjectKeyIdentifierExtension(certificateRequest.PublicKey, false))
					.CreateSelfSigned(issueDateAsDateTime, expiryDateAsDateTime);
			}
		}

		public static X509Certificate2 CreateSampleChildCertificate(this X509Certificate2 rootCertificate, X500DistinguishedName distinguishedName, ZDateTime? issueDate = null, ZDateTime? expiryDate = null)
		{
			var oid_ID_KP_TIMESTAMPING = "1.3.6.1.5.5.7.3.8";
			var issueDateAsDateTime = issueDate.GetValueOrDefault(ZDateTime.Today.AddDays(-1)).ToDateTime();
			var expiryDateAsDateTime = expiryDate.GetValueOrDefault(ZDateTime.Today.AddDays(5)).ToDateTime();

			using (var rng = RandomNumberGenerator.Create())
			using (var dsa = new ECDsaCng())
			{
				return new CertificateRequest(distinguishedName, dsa, HashAlgorithmName.SHA256)
					.Add(certificateRequest => new X509BasicConstraintsExtension(false, false, 0, false))
					.Add(certificateRequest => new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.NonRepudiation, false))
					.Add(certificateRequest => new X509EnhancedKeyUsageExtension(new OidCollection { new Oid(oid_ID_KP_TIMESTAMPING) }, true))
					.Add(certificateRequest => new X509SubjectKeyIdentifierExtension(certificateRequest.PublicKey, false))
					.Create(rootCertificate, issueDateAsDateTime, expiryDateAsDateTime, rng.GetBytes(8));
			}
		}

		static CertificateRequest Add(this CertificateRequest certificateRequest, params Func<CertificateRequest, X509Extension>[] getExtensions)
		{
			foreach (var getExtension in getExtensions)
			{
				certificateRequest.CertificateExtensions.Add(getExtension(certificateRequest));
			}

			return certificateRequest;
		}

		static byte[] GetBytes(this RandomNumberGenerator rng, int length)
		{
			var result = new byte[length];
			rng.GetBytes(result);
			return result;
		}
	}
}
