using System.IO;
using System.Security.Cryptography.X509Certificates;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.UY.Manifest.Business.Testing
{
	sealed class UYSignatureTest : TestCaseWithFactory
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestUYSignature()
		{
			var daeMessage = Path.Combine(BaseSourcePath, DAETestingConstants.DAEManifestWithOutSign);

			var certificate = new X509Certificate2(X509Certificate2TestHelper.ValidCertificate, X509Certificate2TestHelper.ValidPassword);
			var signedMessage = UYSignature.Sign(File.ReadAllText(daeMessage), certificate);

			AssertContains(@"<Signature xmlns=""http://www.w3.org/2000/09/xmldsig#"">", signedMessage);
			AssertContains(@"<CanonicalizationMethod Algorithm=""http://www.w3.org/TR/2001/REC-xml-c14n-20010315"" />", signedMessage);
			AssertContains(@"<SignatureMethod Algorithm=""http://www.w3.org/2000/09/xmldsig#rsa-sha1"" />", signedMessage);
			AssertContains(@"<Transforms><Transform Algorithm=""http://www.w3.org/2000/09/xmldsig#enveloped-signature"" /></Transforms>", signedMessage);
		}
	}
}
