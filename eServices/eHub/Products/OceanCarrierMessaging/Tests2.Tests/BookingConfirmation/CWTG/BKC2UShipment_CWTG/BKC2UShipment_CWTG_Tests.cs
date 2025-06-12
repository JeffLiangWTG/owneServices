using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.BookingConfirmation;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Helper;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.UShipment2UInterchangeInclude;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests
{
  [TestClass]
  public class BKC2UShipment_CWTG_Tests
  {
    const string filePath = "BookingConfirmation.CWTG.BKC2UShipment_CWTG.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestBKC2UShipment_CWTG()
    {
      AssertMapping("Test1_input.xml", "Test1_output_BusinessLayer.xml", "Test1_output_UInterchange.xml");
      AssertMapping("Test2_input.xml", "Test2_output_BusinessLayer.xml", "Test2_output_UInterchange.xml");
      AssertMapping("Test3_input.xml", "Test3_output_BusinessLayer.xml", "Test3_output_UInterchange.xml");
      AssertMapping("Test4_input_TransportLegCollection01.xml", "Test4_output_TransportLegCollection01_BusinessLayer.xml", "Test4_output_TransportLegCollection01_UInterchange.xml");
      AssertMapping("Test4_input_TransportLegCollection02.xml", "Test4_output_TransportLegCollection02_BusinessLayer.xml", "Test4_output_TransportLegCollection02_UInterchange.xml");
      AssertMapping("Test4_input_TransportLegCollection03.xml", "Test4_output_TransportLegCollection03_BusinessLayer.xml", "Test4_output_TransportLegCollection03_UInterchange.xml");
      AssertMapping("Test4_input_TransportLegCollection04.xml", "Test4_output_TransportLegCollection04_BusinessLayer.xml", "Test4_output_TransportLegCollection04_UInterchange.xml");
      AssertMapping("Test5_input.xml", "Test5_output_BusinessLayer.xml", "Test5_output_UInterchange.xml", "AGT");
      AssertMapping("Test6_input_No_RecipientID.xml", "Test6_output_No_RecipientID_BusinessLayer.xml", "Test6_output_No_RecipientID_UInterchange.xml", "AGT", recipientID: "");
    }

    void AssertMapping(string inputFile, string businessLayerFiler, string expectedFinalOutputFile, string shipmentType = "CLD", string recipientID = "KNPROD")
    {
      var input = filePath + inputFile;
      var expectedBusinessLayerFile = filePath + businessLayerFiler;
      var expectedFinalOutput = filePath + expectedFinalOutputFile;

      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
      var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
      var mockOCMHelper = MockRepository.GenerateStrictMock<OCMHelper>();
      var mockSubscriptionHelper = MockRepository.GenerateStrictMock<SubscriptionHelper>();

      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("CWTG").Repeat.Any();
      mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(recipientID).Repeat.Any();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "CWTG", "@recipientId", recipientID, "@ST_ID", "CWTGMG", "@value", "")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "CWTG", "@recipientId", recipientID, "@ST_ID", "CWTGMG", "@value", "CWTG0000000008")).Return("CWTG0000000008");
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "CWTG", "@recipientId", "", "@ST_ID", "CWTGMG", "@value", "CWTG0000000008", "@referenceType", "ShipmentType")).Return(shipmentType).Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "CWTG", "@recipientId", "", "@ST_ID", "CWTGMG", "@value", "CWTG0000000008", "@referenceType", "FormVersion")).Return("2.0.0.0").Repeat.Any();

      mockDateMapper.Expect(x => x.CurrentDateTimeUTC("yyyyMMddHHmm")).Return("201903191436");

      mockCodeMapper.Expect(x => x.GetRecipientCode("CWTG", "CWTG", "CWTG System Configuration", "Event Type", "Event Type", "")).Return("MAA").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("CWTG", "CWTG", "CWTG System Confirmation", "Event Type", "Event Reference", "")).Return("Booking Confirmed").Repeat.Any();
      mockOCMHelper.Expect(x => x.IsCoLoad("CLD")).Return("TRUE").Repeat.Any();
      mockOCMHelper.Expect(x => x.IsCoLoad("AGT")).Return("FALSE").Repeat.Any();

      if (recipientID != "")
      {
        mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("OCMCNF", recipientID, "CWTG", "CLBR963455|20190227110443804000", "CLBR963455|CWTG0000000008|GLCA", "IFTMBC")).Repeat.Any();
      }

      mockSubscriptionHelper.Expect(x => x.GetOCMBEClientID("CWTG", recipientID)).Return("OCM_BookingEngine").Repeat.Any();
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
      mapTester.ExecuteCompiled<BKC2UShipment_CWTG>(input, expectedBusinessLayerFile);

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
