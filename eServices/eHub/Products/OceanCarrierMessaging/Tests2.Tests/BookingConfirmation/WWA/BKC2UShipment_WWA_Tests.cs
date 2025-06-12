using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.BookingConfirmation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Helper;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.UShipment2UInterchangeInclude;
using System;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests
{
  [TestClass]
  public class BKC2UShipment_WWA_Tests
  {
    const string filePath = "BookingConfirmation.WWA.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void BookingConfirmation2UniversalInterchange_WWA()
    {
      AssertMapping("Test01_input_MAA.xml", "Test01_output_MAA_BusinessLayer.xml", "Test01_output_MAA_UInterchange.xml", "CW1");
      AssertMapping("Test02_input_MRR.xml", "Test02_output_MRR_BusinessLayer.xml", "Test02_output_MRR_UInterchange.xml", "CW1");
      AssertMapping("Test03_input_MWA.xml", "Test03_output_MWA_BusinessLayer.xml", "Test03_output_MWA_UInterchange.xml", "CW1");
      AssertMapping("Test04_input_UrlEmpty.xml", "Test04_output_UrlEmpty_BusinessLayer.xml", "Test04_output_UrlEmpty_UInterchange.xml", "CW1");
      AssertMapping("Test05_input_CoLoad.xml", "Test05_output_CoLoad_BusinessLayer.xml", "Test05_output_CoLoad_UInterchange.xml", "CW1", shipmentType: "CLD");
      AssertMapping("Test06_input.xml", "Test06_output_BusinessLayer.xml", "Test06_output_UInterchange.xml", "CW1");
      AssertMapping("Test07_input.xml", "Test07_output_BusinessLayer.xml", "Test07_output_UInterchange.xml", "CW1");
      AssertMapping("Test08_input_MAA.xml", "Test08_output_MAA_BusinessLayer.xml", "Test08_output_MAA_UInterchange.xml", "CW1", false, true, formVersion: "");
      AssertMapping("Test09_input_MAA.xml", "Test09_output_MAA_BusinessLayer.xml", "Test09_output_MAA_UInterchange.xml", "CONTAINER_TRACKING");
      AssertMapping("Test10_input_MAA_DIRECT.xml", "Test10_output_MAA_DIRECT_BusinessLayer.xml", "Test10_output_MAA_DIRECT_UInterchange.xml", "CW1", true);
      AssertMapping("Test11_input_MAA_No_RecipientID.xml", "Test11_output_MAA_No_RecipientID_BusinessLayer.xml", "Test11_output_MAA_No_RecipientID_UInterchange.xml", "");
    }

    void AssertMapping(string inputFile, string businessLayerFile, string expectedOutputFile, string destinationParty, bool isDirect = false, bool noFormVersionWithBookingNumber = false, string shipmentType = "ABC", string formVersion = "2.0.0.0")
    {
      var input = filePath + inputFile;
      var expectedBusinessLayerFile = filePath + businessLayerFile;
      var expectedFinalOutput = filePath + expectedOutputFile;

      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
      var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
      var mockOCMHelper = MockRepository.GenerateStrictMock<OCMHelper>();
      var mockSubscriptionHelper = MockRepository.GenerateStrictMock<SubscriptionHelper>();

      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("WWA").Repeat.AtLeastOnce();
      mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(destinationParty).Repeat.AtLeastOnce();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderID", "WWA", "@recipientId", "", "@ST_ID", "WWAMSG", "@value", "WWA0000000005", "@referenceType", "ShipmentType")).Return(shipmentType).Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderID", "WWA", "@recipientId", "", "@ST_ID", "WWAMSG", "@value", "WWA0000000005", "@referenceType", "JobNumber")).Return("C00001360").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderID", "WWA", "@recipientId", "", "@ST_ID", "WWAMSG", "@value", "WWA0000000005", "@referenceType", "FormVersion")).Return(formVersion).Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderID", "WWA", "@recipientId", "", "@ST_ID", "WWAMSG", "@value", "C00001360", "@referenceType", "FormVersion")).Return("").Repeat.Any();

      mockDateMapper.Expect(x => x.CurrentDateTimeUTC("yyyy-MM-ddThh:mm:ss")).Return("2021-06-09T10:10:10");

      if (destinationParty != "")
      {
        mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("OCMCNF", destinationParty, "WWA", "2665536|1365572191558", "2665536|C00001360|WAIC", "IFTMBC")).Repeat.Any();
      }

      mockOCMHelper.Expect(x => x.IsCoLoad("")).Return("FALSE").Repeat.Any();
      mockOCMHelper.Expect(x => x.IsCoLoad("ABC")).Return("FALSE").Repeat.Any();
      mockOCMHelper.Expect(x => x.IsCoLoad("CLD")).Return("TRUE").Repeat.Any();

      mockSubscriptionHelper.Expect(x => x.GetOCMBEClientID("WWA", destinationParty)).Return("OCM_BookingEngine").Repeat.Any();
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
      mapTester.ExecuteCompiled<BKC2UShipment_WWA>(input, expectedBusinessLayerFile);

      var mockStringHelper = MockRepository.GenerateStrictMock<StringHelper>();
      var mockDateMapper2 = MockRepository.GenerateStrictMock<DateMapper>();
      if (destinationParty == "CONTAINER_TRACKING")
      {
        mockDateMapper2.Expect(x => x.CurrentDateTimeUTC("yyyy-MM-ddTHH:mm:ss.fff")).Return("2021-06-09T10:10:10.999");
      }
      var extensionObjects2 = new Dictionary<string, object>() {
        { "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/DateMapper", mockDateMapper2 },
        { "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
        { "http://schemas.microsoft.com/BizTalk/2003/OCMHelper", mockOCMHelper },
        { "http://schemas.microsoft.com/BizTalk/2003/StringHelper", mockStringHelper }
      };

      var mapTester2 = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects2);
      mapTester2.ExecuteCompiled<UShipment2UInterchangeInclude>(expectedBusinessLayerFile, expectedFinalOutput);
    }
  }
}
