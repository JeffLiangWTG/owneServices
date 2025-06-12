using System;
using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.UniversalShipment2IFTMBF_CMACGM;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;


namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests
{
  [TestClass]
  public class UniversalShipment2IFTMBF_CMACGM_Tests
  {
    const string filePath = "UniversalShipment2IFTMBF_CMACGM.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void UniversalShipment2IFTMBF_CMACGM()
    {
      AssertMapping("Test1_input.xml", "Test1_output.xml", "CMDU", "CMACGM", "NEW", "BNE");
      AssertMapping("Test2_input.xml", "Test2_output.xml", "ANNU", "ANNU", "WTH", "BNE");
      AssertMapping("Test2_AirFlowConversion_input.xml", "Test2_AirFlowConversion_output.xml", "ANNU", "ANNU", "WTH", "BNE");
      AssertMapping("Test3_ShipmentTypeBCNSTD_input.xml", "Test3_ShipmentTypeBCNSTD_output.xml", "CMACGM", "CMACGM", "NEW", "BN1");
      AssertMapping("Test4_ShipmentTypeCLDSTD_input.xml", "Test4_ShipmentTypeCLDSTD_output.xml", "CMDU", "CMACGM", "NEW", "SYD");
      AssertMapping("Test6_SendCNCustomerMessage_input.xml", "Test6_SendCNCustomerMessage_output.xml", "CMDU", "CMACGM", "NEW", "BNE", "CN");
    }

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void UniversalShipment2IFTMBF_CMACGM_HCWithDotsAndSpaces()
    {
      AssertMapping("Test5_HC_WithhDotsAndSpaces_input.xml", "Test5_HC_WithhDotsAndSpaces_output.xml", "CMDU", "CMACGM", "NEW", "BNE");
    }

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestCarrierUniversal2CMACGM_CannotFindUNB3()
    {
      var exceptionMessage = "Could not found matching PartyReceiverID in the UNB3 Lookup code mapping.(Sender: * - Multiple senders, Recipient: SHIPPING_INSTRUCTION, Interface: OCM System Configuration, Code Set: UNB3, Input: [CarrierID:CMACGM_BK], [SCAC:CMDU])";
      AssertMappingException("Test1_input.xml", exceptionMessage, "CMDU", "", "NEW", "BNE");
    }

    Dictionary<string, object> SetupMappingExtensions(string SCAC, string destinationPartyReceiverIdentifier, string actionPurpose, string eventBranch, string countryCode = "")
    {
      var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();

      mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("CMACGM_BK").Repeat.AtLeastOnce();
      mockCodeMapper.Stub(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.CMACGM.UNH1", "@maxlength", "14")).Return("29");
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.CMACGM.BGM", "@maxlength", "14")).Return("1").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "CMACGM", "@recipientId", "TESTSENDER__1", "@ST_ID", "CMAMSG", "@value", "C00676795")).Return("");
      mockDataModelAccessor.Expect(x => x.GetClientRegistrationCode("TESTSENDER__1", eventBranch, "CMACGM")).Return("CGWS");
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CMAID", "CMACGM", "TESTSENDER__1", "CGWS"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CMAMSG", "CMACGM", "TESTSENDER__1", "29", "C00676795"));
      mockDataModelAccessor.Stub(x => x.InsertSubscriptionValue("CMAMSG", "CMACGM", "TESTSENDER__1", "CMA0000000001", "C00676795"));
      mockDataModelAccessor.Stub(x => x.InsertSubscriptionValue("CMAMSG", "CMACGM", "TESTSENDER__1", "C00676795", "CMA0000000001"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CMABRS", "CMACGM", "TESTSENDER__1", "C00676795", actionPurpose));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CMAMSG", "CMACGM", "TESTSENDER__1", "36ACCF3C-893F-40CE-BA01-1D8CBE2DF678", "29"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CMAMSG", "CMACGM", "TESTSENDER__1", "CMA0000000001", "1.0.0", "FormVersion")).Repeat.Any();
      mockContextAccessor.Stub(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("TESTSENDER__1");
      mockContextAccessor.Expect(x => x.GetContextProperty("InternalTrackingID", "http://cargowise.com/ehub/tracking/2010/06")).Return("36ACCF3C-893F-40CE-BA01-1D8CBE2DF678");
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationPartySenderIdentifier", "http://schemas.microsoft.com/Edi/PropertySchema", "CCLOG_CW"));
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationPartySenderQualifier", "http://schemas.microsoft.com/Edi/PropertySchema", "ZZZ"));
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationPartyReceiverIdentifier", "http://schemas.microsoft.com/Edi/PropertySchema", destinationPartyReceiverIdentifier));
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationPartyReceiverQualifier", "http://schemas.microsoft.com/Edi/PropertySchema", "ZZZ"));
      mockContextAccessor.Expect(x => x.SetContextProperty("OverrideEDIHeader", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "true"));
      mockContextAccessor.Expect(x => x.SetContextProperty("UNB5", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "29"));
      mockContextAccessor.Expect(x => x.SetContextProperty("UNH1", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "29"));
      mockContextAccessor.Expect(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "IFTMBF_CGWS_29"));

      if (!string.IsNullOrEmpty(countryCode) && countryCode == "CN")
      {
        mockContextAccessor.Expect(x => x.SetContextProperty("OverrideEmailSubject", "http://cargowise.com/ehub/processing/2010/06", "CN"));
      }

      mockContextAccessor.Expect(x => x.SetContextProperty("UNB9", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", ""));
      mockContextAccessor.Expect(x => x.SetContextProperty("UNB11", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", ""));
      mockContextAccessor.Expect(x => x.SetContextProperty("EarlyTerminateEdifactUnb", "http://cargowise.com/ehub/processing/2010/06", "true"));

      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "UNB3", "PartyReceiverID", "CMACGM_BK", SCAC)).Return(destinationPartyReceiverIdentifier);
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", "CMACGM_BK", "CMDA")).Return(SCAC).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", "CMACGM_BK", "ANNA")).Return(SCAC).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", "CMACGM_BK", "CMACGA")).Return(SCAC).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", "CMACGM_BK", "")).Return(SCAC).Repeat.Any();

      // handle 3 cases for package code:
      // package type
      mockCodeMapper.Expect(x => x.GetRecipientCode("CMACGM_SI", "CMACGM_SI", "Shipping Instruction IFTMIN to CMA CGM", "Package Type", "CMACGM Code", "P_I")).Return("P_O").Repeat.AtLeastOnce();
      // ISO
      mockCodeMapper.Stub(x => x.GetRecipientCode("CMACGM_SI", "CMACGM_SI", "Shipping Instruction IFTMIN to CMA CGM", "Package Type", "CMACGM Code", "I_I")).Return("");
      mockCodeMapper.Stub(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Package Type ISO", "Package Type", "I_I")).Return("I_O");
      // default
      mockCodeMapper.Stub(x => x.GetRecipientCode("CMACGM_SI", "CMACGM_SI", "Shipping Instruction IFTMIN to CMA CGM", "Package Type", "CMACGM Code", "D_I")).Return("");
      mockCodeMapper.Stub(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Package Type ISO", "Package Type", "D_I")).Return("");
      mockCodeMapper.Stub(x => x.GetRecipientCodeUnkeyed("CMACGM_SI", "CMACGM_SI", "Shipping Instruction IFTMIN to CMA CGM", "Defaults", "Package Type")).Return("D_O");

      return new Dictionary<string, object>() {
                { "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
                { "http://schemas.microsoft.com/BizTalk/2003/DateMapper", mockDateMapper },
                { "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
                { "http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor", mockDataModelAccessor },
            };
    }

    void AssertMapping(string inputFile, string expectedOutputFile, string SCAC, string destinationPartyReceiverIdentifier, string actionPurpose, string eventBranch, string countryCode = "")
    {
      var input = filePath + inputFile;
      var expectedOutput = filePath + expectedOutputFile;

      var extensionObjects = SetupMappingExtensions(SCAC, destinationPartyReceiverIdentifier, actionPurpose, eventBranch, countryCode);
      MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.Execute<UniversalShipment2IFTMBF_CMACGM>(input, expectedOutput);

      extensionObjects["http://schemas.microsoft.com/BizTalk/2003/CodeMapper"].VerifyAllExpectations();
      extensionObjects["http://schemas.microsoft.com/BizTalk/2003/DateMapper"].VerifyAllExpectations();
      extensionObjects["http://schemas.microsoft.com/BizTalk/2003/ContextAccessor"].VerifyAllExpectations();
      extensionObjects["http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor"].VerifyAllExpectations();
    }

    void AssertMappingException(string inputFile, string exceptionMessage, string SCAC, string destinationPartyReceiverIdentifier, string actionPurpose, string eventBranch, string countryCode = "")
    {
      var input = filePath + inputFile;

      var extensionObjects = SetupMappingExtensions(SCAC, destinationPartyReceiverIdentifier, actionPurpose, eventBranch, countryCode);

      var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.ExecuteAssertException<UniversalShipment2IFTMBF_CMACGM>(input, exceptionMessage);
    }
  }
}
