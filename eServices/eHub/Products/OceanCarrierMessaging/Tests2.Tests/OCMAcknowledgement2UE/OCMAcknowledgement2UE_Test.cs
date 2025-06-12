using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Helper;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.OCMAcknowledgement2UE;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using System.Collections.Generic;
using System.Reflection;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests
{
  [TestClass]
  public class OCMAcknowledgement2UE_Test
  {
    const string filePath = "OCMAcknowledgement2UE.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestOCMAcknowledgement2UE()
    {
      AssertMapping("Test01_AcknowledgementMessageType_BookingRequest_input.xml", "Test01_AcknowledgementMessageType_BookingRequest_output.xml");
      AssertMapping("Test02_AcknowledgementMessageType_ShippingInstruction_input.xml", "Test02_AcknowledgementMessageType_ShippingInstruction_output.xml");
      AssertMapping("Test03_AcknowledgementMessageType_VerifiedGrossMass_input.xml", "Test03_AcknowledgementMessageType_VerifiedGrossMass_output.xml");

      AssertMapping("Test04_AcknowledgementStatus_Accepted_input.xml", "Test04_AcknowledgementStatus_Accepted_output.xml");
      AssertMapping("Test05_AcknowledgementStatus_Acknowledged_input.xml", "Test05_AcknowledgementStatus_Acknowledged_output.xml");
      AssertMapping("Test06_AcknowledgementStatus_Rejected_input.xml", "Test06_AcknowledgementStatus_Rejected_output.xml");

      AssertMapping("Test07_RemarkCollection_ServiceTypeIsNotVOCC_input.xml", "Test07_RemarkCollection_ServiceTypeIsNotVOCC_output.xml", "CLD");
      AssertMapping("Test08_RemarkCollection_ServiceTypeIsVOCC_input.xml", "Test08_RemarkCollection_ServiceTypeIsVOCC_output.xml");
    }

    void AssertMapping(string inputFile, string expectedOutputFile, string subscribeShipmentType = "AGT")
    {
      var input = filePath + inputFile;
      var expectedOutput = filePath + expectedOutputFile;

      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
      var mockSubscriptionHelper = MockRepository.GenerateStrictMock<SubscriptionHelper>();

      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("SENDER").Repeat.Any();
      mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("CARGOWISE").Repeat.Any();

      mockSubscriptionHelper.Expect(x => x.GetOCMBEClientID("SENDER", "CARGOWISE")).Return("OCM_BookingEngine").Repeat.Any();
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", "OCM_BookingEngine")).Repeat.AtLeastOnce();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "SENDER", "@recipientId", "", "@ST_ID", "ODSMSG", "@value", "MFH73483748393", "@referenceType", "OCMBR")).Return("C03078216");
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "SENDER", "@recipientId", "", "@ST_ID", "ODSMSG", "@value", "MFH73483748393", "@referenceType", "DocumentName")).Return("ForwardingConsol");
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "SENDER", "@recipientId", "", "@ST_ID", "ODSMSG", "@value", "MFH73483748393", "@referenceType", "ShipmentType")).Return(subscribeShipmentType);
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "SENDER", "@recipientId", "", "@ST_ID", "ODSMSG", "@value", "MFH73483748393", "@referenceType", "ForwardingType")).Return("ForwardingType");

      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "NVOCC", "Is Co-load", "CLD")).Return("TRUE").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "NVOCC", "Is Co-load", "AGT")).Return("FALSE").Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "MSGID", "SENDER")).Return("ODSMSG");

      var extensionObjects = new Dictionary<string, object>()
      {
        {"http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper},
        {"http://schemas.microsoft.com/BizTalk/2003/DateMapper", mockDateMapper},
        {"http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor},
        {"http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor", mockDataModelAccessor},
        { "http://schemas.microsoft.com/BizTalk/2003/SubscriptionHelper", mockSubscriptionHelper }
      };

      var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.ExecuteCompiled<OCMAcknowledgement2UE>(input, expectedOutput);

      mockDataModelAccessor.VerifyAllExpectations();
      mockCodeMapper.VerifyAllExpectations();
      mockContextAccessor.VerifyAllExpectations();
    }
  }
}

