using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.ForwardingPortMessaging.FR.APPLUS;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.ForwardingPortMessaging.FR.Tests
{
  [TestClass]
  public class AMQ2UInterchangeInclude_Test
  {
    const string filePath = "APPLUS.AMQ_NOTIF.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestAMQ2UInterchangeInclude()
    {
      AssertMapping("Test1_export_input.xml", "Test1_export_output.xml", "MGI", "AMQ - Export", "UPDATE");
      AssertMapping("Test2_export_input.xml", "Test2_export_output.xml", "SOGET", "AMQ - Export", "UPDATE");
      AssertMapping("Test3_import_input.xml", "Test3_import_output.xml", "MGI", "AMQ - Import", "CREATE");
      AssertMapping("Test4_import_input.xml", "Test4_import_output.xml", "SOGET", "AMQ - Import", "CREATE");
      AssertMapping("Test5_Base_input.xml", "Test5_Base_output.xml", "SOGET", "AMQ - Export", "CREATE", eHubID: "HYEUATCW1");
      AssertMapping("Test6_Base_input.xml", "Test6_Base_output.xml", "SOGET", "AMQ - Export", "UPDATE", eHubID: "HYEUATCW1");
    }

    void AssertMapping(string inputFile, string expectedOutputFile, string serviceProvider, string expectedDocumentName, string action, string eHubID = "")
    {
      var input = filePath + inputFile;
      var expectedOutput = filePath + expectedOutputFile;

      var serviceProviderPrefix = serviceProvider.Substring(0, 3);
      var serviceProviderID = serviceProviderPrefix + "ID";
      var serviceProviderSIC = serviceProviderPrefix + "SIC";
      var serviceProviderMSGID = serviceProviderPrefix + "MSG";

      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();

      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(serviceProvider).Repeat.AtLeastOnce();
      mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("CW1").Repeat.AtLeastOnce();
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", "HYEUATCW1")).Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "SCAC (Inbound)", "SCAC", serviceProvider, "CMACGMAG")).Return("CMDU").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "SCAC (Inbound)", "SCAC", serviceProvider, "INTTRA")).Return("INTTRA").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Port Settings", "ID", serviceProvider)).Return(serviceProviderID).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Port Settings", "MSGID", serviceProvider)).Return(serviceProviderMSGID).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Port Settings", "Name", serviceProvider)).Return(serviceProvider).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Port Settings", "SubscriptionPrefix", serviceProvider)).Return(serviceProviderPrefix).Repeat.Any();

      var jobType = expectedDocumentName.Contains("Export") ? "Export" : "Import";

      mockCodeMapper.Expect(x => x.GetRecipientCode(serviceProvider, serviceProvider, serviceProvider + " System Configuration", "AMQ Event Type", "Event Type", "BASE", action, jobType)).Return("SCM").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode(serviceProvider, serviceProvider, serviceProvider + " System Configuration", "AMQ Event Type", "Event Parameters", "BASE", action, jobType)).Return("|Department=Customs|CustomsReferenceNumber=.|Location=.").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode(serviceProvider, serviceProvider, serviceProvider + " System Configuration", "AMQ Event Type", "Event Reference", "BASE", action, jobType)).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode(serviceProvider, serviceProvider, serviceProvider + " System Configuration", "AMQ Event Type", "Is Estimate", "BASE", action, jobType)).Return("false").Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode(serviceProvider, serviceProvider, serviceProvider + " System Configuration", "AMQ Event Type", "Event Type", "AMQ", action, jobType)).Return("STU").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode(serviceProvider, serviceProvider, serviceProvider + " System Configuration", "AMQ Event Type", "Event Parameters", "AMQ", action, jobType)).Return("Department=Terminal|Type=AMQ Notification|CustomsReferenceNumber=.|Status=.|Location=.").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode(serviceProvider, serviceProvider, serviceProvider + " System Configuration", "AMQ Event Type", "Event Reference", "AMQ", action, jobType)).Return("Event Ref Blah...Blah...Blah...").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode(serviceProvider, serviceProvider, serviceProvider + " System Configuration", "AMQ Event Type", "Is Estimate", "AMQ", action, jobType)).Return("false").Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode(serviceProvider, serviceProvider, serviceProvider + " System Configuration", "Zone UNLOCO", "Output Code", "DKKO")).Return("DKKO").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode(serviceProvider, serviceProvider, serviceProvider + " System Configuration", "Zone UNLOCO", "Output Code", "MRS")).Return("MRS").Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode(serviceProvider, serviceProvider, serviceProvider + " System Configuration", "AMQ Subscription Lookup", "Subscription Type", "AMQ")).Return("AMQ").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode(serviceProvider, serviceProvider, serviceProvider + " System Configuration", "AMQ Subscription Lookup", "Fallback Type", "AMQ")).Return("CRESA").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode(serviceProvider, serviceProvider, serviceProvider + " System Configuration", "AMQ Subscription Lookup", "Subscription Type", "BASE")).Return("AMQ").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode(serviceProvider, serviceProvider, serviceProvider + " System Configuration", "AMQ Subscription Lookup", "Fallback Type", "BASE")).Return("AMQ").Repeat.Any();

      mockDataModelAccessor.Expect(x => x.GeteHubIDByCode("APPLUSDKE,APPLUSDKE", serviceProvider)).Return(eHubID).Repeat.Any();
      if (string.IsNullOrEmpty(eHubID))
      {
        mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedClients", "", "@senderId", serviceProvider, "@ST_ID", serviceProviderID, "@value", "APPLUSDKE,APPLUSDKE")).Return("HYEUATCW1").Repeat.Once();
      }

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedClients", "", "@senderId", serviceProvider, "@ST_ID", serviceProviderMSGID, "@value", serviceProviderPrefix + "00000011")).Return("HYEUATCW1").Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode(serviceProvider, serviceProvider, serviceProvider + " System Configuration", "ContainerTypeToISOCode", serviceProvider + " Code", "22G0")).Return("22G0").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode(serviceProvider, serviceProvider, serviceProvider + " System Configuration", "ContainerTypeToISOCode", serviceProvider + " Code", "42G0")).Return("42G0").Repeat.Any();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", serviceProvider, "@recipientId", "", "@ST_ID", serviceProviderSIC, "@value", "BASE_ECT01651662")).Return(serviceProviderPrefix + "00000011").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", serviceProvider, "@recipientId", "", "@ST_ID", serviceProviderSIC, "@value", "BASE_ECT01651663")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", serviceProvider, "@recipientId", "", "@ST_ID", serviceProviderSIC, "@value", "AMQ_ECT01651662")).Return(serviceProviderPrefix + "00000011").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", serviceProvider, "@recipientId", "", "@ST_ID", serviceProviderMSGID, "@value", serviceProviderPrefix + "00000011", "@referenceType", "JobNumber")).Return("C03078216").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", serviceProvider, "@recipientId", "", "@ST_ID", serviceProviderMSGID, "@value", serviceProviderPrefix + "00000011", "@referenceType", "ForwardingType")).Return("ForwardingConsol").Repeat.Any();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", serviceProvider, "@recipientId", "", "@ST_ID", serviceProviderSIC, "@value", "AMQ_ECT01651663")).Return(serviceProviderPrefix + "00000012").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", serviceProvider, "@recipientId", "", "@ST_ID", serviceProviderMSGID, "@value", serviceProviderPrefix + "00000012", "@referenceType", "JobNumber")).Return("C03078217").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", serviceProvider, "@recipientId", "", "@ST_ID", serviceProviderMSGID, "@value", serviceProviderPrefix + "00000012", "@referenceType", "ForwardingType")).Return("").Repeat.Any();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", serviceProvider, "@recipientId", "", "@ST_ID", serviceProviderMSGID, "@value", serviceProviderPrefix + "00000011", "@referenceType", "DocumentName")).Return(expectedDocumentName).Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", serviceProvider, "@recipientId", "", "@ST_ID", serviceProviderMSGID, "@value", serviceProviderPrefix + "00000012", "@referenceType", "DocumentName")).Return(expectedDocumentName).Repeat.Any();

      var extensionObjects = new Dictionary<string, object>()
      {
        { "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
        { "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor", mockDataModelAccessor },
      };

      var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.ExecuteCompiled<AMQ2UInterchangeInclude>(input, expectedOutput);

      mockContextAccessor.VerifyAllExpectations();
      mockCodeMapper.VerifyAllExpectations();
      mockDataModelAccessor.VerifyAllExpectations();
    }
  }
}
