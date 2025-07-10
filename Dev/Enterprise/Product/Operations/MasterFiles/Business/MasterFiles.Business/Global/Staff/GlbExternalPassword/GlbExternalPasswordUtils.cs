using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public static class GlbExternalPasswordUtils
	{
		public static (ZString key, ZString certificate) ExportRSAKeyAndCertificateStringAsPEM(this GlbExternalPassword glbExternalPassword)
		{
			var keyPEM = ZString.Empty;
			var certificatePEM = ZString.Empty;

			var certificateBytes = (byte[])glbExternalPassword.GP_Certificate;
			var certificatePassword = glbExternalPassword.CurrentDecryptedCertificatePassphrase;
			if (certificateBytes != null && certificateBytes.Length != 0 && !string.IsNullOrEmpty(certificatePassword))
			{
				using (var certificate = new X509Certificate2(certificateBytes, certificatePassword, X509KeyStorageFlags.Exportable))
				{
					if (certificate.HasPrivateKey)
					{
						keyPEM = certificate.ExportRSAPrivateKeyString();
					}
					else
					{
						throw new CryptographicException("Certificate does not contain a private key.");
					}
					certificatePEM = certificate.ExportCertificatePEMString();
				}
			}

			return (keyPEM, certificatePEM);
		}

		static ZString ExportRSAPrivateKeyString(this X509Certificate2 certificate)
		{
			using var writer = new StringWriter();
			using var pk_instance = certificate.GetRSAPrivateKey() ?? throw new CryptographicException("Private key is either null or not in expected RSA form.");
			var pemWriter = new Org.BouncyCastle.OpenSsl.PemWriter(writer);
			var keyPair = Org.BouncyCastle.Security.DotNetUtilities.GetRsaKeyPair(pk_instance);
			pemWriter.WriteObject(keyPair);
			return writer.ToString();
		}

		static ZString ExportCertificatePEMString(this X509Certificate2 certificate)
		{
			using var writer = new StringWriter();
			var pemWriter = new Org.BouncyCastle.OpenSsl.PemWriter(writer);
			var bcCert = Org.BouncyCastle.Security.DotNetUtilities.FromX509Certificate(certificate);
			pemWriter.WriteObject(bcCert);
			return writer.ToString();
		}
	}
}
