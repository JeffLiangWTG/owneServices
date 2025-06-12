using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.ForwardingPortMessaging.NL.Portbase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.ForwardingPortMessaging.NL.Tests
{
  [TestClass]
  public class UShipment2M114_Tests
  {
    const string filePath = "Portbase.UShipment2M114.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestUShipment2M114()
    {
      AssertMapping("Test1_input_ContainerTerminal_ORG.xml", "Test1_output_ContainerTerminal_ORG.xml", "ORG", false, "");
      AssertMapping("Test2_input_ContainerTerminal_AMD.xml", "Test2_output_ContainerTerminal_AMD.xml", "AMD", false);
      AssertMapping("Test3_input_ContainerTerminal_WTH.xml", "Test3_output_ContainerTerminal_WTH.xml", "WTH", false);
      AssertMapping("Test4_input_FerryTerminal_ORG.xml", "Test4_output_FerryTerminal_ORG.xml", "ORG", true, "");
      AssertMapping("Test5_input_FerryTerminal_AMD.xml", "Test5_output_FerryTerminal_AMD.xml", "AMD", true);
      AssertMapping("Test6_input_FerryTerminal_WTH.xml", "Test6_output_FerryTerminal_WTH.xml", "WTH", true);
    }

    void AssertMapping(string sourceFile, string expectedFile, string purpose, bool isFerryTerminal, string previousJobNumber = "PBS0000000088")
    {
      var input = filePath + sourceFile;
      var expectedOutput = filePath + expectedFile;

      var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();

      var sender = "TESTSENDER";
      var serviceProvider = "PORTBASE";
      var destinationParty = serviceProvider + "_M2400";
      var serviceProviderMSGID = serviceProvider.Substring(0, 3) + "MSG";
      var serviceProviderID = serviceProvider.Substring(0, 3) + "ID";
      var serviceProviderPrefix = "PBS";
      var interchangeNumber = "99";
      var formattedInterchangeNumber = serviceProviderPrefix + "0000000099" + "E";

      mockDateMapper.Stub(x => x.CurrentDateTimeUTC(Arg.Is("yyyy-MM-ddTHH:mm:ss"))).Return("2021-07-30T11:22:33");

      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(sender);
      mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(destinationParty);
      mockContextAccessor.Expect(x => x.GetContextProperty("InternalTrackingID", "http://cargowise.com/ehub/tracking/2010/06")).Return("TrackingID_0000001");
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, sender, "TrackingID_0000001", interchangeNumber));
      mockContextAccessor.Expect(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "M114_TESTSENDER_99"));

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.ForwardingPortMessaging.Portbase.MRN", "@maxlength", "14")).Return(interchangeNumber).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Port Settings", "Name", destinationParty)).Return(serviceProvider).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Port Settings", "MSGID", destinationParty)).Return(serviceProviderMSGID);
      mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Port Settings", "ID", destinationParty)).Return(serviceProviderID);
      mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Port Settings", "SubscriptionPrefix", destinationParty)).Return("PBS");
      mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Service Provider Settings", "RecipientID", serviceProvider, "NLRTM")).Return(serviceProvider + "_Recipient_ID");

      mockDataModelAccessor.Expect(x => x.GetClientRegistrationCode(sender, "AMS", serviceProvider)).Return("PORTBASE001");

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", serviceProvider, "@recipientId", sender, "@ST_ID", serviceProviderMSGID, "@value", "C00679603_17DE345355864324E8")).Return(previousJobNumber).Repeat.Any();

      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, sender, formattedInterchangeNumber, "C00679603_17DE345355864324E8"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, sender, "C00679603_17DE345355864324E8", formattedInterchangeNumber));

      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, sender, formattedInterchangeNumber, "ForwardingConsol", "ForwardingType"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, sender, formattedInterchangeNumber, purpose, "Purpose"));

      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, sender, formattedInterchangeNumber, "Export Notification", "DocumentName"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, sender, formattedInterchangeNumber, "NLRTM", "OperationPort"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderID, serviceProvider, sender, "PORTBASE001"));


      if (isFerryTerminal)
      {
        mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, sender, formattedInterchangeNumber, "C00679603", "ShipmentId"));
      }
      else
      {
        mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, sender, "XYZ1234567817DE345355864324E8", "C00679603", "EquipmentID"));
        mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, sender, "ABC1234567817DE345355864324E8", "C00679603", "EquipmentID"));
      }

      var extensionObjects = new Dictionary<string, object>()
      {
        { "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
        { "http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor", mockDataModelAccessor },
        { "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/DateMapper", mockDateMapper }
      };

      MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.Execute<UShipment2M114>(input, expectedOutput);

      mockContextAccessor.VerifyAllExpectations();
      mockDataModelAccessor.VerifyAllExpectations();
    }
  }
}