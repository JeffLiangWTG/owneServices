using CargoWise.eHub.Clients.EDI.EDIFACT.Schemas3.D00B;
using CargoWise.eHub.Products.GCT.Schemas;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.XLANGs.BaseTypes;

namespace CargoWise.eHub.Products.GCT.Tests
{
  [TestClass]
  public class SchemaTests
  {
    const string filePath = "Schemas.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestShipmentStatus_VANGUARD()
    {
      AssertSchema<ShipmentStatus_VANGUARD>(filePath + "Test1_ShipmentStatus_VANGUARD.xml");
      AssertSchema<EFACT_D00B_IFTSTA>(filePath + "Test2_IFTSTA2UInterchange_D00B.xml");
    }

    void AssertSchema<T>(string expectedOutput) where T : SchemaBase, new()
    {
      var report = XmlValidator.Validate<T>(expectedOutput);
      Assert.AreEqual(string.Empty, report, typeof(T).Name + " is not valid: \r\n " + report);
    }
  }
}
