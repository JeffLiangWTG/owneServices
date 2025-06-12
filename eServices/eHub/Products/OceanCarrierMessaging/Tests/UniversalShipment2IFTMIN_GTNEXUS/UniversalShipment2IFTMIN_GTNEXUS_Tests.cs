using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.UniversalShipment2IFTMIN_GTNEXUS;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests
{
  [TestClass]
  public class UniversalShipment2IFTMIN_GTNEXUS_Tests
  {
    const string filePath = "UniversalShipment2IFTMIN_GTNEXUS.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void UniversalShipment2IFTMIN_GTNEXUS()
    {
      AssertMapping("Test1_input.xml", "Test1_output.xml");
      AssertMapping("Test2_input.xml", "Test2_output.xml");
      AssertMapping("Test3_input.xml", "Test3_output.xml");
    }

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void UniversalShipment2IFTMIN_GTNEXUS_HCWithDotsAndSpaces()
    {
      AssertMapping("Test4_HCWithDotsAndSpaces_input.xml", "Test4_HCWithDotsAndSpaces_output.xml");
    }

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void UniversalShipment2IFTMIN_GTNEXUS_MarksAndNosWithSpaces()
    {
      AssertMapping("Test5_input.xml", "Test5_output.xml");
    }

    void AssertMapping(string inputFile, string expectedOutputFile)
    {
      var input = filePath + inputFile;
      var expectedOutput = filePath + expectedOutputFile;

      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.GTNEXUS.BGM", "@maxlength", "14")).Return("1").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.GTNEXUS.UNH1", "@maxlength", "14")).Return("10");
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "GTNEXUS", "@recipientId", "TESTSENDER__1", "@ST_ID", "GTNMSG", "@value", "C00676795")).Return("").Repeat.Once();
      mockDataModelAccessor.Expect(x => x.GetClientRegistrationCode("TESTSENDER__1", "BNE", "GTNEXUS")).Return("CGWS");
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("GNXID", "GTNEXUS", "TESTSENDER__1", "CGWS"));
      mockDataModelAccessor.Stub(x => x.InsertSubscriptionValue("GTNMSG", "GTNEXUS", "TESTSENDER__1", "GTN0000000001", "C00676795"));
      mockDataModelAccessor.Stub(x => x.InsertSubscriptionValue("GTNMSG", "GTNEXUS", "TESTSENDER__1", "C00676795", "GTN0000000001"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("GTNMSG", "GTNEXUS", "TESTSENDER__1", "36ACCF3C-893F-40CE-BA01-1D8CBE2DF678", "10"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("GTNMSG", "GTNEXUS", "TESTSENDER__1", "GTN0000000001", "1.0.0", "FormVersion")).Repeat.Any();
      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("TESTSENDER__1");
      mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("GTNEXUS_SI");
      mockContextAccessor.Expect(x => x.GetContextProperty("InternalTrackingID", "http://cargowise.com/ehub/tracking/2010/06")).Return("36ACCF3C-893F-40CE-BA01-1D8CBE2DF678");

      mockContextAccessor.Expect(x => x.SetContextProperty("UNB9", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", ""));
      mockContextAccessor.Expect(x => x.SetContextProperty("UNB11", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", ""));
      mockContextAccessor.Expect(x => x.SetContextProperty("EarlyTerminateEdifactUnb", "http://cargowise.com/ehub/processing/2010/06", "true"));

      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", "GTNEXUS_SI", "151521SCAA")).Return("151521SCAC").Repeat.AtLeastOnce();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", "GTNEXUS_SI", "1111111SCAA")).Return("1111111SCAC").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", "GTNEXUS_SI", "")).Return("").Repeat.Any();

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
                { "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
                { "http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor", mockDataModelAccessor },
            };

      MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.Execute<UniversalShipment2IFTMIN_GTNEXUS>(input, expectedOutput);

      mockCodeMapper.VerifyAllExpectations();
      mockContextAccessor.VerifyAllExpectations();
      mockDataModelAccessor.VerifyAllExpectations();
    }
  }
}
