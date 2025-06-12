using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.CarrierEmail;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using System;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests
{
  [TestClass]
  public class CarrierEmail_CarrierUniversal2VERMAS_Tests
  {
    const string filePath = "CarrierEmail.CarrierUniversal2VERMAS.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestCarrierUniversal2VERMAS()
    {
      AssertMapping("Test1_input.xml", "Test1_output.xml");
      AssertMapping("Test2_input.xml", "Test2_output.xml", emailAddress: "ShippingLineAddress@cargowise.com", useHQEMail: "N");
      AssertMapping("Test3_input.xml", "Test3_output.xml", ediAttachment: "N");
      AssertMapping("Test4_input.xml", "Test4_output.xml");
    }

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestCarrierUniversal2VERMAS_Exception()
    {
      try
      {
        AssertMapping("Test1_input.xml", "Test1_output.xml", "");
        Assert.Fail("Expect Exception should throw.");
      }
      catch (Exception ex)
      {
        var actualException = ex.InnerException ?? ex;
        Assert.AreEqual("Unable to find matching email for [Port:DKCPH], [Carrier:INTT]", actualException.Message);
      }
    }

    void AssertMapping(string inputFile, string expectedOutputFile, string emailAddress = "email@email.com", string useHQEMail = "Y", string ediAttachment = "Y", string scac = "INTT", string partySenderIdentifier = "CW1", string destinationPartyReceiverIdentifier = "YANGMING")
    {
      var input = filePath + inputFile;
      var expectedOutput = filePath + expectedOutputFile;

      var recipientID = "CARRIER_EMAIL_VM";

      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
      var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();

      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("TESTSENDER__1");
      mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(recipientID);
      mockContextAccessor.Expect(x => x.GetContextProperty("InternalTrackingID", "http://cargowise.com/ehub/tracking/2010/06")).Return("36ACCF3C-893F-40CE-BA01-1D8CBE2DF678");
      mockContextAccessor.Expect(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "VERMAS_CGWS_1"));
      mockContextAccessor.Expect(x => x.SetContextProperty("OverrideEmailSubject", "http://cargowise.com/ehub/processing/2010/06", emailAddress));
      mockContextAccessor.Expect(x => x.SetContextProperty("EarlyTerminateEdifactUnb", "http://cargowise.com/ehub/processing/2010/06", "true"));
      mockContextAccessor.Expect(x => x.SetContextProperty("OverrideEDIHeader", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "true"));

      mockContextAccessor.Expect(x => x.SetContextProperty("UNB5", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "1"));
      mockContextAccessor.Expect(x => x.SetContextProperty("UNH1", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "1"));
      mockContextAccessor.Expect(x => x.SetContextProperty("UNB1_1", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "UNOC"));
      mockContextAccessor.Expect(x => x.SetContextProperty("UNB1_2", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "3"));
      mockContextAccessor.Expect(x => x.SetContextProperty("UNB9", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", ""));
      mockContextAccessor.Expect(x => x.SetContextProperty("UNB11", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", ""));

      mockDateMapper.Expect(x => x.CurrentDateTime("dd-MMM-yyyy HH:mm")).Return("04-Jul-2014 05:43").Repeat.Any();

      mockDataModelAccessor.Expect(x => x.GetClientRegistrationCode("TESTSENDER__1", "BN1", "CARRIER_EMAIL")).Return("CGWS").Repeat.Any();

      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("EMLID", "CARRIER_EMAIL", "TESTSENDER__1", "CGWS"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("EMLMSG", "CARRIER_EMAIL", "TESTSENDER__1", "1", "C00678281"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("EMLMSG", "CARRIER_EMAIL", "TESTSENDER__1", "C00678281", "", "ShipmentType"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("EMLMSG", "CARRIER_EMAIL", "TESTSENDER__1", "C00678281", "AGT", "ShipmentType"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("EMLMSG", "CARRIER_EMAIL", "TESTSENDER__1", "C00678281", "AMD", "ActionPurpose"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("EMLMSG", "CARRIER_EMAIL", "TESTSENDER__1", "C00678281", "ORG", "ActionPurpose"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("EMLMSG", "CARRIER_EMAIL", "TESTSENDER__1", "C00678281", "WTH", "ActionPurpose"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("EMLMSG", "CARRIER_EMAIL", "TESTSENDER__1", "C00678281", "", "DocumentName"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("EMLMSG", "CARRIER_EMAIL", "TESTSENDER__1", "C00678281", "Verified Gross Container Weight", "DocumentName"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("EMLMSG", "CARRIER_EMAIL", "TESTSENDER__1", "C00678281", "ForwardingConsol", "ForwardingType"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("EMLMSG", "CARRIER_EMAIL", "TESTSENDER__1", "C00678281", "1.0.0", "FormVersion"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("EMLMSG", "CARRIER_EMAIL", "TESTSENDER__1", "36ACCF3C-893F-40CE-BA01-1D8CBE2DF678", "1"));

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.CARRIER_EMAIL.UNH1", "@maxlength", "14")).Return("1").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.CARRIER_EMAIL.BGM", "@maxlength", "14")).Return("1").Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", recipientID, "COLA")).Return("COLO").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", recipientID, "INTA")).Return("INTT").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", recipientID, "SKII")).Return("INTT").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Seal Party Type", "Carrier Code", recipientID, "CAR")).Return("CA").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "UNB3", "PartyReceiverID", recipientID, scac)).Return(destinationPartyReceiverIdentifier).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "UNB3", "PartyReceiverID", recipientID, "")).Return(destinationPartyReceiverIdentifier).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "UNB3", "PartySenderID", recipientID, scac)).Return(partySenderIdentifier).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "UNB3", "PartySenderID", recipientID, "")).Return(partySenderIdentifier).Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("CARRIER_EMAIL", "CARRIER_EMAIL", "CARRIER_EMAIL Provider Configuration", "Email_Lookup (VGM)", "HQ Email Address", "DKCPH", "INTT")).Return(emailAddress).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("CARRIER_EMAIL", "CARRIER_EMAIL", "CARRIER_EMAIL Provider Configuration", "Email_Lookup (VGM)", "Use HQ Email", "DKCPH", "INTT")).Return(useHQEMail).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("CARRIER_EMAIL", "CARRIER_EMAIL", "CARRIER_EMAIL Provider Configuration", "Email_Lookup (VGM)", "EDI Attachment", "DKCPH", "INTT")).Return(ediAttachment).Repeat.Any();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "CARRIER_EMAIL", "@recipientId", "TESTSENDER__1", "@ST_ID", "EMLMSG", "@value", "C00678281", "@referenceType", "JobNumber")).Return("");
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "CARRIER_EMAIL", "@recipientId", "TESTSENDER__1", "@ST_ID", "EMLMSG", "@value", "C00678281")).Return("C00678281");

      var extensionObjects = new Dictionary<string, object>
      {
        { "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/DateMapper", mockDateMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
        { "http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor", mockDataModelAccessor }
      };

      MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.Execute<CarrierUniversal2VERMAS>(input, expectedOutput);

      mockDateMapper.VerifyAllExpectations();
      mockCodeMapper.VerifyAllExpectations();
      mockContextAccessor.VerifyAllExpectations();
    }
  }
}