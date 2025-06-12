using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.UniversalShipment2IFTMBF_GTNEXUS;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;


namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests
{
  [TestClass]
  public class UniversalShipment2IFTMBF_GTNEXUS_Tests
  {
    const string filePath = "UniversalShipment2IFTMBF_GTNEXUS.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void UniversalShipment2IFTMBF_GTNEXUS_NEW()
    {
      AssertMapping("Test1_input.xml", "Test1_output.xml", "NEW", "BNE", "151521SCAC");
    }

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void UniversalShipment2IFTMBF_GTNEXUS_WTH()
    {
      AssertMapping("Test2_input.xml", "Test2_output.xml", "WTH", "BNE", "151521SCAC");
    }

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void UniversalShipment2IFTMBF_GTNEXUS_APP()
    {
      AssertMapping("Test3_input.xml", "Test3_output.xml", "APP", "SYD", "SCAC8888888");
    }

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void UniversalShipment2IFTMBF_GTNEXUS_HCWithDotsAndSpaces()
    {
      AssertMapping("Test4_HCWithDotsAndSpaces_input.xml", "Test4_HCWithDotsAndSpaces_output.xml", "NEW", "BNE", "151521SCAC");
    }

    void AssertMapping(string inputFile, string expectedOutputFile, string actionPurpose, string eventBranch, string scac)
    {
      var input = filePath + inputFile;
      var expectedOutput = filePath + expectedOutputFile;

      var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.GTNEXUS.BGM", "@maxlength", "14")).Return("1").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.GTNEXUS.UNH1", "@maxlength", "14")).Return("10");

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "GTNEXUS", "@recipientId", "TESTSENDER__1", "@ST_ID", "GTNMSG", "@value", "C00676795")).Return("");
      mockDataModelAccessor.Expect(x => x.GetClientRegistrationCode("TESTSENDER__1", eventBranch, "GTNEXUS")).Return("CGWS");
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("GTNMSG", "GTNEXUS", "TESTSENDER__1", "C00676795", "GTN0000000001"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("GTNMSG", "GTNEXUS", "TESTSENDER__1", "GTN0000000001", "C00676795"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("GNXID", "GTNEXUS", "TESTSENDER__1", "CGWS"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("GTNMSG", "GTNEXUS", "TESTSENDER__1", "36ACCF3C-893F-40CE-BA01-1D8CBE2DF678", "10"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("GTNBRS", "GTNEXUS", "TESTSENDER__1", "C00676795", actionPurpose));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("GTNMSG", "GTNEXUS", "TESTSENDER__1", "GTN0000000001", "1.0.0", "FormVersion")).Repeat.Any();
      mockContextAccessor.Stub(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("TESTSENDER__1");
      mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("GTNEXUS_BK").Repeat.AtLeastOnce();
      mockContextAccessor.Expect(x => x.GetContextProperty("InternalTrackingID", "http://cargowise.com/ehub/tracking/2010/06")).Return("36ACCF3C-893F-40CE-BA01-1D8CBE2DF678");

      mockContextAccessor.Expect(x => x.SetContextProperty("UNB9", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", ""));
      mockContextAccessor.Expect(x => x.SetContextProperty("UNB11", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", ""));
      mockContextAccessor.Expect(x => x.SetContextProperty("EarlyTerminateEdifactUnb", "http://cargowise.com/ehub/processing/2010/06", "true"));

      switch (scac)
      {
        case "151521SCAC":
          mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", "GTNEXUS_BK", "151521SCAA")).Return("151521SCAC").Repeat.Times(2);
          mockCodeMapper.Stub(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", "GTNEXUS_BK", "")).Return("");
          break;
        case "SCAC8888888":
          mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", "GTNEXUS_BK", "SCAC888888A")).Return("SCAC8888888").Repeat.Times(2);
          break;
      }

      // handle 3 cases for package code:
      // package type
      mockCodeMapper.Expect(x => x.GetRecipientCode("GTNEXUS_SI", "GTNEXUS_SI", "Shipping Instruction IFTMIN to GTNEXUS", "Package Type", "GTNEXUS Code", "P_I")).Return("P_O").Repeat.AtLeastOnce();
      // ISO
      mockCodeMapper.Expect(x => x.GetRecipientCode("GTNEXUS_SI", "GTNEXUS_SI", "Shipping Instruction IFTMIN to GTNEXUS", "Package Type", "GTNEXUS Code", "I_I")).Return("");
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Package Type ISO", "Package Type", "I_I")).Return("I_O");
      // default
      mockCodeMapper.Expect(x => x.GetRecipientCode("GTNEXUS_SI", "GTNEXUS_SI", "Shipping Instruction IFTMIN to GTNEXUS", "Package Type", "GTNEXUS Code", "D_I")).Return("");
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Package Type ISO", "Package Type", "D_I")).Return("");
      mockCodeMapper.Expect(x => x.GetRecipientCodeUnkeyed("GTNEXUS_SI", "GTNEXUS_SI", "Shipping Instruction IFTMIN to GTNEXUS", "Defaults", "Package Type")).Return("D_O");

      var extensionObjects = new Dictionary<string, object>() {
                { "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
                { "http://schemas.microsoft.com/BizTalk/2003/DateMapper", mockDateMapper },
                { "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
                { "http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor", mockDataModelAccessor },
            };
      MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.Execute<UniversalShipment2IFTMBF_GTNEXUS>(input, expectedOutput);

      mockDateMapper.VerifyAllExpectations();
      mockCodeMapper.VerifyAllExpectations();
      mockContextAccessor.VerifyAllExpectations();
    }
  }
}
