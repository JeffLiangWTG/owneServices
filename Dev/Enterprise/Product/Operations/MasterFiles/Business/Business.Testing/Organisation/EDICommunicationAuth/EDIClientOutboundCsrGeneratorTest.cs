using System.IO;
using CargoWise.Application;
using Enterprise.Integration.Licensing;
using NUnit.Framework;
using Org.BouncyCastle.OpenSsl;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class EDIClientOutboundCsrGeneratorTest : TestCase
	{
		public void TestGenerateCsr()
		{
			var ediClientName = "EDIClientName";
			(var privateKeyPem, var csrPem) = EDIClientOutboundCsrGenerator.GeneratePrivateKeyAndCsr(ediClientName);
			Assert(privateKeyPem.StartsWith("-----BEGIN RSA PRIVATE KEY-----"));
			Assert(csrPem.StartsWith("-----BEGIN CERTIFICATE REQUEST-----"));
			using (TextReader textReader = new StringReader(csrPem))
			{
				var reader = new PemReader(textReader);
				var req = reader.ReadObject() as Org.BouncyCastle.Pkcs.Pkcs10CertificationRequest;
				var info = req.GetCertificationRequestInfo();

				var cw1RgistrationKey = ObjectFactory.Get<IProductRegistration>().Key;

				AssertEquals(string.Format("E=support@wisetechglobal.com,C=AU,L=Sydney,ST=New South Wales,O=WiseTech Global,OU={0}/{1}/{2},CN=wisetechglobal.com", cw1RgistrationKey.EnterpriseCode, cw1RgistrationKey.ServerCode, ediClientName), info.Subject.ToString());
			}
			Assert(privateKeyPem.EndsWith(@"-----END RSA PRIVATE KEY-----
"));
			Assert(csrPem.EndsWith(@"-----END CERTIFICATE REQUEST-----
"));
		}
	}
}
