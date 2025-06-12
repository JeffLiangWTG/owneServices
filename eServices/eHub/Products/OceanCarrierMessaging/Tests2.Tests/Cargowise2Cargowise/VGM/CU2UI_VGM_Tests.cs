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
  public class Cargowise2Cargowise_CU2UI_VGM_Tests
  {
    const string filePath = "Cargowise2Cargowise.VGM.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestCU2UI_VGM()
    {
      AssertMapping("Test1_CLD_input.xml", "Test1_CLD_output.xml", "C00001001", "TCMU9383556", "", "NVOCC", "ForwardingConsol", formVersion: "2.0");
      AssertMapping("Test2_AGT_input.xml", "Test2_AGT_output.xml", "C00001002", "TCMU9383556", "VGM0000000099", "ShippingLine", "ForwardingConsol", shipmentType: "AGT");
      AssertMapping("Test3_CLD_Shipment_input.xml", "Test3_CLD_Shipment_output.xml", "C00001001", "TCMU9383556",  "", "NVOCC", "ForwardingShipment");
      AssertMapping("Test4_CLD_Other_input.xml", "Test4_CLD_Other_output.xml", "C00001001", "TCMU9383556", "", "NVOCC", "ForwardingOther");
    }

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestCU2UI_VGM_RecipientIDNotFound()
    {
      var isThrownException = false;
      try
      {
        AssertMapping("Test1_CLD_input.xml", "Test1_CLD_output.xml", "C00001001", "TCMU9383556", "", "NVOCC", "ForwardingConsol", registeredClientID: "", formVersion: "2.0");
      }
      catch (ArgumentException ex)
      {
        isThrownException = true;
        Assert.AreEqual("Could not found matching PartyReceiverID in the Client Registration Lookup.(Client Registration: CARGOWISE, Input: [SCAC/C1C:C1GS])", ex.Message.Trim());
      }
      Assert.IsTrue(isThrownException);
    }

    void AssertMapping(string inputFile, string expectedOutputFile, string consolNumber, string containerNumber, string previousVGMSubscriptReference, string ehubPartyType, string forwardingType = "", string registeredClientID = "HYEDAUUG1", string shipmentType = "CLD", string formVersion = "")
    {
      var input = filePath + inputFile;
      var expectedOutput = filePath + expectedOutputFile;

      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
      var mockOCMHelper = MockRepository.GenerateStrictMock<OCMHelper>();

      mockContextAccessor.Expect(x => x.GetContextProperty("InternalTrackingID", "http://cargowise.com/ehub/tracking/2010/06")).Return("36ACCF3C-893F-40CE-BA01-1D8CBE2DF678");
      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("HYEUAT001").Repeat.Once();
      mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("CARGOWISE_SI");
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", registeredClientID)).Repeat.Once();

      var eHubPartyTypeFlag = ehubPartyType == "NVOCC" ? "1" : "0";
      mockDataModelAccessor.Expect(x => x.GeteHubIDByQualifier("C1GS", "CARGOWISE")).Return(registeredClientID).Repeat.Once();
      mockDataModelAccessor.Expect(x => x.GetClientRegistrationFlag1AsString(registeredClientID, "C1GS", "CARGOWISE")).Return(eHubPartyTypeFlag).Repeat.Any();

      mockOCMHelper.Expect(x => x.IsCoLoad("AGT")).Return("FALSE").Repeat.Any();
      mockOCMHelper.Expect(x => x.IsCoLoad("CLD")).Return("TRUE").Repeat.Any();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.CARGOWISE2CARGOWISE", "@maxlength", "14")).Return("99").Repeat.Once();

      var vgmReference = string.Empty;
      if (previousVGMSubscriptReference != "")
      {
        mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", registeredClientID, "@recipientId", "HYEUAT001", "@ST_ID", "CW1MSG", "@value", string.Concat(consolNumber, "_", containerNumber), "@referenceType", "JobNumber")).Return(previousVGMSubscriptReference).Repeat.Once();
        vgmReference = previousVGMSubscriptReference;
      }
      else if (previousVGMSubscriptReference == "")
      {
        mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", registeredClientID, "@recipientId", "HYEUAT001", "@ST_ID", "CW1MSG", "@value", string.Concat(consolNumber, "_", containerNumber), "@referenceType", "JobNumber")).Return(previousVGMSubscriptReference).Repeat.Once();
        vgmReference = "VGM0000000099";

        mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CW1MSG", registeredClientID, "HYEUAT001", vgmReference, string.Concat(consolNumber, "_", containerNumber), "JobNumber")).Repeat.Once();
        mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CW1MSG", registeredClientID, "HYEUAT001", string.Concat(consolNumber, "_", containerNumber), vgmReference, "JobNumber")).Repeat.Once();
      }

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", registeredClientID, "@recipientId", "HYEUAT001", "@ST_ID", "CW1MSG", "@value", vgmReference, "@referenceType", "PartyType")).Return("").Repeat.Once();

      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CW1MSG", registeredClientID, "HYEUAT001", "36ACCF3C-893F-40CE-BA01-1D8CBE2DF678", "99")).Repeat.Once();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CW1MSG", registeredClientID, "HYEUAT001", vgmReference, ehubPartyType, "PartyType")).Repeat.Once();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CW1MSG", registeredClientID, "HYEUAT001", "99", vgmReference, "InterchangeNumber")).Repeat.Once();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CW1MSG", registeredClientID, "HYEUAT001", vgmReference, "ORG", "ActionPurpose")).Repeat.Once();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CW1MSG", registeredClientID, "HYEUAT001", vgmReference, shipmentType, "ShipmentType")).Repeat.Once();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CW1MSG", registeredClientID, "HYEUAT001", vgmReference, "Verified Gross Container Weight", "DocumentName")).Repeat.Once();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CW1MSG", registeredClientID, "HYEUAT001", vgmReference, forwardingType, "ForwardingType")).Repeat.Once();

      if (formVersion != "")
      {
        mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CW1MSG", registeredClientID, "HYEUAT001", vgmReference, formVersion, "FormVersion")).Repeat.Once();
      }

      var extensionObjects = new Dictionary<string, object>()
      {
        {"http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper},
        {"http://schemas.microsoft.com/BizTalk/2003/OCMHelper", mockOCMHelper},
        {"http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor},
        {"http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor", mockDataModelAccessor}
      };

      var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.ExecuteCompiled<CU2UI_VGM>(input, expectedOutput);

      mockCodeMapper.VerifyAllExpectations();
      mockContextAccessor.VerifyAllExpectations();
      mockDataModelAccessor.VerifyAllExpectations();
      mockOCMHelper.VerifyAllExpectations();
    }
  }
}

