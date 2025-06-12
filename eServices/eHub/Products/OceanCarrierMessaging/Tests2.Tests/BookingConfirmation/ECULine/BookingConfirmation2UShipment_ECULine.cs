using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.BookingConfirmation;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests
{
  [TestClass]
  public class BookingConfirmation2UShipment_ECULine_Tests
  {
    const string filePath = "BookingConfirmation.ECULine.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestBookingConfirmation2UShipment_ECULine()
    {
      AssertMapping("Test1_input.xml", "Test1_output_AGT.xml", "AGT");
      AssertMapping("Test1_input.xml", "Test1_output_CLD.xml", "CLD");
      AssertMapping("Test1_input.xml", "Test1_output_AGT_No_RecipientID.xml", "AGT", "");
    }

    void AssertMapping(string inputFile, string expectedOutputFile, string expectedShipmentType, string recipientID = "recipient123")
    {
      var input = filePath + inputFile;
      var expectedOutput = filePath + expectedOutputFile;

      var headerRecipientID = "HYEBNEUAT";

      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
      var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
      var mockOCMHelper = MockRepository.GenerateStrictMock<OCMHelper>();
      var mockSubscriptionHelper = MockRepository.GenerateStrictMock<SubscriptionHelper>();

      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("ECULINE").Repeat.AtLeastOnce();
      mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("ZAZAZA").Repeat.AtLeastOnce();

      mockDataModelAccessor.Expect(x => x.GeteHubIDByCode(headerRecipientID, "ECULINE", false)).Return(recipientID).Repeat.Once();

      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "MSGID", "ECULINE")).Return("ECUMSG").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "ECULINE", "@recipientId", recipientID, "@ST_ID", "ECUMSG", "@value", "C000080001", "@referenceType", "FormVersion")).Return("2.0.0.0").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "ECULINE", "@recipientId", recipientID, "@ST_ID", "ECUMSG", "@value", "C000080001", "@referenceType", "ShipmentType")).Return(expectedShipmentType).Repeat.Any();

      var currentUTCDateTime = "1741008269";
      if (recipientID != "")
      {
        mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("OCMCNF", recipientID, "ECULINE", $"CO/MBN/000002|{currentUTCDateTime}", $"CO/BKGREF/000001|C000080001|ECUW", "IFTMBC")).Repeat.Any();
      }

      mockOCMHelper.Expect(x => x.IsCoLoad("AGT")).Return("FALSE").Repeat.Any();
      mockOCMHelper.Expect(x => x.IsCoLoad("CLD")).Return("TRUE").Repeat.Any();
      mockDateMapper.Expect(x => x.CurrentDateTimeUTC("yyyyMMddHHmmss")).Return(currentUTCDateTime).Repeat.Any();

      mockSubscriptionHelper.Expect(x => x.GetOCMBEClientID("ECULINE", "ZAZAZA")).Return("OCM_BookingEngine").Repeat.Any();
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", "OCM_BookingEngine")).Repeat.AtLeastOnce();

      var extensionObjects = new Dictionary<string, object>()
      {
        {"http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper},
        {"http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor},
        {"http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor", mockDataModelAccessor },
        {"http://schemas.microsoft.com/BizTalk/2003/OCMHelper", mockOCMHelper},
        { "http://schemas.microsoft.com/BizTalk/2003/DateMapper", mockDateMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/SubscriptionHelper", mockSubscriptionHelper },
      };

      MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.Execute<BookingConfirmation2UShipment_ECULine>(input, expectedOutput);
    }
  }
}