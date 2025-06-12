using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.UniversalShipment2VERMAS_GTNEXUS;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests
{
  [TestClass]
  public class UniversalShipment2VERMAS_GTNEXUS_Tests
  {
    const string filePath = "UniversalShipment2VERMAS_GTNEXUS.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestUniversalShipment2Vermas_GTNEXUS()
    {
      AssertMapping("Test1_input.xml", "Test1_output.xml");
    }

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestUniversalShipment2Vermas_GTNEXUS_WeigthUnitLB()
    {
       AssertMapping("Test2_input.xml", "Test2_output.xml");
    }

    void AssertMapping(string inputFile, string expectedOutputFile)
    {
      var input = filePath + inputFile;
      var expectedOutput = filePath + expectedOutputFile;

      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
      var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.GTNEXUS.BGM", "@maxlength", "14")).Return("1");
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "GTNEXUS", "@recipientId", "TESTSENDER__1", "@ST_ID", "GTNMSG", "@value", "C00678281_MAEU0494852")).Return("");
      mockDataModelAccessor.Expect(x => x.GetClientRegistrationCode("TESTSENDER__1", "BN1", "GTNEXUS")).Return("CGWS");
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("GTNMSG", "GTNEXUS", "TESTSENDER__1", "36ACCF3C-893F-40CE-BA01-1D8CBE2DF678", "10"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("GNXID", "GTNEXUS", "TESTSENDER__1", "CGWS"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("GTNMSG", "GTNEXUS", "TESTSENDER__1", "GTNV000000001", "C00678281_MAEU0494852"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("GTNMSG", "GTNEXUS", "TESTSENDER__1", "C00678281_MAEU0494852", "GTNV000000001"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("GTNMSG", "GTNEXUS", "TESTSENDER__1", "GTNV000000001", "Verified Gross Container Weight", "DocumentName"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("GTNMSG", "GTNEXUS", "TESTSENDER__1", "C00678281", "ORG", "ActionPurpose"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("GTNMSG", "GTNEXUS", "TESTSENDER__1", "C00678281", "", "ShipmentType"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("GTNMSG", "GTNEXUS", "TESTSENDER__1", "GTNV000000001", "ForwardingConsol", "ForwardingType"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("GTNMSG", "GTNEXUS", "TESTSENDER__1", "GTNV000000001", "Container", "SubMessageType"));
      mockContextAccessor.Stub(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("TESTSENDER__1");
      mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("GTNEXUS_VM");
      mockContextAccessor.Expect(x => x.GetContextProperty("InternalTrackingID", "http://cargowise.com/ehub/tracking/2010/06")).Return("36ACCF3C-893F-40CE-BA01-1D8CBE2DF678");
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.GTNEXUS.UNH1", "@maxlength", "14")).Return("10");
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("GTNMSG", "GTNEXUS", "TESTSENDER__1", "10", "C00678281_MAEU0494852"));
      mockContextAccessor.Expect(x => x.SetContextProperty("OverrideEDIHeader", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "true"));
      mockContextAccessor.Expect(x => x.SetContextProperty("UNB5", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "10"));
      mockContextAccessor.Expect(x => x.SetContextProperty("UNH1", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "10"));
      mockCodeMapper.Expect(x => x.GetRecipientCode("GTNEXUS_SI", "GTNEXUS_SI", "Shipping Instruction IFTMIN to GTNEXUS", "ContainerTypeToISOCode", "GTNEXUS Code", "45G0")).Return("");
      mockCodeMapper.Expect(x => x.GetRecipientCode("GTNEXUS_SI", "GTNEXUS_SI", "Shipping Instruction IFTMIN to GTNEXUS", "ContainerTypeToISOCode", "GTNEXUS Code", "25G0")).Return("");
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration",  "ContainerTypeToISOCode" , "Carrier Code",  "25G0")).Return("48T8");
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "ContainerTypeToISOCode", "Carrier Code", "45G0")).Return("48T8");
      mockContextAccessor.Expect(x => x.SetContextProperty("UNB9", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", ""));
      mockContextAccessor.Expect(x => x.SetContextProperty("UNB11", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", ""));
      mockContextAccessor.Expect(x => x.SetContextProperty("EarlyTerminateEdifactUnb", "http://cargowise.com/ehub/processing/2010/06", "true"));

      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", "GTNEXUS_VM", "CHHK")).Return("COLO").Repeat.Any();

      var extensionObjects = new Dictionary<string, object>() {
        { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockCodeMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockDateMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS3", mockContextAccessor },
        { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS4", mockDataModelAccessor },
      };

      var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.ExecuteCompiled<UniversalShipment2VERMAS_GTNEXUS>(input, expectedOutput);

      mockDateMapper.VerifyAllExpectations();
      mockCodeMapper.VerifyAllExpectations();
      mockContextAccessor.VerifyAllExpectations();
    }
  }
}