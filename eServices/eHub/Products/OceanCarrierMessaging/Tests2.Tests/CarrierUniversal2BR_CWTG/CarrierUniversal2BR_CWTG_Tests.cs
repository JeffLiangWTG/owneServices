using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.CarrierUniversal2BR_CWTG;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests
{
  [TestClass]
  public class CarrierUniversal2BRCWTG_Tests
  {
    const string filePath = "CarrierUniversal2BR_CWTG.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestCarrierUniversal2BR_CWTG()
    {
      AssertMapping("Test1_input.xml", "Test1_output.xml");
      AssertMapping("Test2_input.xml", "Test2_output.xml", payableElseWhere: "FA");
      AssertMapping("Test3_input.xml", "Test3_output.xml");
      AssertMapping("Test4_input.xml", "Test4_output.xml");
      AssertMapping("Test5_input.xml", "Test5_output.xml");
      AssertMapping("Test6_input.xml", "Test6_output.xml");
    }

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestCarrierUniversal2BR_CWTG_GroupingMethod()
    {
      AssertMapping("Test7_GroupingMethod_SHP_input.xml", "Test7_GroupingMethod_SHP_output.xml");
      AssertMapping("Test8_GroupingMethod_PKL_input.xml", "Test8_GroupingMethod_PKL_output.xml");
    }

    void AssertMapping(string inputFile, string expectedOutputFile, string payableElseWhere = "")
    {
      var input = filePath + inputFile;
      var expectedOutput = filePath + expectedOutputFile;

      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
      var mockOCMHelper = MockRepository.GenerateStrictMock<OCMHelper>();
      var mockSubscriptionHelper = MockRepository.GenerateStrictMock<SubscriptionHelper>();

      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("TESTSENDER_CW").Repeat.Any();

      mockContextAccessor.Expect(x => x.GetContextProperty("InternalTrackingID", "http://cargowise.com/ehub/tracking/2010/06")).Return("36ACCF3C-893F-40CE-BA01-1D8CBE2DF678").Repeat.Any();

      mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("CWTG").Repeat.Any();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.CWTG", "@maxlength", "14")).Return("12").Repeat.Any();

      mockDataModelAccessor.Expect(x => x.GetClientRegistrationCode("TESTSENDER_CW", "BNE", "CWTG")).Return("CWTGBNE").Repeat.Any();
      mockDataModelAccessor.Expect(x => x.GetClientRegistrationCode("TESTSENDER_CW", "BN1", "CWTG")).Return("").Repeat.Any();

      mockDateMapper.Expect(x => x.CurrentDateTime("yyyyMMdd_mmssms")).Return("20190123_01235900").Repeat.Any();
      mockContextAccessor.Expect(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "CWTG_BR_TESTSENDER_CW_12")).Repeat.Any();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "CWTG", "@recipientId", "TESTSENDER_CW", "@ST_ID", "CWTGMG", "@value", "CEBS0000682395")).Return("CEBS0000682395").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "CWTG", "@recipientId", "TESTSENDER_CW", "@ST_ID", "CWTGMG", "@value", "CEBS0000682392")).Return("CEBS0000682392").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "CWTG", "@recipientId", "TESTSENDER_CW", "@ST_ID", "CWTGMG", "@value", "C00001000")).Return("C00001000").Repeat.Any();

      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CWTGMG", "CWTG", "TESTSENDER_CW", "12", "CEBS0000682395")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CWTGMG", "CWTG", "TESTSENDER_CW", "12", "CEBS0000682392")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CWTGMG", "CWTG", "TESTSENDER_CW", "12", "C00001000")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CWTGMG", "CWTG", "TESTSENDER_CW", "36ACCF3C-893F-40CE-BA01-1D8CBE2DF678", "12")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CWTGMG", "CWTG", "TESTSENDER_CW", "CEBS0000682392", "AMD", "ActionPurpose")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CWTGMG", "CWTG", "TESTSENDER_CW", "CEBS0000682392", "CLD", "ShipmentType")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CWTGMG", "CWTG", "TESTSENDER_CW", "CEBS0000682392", "1.0.0", "FormVersion")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CWTGMG", "CWTG", "TESTSENDER_CW", "C00001000", "ORG", "ActionPurpose")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CWTGMG", "CWTG", "TESTSENDER_CW", "C00001000", "CLD", "ShipmentType")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CWTGMG", "CWTG", "TESTSENDER_CW", "C00001000", "AMD", "ActionPurpose")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CWTGMG", "CWTG", "TESTSENDER_CW", "CEBS0000682395", "WTH", "ActionPurpose")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CWTGMG", "CWTG", "TESTSENDER_CW", "CEBS0000682395", "AGT", "ShipmentType")).Repeat.Any();

      mockCodeMapper.Stub(x => x.GetRecipientCodeUnkeyed("CWTG", "CWTG", "CWTG Provider Configuration", "Sender ID", "Sender")).Return("01001669");
      mockCodeMapper.Stub(x => x.GetRecipientCodeUnkeyed("CWTG", "CWTG", "CWTG Provider Configuration", "Receiver ID", "Receiver")).Return("01001669");
      mockCodeMapper.Stub(x => x.GetRecipientCodeUnkeyed("CWTG", "CWTG", "CWTG Provider Configuration", "Password", "Password")).Return("");

      mockCodeMapper.Stub(x => x.GetRecipientCode("CWTG", "CWTG", "CWTG Provider Configuration", "ContainerTypeToISOCode", "CWTG Code", "")).Return("").Repeat.Any();
      mockCodeMapper.Stub(x => x.GetRecipientCode("CWTG", "CWTG", "CWTG Provider Configuration", "ContainerTypeToISOCode", "CWTG Code", "22G0")).Return("");
      mockCodeMapper.Stub(x => x.GetRecipientCode("CWTG", "CWTG", "CWTG Provider Configuration", "ContainerTypeToISOCode", "CWTG Code", "42G0")).Return("");

      mockOCMHelper.Expect(x => x.IsCoLoad("AGT")).Return("FALSE").Repeat.Any();
      mockOCMHelper.Expect(x => x.IsCoLoad("CLD")).Return("TRUE").Repeat.Any();

      mockOCMHelper.Expect(x => x.GetServiceProvider("CWTG")).Return("CWTG").Repeat.Any();
      mockOCMHelper.Expect(x => x.GetPayableElseWhereOutputCode("CWTG")).Return(payableElseWhere).Repeat.Any();

      mockSubscriptionHelper.Expect(x => x.InsertBoleroSubscription("TESTSENDER_CW", "", "CWTG", "CEBS0000682392", "CEBS0000682392")).Repeat.Any();
      mockSubscriptionHelper.Expect(x => x.InsertBoleroSubscription("TESTSENDER_CW", "CWTGBNE", "CWTG", "C00001000", "C00001000")).Repeat.Any();
      mockSubscriptionHelper.Expect(x => x.InsertBoleroSubscription("TESTSENDER_CW", "", "CWTG", "CEBS0000682395", "CEBS0000682395")).Repeat.Any();

      var extensionObjects = new Dictionary<string, object>()
      {
        {"http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper},
        {"http://schemas.microsoft.com/BizTalk/2003/DateMapper", mockDateMapper},
        {"http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor},
        {"http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor", mockDataModelAccessor},
        { "http://schemas.microsoft.com/BizTalk/2003/OCMHelper", mockOCMHelper },
        { "http://schemas.microsoft.com/BizTalk/2003/SubscriptionHelper", mockSubscriptionHelper }
      };

      var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.ExecuteCompiled<CarrierUniversal2BR_CWTG>(input, expectedOutput);

      mockCodeMapper.VerifyAllExpectations();
      mockDateMapper.VerifyAllExpectations();
      mockContextAccessor.VerifyAllExpectations();
      mockDataModelAccessor.VerifyAllExpectations();

    }
  }
}

