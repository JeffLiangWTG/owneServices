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
  public class Cargowise2Cargowise_CU2UI_CAD_Tests
  {
    const string filePath = "Cargowise2Cargowise.ConsolidationAdvice.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestCU2UI_CAD()
    {
      AssertMapping("Test1_CLD_input.xml", "Test1_CLD_output.xml", "SMX2104661452", "", "NVOCC", "ForwardingShipment", formVersion: "2.0");
      AssertMapping("Test2_CLD_input.xml", "Test2_CLD_output.xml", "SMX2104661452", "", "NVOCC", "ForwardingShipment", formVersion: "2.0");
      AssertMapping("Test3_IRJ_Event_input.xml", "Test3_IRJ_Event_output.xml", "SMX2104661452", "", "ShippingLine", "ForwardingShipment", "", formVersion: "2.0");
    }

    void AssertMapping(string inputFile, string expectedOutputFile, string consolNumber, string previousSubscriptReference, string ehubPartyType, string forwardingType = "", string registeredClientID = "HYEDAUUG1", string formVersion = "")
    {
      var input = filePath + inputFile;
      var expectedOutput = filePath + expectedOutputFile;

      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
      var mockOCMHelper = MockRepository.GenerateStrictMock<OCMHelper>();
      var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();

      var consolReference = "CAD0000000099";
      if (previousSubscriptReference != "")
      {
        consolReference = previousSubscriptReference;
      }

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedClients", "", "@senderId", "HYEUAT001", "@ST_ID", "CW1MSG", "@value", "SHP0000001081")).Return(registeredClientID);

      mockContextAccessor.Expect(x => x.GetContextProperty("InternalTrackingID", "http://cargowise.com/ehub/tracking/2010/06")).Return("36ACCF3C-893F-40CE-BA01-1D8CBE2DF678");
      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("HYEUAT001").Repeat.Any();
      mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("CARGOWISE_CA");
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", registeredClientID)).Repeat.Any();
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", "HYEUAT001")).Repeat.Any();
      mockContextAccessor.Expect(x => x.SetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", "CARGOWISE_CA")).Repeat.Any();

      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CW1MSG", registeredClientID, "HYEUAT001", "36ACCF3C-893F-40CE-BA01-1D8CBE2DF678", "99")).Repeat.Any();

      if (previousSubscriptReference == "")
      {
        mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CW1MSG", registeredClientID, "HYEUAT001", consolReference, consolNumber, "JobNumber")).Repeat.Any();
        mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CW1MSG", registeredClientID, "HYEUAT001", consolNumber, consolReference, "JobNumber")).Repeat.Any();
      }

      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CW1MSG", registeredClientID, "HYEUAT001", consolReference, ehubPartyType, "PartyType")).Repeat.Any();

      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CW1MSG", registeredClientID, "HYEUAT001", "99", consolReference, "InterchangeNumber")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CW1MSG", registeredClientID, "HYEUAT001", consolReference, "ORG", "ActionPurpose")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CW1MSG", registeredClientID, "HYEUAT001", consolReference, "CLD", "ShipmentType")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CW1MSG", registeredClientID, "HYEUAT001", consolReference, "Consolidation Advice", "DocumentName")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CW1MSG", registeredClientID, "HYEUAT001", consolReference, forwardingType, "ForwardingType")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CW1MSG", registeredClientID, "HYEUAT001", consolReference, formVersion, "FormVersion")).Repeat.Any();

      var eHubPartyTypeFlag = ehubPartyType == "NVOCC" ? "1" : "0";
      mockDataModelAccessor.Expect(x => x.GetClientRegistrationFlag1AsString("HYEDAUUG1", "", "CARGOWISE")).Return(eHubPartyTypeFlag).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.GeteHubIDByQualifier("", "CARGOWISE")).Return("HYEDAUUG1").Repeat.Any();

      mockOCMHelper.Expect(x => x.IsCoLoad("CLD")).Return("TRUE").Repeat.Any();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.CARGOWISE2CARGOWISE", "@maxlength", "14")).Return("99").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", registeredClientID, "@recipientId", "HYEUAT001", "@ST_ID", "CW1MSG", "@value", consolNumber, "@referenceType", "JobNumber")).Return(previousSubscriptReference).Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", registeredClientID, "@recipientId", "HYEUAT001", "@ST_ID", "CW1MSG", "@value", consolReference, "@referenceType", "PartyType")).Return("").Repeat.Any();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "HYEUAT001", "@recipientId", registeredClientID, "@ST_ID", "CW1MSG", "@value", "SHP0000001081", "@referenceType", "JobNumber")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "HYEUAT001", "@recipientId", registeredClientID, "@ST_ID", "CW1MSG", "@value", "SHP0000001082", "@referenceType", "JobNumber")).Return("").Repeat.Any();

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
      mapTester.ExecuteCompiled<US2UI_CAD>(input, expectedOutput);

      mockCodeMapper.VerifyAllExpectations();
      mockContextAccessor.VerifyAllExpectations();
      mockDataModelAccessor.VerifyAllExpectations();
      mockOCMHelper.VerifyAllExpectations();
      mockDateMapper.VerifyAllExpectations();
    }
  }
}

