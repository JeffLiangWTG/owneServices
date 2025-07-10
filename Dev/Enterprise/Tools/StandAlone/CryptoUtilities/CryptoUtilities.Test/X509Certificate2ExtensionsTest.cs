using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using CargoWise.IO;
using Enterprise.CryptoUtilities;
using NUnit.Framework;

namespace Enterprise.CMREdifact.CryptoUtilities.Testing
{
	sealed class X509Certificate2ExtensionsTest : TestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestIsIssuedBy()
		{
			using (var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly))
			{
				string certificateFileName = resourceRetriever.SaveResourceToFile("generated.out.intermediate-k1-730.cer");
				using (var cert = new X509Certificate2(Path.Combine(certificateFileName)))
				{
					AssertExceptionThrown<NullReferenceException>(() =>
						X509Certificate2Extensions.IsIssuedBy(null, null));
					AssertExceptionThrown<NullReferenceException>(() =>
						X509Certificate2Extensions.IsIssuedBy(null, cert));
					AssertExceptionThrown<NullReferenceException>(() =>
						X509Certificate2Extensions.IsIssuedBy(cert, null));
				}

				certificateFileName = resourceRetriever.SaveResourceToFile("Conf.cer");
				using (var cert = new X509Certificate2(Path.Combine(certificateFileName)))
				{
					certificateFileName = resourceRetriever.SaveResourceToFile("trustedcert.cer");
					using (var trustedCert = new X509Certificate2(certificateFileName))
					{
						AssertEquals("CertificateTrusted", false, cert.IsIssuedBy(trustedCert));
					}

					certificateFileName = resourceRetriever.SaveResourceToFile("acme_ca.crt");
					using (var trustedCert2 = new X509Certificate2(certificateFileName))
					{
						AssertEquals("CertificateTrusted", true, cert.IsIssuedBy(trustedCert2));
					}
				}

				certificateFileName = resourceRetriever.SaveResourceToFile("generated.out.sample-org.cer");
				using (var cert = new X509Certificate2(certificateFileName))
				{
					// Intermediate CA
					certificateFileName = resourceRetriever.SaveResourceToFile("generated.out.intermediate-k1-730.cer");
					using (var issuer = new X509Certificate2(certificateFileName))
					{
						AssertEquals(true, cert.IsIssuedBy(issuer));
					}

					// same Intermediate CA, but different certificate with same public key
					certificateFileName = resourceRetriever.SaveResourceToFile("generated.out.intermediate-k1-365.cer");
					using (var issuer = new X509Certificate2(certificateFileName))
					{
						AssertEquals(true, cert.IsIssuedBy(issuer));
					}

					// same Intermediate CA, different certificate with different public key
					certificateFileName = resourceRetriever.SaveResourceToFile("generated.out.intermediate-k2-900.cer");
					using (var issuer = new X509Certificate2(certificateFileName))
					{
						AssertEquals(false, cert.IsIssuedBy(issuer));
					}

					// Root CA
					certificateFileName = resourceRetriever.SaveResourceToFile("generated.out.root-ca.cer");
					using (var issuer = new X509Certificate2(certificateFileName))
					{
						AssertEquals(false, cert.IsIssuedBy(issuer));
					}
				}
			}
		}
	}
}
