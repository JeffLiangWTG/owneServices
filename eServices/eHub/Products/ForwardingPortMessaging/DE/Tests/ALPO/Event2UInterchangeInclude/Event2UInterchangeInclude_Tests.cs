using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.ForwardingPortMessaging.DE.ALPO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.ForwardingPortMessaging.DE.Tests
{
  [TestClass]
  public class Event2UInterchangeInclude_Tests
  {
    const string filePath = "ALPO.Event2UInterchangeInclude.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestEvent2UInterchangeInclude()
    {
      AssertMapping("Test1_input_example_ACKNOWLEDGE.xml", "Test1_output.xml", "MOL.PSN-154A-201504291524.1430336612407");
      AssertMapping("Test2_input_example_REJECTED.xml", "Test2_output.xml", "672096389");
      AssertMapping("Test3_input_example_BHT_AUB.xml", "Test3_output.xml", "MOL.PSN-154A-201504291524.1430336612407");
      AssertMapping("Test4_input_example_BHT_FRZ.xml", "Test4_output.xml", "MOL.PSN-154A-201504291524.1430336612407");
      AssertMapping("Test5_input_example_BHT_TOU.xml", "Test5_output.xml", "MOL.PSN-154A-201504291524.1430336612407");
      AssertMapping("Test6_input_example_BHT_multiple.xml", "Test6_output.xml", "MOL.PSN-154A-201504291524.1430336612407");
      AssertMapping("Test7_input_example_REJECTED.xml", "Test7_output.xml", "672096389");
      AssertMapping("Test8_input_example_REJECTED.xml", "Test8_output.xml", "672096389");
    }
    void AssertMapping(string inputFile, string expectedOutputFile, string reference)
    {
      var input = filePath + inputFile;
      var expectedOutput = filePath + expectedOutputFile;

      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();

      var serviceProvider = "ALPO";
      var serviceProviderMSGID = serviceProvider.Substring(0, 3) + "MSG";

      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(serviceProvider).Repeat.AtLeastOnce();
      mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("CW1").Repeat.AtLeastOnce();

      mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Port Settings", "Name", serviceProvider)).Return(serviceProvider);
      mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Port Settings", "MSGID", serviceProvider)).Return(serviceProviderMSGID);

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", serviceProvider, "@recipientId", "", "@ST_ID", serviceProviderMSGID, "@value", reference, "@referenceType", "DocumentName")).Return("ALPO Order").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", serviceProvider, "@recipientId", "", "@ST_ID", serviceProviderMSGID, "@value", reference, "@referenceType", "JobNumber")).Return("C03078216").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", serviceProvider, "@recipientId", "", "@ST_ID", serviceProviderMSGID, "@value", reference, "@referenceType", "ForwardingType")).Return("ForwardingConsol").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", serviceProvider, "@recipientId", "", "@ST_ID", serviceProviderMSGID, "@value", reference, "@referenceType", "OperationPort")).Return("DEBRV").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", serviceProvider, "@recipientId", "", "@ST_ID", serviceProviderMSGID, "@value", reference + "-", "@referenceType", "ContainerNumber")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", serviceProvider, "@recipientId", "", "@ST_ID", serviceProviderMSGID, "@value", reference + "-0000", "@referenceType", "ContainerNumber")).Return("WTGU2206230").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", serviceProvider, "@recipientId", "", "@ST_ID", serviceProviderMSGID, "@value", reference + "-0001", "@referenceType", "ContainerNumber")).Return("WTGU2206231").Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode(serviceProvider, serviceProvider, "ALPO System Configuration", "Event Type", "EventType", "ACKNOWLEDGE", "")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode(serviceProvider, serviceProvider, "ALPO System Configuration", "Event Type", "EventType", "REJECTED", "")).Return("MRJ").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode(serviceProvider, serviceProvider, "ALPO System Configuration", "Event Type", "EventType", "BHT", "AUB")).Return("MAA").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode(serviceProvider, serviceProvider, "ALPO System Configuration", "Event Type", "EventType", "BHT", "FRZ")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode(serviceProvider, serviceProvider, "ALPO System Configuration", "Event Type", "EventType", "BHT", "TOU")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode(serviceProvider, serviceProvider, "ALPO System Configuration", "Event Type", "EventType", "ZAPP", "MAA")).Return("MAA").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode(serviceProvider, serviceProvider, "ALPO System Configuration", "Event Type", "EventParameters", "ACKNOWLEDGE", "")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode(serviceProvider, serviceProvider, "ALPO System Configuration", "Event Type", "EventParameters", "REJECTED", "")).Return("Department=Terminal|MessageType=.|EquipmentReferenceNumber=.|ReferenceNumber=.|CustomsReferenceNumber=.|Location=.|Reason=.").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode(serviceProvider, serviceProvider, "ALPO System Configuration", "Event Type", "EventParameters", "BHT", "AUB")).Return("Department=Terminal|MessageType=.|EquipmentReferenceNumber=.|ReferenceNumber=.|CustomsReferenceNumber=.|Location=.|Reason=.").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode(serviceProvider, serviceProvider, "ALPO System Configuration", "Event Type", "EventParameters", "BHT", "FRZ")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode(serviceProvider, serviceProvider, "ALPO System Configuration", "Event Type", "EventParameters", "BHT", "TOU")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode(serviceProvider, serviceProvider, "ALPO System Configuration", "Event Type", "EventParameters", "ZAPP", "MAA")).Return("").Repeat.Any();

      var extensionObjects = new Dictionary<string, object>()
      {
        { "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
        { "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
      };

      var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.ExecuteCompiled<Event2UInterchangeInclude>(input, expectedOutput);

      mockContextAccessor.VerifyAllExpectations();
      mockCodeMapper.VerifyAllExpectations();
    }
  }
}
