using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.CarrierUniversal2BLI_WWA;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Helper;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests
{
  [TestClass]
  public class CarrierUniversal2BLI_WWA_Tests
  {
    const string filePath = "CarrierUniversal2BLI_WWA.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestCarrierUniversal2BLI_WWA()
    {
      AssertMapping("Test1_input.xml", "Test1_output.xml");
      AssertMapping("Test2_input.xml", "Test2_output.xml", "CLD", payableElseWhere: "A");
      AssertMapping("Test3_input.xml", "Test3_output.xml");
      AssertMapping("Test4_input_group.xml", "Test4_output_group.xml", "AGT");
    }

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestCarrierUniversal2BLI_WWA_ICS2()
    {
      AssertMapping("ICS2_Test1_input_Carrier.xml", "ICS2_Test1_output_Carrier.xml", "AGT");
      AssertMapping("ICS2_Test2_input_Declarant.xml", "ICS2_Test2_output_Declarant.xml", "AGT");
    }

    void AssertMapping(string inputFile, string expectedOutputFile, string shipmentType = "", string payableElseWhere = "")
    {
      var input = filePath + inputFile;
      var expectedOutput = filePath + expectedOutputFile;
      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
      var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
      var mockOCMHelper = MockRepository.GenerateStrictMock<OCMHelper>();
      var mockSubscriptionHelper = MockRepository.GenerateStrictMock<SubscriptionHelper>();

      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("edi_test_prod");
      mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("WWA_SI1");
      mockContextAccessor.Expect(x => x.GetContextProperty("InternalTrackingID", "http://cargowise.com/ehub/tracking/2010/06")).Return("36ACCF3C-893F-40CE-BA01-1D8CBE2DF678");
      mockDateMapper.Expect(x => x.CurrentDateTime("yyyyMMdd_mmssms")).Return("20180101_115059_100");


      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "WWA", "@recipientId", "edi_test_prod", "@ST_ID", "WWAMSG", "@value", "CEIS0000680261", "@referenceType", "JobNumber")).Return("C03078216").Repeat.Any();

      mockContextAccessor.Expect(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "WWA_ESI_Cargowise_20180101_115059_100_11"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("WWAMSG", "WWA", "edi_test_prod", "11", "CEIS0000680261"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("WWAMSG", "WWA", "edi_test_prod", "36ACCF3C-893F-40CE-BA01-1D8CBE2DF678", "11"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("WWAMSG", "WWA", "edi_test_prod", "C03078216", "", "ActionPurpose")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("WWAMSG", "WWA", "edi_test_prod", "C03078216", "APP", "ActionPurpose")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("WWAMSG", "WWA", "edi_test_prod", "C03078216", "ORG", "ActionPurpose")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("WWAMSG", "WWA", "edi_test_prod", "C03078216", "AMD", "ActionPurpose")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("WWAMSG", "WWA", "edi_test_prod", "C03078216", shipmentType, "ShipmentType")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.GetClientRegistrationCode("edi_test_prod", "BN1", "WWA")).Return("CGWS");

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.WWA.SI", "@maxlength", "14")).Return("11");
      mockCodeMapper.Expect(x => x.GetRecipientCode("WWA", "WWA", "WWA Provider Configuration", "ContainerTypeToISOCode", "WWA Code", "42G0")).Return("42G0").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Reference Label Type", "Use Long Reference", "WWA_SI1")).Return("false").Repeat.Any();

      //// handle 3 cases for package code:
      // Password
      mockCodeMapper.Stub(x => x.GetRecipientCodeUnkeyed("WWA", "WWA", "WWA Provider Configuration", "Password", "WWA Password")).Return("Aw67C5jL2RRCb2sLYzQg");
      mockCodeMapper.Stub(x => x.GetRecipientCodeUnkeyed("WWA", "WWA", "WWA Provider Configuration", "Receiver ID", "Receiver")).Return("wwalliance");
      mockCodeMapper.Stub(x => x.GetRecipientCodeUnkeyed("WWA", "WWA", "WWA Provider Configuration", "Sender ID", "Sender")).Return("edi_cargowise_prod");

      //// package type
      mockCodeMapper.Stub(x => x.GetRecipientCode("WWA", "WWA", "WWA Provider Configuration", "Package Type", "WWA Code", "PLT")).Return("PX");
      mockCodeMapper.Stub(x => x.GetRecipientCode("WWA", "WWA", "WWA Provider Configuration", "Package Type", "WWA Code", "PKG")).Return("PK");
      // ISO
      mockCodeMapper.Stub(x => x.GetRecipientCode("WWA", "WWA", "WWA Provider Configuration", "Package Type", "WWA Code", "I_I")).Return("");
      mockCodeMapper.Stub(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Package Type ISO", "Package Type", "I_I")).Return("I_O");
      // default
      mockCodeMapper.Stub(x => x.GetRecipientCode("WWA", "WWA", "WWA Provider Configuration", "Package Type", "WWA Code", "D_I")).Return("");
      mockCodeMapper.Stub(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Package Type ISO", "Package Type", "D_I")).Return("");
      mockCodeMapper.Stub(x => x.GetRecipientCodeUnkeyed("WWA", "WWA", "WWA Provider Configuration", "Defaults", "Package Type")).Return("D_O");

      mockOCMHelper.Stub(x => x.IsCoLoad("")).Return("FALSE");
      mockOCMHelper.Stub(x => x.IsCoLoad("CLD")).Return("TRUE");
      mockOCMHelper.Stub(x => x.IsCoLoad("AGT")).Return("FALSE");

      mockOCMHelper.Expect(x => x.GetServiceProvider("WWA_SI1")).Return("WWA").Repeat.Any();
      mockOCMHelper.Expect(x => x.GetPayableElseWhereOutputCode("WWA")).Return(payableElseWhere).Repeat.Any();

      mockSubscriptionHelper.Expect(x => x.InsertBoleroSubscription("edi_test_prod", "CGWS", "WWA", "C03078216", "CEIS0000680261")).Repeat.Any();

      var extensionObjects = new Dictionary<string, object>() {
        { "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/DateMapper", mockDateMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
        { "http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor", mockDataModelAccessor },
        { "http://schemas.microsoft.com/BizTalk/2003/OCMHelper", mockOCMHelper },
        { "http://schemas.microsoft.com/BizTalk/2003/SubscriptionHelper", mockSubscriptionHelper }
      };

      MapTester mapTester1 = new MapTester(Assembly.GetExecutingAssembly());
      MapTester mapTester2 = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester2.Execute<CarrierUniversal2BLI_WWA>(input, expectedOutput);

      mockDateMapper.VerifyAllExpectations();
      mockCodeMapper.VerifyAllExpectations();
      mockContextAccessor.VerifyAllExpectations();
      mockDataModelAccessor.VerifyAllExpectations();
    }
  }
}
