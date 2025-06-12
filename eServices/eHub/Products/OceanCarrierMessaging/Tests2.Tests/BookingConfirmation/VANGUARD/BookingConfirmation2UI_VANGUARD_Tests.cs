using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.BookingConfirmation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Helper;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.UShipment2UInterchangeInclude;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests
{
  [TestClass]
  public class BookingConfirmation2UIVANGUARD_Tests
  {
    const string filePath = "BookingConfirmation.VANGUARD.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestBookingConfirmation2UIVANGUARD()
    {
      AssertMapping("Test1_input.xml", "Test1_output_BusinessLayer.xml", "Test1_output_UInterchange.xml", "N", "MAA", "Booking Confirmed");
      AssertMapping("Test2_input.xml", "Test2_output_BusinessLayer.xml", "Test2_output_UInterchange.xml", "N", "MAA", "Booking Confirmed");
      AssertMapping("Test3_input.xml", "Test3_output_BusinessLayer.xml", "Test3_output_UInterchange.xml", "N", "MAA", "Booking Confirmed");
      AssertMapping("Test4_input_No_RecipientID.xml", "Test4_output_No_RecipientID_BusinessLayer.xml", "Test4_output_No_RecipientID_UInterchange.xml", "N", "MAA", "Booking Confirmed", recipientID: "");
    }

    void AssertMapping(string inputFile, string businessLayerFile, string expectedFinalOutputFile, string eventCode, string eventType, string eventRef, string recipientID = "CARGOWISE")
    {
      var input = filePath + inputFile;
      var expectedBusinessLayerFile = filePath + businessLayerFile;
      var expectedFinalOutput = filePath + expectedFinalOutputFile;

      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
      var mockOCMHelper = MockRepository.GenerateStrictMock<OCMHelper>();
      var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
      var mockSubscriptionHelper = MockRepository.GenerateStrictMock<SubscriptionHelper>();

      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("VANGUARD").Repeat.Any();
      mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(recipientID).Repeat.Any();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "VANGUARD", "@recipientId", recipientID, "@ST_ID", "VGDMSG", "@value", "VGD10002008", "@referenceType", "JobNumber")).Return("C00001360").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "VANGUARD", "@recipientId", recipientID, "@ST_ID", "VGDMSG", "@value", "VGD10002008", "@referenceType", "ShipmentType")).Return("AGT").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "VANGUARD", "@recipientId", "", "@ST_ID", "VGDMSG", "@value", "VGD10002008", "@referenceType", "FormVersion")).Return("2.0.0.0").Repeat.Once();

      mockCodeMapper.Expect(x => x.GetRecipientCode("VANGUARD", "VANGUARD", "Booking Confirmation from VANGUARD", "Event Type", "Event Type", eventCode)).Return(eventType).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("VANGUARD", "VANGUARD", "Booking Confirmation from VANGUARD", "Event Type", "Event Reference", eventCode)).Return(eventRef).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Package Type ISO", "Package Type", "PX")).Return("PKG").Repeat.Any();

      if (recipientID != "")
      {
        mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("OCMCNF", recipientID, "VANGUARD", "LONBAH8567265V|203554", "LONBAH8567265V|C00001360|CHNJ", "IFTMBC")).Repeat.Any();
        mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("OCMCNF", recipientID, "VANGUARD", "LONBAH8567265V|203554", "LONBAH8567265V|C00001360|", "IFTMBC")).Repeat.Any();
      }

      mockOCMHelper.Expect(x => x.IsCoLoad("")).Return("FALSE").Repeat.Any();
      mockOCMHelper.Expect(x => x.IsCoLoad("AGT")).Return("FALSE").Repeat.Any();
      mockOCMHelper.Expect(x => x.IsCoLoad("CLD")).Return("TRUE").Repeat.Any();

      mockSubscriptionHelper.Expect(x => x.GetOCMBEClientID("VANGUARD", recipientID)).Return("OCM_BookingEngine").Repeat.Any();
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", "OCM_BookingEngine")).Repeat.AtLeastOnce();

      var extensionObjects = new Dictionary<string, object>()
      {
        { "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/DateMapper", mockDateMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
        { "http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor", mockDataModelAccessor },
        { "http://schemas.microsoft.com/BizTalk/2003/OCMHelper", mockOCMHelper },
        { "http://schemas.microsoft.com/BizTalk/2003/SubscriptionHelper", mockSubscriptionHelper },
      };

      MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.ExecuteCompiled<BookingConfirmation2UI_VANGUARD>(input, expectedBusinessLayerFile);

      var mockStringHelper = MockRepository.GenerateStrictMock<StringHelper>();
      var extensionObjects2 = new Dictionary<string, object>() {
        { "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/DateMapper", mockDateMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
        { "http://schemas.microsoft.com/BizTalk/2003/OCMHelper", mockOCMHelper },
        { "http://schemas.microsoft.com/BizTalk/2003/StringHelper", mockStringHelper }
      };

      var mapTester2 = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects2);
      mapTester2.ExecuteCompiled<UShipment2UInterchangeInclude>(expectedBusinessLayerFile, expectedFinalOutput);
    }
  }
}
