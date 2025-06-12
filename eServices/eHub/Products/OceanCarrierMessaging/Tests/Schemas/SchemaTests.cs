using CargoWise.eHub.Products.OceanCarrierMessaging.Schemas;
using CargoWise.eHub.Products.OceanCarrierMessaging.Schemas.OCM;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.XLANGs.BaseTypes;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests.Schemas
{
  [TestClass]
  public class SchemaTests
  {
    const string filePath = "Schemas.TestFiles.";
    const string ocmFilePath = "Schemas.TestFiles.OCM.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void VERMAS_D16A()
    {
      AssertSchema<VERMAS_D16A>(filePath + "VERMAS.xml");
    }

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void VERMAS_D16A_PORTRIX_VM()
    {
      AssertSchema<VERMAS_D16A>(filePath + "PORTRIX_VM_VERMAS_1.xml");
    }

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void OCM_BookingRequest()
    { 
      AssertSchema<BookingRequest_v1>(ocmFilePath + "BookingRequest.xml");
    }

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void OCM_ShippingInstruction()
    {
      AssertSchema<ShippingInstruction_v1>(ocmFilePath + "ShippingInstruction.xml");
    }
    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void OCM_Acknowledgement()
    {
      AssertSchema<Acknowledgement_v1>(ocmFilePath + "Acknowledgement.xml");
    }

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void OCM_BookingConfirmation()
    {
      AssertSchema<BookingConfirmation_v1>(ocmFilePath + "BookingConfirmation.xml");
    }

    void AssertSchema<T>(string expectedOutput) where T : SchemaBase, new()
    {
      var report = XmlValidator.Validate<T>(expectedOutput);
      Assert.AreEqual(string.Empty, report, typeof(T).Name + " is not valid: \r\n " + report);
    }
  }
}
