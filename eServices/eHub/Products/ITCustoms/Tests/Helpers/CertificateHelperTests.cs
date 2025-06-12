using System.Text;
using CargoWise.eHub.Shared.Crypto;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Products.ITCustoms.Tests.Helpers
{
	[TestClass]
	public class CertificateHelperTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestSignMessage()
		{
			var message = TestHelper.GetResourceText("TestFiles.7UDG0830.RA7");
			var messageBinary = Encoding.Default.GetBytes(message);
			var contentData = TestHelper.GetResourceData("TestFiles.keystore.p12");
			var certPassword = "SY4F17N1";
			var base64Enconded = CmsHelpers.ComputeSignatureBase64(messageBinary, contentData, certPassword, 0, false);
			var expectedBase64Encoded = TestHelper.GetResourceText("TestFiles.ExpectedBase64Encrypted.txt");
			Assert.AreEqual(expectedBase64Encoded, base64Enconded);
		}
	}
}
