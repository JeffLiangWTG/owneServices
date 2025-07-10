using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using CargoWise.Application;
using Enterprise.Integration.Licensing;
using Enterprise.ZArchitecture.Core;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;

namespace Enterprise.MasterFiles.Business
{
	public static class EDIClientOutboundCsrGenerator
	{
		#region CovertToPem

		static string ConvertToPem(object obj)
		{
			using (var stringWriter = new StringWriter())
			{
				var pemWriter = new PemWriter(stringWriter);
				pemWriter.WriteObject(obj);
				return stringWriter.ToString();
			}
		}

		static string ConvertCsrToPem(Pkcs10CertificationRequest csr) => ConvertToPem(csr);

		static string ConvertPrivateKeyToPem(AsymmetricCipherKeyPair keyPair) => ConvertToPem(keyPair.Private);

		#endregion

		#region CovertFromPem

		static AsymmetricCipherKeyPair ConvertPrivateKeyFromPem(string pem)
		{
			try
			{
				using (var stringReader = new StringReader(pem))
				{
					var pemReader = new PemReader(stringReader);
					return (AsymmetricCipherKeyPair)pemReader.ReadObject();
				}
			}
			catch
			{
				throw new ArgumentException(Res.GetString("EDIClientOutboundCsrGenerator|ErrorMessages|PrimaryKeyNotValid", "Private Key is not valid."));
			}
		}

		static X509Certificate2 ConvertCertificateFromPem(string pem)
		{
			try
			{
				using (var stringReader = new StringReader(pem))
				{
					var pemReader = new PemReader(stringReader);
					var bcCert = (Org.BouncyCastle.X509.X509Certificate)pemReader.ReadObject();
					return new X509Certificate2(DotNetUtilities.ToX509Certificate(bcCert));
				}
			}
			catch
			{
				throw new ArgumentException(Res.GetString("EDIClientOutboundCsrGenerator|ErrorMessages|CertificateNotValid", "Certificate is not valid."));
			}
		}

		#endregion

		#region GenerateKey

		static AsymmetricCipherKeyPair GeneratePrivateKey(int keySize = 4096)
		{
			var keyGenerationParameters = new KeyGenerationParameters(new SecureRandom(), keySize);
			var keyPairGenerator = new RsaKeyPairGenerator();
			keyPairGenerator.Init(keyGenerationParameters);
			return keyPairGenerator.GenerateKeyPair();
		}

		static X509Name GetCsrSubject(string ediClientName)
		{
			var cw1RegistrationKey = ObjectFactory.Get<IProductRegistration>().Key;
			var subjectAttributes = new Dictionary<DerObjectIdentifier, string>
				{
					{ X509Name.EmailAddress, "support@wisetechglobal.com" },
					{ X509Name.C, "AU" },
					{ X509Name.L, (NoResString)"Sydney" },
					{ X509Name.ST, (NoResString)"New South Wales" },
					{ X509Name.O, (NoResString)"WiseTech Global" },
					{ X509Name.OU, string.Format("{0}/{1}/{2}", cw1RegistrationKey.EnterpriseCode, cw1RegistrationKey.ServerCode, ediClientName) },
					{ X509Name.CN, "wisetechglobal.com" },
				};

			return new X509Name(subjectAttributes.Keys.ToList(), subjectAttributes);
		}

		static string GetCsrSubjectString(string ediClientName)
		{
			return GetCsrSubject(ediClientName).ToString();
		}

		public static (string privateKey, string csrPem) GeneratePrivateKeyAndCsr(string ediClientName)
		{
			var keyPair = GeneratePrivateKey();
			var privateKeyPem = ConvertPrivateKeyToPem(keyPair);
			var x509Name = GetCsrSubject(ediClientName);
			var pkcs10CertificationRequest = new Pkcs10CertificationRequest("SHA256WITHRSA", x509Name, keyPair.Public, null, keyPair.Private);
			var csrPem = ConvertCsrToPem(pkcs10CertificationRequest);

			return (privateKeyPem, csrPem);
		}

		#endregion

		#region Validation

		public static bool IsCertificateMatchingPrivateKey(string certificatePem, string privateKeyPem)
		{
			try
			{
				var certificate = ConvertCertificateFromPem(certificatePem);
				var keyPair = ConvertPrivateKeyFromPem(privateKeyPem);
				var certificatePublicKey = DotNetUtilities.FromX509Certificate(certificate).GetPublicKey();
				return certificatePublicKey.Equals(keyPair.Public);
			}
			catch
			{
				return false;
			}
		}

		public static bool IsCertificateMatchingCsrSubject(string certificatePem, string ediClientName, string csrSubject = "")
		{
			try
			{
				var certificate = ConvertCertificateFromPem(certificatePem);
				var certificateSubject = certificate.Subject.Replace("S=", "ST=");
				if (string.IsNullOrEmpty(csrSubject))
				{
					csrSubject = GetCsrSubjectString(ediClientName);
				}

				var certificateSubjectDict = ConvertSubjectToDictionary(certificateSubject);
				var csrSubjectDict = ConvertSubjectToDictionary(csrSubject);

				var areEqual = certificateSubjectDict.OrderBy(kv => kv.Key).SequenceEqual(csrSubjectDict.OrderBy(kv => kv.Key));
				return areEqual;
			}
			catch
			{
				return false;
			}

			Dictionary<string, string> ConvertSubjectToDictionary(string subject)
			{
				var keyValuePairs = new Dictionary<string, string>();
				var attributes = subject.Split(',');
				foreach (var attribute in attributes)
				{
					var keyValue = attribute.Split('=');
					if (keyValue.Length == 2)
					{
						var key = keyValue[0].Trim();
						var value = keyValue[1].Trim();
						keyValuePairs[key] = value;
					}
				}

				return keyValuePairs;
			}
		}

		public static bool IsCertificateValid(string certificatePem)
		{
			try
			{
				ConvertCertificateFromPem(certificatePem);
				return true;
			}
			catch
			{
				return false;
			}
		}

		#endregion
	}
}
