using System;
using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.UniversalShipment2IFTMIN_CMACGM;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.UShipment2UShipmentCharCleanup;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests
{
  [TestClass]
  public class UniversalShipment2IFTMIN_CMACGM_Tests
  {
    const string filePath = "UniversalShipment2IFTMIN_CMACGM.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void UniversalShipment2IFTMIN_CMACGM()
    {
      AssertMapping("Test1_input.xml", "Test1_output.xml");
      AssertMapping("Test2_input.xml", "Test2_output.xml", "ANNU", "ANNU");
      AssertMapping("Test3_USExportJob_input.xml", "Test3_USExportJob_output.xml", "ANNU", "ANNU");
      AssertMapping("Test4_ShipmentTypeBCNSTD_input.xml", "Test4_ShipmentTypeBCNSTD_output.xml", "INTT", "INTT");
      AssertMapping("Test5_ShipmentTypeCLDSTD_input.xml", "Test5_ShipmentTypeCLDSTD_output.xml");
      AssertMapping("Test6_input.xml", "Test6_output.xml", "151521SCAC", "151521SCAC");
      AssertMapping("Test7_input.xml", "Test7_output.xml", "151521SCAC", "151521SCAC");
      AssertMapping("Test8_input.xml", "Test8_output.xml", "ANNU", "ANNU", "Test8_input_cleanup.xml");
      AssertMapping("Test10_ShipmentTypeDRT_input.xml", "Test10_ShipmentTypeDRT_output.xml");
      AssertMapping("Test12_input.xml", "Test12_output.xml", "CMDU", "CMACGM", "", "CN");
    }

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void UniversalShipment2IFTMIN_CMACGM_HCWithDotsAndSpaces()
    {
      AssertMapping("Test9_HCWithDotsAndSpaces_input.xml", "Test9_HCWithDotsAndSpaces_output.xml");
    }

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void UniversalShipment2IFTMIN_CMACGM_MarksAndNosWithSpaces()
    {
      AssertMapping("Test11_input.xml", "Test11_output.xml");
    }

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestCarrierUniversal2CMACGM_CannotFindUNB3()
    {
      var exceptionMessage = "Could not found matching PartyReceiverID in the UNB3 Lookup code mapping.(Sender: * - Multiple senders, Recipient: SHIPPING_INSTRUCTION, Interface: OCM System Configuration, Code Set: UNB3, Input: [CarrierID:CMACGM_SI], [SCAC:CMDU])";
      AssertMappingException("Test1_input.xml", exceptionMessage, "CMDU", "");
    }


    Dictionary<string, object> SetupMappingExtensions(string SCAC = "CMDU", string destinationPartyReceiverIdentifier = "CMACGM", string cleanupFile = "", string countryCode = "")
    {
      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
      var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();

      mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("CMACGM_SI");
      mockContextAccessor.Expect(x => x.GetContextProperty("InternalTrackingID", "http://cargowise.com/ehub/tracking/2010/06")).Return("36ACCF3C-893F-40CE-BA01-1D8CBE2DF678");
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.CMACGM.UNH1", "@maxlength", "14")).Return("29");
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.CMACGM.BGM", "@maxlength", "14")).Return("1").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "CMACGM", "@recipientId", "TESTSENDER__1", "@ST_ID", "CMAMSG", "@value", "C00676795")).Return("").Repeat.AtLeastOnce();
      mockDataModelAccessor.Expect(x => x.GetClientRegistrationCode("TESTSENDER__1", "BNE", "CMACGM")).Return("CGWS");
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CMAID", "CMACGM", "TESTSENDER__1", "CGWS"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CMAMSG", "CMACGM", "TESTSENDER__1", "29", "C00676795"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CMAMSG", "CMACGM", "TESTSENDER__1", "CMA0000000001", "C00676795"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CMAMSG", "CMACGM", "TESTSENDER__1", "C00676795", "CMA0000000001"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CMAMSG", "CMACGM", "TESTSENDER__1", "36ACCF3C-893F-40CE-BA01-1D8CBE2DF678", "29"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CMAMSG", "CMACGM", "TESTSENDER__1", "CMA0000000001", "1.0.0", "FormVersion")).Repeat.Any();
      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("TESTSENDER__1");
      mockContextAccessor.Expect(x => x.SetContextProperty("OverrideEDIHeader", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "true"));
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationPartySenderIdentifier", "http://schemas.microsoft.com/Edi/PropertySchema", "CCLOG_CW"));
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationPartySenderQualifier", "http://schemas.microsoft.com/Edi/PropertySchema", "ZZZ"));
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationPartyReceiverIdentifier", "http://schemas.microsoft.com/Edi/PropertySchema", destinationPartyReceiverIdentifier));
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationPartyReceiverQualifier", "http://schemas.microsoft.com/Edi/PropertySchema", "ZZZ"));
      mockContextAccessor.Expect(x => x.SetContextProperty("UNB5", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "29"));
      mockContextAccessor.Expect(x => x.SetContextProperty("UNH1", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "29"));
      mockContextAccessor.Expect(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "IFTMIN_CGWS_29"));

      mockContextAccessor.Expect(x => x.SetContextProperty("UNB9", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", ""));
      mockContextAccessor.Expect(x => x.SetContextProperty("UNB11", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", ""));
      mockContextAccessor.Expect(x => x.SetContextProperty("EarlyTerminateEdifactUnb", "http://cargowise.com/ehub/processing/2010/06", "true"));
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "UNB3", "PartyReceiverID", "CMACGM_SI", SCAC)).Return(destinationPartyReceiverIdentifier);
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", "CMACGM_SI", "CMDA")).Return(SCAC).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", "CMACGM_SI", "ANNA")).Return(SCAC).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", "CMACGM_SI", "INTT")).Return(SCAC).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", "CMACGM_SI", "151521SCAC")).Return(SCAC).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", "CMACGM_SI", "123456SCAC")).Return("12SCAC").Repeat.Any();

      if (!string.IsNullOrEmpty(countryCode) && countryCode == "CN")
      {
        mockContextAccessor.Expect(x => x.SetContextProperty("OverrideEmailSubject", "http://cargowise.com/ehub/processing/2010/06", "CN"));
      }
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

      return new Dictionary<string, object>
            {
                { "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
                { "http://schemas.microsoft.com/BizTalk/2003/DateMapper", mockDateMapper },
                { "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
                { "http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor", mockDataModelAccessor },
            };
    }

    void AssertMapping(string inputFile, string expectedOutputFile, string SCAC = "CMDU", string destinationPartyReceiverIdentifier = "CMACGM", string cleanupFile = "", string countryCode = "")
    {
      var input = filePath + inputFile;
      var cleanupInput = cleanupFile == ""
                  ? input
                  : filePath + cleanupFile;
      var expectedOutput = filePath + expectedOutputFile;

      var extensionObjects = SetupMappingExtensions(SCAC, destinationPartyReceiverIdentifier, cleanupFile, countryCode);
      var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      if (cleanupInput != input)
      {
        var mapTester1 = new MapTester(Assembly.GetExecutingAssembly());
        mapTester1.Execute<UShipment2UShipmentCharCleanup>(input, cleanupInput);
      }

      mapTester.Execute<UniversalShipment2IFTMIN_CMACGM>(cleanupInput, expectedOutput);

      extensionObjects["http://schemas.microsoft.com/BizTalk/2003/DateMapper"].VerifyAllExpectations();
      extensionObjects["http://schemas.microsoft.com/BizTalk/2003/CodeMapper"].VerifyAllExpectations();
      extensionObjects["http://schemas.microsoft.com/BizTalk/2003/ContextAccessor"].VerifyAllExpectations();
    }

    void AssertMappingException(string inputFile, string exceptionMessage, string SCAC = "CMDU", string destinationPartyReceiverIdentifier = "CMACGM", string cleanupFile = "", string countryCode = "")
    {
      var input = filePath + inputFile;

      var extensionObjects = SetupMappingExtensions(SCAC, destinationPartyReceiverIdentifier, cleanupFile, countryCode);

      var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.ExecuteAssertException<UniversalShipment2IFTMIN_CMACGM>(input, exceptionMessage);
    }
  }
}
