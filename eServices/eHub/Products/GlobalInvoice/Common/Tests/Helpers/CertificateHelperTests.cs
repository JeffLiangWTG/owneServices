using System.Text;
using CargoWise.eHub.Shared.Crypto;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Products.GlobalInvoice.Common.Tests
{
  [TestClass]
  public class CertificateHelperTests
  {
    [TestMethod]
    public void TestSignMessage()
    {
      var message = TestHelper.GetResourceText("TestFiles.Unsigned.xml");
      var messageBinary = Encoding.Default.GetBytes(message);
      var contentData = TestHelper.GetResourceData("TestFiles.SDI-13149600150.pfx");
      var certPassword = "Aco00Bsu";
      var base64Encoded = CmsHelpers.ComputeSignatureBase64(messageBinary, contentData, certPassword, 0, false);
      var expectedBase64Encoded = TestHelper.GetResourceText("TestFiles.ExpectedBase64Encrypted.txt");
      Assert.AreEqual(expectedBase64Encoded, base64Encoded);
    }
  }
}
