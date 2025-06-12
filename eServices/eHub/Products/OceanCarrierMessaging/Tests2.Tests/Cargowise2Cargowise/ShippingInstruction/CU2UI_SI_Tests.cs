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
  public class Cargowise2Cargowise_CU2UI_SI_Tests
  {
    const string filePath = "Cargowise2Cargowise.ShippingInstruction.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestCU2UI_SI()
    {
      AssertMapping("Test1_CLD_input.xml", "Test1_CLD_output.xml", "C00001001", "", "NVOCC", "ForwardingConsol", formVersion : "2.5.0");
      AssertMapping("Test2_AGT_input.xml", "Test2_AGT_output.xml", "C00001002", "WTG0000000098", "ShippingLine", "ForwardingConsol", formVersion: "2.5.0");

      AssertMapping("Test3_CLD_input.xml", "Test3_CLD_output.xml", "C00001001", "", "NVOCC", "ForwardingConsol");
      AssertMapping("Test4_AGT_input.xml", "Test4_AGT_output.xml", "C00001002", "WTG0000000098", "ShippingLine", "ForwardingConsol");

      AssertMapping("Test5_input.xml", "Test5_output.xml", "C00001001", "", "NVOCC", "ForwardingConsol");

      AssertMapping("Test6_CLD_Shipment_input.xml", "Test6_CLD_Shipment_output.xml", "C00001001", "", "NVOCC", "ForwardingShipment");
      AssertMapping("Test7_CLD_Other_input.xml", "Test7_CLD_Other_output.xml", "C00001001", "", "NVOCC", "ForwardingOther");
      AssertMapping("Test8_CLD_Other_input.xml", "Test8_CLD_Other_output.xml", "C00001001", "", "ShippingLine", "ForwardingOther");

      AssertMapping("Test9_GroupingMethod_input.xml", "Test9_GroupingMethod_output.xml", "C00001001", "WTG0000000098", "ShippingLine", "ForwardingConsol", formVersion: "3.0.0");
    }

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestCU2UI_SI_RecipientIDNotFound()
    {
      var isThrownException = false;
      try
      {
        AssertMapping("Test1_CLD_input.xml", "Test1_CLD_output.xml", "C00001001", "", "NVOCC", "ForwardingConsol", "");
      }
      catch (ArgumentException ex)
      {
        isThrownException = true;
        Assert.AreEqual("Could not found matching PartyReceiverID in the Client Registration Lookup.(Client Registration: CARGOWISE, Input: [SCAC/C1C:C1GS])", ex.Message.Trim());
      }
      Assert.IsTrue(isThrownException);
    }

    void AssertMapping(string inputFile, string expectedOutputFile, string consolNumber, string previousSubscriptReference, string ehubPartyType, string forwardingType = "", string registeredClientID = "HYEDAUUG1", string formVersion = "1.0")
    {
      var input = filePath + inputFile;
      var expectedOutput = filePath + expectedOutputFile;

      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
      var mockOCMHelper = MockRepository.GenerateStrictMock<OCMHelper>();
      var mockSubscriptionHelper = MockRepository.GenerateStrictMock<SubscriptionHelper>();

      var consolReference = "WTG0000000099";
      if (previousSubscriptReference != "")
      {
        consolReference = previousSubscriptReference;
      }
      else
      {
        if (forwardingType == "ForwardingConsol")
        {
          consolReference = "CON0000000099";
        }
        if (forwardingType == "ForwardingShipment")
        {
          consolReference = "SHP0000000099";
        }
      }

      mockContextAccessor.Expect(x => x.GetContextProperty("InternalTrackingID", "http://cargowise.com/ehub/tracking/2010/06")).Return("36ACCF3C-893F-40CE-BA01-1D8CBE2DF678");
      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("HYEUAT001").Repeat.Any();
      mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("CARGOWISE_SI");
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", "HYEDAUUG1")).Repeat.Any();

      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CW1MSG", "HYEDAUUG1", "HYEUAT001", "36ACCF3C-893F-40CE-BA01-1D8CBE2DF678", "99")).Repeat.Any();

      if (previousSubscriptReference == "")
      {
        mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CW1MSG", "HYEDAUUG1", "HYEUAT001", consolReference, consolNumber, "JobNumber")).Repeat.Any();
        mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CW1MSG", "HYEDAUUG1", "HYEUAT001", consolNumber, consolReference, "JobNumber")).Repeat.Any();
      }

      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CW1MSG", "HYEDAUUG1", "HYEUAT001", consolReference, ehubPartyType, "PartyType")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CW1MSG", "HYEDAUUG1", "HYEUAT001", "99", consolReference, "InterchangeNumber")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CW1MSG", "HYEDAUUG1", "HYEUAT001", consolReference, "ORG", "ActionPurpose")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CW1MSG", "HYEDAUUG1", "HYEUAT001", consolReference, "AMD", "ActionPurpose")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CW1MSG", "HYEDAUUG1", "HYEUAT001", consolReference, "CLD", "ShipmentType")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CW1MSG", "HYEDAUUG1", "HYEUAT001", consolReference, "AGT", "ShipmentType")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CW1MSG", "HYEDAUUG1", "HYEUAT001", consolReference, "Shipping Instruction", "DocumentName")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CW1MSG", "HYEDAUUG1", "HYEUAT001", consolReference, forwardingType, "ForwardingType")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CW1MSG", "HYEDAUUG1", "HYEUAT001", consolReference, formVersion, "FormVersion")).Repeat.Any();

      var eHubPartyTypeFlag = ehubPartyType == "NVOCC" ? "1" : "0";
      mockDataModelAccessor.Expect(x => x.GetClientRegistrationFlag1AsString("HYEDAUUG1", "C1GS", "CARGOWISE")).Return(eHubPartyTypeFlag).Repeat.Once();
      mockDataModelAccessor.Expect(x => x.GeteHubIDByQualifier("C1GS", "CARGOWISE")).Return(registeredClientID).Repeat.Any();

      mockOCMHelper.Expect(x => x.IsCoLoad("AGT")).Return("FALSE").Repeat.Any();
      mockOCMHelper.Expect(x => x.IsCoLoad("CLD")).Return("TRUE").Repeat.Any();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.CARGOWISE2CARGOWISE", "@maxlength", "14")).Return("99").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "HYEDAUUG1", "@recipientId", "HYEUAT001", "@ST_ID", "CW1MSG", "@value", consolNumber, "@referenceType", "JobNumber")).Return(previousSubscriptReference).Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "HYEDAUUG1", "@recipientId", "HYEUAT001", "@ST_ID", "CW1MSG", "@value", consolReference, "@referenceType", "PartyType")).Return("").Repeat.Any();

      mockSubscriptionHelper.Expect(x => x.InsertBoleroSubscription("HYEUAT001", registeredClientID, "CARGOWISE", consolReference, consolNumber)).Repeat.Any();

      var extensionObjects = new Dictionary<string, object>()
      {
        {"http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper},
        {"http://schemas.microsoft.com/BizTalk/2003/OCMHelper", mockOCMHelper},
        {"http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor},
        {"http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor", mockDataModelAccessor},
        {"http://schemas.microsoft.com/BizTalk/2003/SubscriptionHelper", mockSubscriptionHelper }
      };

      var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.ExecuteCompiled<CU2UI_SI>(input, expectedOutput);

      mockCodeMapper.VerifyAllExpectations();
      mockContextAccessor.VerifyAllExpectations();
      mockDataModelAccessor.VerifyAllExpectations();
      mockOCMHelper.VerifyAllExpectations();
    }
  }
}

