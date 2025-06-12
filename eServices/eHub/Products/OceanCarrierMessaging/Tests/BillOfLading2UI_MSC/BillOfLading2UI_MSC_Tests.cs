using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.BillOfLading2UI_MSC;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests
{
  [TestClass]
  public class BillOfLading2UI_MSC_Tests
  {
    const string filePath = "BillOfLading2UI_MSC.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void BillOfLading2UI_MSC()
    {
      AssertMapping("Test1_NoShipment_input.xml", "Test1_NoShipment_output.xml", "CARGOSMART", "ZAZAZA");
      AssertMapping("Test2_input.xml", "Test2_output.xml", "CARGOSMART", "ZAZAZA");
      AssertMapping("Test3_GCT_input.xml", "Test3_GCT_output.xml", "CARGOSMART", "CONTAINER_TRACKING");
      AssertMapping("Test4_GCT_input.xml", "Test4_GCT_output.xml", "CARGOSMART", "CONTAINER_TRACKING");
      AssertMapping("Test5_GCT_NoConsol_input.xml", "Test5_GCT_NoConsol_output.xml", "CARGOSMART", "CONTAINER_TRACKING");
      AssertMapping("Test6_input.xml", "Test6_output.xml", "CARGOSMART", "ZAZAZA");
    }

    void AssertMapping(string inputFile, string expectedOutputFile, string senderID, string recipientID)
    {
      var input = filePath + inputFile;
      var expectedOutput = filePath + expectedOutputFile;

      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
      var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
      var mockSubscriptionHelper = MockRepository.GenerateStrictMock<SubscriptionHelper>();

      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(senderID);
      mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(recipientID).Repeat.AtLeastOnce();

      mockCodeMapper.Expect(x => x.GetRecipientCode("MSC", "MSC", "MSC Provider Configuration", "Event Type (BOL)", "Event Type", "NEW")).Return("MAA").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("MSC", "MSC", "MSC Provider Configuration", "Event Type (BOL)", "Event Type", "UPDATE")).Return("MRR").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("MSC", "MSC", "MSC Provider Configuration", "Event Type (BOL)", "Event Type", "DELETE")).Return("MWA").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("MSC", "MSC", "MSC Provider Configuration", "Event Type (BOL)", "Event Reference", "NEW")).Return("Bill of Lading Confirmed").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("MSC", "MSC", "MSC Provider Configuration", "Event Type (BOL)", "Event Reference", "UPDATE")).Return("Bill of Lading Confirmed").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("MSC", "MSC", "MSC Provider Configuration", "Event Type (BOL)", "Event Reference", "DELETE")).Return("Bill of Lading Confirmed").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMCARGOWISE", "OCMCARGOWISE", "OCM Cargowise System Configuration", "SCAC (Inbound)", "SCAC", senderID, "")).Return("OOLU").Repeat.Any();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", "MSCMSG", "@value", "MSC000000000", "@referenceType", "JobNumber")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", "MSCMSG", "@value", "MSC000000111", "@referenceType", "JobNumber")).Return("C01329220").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", "MSCMSG", "@value", "MSC000000222", "@referenceType", "JobNumber")).Return("C01329221").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", "MSCMSG", "@value", "MSC000000888", "@referenceType", "JobNumber")).Return("C01329221").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", "MSCMSG", "@value", "MSC000000999", "@referenceType", "JobNumber")).Return("C01329221").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", "MSCMSG", "@value", "MSC0000000061", "@referenceType", "JobNumber")).Return("C01329221").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedClients", "", "@senderId", senderID, "@ST_ID", "MSCMSG", "@value", "MSC000000000")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedClients", "", "@senderId", senderID, "@ST_ID", "MSCMSG", "@value", "MSC000000111")).Return("HTLBLRIND1").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedClients", "", "@senderId", senderID, "@ST_ID", "MSCMSG", "@value", "MSC000000222")).Return("HTLBLRIND2").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedClients", "", "@senderId", senderID, "@ST_ID", "MSCMSG", "@value", "MSC000000888")).Return("HTLBLRIND1").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedClients", "", "@senderId", senderID, "@ST_ID", "MSCMSG", "@value", "MSC000000999")).Return("HTLBLRIND2").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedClients", "", "@senderId", senderID, "@ST_ID", "MSCMSG", "@value", "MSC0000000061")).Return("HTLBLRIND1").Repeat.Any();

      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("OCMCNF", "HTLBLRIND1", senderID, "MEDUXX127126|03128-1", "421MS5528650|C01329220|OOLU", "MSCBillOfLadingOut")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("OCMCNF", "HTLBLRIND2", senderID, "MEDUXX127127|03128-1", "421MS5528651|C01329221|OOLU", "MSCBillOfLadingOut")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("OCMCNF", "HTLBLRIND1", senderID, "MEDUXX127128|03128-1", "421MS5528652|C01329221|OOLU", "MSCBillOfLadingOut")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("OCMCNF", "HTLBLRIND1", senderID, "MEDUXX127128|03128-2", "421MS5528652|C01329221|OOLU", "MSCBillOfLadingOut")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("OCMCNF", "HTLBLRIND2", senderID, "MEDUXX127129|03128-1", "421MS5528653|C01329221|OOLU", "MSCBillOfLadingOut")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("OCMCNF", "HTLBLRIND2", senderID, "MEDUXX127129|03128-2", "421MS5528653|C01329221|OOLU", "MSCBillOfLadingOut")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("OCMCNF", "HTLBLRIND1", senderID, "MEDUAR608350|001172-1", "213B1381013|C01329221|OOLU", "MSCBillOfLadingOut")).Repeat.Any();

      mockDateMapper.Expect(x => x.CurrentDateTimeUTC("yyyy-MM-ddTHH:mm:ss.fff")).Return("2017-05-26T11:12:51.000").Repeat.Any();

      mockSubscriptionHelper.Expect(x => x.GetOCMBEClientID(senderID, recipientID)).Return("OCM_BookingEngine").Repeat.Any();
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", "OCM_BookingEngine")).Repeat.AtLeastOnce();

      var extensionObjects = new Dictionary<string, object>()
      {
        { "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/DateMapper", mockDateMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
        { "http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor", mockDataModelAccessor },
        { "http://schemas.microsoft.com/BizTalk/2003/SubscriptionHelper", mockSubscriptionHelper },
      };

      MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.ExecuteCompiled<BillOfLading2UI_MSC>(input, expectedOutput);
    }
  }
}
