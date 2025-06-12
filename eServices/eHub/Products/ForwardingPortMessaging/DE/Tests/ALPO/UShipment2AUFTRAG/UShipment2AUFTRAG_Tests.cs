using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.ForwardingPortMessaging.DE.ALPO;
using CargoWise.eHub.Products.ForwardingPortMessaging.DE.Schemas.ALPO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.ForwardingPortMessaging.DE.Tests
{
  [TestClass]
  public class UShipment2AUFTRAG_Tests
  {
    const string filePath = "ALPO.UShipment2AUFTRAG.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestUShipment2AUFTRAG()
    {
      AssertMapping("Test1_input.xml", "Test1_output.xml", "C00001280", "ORG", "DEHAM", "HAM", "EXP", "20200627122959");
      AssertMapping("Test2_input.xml", "Test2_output.xml", "CBR2DDEO02S100690267", "ORG", "DEBRV", "HAM", "IMP", "20200627122959");
      AssertMapping("Test3_input.xml", "Test3_output.xml", "C00001280", "ORG", "DEHAM", "HAM", "EXP", "20200627122959");
      AssertMapping("Test4_input_subshipments.xml", "Test4_output_subshipments.xml", "C00699607", "ORG", "DEBRV", "BR0", "EXP", "20200627122959");
      AssertMapping("Test5_input_subshipments_Import_And_ExportReferenceNumber.xml", "Test5_output_subshipments_Import_And_ExportReferenceNumber.xml", "C00001231", "ORG", "DEBRV", "HAM", "EXP", "20230420135201");
      AssertMapping("Test6_input_subshipments_Import.xml", "Test6_output_subshipments_Import.xml", "C00001231", "ORG", "DEBRV", "HAM", "IMP", "20230420135201");
      AssertMapping("Test7_input_subshipments_Export.xml", "Test7_output_subshipments_Export.xml", "C00001231", "ORG", "DEBRV", "HAM", "EXP", "20230420135201");
      AssertMapping("Test8_input_subshipments_Export.xml", "Test8_output_subshipments_Export.xml", "C00001231", "ORG", "DEBRV", "HAM", "EXP", "20230420135201");
      AssertMapping("Test9_input.xml", "Test9_output.xml", "C00699607", "ORG", "DEBRV", "BR0", "EXP", "20200627122959");
      AssertMapping("Test10_input.xml", "Test10_output.xml", "C00699607", "ORG", "DEBRV", "BR0", "EXP", "20200627122959");
    }

    void AssertMapping(string inputFile, string expectedOutputFile, string consolID, string purpose, string operationalPortCode, string eventBranch, string dirShort, string triggerDate)
    {
      var input = filePath + inputFile;
      var expectedOutput = filePath + expectedOutputFile;

      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
      var mockStringMapper = MockRepository.GenerateStrictMock<StringMapper>();
      var mockUnitConvertorMapper = MockRepository.GenerateStrictMock<UnitConverter>();

      var serviceProvider = "ALPO";
      var serviceProviderMSGID = serviceProvider.Substring(0, 3) + "MSG";
      var serviceProviderID = serviceProvider.Substring(0, 3) + "ID";
      var msgPrefix = serviceProvider.Substring(0, 3);

      var senderID = "TESTSENDER";
      var recipientID = serviceProvider + "_AUFTRAG";

      var interchangeID = "1";
      var formattedMessageID = msgPrefix + "000000000" + interchangeID;
      var inboxPK = "TrackingID_0000001";

      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(senderID);
      mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(recipientID);

      mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Port Settings", "Name", recipientID)).Return(serviceProvider).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Port Settings", "MSGID", recipientID)).Return(serviceProviderMSGID);
      mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Port Settings", "ID", recipientID)).Return(serviceProviderID);

      mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Port Settings", "SubscriptionPrefix", recipientID)).Return(msgPrefix);

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.ForwardingPortMessaging.DE.ALPO.Interchange", "@maxlength", "14")).Return(interchangeID);

      mockContextAccessor.Expect(x => x.GetContextProperty("InternalTrackingID", "http://cargowise.com/ehub/tracking/2010/06")).Return(inboxPK);
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, senderID, inboxPK, interchangeID));

      mockDataModelAccessor.Expect(x => x.GetClientRegistrationCode(senderID, eventBranch, serviceProvider)).Return("ALPO001");

      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, senderID, formattedMessageID, consolID, "JobNumber"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, senderID, consolID, formattedMessageID, "JobNumber"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, senderID, formattedMessageID, "ForwardingConsol", "ForwardingType"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, senderID, formattedMessageID, purpose, "Purpose"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, senderID, formattedMessageID, "Port Order (ALPO)", "DocumentName"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, senderID, formattedMessageID, operationalPortCode, "OperationPort"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderID, serviceProvider, senderID, "ALPO001"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, senderID, formattedMessageID + "-0001", "MSCU1245787", "ContainerNumber")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, senderID, formattedMessageID + "-0002", "MSCU1247856", "ContainerNumber")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, senderID, formattedMessageID + "-0003", "MSCU8757656", "ContainerNumber")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, senderID, formattedMessageID + "-0001", "BGHE6537539", "ContainerNumber")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, senderID, formattedMessageID + "-0001", "WTGU2303246", "ContainerNumber")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, senderID, formattedMessageID + "-0002", "WTGU2303251", "ContainerNumber")).Repeat.Any();

      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, senderID, formattedMessageID + "-0001", "TBNN1111111", "ContainerNumber")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, senderID, formattedMessageID + "-0002", "TBNN2222222", "ContainerNumber")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, senderID, formattedMessageID + "-0003", "TBNN3333333", "ContainerNumber")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, senderID, formattedMessageID + "-0004", "TBNN4444444", "ContainerNumber")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, senderID, formattedMessageID + "-0005", "TBNN5555555", "ContainerNumber")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, senderID, formattedMessageID + "-0006", "TBNN6666666", "ContainerNumber")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, senderID, formattedMessageID + "-0007", "TBNN7777777", "ContainerNumber")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, senderID, formattedMessageID + "-0008", "TBNN8888888", "ContainerNumber")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, senderID, formattedMessageID + "-0009", "TBNN9999999", "ContainerNumber")).Repeat.Any();

      mockContextAccessor.Expect(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "ORDER_" + dirShort + "_ALPO_ALPO001_" + consolID + "_" + senderID + "_" + triggerDate));

      mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Service Provider Settings", "RecipientID", serviceProvider, operationalPortCode, "")).Return(serviceProvider + "_Recipient_ID");

      mockCodeMapper.Expect(x => x.GetRecipientCode(serviceProvider, serviceProvider, serviceProvider + " System Configuration", "Package Type ISO", serviceProvider + " Code", "")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode(serviceProvider, serviceProvider, serviceProvider + " System Configuration", "Package Type ISO", serviceProvider + " Code", "BAG")).Return("BAG").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode(serviceProvider, serviceProvider, serviceProvider + " System Configuration", "Package Type ISO", serviceProvider + " Code", "PLT")).Return("PLT").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode(serviceProvider, serviceProvider, serviceProvider + " System Configuration", "Package Type ISO", serviceProvider + " Code", "PKG")).Return("PKG").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Package Type ISO", "Output Code", "")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Package Type ISO", "Output Code", "PLT")).Return("PLT").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Package Type ISO", "Output Code", "BAG")).Return("BAG").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Package Type ISO", "Output Code", "PKG")).Return("PKG").Repeat.Any();

      var extensionObjects = new Dictionary<string, object>()
      {
        { "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
        { "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor", mockDataModelAccessor },
        { "http://schemas.microsoft.com/BizTalk/2003/StringMapper", mockStringMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/UnitConverter", mockUnitConvertorMapper },
      };

      var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.ExecuteCompiled<UShipment2AUFTRAG>(input, expectedOutput);

      mockContextAccessor.VerifyAllExpectations();
      mockCodeMapper.VerifyAllExpectations();
      mockDataModelAccessor.VerifyAllExpectations();
      mockStringMapper.VerifyAllExpectations();
      mockUnitConvertorMapper.VerifyAllExpectations();

      var report = XMLValidator.Validate<ALPO_OrderIn_extern_V1_37>(expectedOutput);
      Assert.AreEqual(string.Empty, report, typeof(ALPO_OrderIn_extern_V1_37).Name + " is not valid: \r\n " + report);
    }
  }
}
