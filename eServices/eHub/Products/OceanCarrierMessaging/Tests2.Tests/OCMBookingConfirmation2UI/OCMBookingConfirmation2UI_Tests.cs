using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.OCMBookingConfirmation2UI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using System.Collections.Generic;
using System.Reflection;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests
{
  [TestClass]
  public class OCMBookingConfirmation2UI_Tests
  {
    const string filePath = "OCMBookingConfirmation2UI.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestOCMBookingConfirmation2UI()
    {
      AssertMapping("Test1_input.xml", "Test1_output.xml");
      AssertMapping("Test2_input.xml", "Test2_output.xml");
      AssertMapping("Test3_input.xml", "Test3_output.xml", "CLD");
      AssertMapping("Test4_input.xml", "Test4_output.xml");
    }

    void AssertMapping(string inputFile, string expectedOutputFile, string subscribeShipmentType = "AGT")
    {
      var input = filePath + inputFile;
      var expectedOutput = filePath + expectedOutputFile;

      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
      var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();

      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("ODYSSEY").Repeat.AtLeastOnce();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "ODYSSEY", "@recipientId", "", "@ST_ID", "ODSMSG", "@value", "ODY00000001", "@referenceType", "OCMBR")).Return("C00001360").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "MSGID", "ODYSSEY")).Return("ODSMSG").Repeat.Any();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "ODYSSEY", "@recipientId", "", "@ST_ID", "ODSMSG", "@value", "ODY00000001", "@referenceType", "DocumentName")).Return("Booking Request").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "ODYSSEY", "@recipientId", "", "@ST_ID", "ODSMSG", "@value", "ODY00000001", "@referenceType", "ShipmentType")).Return(subscribeShipmentType).Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "ODYSSEY", "@recipientId", "", "@ST_ID", "ODSMSG", "@value", "ODY00000001", "@referenceType", "ForwardingType")).Return("ForwardingConsol").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "NVOCC", "Is Co-load", "CLD")).Return("TRUE").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "NVOCC", "Is Co-load", "AGT")).Return("FALSE").Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMCargowise", "OCMCargowise", "OCM Cargowise System Configuration", "BKC Event Type", "EventType", "ODYSSEY", "RE")).Return("MRJ").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMCargowise", "OCMCargowise", "OCM Cargowise System Configuration", "BKC Event Type", "EventReference", "ODYSSEY", "RE")).Return("Booking Rejected").Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMCargowise", "OCMCargowise", "OCM Cargowise System Configuration", "BKC Event Type", "EventType", "ODYSSEY", "CA")).Return("MAA").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMCargowise", "OCMCargowise", "OCM Cargowise System Configuration", "BKC Event Type", "EventReference", "ODYSSEY", "CA")).Return("Booking Conditionally Accepted").Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMCargowise", "OCMCargowise", "OCM Cargowise System Configuration", "BKC Event Type", "EventType", "ODYSSEY", "AP")).Return("MAA").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMCargowise", "OCMCargowise", "OCM Cargowise System Configuration", "BKC Event Type", "EventReference", "ODYSSEY", "AP")).Return("Booking Confirmed").Repeat.Any();

      mockDateMapper.Expect(x => x.CurrentDateTimeUTC("dd-MMM-yyyy HH:mm")).Return("03-Oct-2019 09:14");

      var extensionObjects = new Dictionary<string, object>()
      {
        {"http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper},
        {"http://schemas.microsoft.com/BizTalk/2003/DateMapper", mockDateMapper},
        {"http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor},
        {"http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor", mockDataModelAccessor}
      };

      MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.Execute<OCMBookingConfirmation2UI>(input, expectedOutput);

      mockDataModelAccessor.VerifyAllExpectations();
      mockCodeMapper.VerifyAllExpectations();
      mockContextAccessor.VerifyAllExpectations();
    }
  }
}