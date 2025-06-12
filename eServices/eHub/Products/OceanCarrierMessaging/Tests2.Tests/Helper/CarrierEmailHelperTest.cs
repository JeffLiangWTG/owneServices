using Microsoft.VisualStudio.TestTools.UnitTesting;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Helper;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests2.Tests.CarrierEmailHelperTest
{
  [TestClass]
  public class CarrierEmailHelperTest
  {
    CarrierEmailHelper Helper => helper ?? (helper = new CarrierEmailHelper());
    CarrierEmailHelper helper;

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestGenerateToken()
    {
      var secretKey = "12345678901234567890123456789012";
      var token = Helper.GenerateToken(secretKey, "SI", "REF123");
      var decryptedToken = Helper.DecryptString(secretKey, token);
      var decryptedTokenJson = JObject.Parse(decryptedToken);

      Assert.IsTrue(Regex.IsMatch(token, @"^[A-Za-z0-9+/=]*$"), "is base64 string");
      Assert.AreEqual(decryptedTokenJson.GetValue("MessageType")?.ToString(), "SI");
      Assert.AreEqual(decryptedTokenJson.GetValue("MessageReference")?.ToString(), "REF123");
    }
  }
}