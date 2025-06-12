using CargoWise.eHub.Products.ForwardingPortMessaging.DE.Schemas.ALPO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.XLANGs.BaseTypes;

namespace CargoWise.eHub.Products.ForwardingPortMessaging.DE.Tests
{
  [TestClass]
  public class SchemaTests
  {
    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestALPO_Schema()
    {
      AssertSchema<ALPO_OrderIn_extern_V1_37>("Test1_AUFTRAG.xml");
    }

    void AssertSchema<T>(string expectedOutput) where T : SchemaBase, new()
    {
      var filePath = "Schemas.ALPO.TestFiles.";
      var report = XMLValidator.Validate<T>(filePath + expectedOutput);
      Assert.AreEqual(string.Empty, report, typeof(T).Name + " is not valid: \r\n " + report);
    }
  }
}