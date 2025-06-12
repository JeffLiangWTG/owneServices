using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Products.ForwardingPortMessaging.BE.CPOINT;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using CargoWise.eHub.Core.Transforms.Helper;
using System.Collections.Generic;

namespace CargoWise.eHub.Products.ForwardingPortMessaging.BE.Tests
{
  [TestClass]
  public class UShipment2IFTDGN_Tests
  {
    const string filePath = "CPOINT.UShipment2IFTDGN.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestUShipment2IFTDGN()
    {
      AssertMapping("Test1_input.xml", "Test1_output.xml", "Notification of Dangerous Goods - Import", "PSNPSN000001", "C00001015", "ORG", "BEANR", "SHA");
      AssertMapping("Test2_input_UDM_Amendment_Export_IFTDGN.xml", "Test2_output.xml", "Dangerous Goods Notification (BE) - Export", "YM2000", "C00001282", "AMD", "BEANR", "ASY");
      AssertMapping("Test3_input_UDM_WithDrawal_Export_IFTDGN.xml", "Test3_output.xml", "Dangerous Goods Notification (BE) - Export", "YM2000", "C00001282", "WTH", "BEANR", "ASY");
    }

    void AssertMapping(string inputFile, string expectedOutputFile, string documentName, string registrationPSN, string consolID, string purpose, string operationPortCode, string eventBranch)
    {
      var input = filePath + inputFile;
      var expectedOutput = filePath + expectedOutputFile;

      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();

      var serviceProvider = "CPOINT";
      var serviceProviderPrefix = serviceProvider.Substring(0, 3);
      var serviceProviderMSGID = serviceProviderPrefix + "MSGID";
      var serviceProviderID = serviceProviderPrefix + "ID";

      var senderID = "TESTSENDER";
      var recipientID = serviceProvider + "_IFTDGN";

      var interchangeID = "1";
      var InboxPK = "TrackingID_0000001";

      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(senderID);
      mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(recipientID);

      mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Port Settings", "Name", recipientID)).Return(serviceProvider).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Port Settings", "MSGID", recipientID)).Return(serviceProviderMSGID);
      mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Port Settings", "ID", recipientID)).Return(serviceProviderID);
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.ForwardingPortMessaging.BE.CPOINT.Interchange", "@maxlength", "50")).Return(interchangeID);
      mockCodeMapper.Expect(x => x.GetRecipientCode("CPOINT", "CPOINT", "CPOINT System Configuration", "DG Regulation", "Output Code", "IMO")).Return("IMD").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("CPOINT", "CPOINT", "CPOINT System Configuration", "DG Regulation", "Output Code", "ADN")).Return("ADN").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("CPOINT", "CPOINT", "CPOINT System Configuration", "Package Type", "CPOINT Code", "PLT")).Return("PLT11").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("CPOINT", "CPOINT", "CPOINT System Configuration", "Package Type", "CPOINT Code", "BOX")).Return("BOX11").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("CPOINT", "CPOINT", "CPOINT System Configuration", "Package Type", "CPOINT Code", "BAG")).Return("BAG11").Repeat.Any();

      mockContextAccessor.Expect(x => x.GetContextProperty("InternalTrackingID", "http://cargowise.com/ehub/tracking/2010/06")).Return(InboxPK);
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, senderID, InboxPK, interchangeID));

      mockContextAccessor.Expect(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "IFTDGN_" + senderID + "_" + interchangeID));

      var messageIdentifier = registrationPSN.Substring(0, 6) + "000000001" + "01";
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, senderID, messageIdentifier, consolID, "IFTDGN"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, senderID, consolID, messageIdentifier, "IFTDGN"));

      mockContextAccessor.Expect(x => x.SetContextProperty("OverrideEmailSubject", "http://cargowise.com/ehub/processing/2010/06", "FPM_" + serviceProvider + "_" + operationPortCode));
      mockDataModelAccessor.Expect(x => x.GetClientRegistrationCode(senderID, eventBranch, serviceProvider)).Return("CPOINT001");

      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, senderID, messageIdentifier, "ForwardingConsol", "ForwardingType"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, senderID, messageIdentifier, purpose, "Purpose"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, senderID, messageIdentifier, documentName, "DocumentName"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, senderID, messageIdentifier, operationPortCode, "OperationPort"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderID, serviceProvider, senderID, "CPOINT001"));

      var extensionObjects = new Dictionary<string, object>()
      {
        { "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
        { "http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor", mockDataModelAccessor },
        { "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
      };

      var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.ExecuteCompiled<UShipment2IFTDGN>(input, expectedOutput);

      mockContextAccessor.VerifyAllExpectations();
      mockDataModelAccessor.VerifyAllExpectations();
      mockCodeMapper.VerifyAllExpectations();
    }
  }
}