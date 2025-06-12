using System;
using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.CARGOWISE2CARGOWISE;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests
{
  [TestClass]
  public class Cargowise2Cargowise_CU2UI_CRA_Tests
  {
    const string filePath = "Cargowise2Cargowise.CargoReceivalAdvice.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestCU2UI_CRA()
    {
      AssertMapping("Test1_input.xml", "Test1_output.xml", "NVOCC", "SESYDDAU54652003", "", "ForwardingShipment");
      AssertMapping("Test2_IRJ_Event_input.xml", "Test2_IRJ_Event_output.xml", "ShippingLine", "SESYDDAU54652003", "", "ForwardingShipment", "");
    }

    void AssertMapping(string inputFile, string expectedOutputFile, string ehubPartyType, string shipmentNumber, string previousSubscriptReference, string forwardingType, string registeredClientID = "HYEDAUUG1", string formVersion = "")
    {
      var input = filePath + inputFile;
      var expectedOutput = filePath + expectedOutputFile;

      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
      var mockOCMHelper = MockRepository.GenerateStrictMock<OCMHelper>();
      var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();

      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("HYEUAT001").Repeat.Any();
      mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("CARGOWISE_CRA").Repeat.Any();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedClients", "", "@senderId", "HYEUAT001", "@ST_ID", "CW1MSG", "@value", "SHP0000001264")).Return(registeredClientID);

      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", registeredClientID)).Repeat.Any();
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", "HYEUAT001")).Repeat.Any();
      mockContextAccessor.Expect(x => x.SetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", "CARGOWISE_CRA")).Repeat.Any();

      var eHubPartyTypeFlag = ehubPartyType == "NVOCC" ? "1" : "0";
      mockDataModelAccessor.Expect(x => x.GetClientRegistrationFlag1AsString("HYEDAUUG1", "", "CARGOWISE")).Return(eHubPartyTypeFlag).Repeat.Any();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.CARGOWISE2CARGOWISE", "@maxlength", "14")).Return("99").Repeat.Any();

      var shipmentReference = "CRA0000000099";
      if (previousSubscriptReference != "")
      {
        shipmentReference = previousSubscriptReference;
      }
      if (previousSubscriptReference == "")
      {
        mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CW1MSG", registeredClientID, "HYEUAT001", shipmentReference, shipmentNumber, "JobNumber")).Repeat.Any();
        mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CW1MSG", registeredClientID, "HYEUAT001", shipmentNumber, shipmentReference, "JobNumber")).Repeat.Any();
      }

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", registeredClientID, "@recipientId", "HYEUAT001", "@ST_ID", "CW1MSG", "@value", shipmentNumber, "@referenceType", "JobNumber")).Return(previousSubscriptReference).Repeat.Any();

      mockContextAccessor.Expect(x => x.GetContextProperty("InternalTrackingID", "http://cargowise.com/ehub/tracking/2010/06")).Return("36ACCF3C-893F-40CE-BA01-1D8CBE2DF678").Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CW1MSG", registeredClientID, "HYEUAT001", "36ACCF3C-893F-40CE-BA01-1D8CBE2DF678", "99")).Repeat.Any();

      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CW1MSG", registeredClientID, "HYEUAT001", "99", shipmentReference, "InterchangeNumber")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CW1MSG", registeredClientID, "HYEUAT001", shipmentReference, "ORG", "ActionPurpose")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CW1MSG", registeredClientID, "HYEUAT001", shipmentReference, "", "ShipmentType")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CW1MSG", registeredClientID, "HYEUAT001", shipmentReference, "Cargo Receipt Advice", "DocumentName")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CW1MSG", registeredClientID, "HYEUAT001", shipmentReference, forwardingType, "ForwardingType")).Repeat.Any();
      if (formVersion != "")
      {
        mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CW1MSG", registeredClientID, "HYEUAT001", shipmentReference, formVersion, "FormVersion")).Repeat.Any();
      }

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", registeredClientID, "@recipientId", "HYEUAT001", "@ST_ID", "CW1MSG", "@value", shipmentReference, "@referenceType", "PartyType")).Return("").Repeat.Any();

      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CW1MSG", registeredClientID, "HYEUAT001", shipmentReference, ehubPartyType, "PartyType")).Repeat.Any();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "HYEUAT001", "@recipientId", registeredClientID, "@ST_ID", "CW1MSG", "@value", "SHP0000001264", "@referenceType", "JobNumber")).Return("KEY123").Repeat.Any();

      mockDateMapper.Expect(x => x.CurrentDateTimeUTC(Arg.Is("yyyy-MM-ddTHH:mm:ss.fff"))).Return("2022-01-10T09:54:30").Repeat.Any();

      var extensionObjects = new Dictionary<string, object>()
      {
        {"http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper},
        {"http://schemas.microsoft.com/BizTalk/2003/OCMHelper", mockOCMHelper},
        {"http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor},
        {"http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor", mockDataModelAccessor},
        {"http://schemas.microsoft.com/BizTalk/2003/DateMapper", mockDateMapper}
      };

      var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.ExecuteCompiled<US2UI_CRA>(input, expectedOutput);

      mockCodeMapper.VerifyAllExpectations();
      mockContextAccessor.VerifyAllExpectations();
      mockDataModelAccessor.VerifyAllExpectations();
      mockOCMHelper.VerifyAllExpectations();
    }
  }
}

