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
  public class Cargowise2Cargowise_US2UI_BLData_Tests
  {
    const string filePath = "Cargowise2Cargowise.BLData.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestUS2UI_BLData()
    {
      AssertMapping("Test1_CLD_input.xml", "Test1_CLD_output.xml", "C00001001", "NVOCC", "CLD");
      AssertMapping("Test2_AGT_input.xml", "Test2_AGT_output.xml", "C00001002", "ShippingLine", "AGT");
      AssertMapping("Test3_Invalid_input.xml", "Test3_Invalid_output.xml", "C00001002", "ShippingLine", "AGT");
    }

    void AssertMapping(string inputFile, string expectedOutputFile, string consolNumber, string ehubPartyType, string shipmentType)
    {
      var input = filePath + inputFile;
      var expectedOutput = filePath + expectedOutputFile;

      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
      var mockOCMHelper = MockRepository.GenerateStrictMock<OCMHelper>();
      var mockSubscriptionHelper = MockRepository.GenerateStrictMock<SubscriptionHelper>();

      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("HYEUAT001").Repeat.Any();
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", "HYEUAT999")).Repeat.Any();

      mockSubscriptionHelper.Expect(x => x.SelectSubscriptionsByValue(Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything)).Repeat.Once();
      mockSubscriptionHelper.Expect(x => x.GetSubscriber()).Return("HYEUAT999").Repeat.Any();
      mockSubscriptionHelper.Expect(x => x.GetProvider()).Return("HYEUAT001").Repeat.Any();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "HYEUAT001", "@recipientId", "", "@ST_ID", "CW1MSG", "@value", "WTG0000000001", "@referenceType", "JobNumber")).Return(consolNumber).Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "HYEUAT001", "@recipientId", "", "@ST_ID", "CW1MSG", "@value", "WTG0000000001", "@referenceType", "ShipmentType")).Return(shipmentType).Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "HYEUAT001", "@recipientId", "", "@ST_ID", "CW1MSG", "@value", "WTG0000000001", "@referenceType", "ActionPurpose")).Return("AMD").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "HYEUAT001", "@recipientId", "", "@ST_ID", "CW1MSG", "@value", "WTG0000000001", "@referenceType", "PartyType")).Return(ehubPartyType).Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "HYEUAT001", "@recipientId", "", "@ST_ID", "CW1MSG", "@value", "WTG0000000001", "@referenceType", "ForwardingType")).Return("ForwardingConsol").Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("CARGOWISE", "CARGOWISE", "CARGOWISE Provider Configuration", "Event Type", "Event Type", "BL Data", "AMD", "MAA")).Return("MAA").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("CARGOWISE", "CARGOWISE", "CARGOWISE Provider Configuration", "Event Type", "Event Reference", "BL Data", "AMD", "MAA")).Return("Booking Accepted").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("CARGOWISE", "CARGOWISE", "CARGOWISE Provider Configuration", "Event Type", "Event Type", "BL Data", "AMD", "MRJ")).Return("MRJ").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("CARGOWISE", "CARGOWISE", "CARGOWISE Provider Configuration", "Event Type", "Event Reference", "BL Data", "AMD", "MRJ")).Return("Booking Rejected").Repeat.Any();

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
      mapTester.ExecuteCompiled<US2UI_BLData>(input, expectedOutput);

      mockCodeMapper.VerifyAllExpectations();
      mockContextAccessor.VerifyAllExpectations();
      mockDataModelAccessor.VerifyAllExpectations();
      mockOCMHelper.VerifyAllExpectations();
      mockSubscriptionHelper.VerifyAllExpectations();
    }
  }
}