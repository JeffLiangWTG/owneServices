using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.CarrierUniversal2BR_WWA;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Helper;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests
{
  [TestClass]
  [System.Runtime.InteropServices.Guid("497FF58E-39A6-436C-A452-99DC581C4D0B")]
  public class CarrierUniversal2BR_WWA_Test
  {
    const string filePath = "CarrierUniversal2BR_WWA.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestCarrierUniversal2BR_WWA()
    {
      AssertMapping("Test1_input.xml", "Test1_output.xml");
      AssertMapping("Test2_input.xml", "Test2_output.xml", payableElseWhere: "A");
      AssertMapping("Test3_input.xml", "Test3_output.xml");
      AssertMapping("Test4_input_CoLoad.xml", "Test4_output_CoLoad.xml", "CLD");
      AssertMapping("Test5_input.xml", "Test5_output.xml");
      AssertMapping("Test6_input.xml", "Test6_output.xml");
      AssertMapping("Test7_input.xml", "Test7_output.xml");
      AssertMapping("Test8_input_group.xml", "Test8_output_group.xml", "AGT");
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

      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("edi_test_prod").Repeat.Any();
      mockContextAccessor.Expect(x => x.GetContextProperty("InternalTrackingID", "http://cargowise.com/ehub/tracking/2010/06")).Return("36ACCF3C-893F-40CE-BA01-1D8CBE2DF678").Repeat.Any();
      mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("WWA_BK1").Repeat.Any();
      mockDateMapper.Expect(x => x.CurrentDateTime("yyyyMMdd_mmssms")).Return("20180101_115059_100").Repeat.Any();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "WWA", "@recipientId", "edi_test_prod", "@ST_ID", "WWAMSG", "@value", "TEST30975", "@referenceType", "JobNumber")).Return("C03078216").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "WWA", "@recipientId", "edi_test_prod", "@ST_ID", "WWAMSG", "@value", "C00678281", "@referenceType", "JobNumber")).Return("C03078216").Repeat.Any();

      mockContextAccessor.Expect(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "WWA_Booking_Cargowise_20180101_115059_100_11")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("WWAMSG", "WWA", "edi_test_prod", "11", "TEST30975")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("WWAMSG", "WWA", "edi_test_prod", "11", "C00678281")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("WWAMSG", "WWA", "edi_test_prod", "36ACCF3C-893F-40CE-BA01-1D8CBE2DF678", "11")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("WWAMSG", "WWA", "edi_test_prod", "C03078216", "", "ActionPurpose")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("WWAMSG", "WWA", "edi_test_prod", "C03078216", "APP", "ActionPurpose")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("WWAMSG", "WWA", "edi_test_prod", "C03078216", "ORG", "ActionPurpose")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("WWAMSG", "WWA", "edi_test_prod", "C03078216", "AMD", "ActionPurpose")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("WWAMSG", "WWA", "edi_test_prod", "C03078216", shipmentType, "ShipmentType")).Repeat.Any();

      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("WWAMSG", "WWA", "edi_test_prod", "C03078216", "1.0.0", "FormVersion")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("WWAMSG", "WWA", "edi_test_prod", "C03078216", "3.0.0", "FormVersion")).Repeat.Any();

      mockDataModelAccessor.Expect(x => x.GetClientRegistrationCode("edi_test_prod", "BN1", "WWA")).Return("CGWS");

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.WWA.SI", "@maxlength", "14")).Return("11").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("WWA", "WWA", "WWA Provider Configuration", "ContainerTypeToISOCode", "WWA Code", "42G0")).Return("42G0").Repeat.Any();

      mockOCMHelper.Expect(x => x.IsCoLoad("")).Return("FALSE").Repeat.Any();
      mockOCMHelper.Expect(x => x.IsCoLoad("CLD")).Return("TRUE").Repeat.Any();
      mockOCMHelper.Stub(x => x.IsCoLoad("AGT")).Return("FALSE");

      mockOCMHelper.Expect(x => x.GetServiceProvider("WWA_BK1")).Return("WWA").Repeat.Any();
      mockOCMHelper.Expect(x => x.GetPayableElseWhereOutputCode("WWA")).Return(payableElseWhere).Repeat.Any();

      // Password
      mockCodeMapper.Stub(x => x.GetRecipientCodeUnkeyed("WWA", "WWA", "WWA Provider Configuration", "Password", "WWA Password")).Return("Aw67C5jL2RRCb2sLYzQg");
      mockCodeMapper.Stub(x => x.GetRecipientCodeUnkeyed("WWA", "WWA", "WWA Provider Configuration", "Sender ID", "Sender")).Return("edi_cargowise_prod");
      mockCodeMapper.Stub(x => x.GetRecipientCodeUnkeyed("WWA", "WWA", "WWA Provider Configuration", "Receiver ID", "Receiver")).Return("wwalliance");

      mockSubscriptionHelper.Expect(x => x.InsertBoleroSubscription("edi_test_prod", "CGWS", "WWA", "C03078216", "TEST30975")).Repeat.Any();
      mockSubscriptionHelper.Expect(x => x.InsertBoleroSubscription("edi_test_prod", "CGWS", "WWA", "C03078216", "C00678281")).Repeat.Any();

      var extensionObjects = new Dictionary<string, object>() {
        { "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/DateMapper", mockDateMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
        { "http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor", mockDataModelAccessor },
        { "http://schemas.microsoft.com/BizTalk/2003/OCMHelper", mockOCMHelper },
        { "http://schemas.microsoft.com/BizTalk/2003/SubscriptionHelper", mockSubscriptionHelper }
      };

      var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.ExecuteCompiled<CarrierUniversal2BR_WWA>(input, expectedOutput);
    }
  }
}
