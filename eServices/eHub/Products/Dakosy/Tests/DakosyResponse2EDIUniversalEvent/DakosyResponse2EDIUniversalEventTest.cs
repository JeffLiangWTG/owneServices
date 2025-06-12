using System;
using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.Dakosy.BT.Transforms.DakosyResponse2EDIUniversalEvent;
using CargoWise.eHub.Products.Dakosy.BT.Transforms.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.EDIFACT.Tests
{
  [TestClass]
  public class DakosyResponse2EDIUniversalEventTest
  {
    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestDakosyResponse2EDIUniversalEvent()
    {
      AssertMapping("Test1_error_input.xml", "Test1_error_output.xml");
      AssertMapping("Test2_success_input.xml", "Test2_success_output.xml");
      AssertMapping("Test3_warning_input.xml", "Test3_warning_output.xml");
      AssertMapping("Test5_MRJ_input.xml", "Test5_MRJ_output.xml");
      AssertMapping("Test6_503_998_input.xml", "Test6_503_998_output.xml");
      AssertMapping("Test7_010_998_input.xml", "Test7_010_998_output.xml");
      AssertMapping("Test8_500_998_input.xml", "Test8_500_998_output.xml");
      AssertMapping("Test9_998_input.xml", "Test9_998_output.xml");
      AssertMapping("Test10_MAA_IRA_STU_input.xml", "Test10_MAA_IRA_STU_output.xml");
    }

    public void AssertMapping(string inputFile, string outputFile)
    {
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockDataModelAccessor = MockRepository.GenerateMock<DataModelAccessor>();
      var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
      var mockSubscriptionHelper = MockRepository.GenerateStrictMock<SubscriptionHelper>();

      mockSubscriptionHelper.Expect(x => x.SelectSubscriptions(Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything)).Repeat.Any();
      mockSubscriptionHelper.Expect(x => x.GetSubscribedDateTime()).Return("2015-12-03 11:59:58.088").Repeat.Any();

      mockCodeMapper.Stub(x => x.GetRecipientCode(Arg.Is("DAKOSYHAM"), Arg.Is("DAKOSYHAM"), Arg.Is("Import Dakosy Response"), Arg.Is("Error Event"), Arg.Is("Event Type"), Arg<string>.Is.Anything)).Return("MRJ");

      mockCodeMapper.Stub(x => x.GetRecipientCode("DAKOSYHAM", "DAKOSYHAM", "Import Dakosy Response", "Error Description", "Description", "157")).Return("One of the sent B/Z Numbers (box 165) does not exist or is stopped.");
      mockCodeMapper.Stub(x => x.GetRecipientCode("DAKOSYHAM", "DAKOSYHAM", "Import Dakosy Response", "Error Description", "Short Text", "157")).Return("B/Z Number invalid");

      mockCodeMapper.Stub(x => x.GetRecipientCode("DAKOSYHAM", "DAKOSYHAM", "Import Dakosy Response", "Error Description", "Description", "W40")).Return(String.Empty);
      mockCodeMapper.Stub(x => x.GetRecipientCode("DAKOSYHAM", "DAKOSYHAM", "Import Dakosy Response", "Error Description", "Short Text", "W40")).Return("Different terminals stated in presentation and Gate-In");

      mockCodeMapper.Stub(x => x.GetRecipientCode("DAKOSYHAM", "DAKOSYHAM", "Import Dakosy Response", "Error Description", "Short Text", "W41")).Return("blah....blah....blah");
      mockCodeMapper.Stub(x => x.GetRecipientCode("DAKOSYHAM", "DAKOSYHAM", "Import Dakosy Response", "Error Description", "Description", "W41")).Return("blah....blah....blah....blah....blah");

      mockCodeMapper.Stub(x => x.GetRecipientCode("DAKOSYHAM", "DAKOSYHAM", "Import Dakosy Response", "Confirmation Event", "Event Type", "500", "", "HDS")).Return("IRA");
      mockCodeMapper.Stub(x => x.GetRecipientCode("DAKOSYHAM", "DAKOSYHAM", "Import Dakosy Response", "Confirmation Event", "Description", "500", "", "HDS")).Return("Interchange Receipt Acknlowledged");
      mockCodeMapper.Stub(x => x.GetRecipientCode("DAKOSYHAM", "DAKOSYHAM", "Import Dakosy Response", "Confirmation Event", "EventReference", "500", "", "HDS")).Return("Quay Order Delivery Confirmation|DEP=Dakosy");

      mockCodeMapper.Stub(x => x.GetRecipientCode("DAKOSYHAM", "DAKOSYHAM", "Import Dakosy Response", "Confirmation Event", "Event Type", "500", "", "S01")).Return("MWA");
      mockCodeMapper.Stub(x => x.GetRecipientCode("DAKOSYHAM", "DAKOSYHAM", "Import Dakosy Response", "Confirmation Event", "Description", "500", "", "S01")).Return("Message Withdraw/Cancel Accepted");
      mockCodeMapper.Stub(x => x.GetRecipientCode("DAKOSYHAM", "DAKOSYHAM", "Import Dakosy Response", "Confirmation Event", "EventReference", "500", "", "S01")).Return("Dakosy Port Message Cancellation Accepted");

      mockCodeMapper.Stub(x => x.GetRecipientCode("DAKOSYHAM", "DAKOSYHAM", "Import Dakosy Response", "Confirmation Event", "Event Type", "500", "", "A08")).Return("MAA");
      mockCodeMapper.Stub(x => x.GetRecipientCode("DAKOSYHAM", "DAKOSYHAM", "Import Dakosy Response", "Confirmation Event", "Description", "500", "", "A08")).Return("Message Accepted");
      mockCodeMapper.Stub(x => x.GetRecipientCode("DAKOSYHAM", "DAKOSYHAM", "Import Dakosy Response", "Confirmation Event", "EventReference", "500", "", "A08")).Return("Dakosy Port Message Acknowledged#SZB#|DEP=Dakosy");

      mockCodeMapper.Stub(x => x.GetRecipientCode("DAKOSYHAM", "DAKOSYHAM", "Import Dakosy Response", "Confirmation Event", "Event Type", "500", "", "")).Return("MWA");
      mockCodeMapper.Stub(x => x.GetRecipientCode("DAKOSYHAM", "DAKOSYHAM", "Import Dakosy Response", "Confirmation Event", "Description", "500", "", "")).Return("Message Withdraw/Cancel Accepted");
      mockCodeMapper.Stub(x => x.GetRecipientCode("DAKOSYHAM", "DAKOSYHAM", "Import Dakosy Response", "Confirmation Event", "EventReference", "500", "", "")).Return("Dakosy Port Message Cancellation Accepted");

      mockCodeMapper.Stub(x => x.GetRecipientCode("DAKOSYHAM", "DAKOSYHAM", "Import Dakosy Response", "Confirmation Event", "Event Type", "503", "", "HDS")).Return("MAA");
      mockCodeMapper.Stub(x => x.GetRecipientCode("DAKOSYHAM", "DAKOSYHAM", "Import Dakosy Response", "Confirmation Event", "Description", "503", "", "HDS")).Return("Message Accepted");
      mockCodeMapper.Stub(x => x.GetRecipientCode("DAKOSYHAM", "DAKOSYHAM", "Import Dakosy Response", "Confirmation Event", "EventReference", "503", "", "HDS")).Return("Dakosy Port Message Acknowledged#SZB#|DEP=Dakosy");

      mockCodeMapper.Stub(x => x.GetRecipientCode("DAKOSYHAM", "DAKOSYHAM", "Import Dakosy Response", "Confirmation Event", "Event Type", "503", "NRL", "HDS")).Return("MAA");
      mockCodeMapper.Stub(x => x.GetRecipientCode("DAKOSYHAM", "DAKOSYHAM", "Import Dakosy Response", "Confirmation Event", "Description", "503", "NRL", "HDS")).Return("Message Accepted");
      mockCodeMapper.Stub(x => x.GetRecipientCode("DAKOSYHAM", "DAKOSYHAM", "Import Dakosy Response", "Confirmation Event", "EventReference", "503", "NRL", "HDS")).Return("Dakosy Port Message Acknowledged#SZB#|DEP=Dakosy");

      mockCodeMapper.Stub(x => x.GetRecipientCode("DAKOSYHAM", "DAKOSYHAM", "Import Dakosy Response", "Confirmation Event", "Event Type", "505", "NRL", "HDS")).Return("MAA");
      mockCodeMapper.Stub(x => x.GetRecipientCode("DAKOSYHAM", "DAKOSYHAM", "Import Dakosy Response", "Confirmation Event", "Description", "505", "NRL", "HDS")).Return("Message Accepted");
      mockCodeMapper.Stub(x => x.GetRecipientCode("DAKOSYHAM", "DAKOSYHAM", "Import Dakosy Response", "Confirmation Event", "EventReference", "505", "NRL", "HDS")).Return("Dakosy Port Message Acknowledged|DEP=Dakosy");

      mockCodeMapper.Stub(x => x.GetRecipientCode("DAKOSYHAM", "DAKOSYHAM", "Import Dakosy Response", "Confirmation Event", "Event Type", "505", "RLS", "HDS")).Return("MAA");
      mockCodeMapper.Stub(x => x.GetRecipientCode("DAKOSYHAM", "DAKOSYHAM", "Import Dakosy Response", "Confirmation Event", "Description", "505", "RLS", "HDS")).Return("Message Accepted");
      mockCodeMapper.Stub(x => x.GetRecipientCode("DAKOSYHAM", "DAKOSYHAM", "Import Dakosy Response", "Confirmation Event", "EventReference", "505", "RLS", "HDS")).Return("Dakosy Port Customs Cleared|DEP=Dakosy");

      mockCodeMapper.Stub(x => x.GetRecipientCode("DAKOSYHAM", "DAKOSYHAM", "Import Dakosy Response", "Confirmation Event", "Event Type", "998", "", "")).Return("MPP");
      mockCodeMapper.Stub(x => x.GetRecipientCode("DAKOSYHAM", "DAKOSYHAM", "Import Dakosy Response", "Confirmation Event", "Description", "998", "", "")).Return("Message Pending Processing");
      mockCodeMapper.Stub(x => x.GetRecipientCode("DAKOSYHAM", "DAKOSYHAM", "Import Dakosy Response", "Confirmation Event", "EventReference", "998", "", "")).Return("Dakosy Processing Report|DEP=Dakosy");

      mockCodeMapper.Stub(x => x.GetRecipientCode("DAKOSYHAM", "DAKOSYHAM", "Import Dakosy Response", "Confirmation Event", "Event Type", "010", "W40", "")).Return("IRA");
      mockCodeMapper.Stub(x => x.GetRecipientCode("DAKOSYHAM", "DAKOSYHAM", "Import Dakosy Response", "Confirmation Event", "Description", "010", "W40", "")).Return("W40 Description");
      mockCodeMapper.Stub(x => x.GetRecipientCode("DAKOSYHAM", "DAKOSYHAM", "Import Dakosy Response", "Confirmation Event", "EventReference", "010", "W40", "")).Return("W40 Event Reference");

      mockCodeMapper.Stub(x => x.GetRecipientCode("DAKOSYHAM", "DAKOSYHAM", "Import Dakosy Response", "Confirmation Event", "Event Type", "010", "W41", "")).Return("XXXXX");
      mockCodeMapper.Stub(x => x.GetRecipientCode("DAKOSYHAM", "DAKOSYHAM", "Import Dakosy Response", "Confirmation Event", "Description", "010", "W41", "")).Return("W41 Description");
      mockCodeMapper.Stub(x => x.GetRecipientCode("DAKOSYHAM", "DAKOSYHAM", "Import Dakosy Response", "Confirmation Event", "EventReference", "010", "W41", "")).Return("W41 Event Reference");

      mockCodeMapper.Stub(x => x.GetRecipientCode("DAKOSYHAM", "DAKOSYHAM", "Import Dakosy Response", "Confirmation Event", "Event Type", "010", "", "")).Return("IRA");
      mockCodeMapper.Stub(x => x.GetRecipientCode("DAKOSYHAM", "DAKOSYHAM", "Import Dakosy Response", "Confirmation Event", "Description", "010", "", "")).Return("Interchange Receipt Acknlowledged");
      mockCodeMapper.Stub(x => x.GetRecipientCode("DAKOSYHAM", "DAKOSYHAM", "Import Dakosy Response", "Confirmation Event", "EventReference", "010", "", "")).Return("Dakosy Quay Order Processing Warning|DEP=Dakosy");

      mockCodeMapper.Stub(x => x.GetRecipientCode("DAKOSYHAM", "DAKOSYHAM", "Import Dakosy Response", "Release Status", "Context Information", "RLS")).Return("RELEASED");
      mockCodeMapper.Stub(x => x.GetRecipientCode("DAKOSYHAM", "DAKOSYHAM", "Import Dakosy Response", "Release Status", "Context Information", "NRL")).Return("NOT RELEASED");
      mockCodeMapper.Stub(x => x.GetRecipientCode("DAKOSYHAM", "DAKOSYHAM", "Import Dakosy Response", "Release Status", "Context Information", "")).Return("");

      mockCodeMapper.Stub(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "DAKOSYHAM", "@recipientId", "", "@ST_ID", "DAKREF", "@value", "SNAT_0000000213")).Return("S12SHAM0000263S");
      mockCodeMapper.Stub(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "DAKOSYHAM", "@recipientId", "", "@ST_ID", "DAKREF", "@value", "SNAT_0000000206")).Return("S12SHAM0000263S");
      mockCodeMapper.Stub(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "DAKOSYHAM", "@recipientId", "", "@ST_ID", "DAKREF", "@value", "SNAT_0000000382")).Return("S12SSEE0000310S");
      mockCodeMapper.Stub(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "DAKOSYHAM", "@recipientId", "", "@ST_ID", "DAKREF", "@value", "JASN_S004398357S")).Return("S004398357S");
      mockCodeMapper.Stub(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "DAKOSYHAM", "@recipientId", "", "@ST_ID", "DAKREF", "@value", "JASN_S004385360S")).Return("S004385360S");
      mockCodeMapper.Stub(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "DAKOSYHAM", "@recipientId", "", "@ST_ID", "DAKREF", "@value", "JASN_0000292443")).Return("S004398357S");
      mockCodeMapper.Stub(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "DAKOSYHAM", "@recipientId", "", "@ST_ID", "DAKREF", "@value", "JASN_0000292444")).Return("S004385360S");

      mockCodeMapper.Stub(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "DAKOSYHAM", "@recipientId", "", "@ST_ID", "DAKREF", "@value", "SNAT_0000000213", "@referenceType", "MessagePurpose")).Return("HDS");
      mockCodeMapper.Stub(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "DAKOSYHAM", "@recipientId", "", "@ST_ID", "DAKREF", "@value", "SNAT_0000000206", "@referenceType", "MessagePurpose")).Return("HDS");
      mockCodeMapper.Stub(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "DAKOSYHAM", "@recipientId", "", "@ST_ID", "DAKREF", "@value", "SNAT_0000000382", "@referenceType", "MessagePurpose")).Return("HDS");
      mockCodeMapper.Stub(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "DAKOSYHAM", "@recipientId", "", "@ST_ID", "DAKREF", "@value", "JASN_S004398357S", "@referenceType", "MessagePurpose")).Return("HDS");
      mockCodeMapper.Stub(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "DAKOSYHAM", "@recipientId", "", "@ST_ID", "DAKREF", "@value", "JASN_S004385360S", "@referenceType", "MessagePurpose")).Return("HDS");
      mockCodeMapper.Stub(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "DAKOSYHAM", "@recipientId", "", "@ST_ID", "DAKREF", "@value", "JASN_0000292443", "@referenceType", "MessagePurpose")).Return("HDS");
      mockCodeMapper.Stub(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "DAKOSYHAM", "@recipientId", "", "@ST_ID", "DAKREF", "@value", "JASN_0000292444", "@referenceType", "MessagePurpose")).Return("HDS");
      mockCodeMapper.Stub(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "DAKOSYHAM", "@recipientId", "", "@ST_ID", "DAKREF", "@value", "SNAT_0000013493", "@referenceType", "MessagePurpose")).Return("HDS");
      mockCodeMapper.Stub(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "DAKOSYHAM", "@recipientId", "", "@ST_ID", "DAKREF", "@value", "SNAT_0000000346", "@referenceType", "MessagePurpose")).Return("HDS");
      mockCodeMapper.Stub(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "DAKOSYHAM", "@recipientId", "", "@ST_ID", "DAKREF", "@value", "SNAT_0000000367", "@referenceType", "MessagePurpose")).Return("HDS");

      mockContextAccessor.Stub(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("SENDERID");

      mockDateMapper.Stub(x => x.CurrentDateTimeUTC("O")).Return("2015-12-03T05:48:58.6464725Z");

      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("DAKSZB", "DAKOSYHAM", "SENDERID", "Z12000007639", "S12SHAM0000625S")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("DAKSZB", "DAKOSYHAM", "SENDERID", "B13000384900", "S13SZRH0005026S")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("DAKSZB", "DAKOSYHAM", "SENDERID", "Z12009787617", "S12SMHG0000147S")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("DAKSZB", "DAKOSYHAM", "SENDERID", "Z12009803507", "S12SSEE0000310S")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("DAKSZB", "DAKOSYHAM", "SENDERID", "Z12009791807", "S12SFMO0000234S")).Repeat.Any();

      var extensionObjects = new Dictionary<string, object>() {
                { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockCodeMapper },
                { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockContextAccessor },
                { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockDataModelAccessor },
                { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS3", mockDateMapper },
                { "http://schemas.microsoft.com/BizTalk/2003/SubscriptionHelper", mockSubscriptionHelper }
            };
      MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);

      string sourceFile = "DakosyResponse2EDIUniversalEvent.TestFiles." + inputFile;
      string expectedFile = "DakosyResponse2EDIUniversalEvent.TestFiles." + outputFile;
      mapTester.Execute<DakosyResponse2EDIUniversalEvent>(sourceFile, expectedFile);
    }

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestDakosyResponse2EDIUniversalEvent_ParseRecipientFailed_ThrowException()
    {
      var sourceFile = "DakosyResponse2EDIUniversalEvent.TestFiles.Test2_success_input.xml";
      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      mockContextAccessor.Stub(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("");
      var extensionObjects = new Dictionary<string, object>() {
                { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockContextAccessor }
            };
      MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);

      mapTester.ExecuteAssertException<DakosyResponse2EDIUniversalEvent>(sourceFile, "Unable to resolve recipientId");
    }
  }
}
