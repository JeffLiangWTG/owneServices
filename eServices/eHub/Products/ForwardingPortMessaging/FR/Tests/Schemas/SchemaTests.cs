using CargoWise.eHub.Products.ForwardingPortMessaging.FR.Schemas.APPLUS;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.XLANGs.BaseTypes;

namespace CargoWise.eHub.Products.ForwardingPortMessaging.FR.Tests
{
  [TestClass]
  public class SchemaTests
  {
    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestAPPLUS_Schema()
    {
      AssertSchema<acq>("ACQ_acknowledgement.xml");
      AssertSchema<amq>("AMQ_request.xml");
      AssertSchema<caed>("CAED.xml");
      //AssertSchema<dos>("DOS_ack_ko.xml");
      //AssertSchema<dos>("DOS_ack_ok.xml");
      //AssertSchema<dos>("DOS_request.xml");
      //AssertSchema<dos>("DOS_request_BL.xml");
	  //AssertSchema<dos>("DOS_request_BOOKING.xml");
	  AssertSchema<doa>("DOA.xml");
      AssertSchema<cdm>("CDM_request.xml");
      AssertSchema<cresa>("CRESA_request.xml");
      AssertSchema<lde>("LDE_request.xml");
      AssertSchema<lpd>("LPD_request.xml");
      AssertSchema<dti>("DTI_DTE_complet.xml");
      AssertSchema<dte>("DTI_DTE_complet.xml");
    }

    void AssertSchema<T>(string expectedOutput) where T : SchemaBase, new()
    {
      var filePath = "Schemas.APPLUS.TestFiles.";
      var report = XMLValidator.Validate<T>(filePath + expectedOutput);
      Assert.AreEqual(string.Empty, report, typeof(T).Name + " is not valid: \r\n " + report);
    }
  }
}
