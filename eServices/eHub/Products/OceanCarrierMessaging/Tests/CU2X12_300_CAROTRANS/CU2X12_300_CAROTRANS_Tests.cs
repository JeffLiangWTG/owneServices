using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.CU2X12_300_CAROTRANS;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using System.Collections.Generic;
using System.Reflection;


namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests
{
  [TestClass]
  public class CarrierUniversal2X12_300_Carotrans_Tests
  {
    const string filePath = "CU2X12_300_CAROTRANS.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void Test_CarrierUniversal2X12_300_Carotrans()
    {
      AssertMapping("Test1_input.xml", "Test1_output.xml");
      AssertMapping("Test2_input.xml", "Test2_output.xml");
      AssertMapping("Test3_input.xml", "Test3_output.xml");
      AssertMapping("Test4_input.xml", "Test4_output.xml");
      AssertMapping("Test5_input.xml", "Test5_output.xml");
      AssertMapping("Test6_input.xml", "Test6_output.xml");
      AssertMapping("Test7_input.xml", "Test7_output.xml");
      AssertMapping("Test8_input_CoLoad.xml", "Test8_output_CoLoad.xml", "CLD");
    }

    void AssertMapping(string inputFile, string expectedOutputFile, string shipmentType = "")
    {
      var input = filePath + inputFile;
      var expectedOutput = filePath + expectedOutputFile;

      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
      var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
      var mockOCMHelper = MockRepository.GenerateStrictMock<OCMHelper>();
      var mockSubscriptionHelper = MockRepository.GenerateStrictMock<SubscriptionHelper>();

      mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("CAROTRANS_BK1");
      mockContextAccessor.Expect(x => x.SetContextProperty("OverrideEDIHeader", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "true"));
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationPartyName", "http://schemas.microsoft.com/Edi/PropertySchema", "Override"));
      mockContextAccessor.Expect(x => x.GetContextProperty("InternalTrackingID", "http://cargowise.com/ehub/tracking/2010/06")).Return("36ACCF3C-893F-40CE-BA01-1D8CBE2DF678");

      mockContextAccessor.Expect(x => x.SetContextProperty("ISA01", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "00"));
      mockContextAccessor.Expect(x => x.SetContextProperty("ISA03", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "00"));
      mockContextAccessor.Expect(x => x.SetContextProperty("ISA05", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "ZZ"));
      mockContextAccessor.Expect(x => x.SetContextProperty("ISA06", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "CARGOWISE"));
      mockContextAccessor.Expect(x => x.SetContextProperty("ISA07", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "ZZ"));
      mockContextAccessor.Expect(x => x.SetContextProperty("ISA08", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "CAROTRANS"));
      mockContextAccessor.Expect(x => x.SetContextProperty("ISA09", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "170302"));
      mockContextAccessor.Expect(x => x.SetContextProperty("ISA10", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "0951"));
      mockContextAccessor.Expect(x => x.SetContextProperty("ISA11", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "U"));
      mockContextAccessor.Expect(x => x.SetContextProperty("ISA12", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "00401"));
      mockContextAccessor.Expect(x => x.SetContextProperty("ISA13", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "57321"));
      mockContextAccessor.Expect(x => x.SetContextProperty("ISA14", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "1"));
      mockContextAccessor.Expect(x => x.SetContextProperty("ISA15", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "P"));
      mockContextAccessor.Expect(x => x.SetContextProperty("ISA16", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", ":"));

      mockContextAccessor.Expect(x => x.SetContextProperty("GS01", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "RO"));
      mockContextAccessor.Expect(x => x.SetContextProperty("GS02", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "CARGOWISE"));
      mockContextAccessor.Expect(x => x.SetContextProperty("GS03", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "CAROTRANS"));
      mockContextAccessor.Expect(x => x.SetContextProperty("GS04", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "20170302"));
      mockContextAccessor.Expect(x => x.SetContextProperty("GS05", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "0951"));
      mockContextAccessor.Expect(x => x.SetContextProperty("GS06", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "57321"));
      mockContextAccessor.Expect(x => x.SetContextProperty("GS07", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "X"));
      mockContextAccessor.Expect(x => x.SetContextProperty("GS08", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "004010"));

      mockContextAccessor.Expect(x => x.SetContextProperty("ST02", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "CTR057321"));

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Carotrans.ST", "@maxlength", "14")).Return("57321");
      mockCodeMapper.Expect(x => x.GetRecipientCode("CAROTRANS_BK1", "CAROTRANS_BK1", "Booking Request X12_300 to CAROTRANS (v1)", "Package Type", "CAROTRANS Code", "VE")).Return("BAG").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "CAROTRANS", "@recipientId", "TESTSENDER__1", "@ST_ID", "CTRMSG", "@value", "C00679211", "@referenceType", "JobNumber")).Return("");
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "CAROTRANS", "@recipientId", "TESTSENDER__1", "@ST_ID", "CTRMSG", "@value", "C00679211")).Return("");
      mockDataModelAccessor.Expect(x => x.GetClientRegistrationCode("TESTSENDER__1", "BN1", "CAROTRANS")).Return("XXXUSAOR1");
      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("TESTSENDER__1");
      mockContextAccessor.Expect(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "TESTSE_300_57321"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CTRID", "CAROTRANS", "TESTSENDER__1", "XXXUSAOR1"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CTRMSG", "CAROTRANS", "TESTSENDER__1", "57321", "C00679211"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CTRMSG", "CAROTRANS", "TESTSENDER__1", "CTR057321", "C00679211", "JobNumber"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CTRMSG", "CAROTRANS", "TESTSENDER__1", "C00679211", "CTR057321", "JobNumber"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CTRMSG", "CAROTRANS", "TESTSENDER__1", "C00679211", "AMD", "ActionPurpose"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CTRMSG", "CAROTRANS", "TESTSENDER__1", "36ACCF3C-893F-40CE-BA01-1D8CBE2DF678", "57321"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CTRMSG", "CAROTRANS", "TESTSENDER__1", "CTR057321", shipmentType, "ShipmentType"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CTRMSG", "CAROTRANS", "TESTSENDER__1", "CTR057321", "1.0.0", "FormVersion")).Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", "CAROTRANS_BK1", "CDNU")).Return("CDNU");

      mockOCMHelper.Expect(x => x.IsCoLoad("")).Return("FALSE").Repeat.Any();
      mockOCMHelper.Expect(x => x.IsCoLoad("CLD")).Return("TRUE").Repeat.Any();
      mockOCMHelper.Expect(x => x.GetServiceProvider("CAROTRANS_BK1")).Return("CAROTRANS").Repeat.Any();

      mockSubscriptionHelper.Expect(x => x.InsertBoleroSubscription("TESTSENDER__1", "XXXUSAOR1", "CAROTRANS", "CTR057321", "C00679211")).Repeat.Any();

      var extensionObjects = new Dictionary<string, object>() {
        { "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/DateMapper", mockDateMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
        { "http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor", mockDataModelAccessor },
        { "http://schemas.microsoft.com/BizTalk/2003/OCMHelper", mockOCMHelper },
        { "http://schemas.microsoft.com/BizTalk/2003/SubscriptionHelper", mockSubscriptionHelper }
      };

      MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.ExecuteCompiled<CU2X12_300_CAROTRANS>(input, expectedOutput);

      mockDateMapper.VerifyAllExpectations();
      mockCodeMapper.VerifyAllExpectations();
      mockContextAccessor.VerifyAllExpectations();
      mockDataModelAccessor.VerifyAllExpectations();
    }
  }
}
