using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.CARGOWISE2CARGOWISE;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests
{
  [TestClass]
  public class Cargowise2Cargowise_UI2UI_ACK_Tests
  {
    const string filePath = "Cargowise2Cargowise.Acknowledgement.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestUI2UI_ACK()
    {
      AssertMapping("Test1_CLD_NVOCC_input.xml", "Test1_CLD_NVOCC_output.xml", "C00001002", "CLD");
      AssertMapping("Test2_CLD_ShippingLine_input.xml", "Test2_CLD_ShippingLine_output.xml", "C00001002_TBNN1234567", "CLD");

      AssertMapping("Test3_AGT_NVOCC_input.xml", "Test3_AGT_NVOCC_output.xml", "C00001002_TBNN1234567", "AGT");
      AssertMapping("Test4_AGT_ShippingLine_input.xml", "Test4_AGT_ShippingLine_output.xml", "C00001002_TBNN1234567", "AGT");
    }

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestUI2UI_ACK_UEvent()
    {
      AssertMapping("Test5_UEvent_WAR_input.xml", "Test5_UEvent_WAR_output.xml", "C00001002_TBNN1234567", "CLD");
      AssertMapping("Test6_UEvent_ERR_input.xml", "Test6_UEvent_ERR_output.xml", "C00001002_TBNN1234567", "CLD");
    }

    void AssertMapping(string inputFile, string expectedOutputFile, string jobnumber, string shipmentType)
    {
      var input = filePath + inputFile;
      var expectedOutput = filePath + expectedOutputFile;

      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
      var mockOCMHelper = MockRepository.GenerateStrictMock<OCMHelper>();
      var mockSubscriptionHelper = MockRepository.GenerateStrictMock<SubscriptionHelper>();

      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("HYEUAT001").Repeat.Any();
      mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("HYEUAT999").Repeat.Any();
      mockSubscriptionHelper.Expect(x => x.GetOCMBEClientID("HYEUAT001", "HYEUAT999")).Return("OCM_BookingEngine").Repeat.Any();
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", "OCM_BookingEngine")).Repeat.AtLeastOnce();

      mockSubscriptionHelper.Expect(x => x.SelectSubscriptionsByValue(Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything)).Repeat.Once();
      mockSubscriptionHelper.Expect(x => x.GetSubscriber()).Return("HYEUAT999").Repeat.Any();
      mockSubscriptionHelper.Expect(x => x.GetProvider()).Return("HYEUAT001").Repeat.Any();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "HYEUAT001", "@recipientId", "", "@ST_ID", "CW1MSG", "@value", "WTG0000000001", "@referenceType", "JobNumber")).Return(jobnumber).Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "HYEUAT001", "@recipientId", "", "@ST_ID", "CW1MSG", "@value", "WTG0000000001", "@referenceType", "ShipmentType")).Return(shipmentType).Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "HYEUAT001", "@recipientId", "", "@ST_ID", "CW1MSG", "@value", "WTG0000000001", "@referenceType", "ActionPurpose")).Return("AMD").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "HYEUAT001", "@recipientId", "", "@ST_ID", "CW1MSG", "@value", "WTG0000000001", "@referenceType", "ForwardingType")).Return("ForwardingConsol").Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("CARGOWISE", "CARGOWISE", "CARGOWISE Provider Configuration", "Event Type", "Event Type", "Acknowledgement", "AMD", "MAA")).Return("MAA").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("CARGOWISE", "CARGOWISE", "CARGOWISE Provider Configuration", "Event Type", "Event Reference", "Acknowledgement", "AMD", "MAA")).Return("Booking Accepted").Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("CARGOWISE", "CARGOWISE", "CARGOWISE Provider Configuration", "Event Type", "Event Type", "Acknowledgement", "AMD", "WAR")).Return("IRA").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("CARGOWISE", "CARGOWISE", "CARGOWISE Provider Configuration", "Event Type", "Event Reference", "Acknowledgement", "AMD", "WAR")).Return("Interchange Receipt Acknowledged").Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("CARGOWISE", "CARGOWISE", "CARGOWISE Provider Configuration", "Event Type", "Event Type", "Acknowledgement", "AMD", "ERR")).Return("IRJ").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("CARGOWISE", "CARGOWISE", "CARGOWISE Provider Configuration", "Event Type", "Event Reference", "Acknowledgement", "AMD", "ERR")).Return("Interchange Receipt Rejected").Repeat.Any();

      mockOCMHelper.Expect(x => x.IsCoLoad("AGT")).Return("FALSE").Repeat.Any();
      mockOCMHelper.Expect(x => x.IsCoLoad("CLD")).Return("TRUE").Repeat.Any();

      var extensionObjects = new Dictionary<string, object>()
      {
        {"http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper},
        {"http://schemas.microsoft.com/BizTalk/2003/OCMHelper", mockOCMHelper},
        {"http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor},
        {"http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor", mockDataModelAccessor},
        {"http://schemas.microsoft.com/BizTalk/2003/SubscriptionHelper", mockSubscriptionHelper}
      };

      var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.ExecuteCompiled<UI2UI_ACK>(input, expectedOutput);

      mockCodeMapper.VerifyAllExpectations();
      mockContextAccessor.VerifyAllExpectations();
      mockDataModelAccessor.VerifyAllExpectations();
      mockOCMHelper.VerifyAllExpectations();
      mockSubscriptionHelper.VerifyAllExpectations();
    }
  }
}