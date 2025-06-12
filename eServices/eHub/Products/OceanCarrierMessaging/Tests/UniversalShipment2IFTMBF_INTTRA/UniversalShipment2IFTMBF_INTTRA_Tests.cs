using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.UniversalShipment2IFTMBF_INTTRA;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests
{
  [TestClass]
  public class UniversalShipment2IFTMBF_INTTRA_Tests
  {
    const string filePath = "UniversalShipment2IFTMBF_INTTRA.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void UniversalShipment2IFTMBF_INTTRA()
    {
      AssertMapping("Test1_input.xml", "Test1_output.xml");
    }

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void UniversalShipment2IFTMBF_INTTRA_HCWithDotsAndSpaces()
    {
      AssertMapping("Test5_HCWithDotsAndSpaces_input.xml", "Test5_HCWithDotsAndSpaces_output.xml");
    }

    private static void AssertMapping(string inputFile, string expectedOutputFile)
    {
      string input = filePath + inputFile;
      string expectedOutput = filePath + expectedOutputFile;

      var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.INTTRA.UNH1", "@maxlength", "14")).Return("29");
      mockCodeMapper.Expect(x => x.GetRecipientCode("INTTRA_SI", "INTTRA_SI", "Shipping Instruction IFTMIN to INTTRA", "Package Type", "INTTRA Code", "PCE")).Return("PS");
      mockCodeMapper.Expect(x => x.GetRecipientCode("INTTRA_SI", "INTTRA_SI", "Shipping Instruction IFTMIN to INTTRA", "Package Type", "INTTRA Code", "PKG")).Return("PK").Repeat.Times(3);
      mockCodeMapper.Expect(x => x.GetRecipientCode("INTTRA_SI", "INTTRA_SI", "Shipping Instruction IFTMIN to INTTRA", "ContainerTypeToISOCode", "INTTRA Code", "42R0")).Return("");
      mockCodeMapper.Expect(x => x.GetRecipientCode("INTTRA_SI", "INTTRA_SI", "Shipping Instruction IFTMIN to INTTRA", "ContainerTypeToISOCode", "INTTRA Code", "48K8")).Return("48T8");
      mockCodeMapper.Expect(x => x.GetRecipientCode("INTTRA_SI", "INTTRA_SI", "Shipping Instruction IFTMIN to INTTRA", "ContainerTypeToISOCode", "INTTRA Code", "")).Return("").Repeat.Any();
      mockDataModelAccessor.Expect(x => x.GetClientRegistrationCode("TESTSENDER__1", "BNE", "INTTRA")).Return("CGWS");
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("INTID", "INTTRA", "TESTSENDER__1", "CGWS"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("INTMSG", "INTTRA", "TESTSENDER__1", "29", "C00676795"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("INTBRS", "INTTRA", "TESTSENDER__1", "C00676795", "NEW"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("INTBRS", "INTTRA", "TESTSENDER__1", "C00676795", "AGT", "ShipmentType"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("INTMSG", "INTTRA", "TESTSENDER__1", "36ACCF3C-893F-40CE-BA01-1D8CBE2DF678", "29"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("INTMSG", "INTTRA", "TESTSENDER__1", "C00676795", "1.0.0", "FormVersion")).Repeat.Any();
      mockCodeMapper.Stub(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "NVOCC", "Is Co-load", "CLD")).Return("TRUE");
      mockCodeMapper.Stub(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "NVOCC", "Is Co-load", "AGT")).Return("FALSE");

      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("TESTSENDER__1");
      mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("INTTRA_BK");
      mockContextAccessor.Expect(x => x.GetContextProperty("InternalTrackingID", "http://cargowise.com/ehub/tracking/2010/06")).Return("36ACCF3C-893F-40CE-BA01-1D8CBE2DF678");
      mockContextAccessor.Expect(x => x.SetContextProperty("OverrideEDIHeader", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "true"));
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationPartySenderIdentifier", "http://schemas.microsoft.com/Edi/PropertySchema", "CARGOWISE"));
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationPartySenderQualifier", "http://schemas.microsoft.com/Edi/PropertySchema", "ZZZ"));
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationPartyReceiverIdentifier", "http://schemas.microsoft.com/Edi/PropertySchema", "INTTRANG2"));
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationPartyReceiverQualifier", "http://schemas.microsoft.com/Edi/PropertySchema", "ZZZ"));
      mockContextAccessor.Expect(x => x.SetContextProperty("UNB5", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "29"));
      mockContextAccessor.Expect(x => x.SetContextProperty("UNH1", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "29"));
      mockContextAccessor.Expect(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "IFTMBF_CGWS_29"));

      mockContextAccessor.Expect(x => x.SetContextProperty("UNB9", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", ""));
      mockContextAccessor.Expect(x => x.SetContextProperty("UNB11", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", ""));
      mockContextAccessor.Expect(x => x.SetContextProperty("EarlyTerminateEdifactUnb", "http://cargowise.com/ehub/processing/2010/06", "true"));

      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", "INTTRA_BK", "151521SCAA")).Return("151521SCAC").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", "INTTRA_BK", "")).Return("");

      var extensionObjects = new Dictionary<string, object>() {
                { "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
                { "http://schemas.microsoft.com/BizTalk/2003/DateMapper", mockDateMapper },
                { "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
                { "http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor", mockDataModelAccessor },
            };
      MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);

      mapTester.Execute<UniversalShipment2IFTMBF_INTTRA>(input, expectedOutput);

      mockDateMapper.VerifyAllExpectations();
      mockCodeMapper.VerifyAllExpectations();
      mockContextAccessor.VerifyAllExpectations();
      mockDataModelAccessor.VerifyAllExpectations();
    }

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void UniversalShipment2IFTMBF_INTTRA_151521SCAC()
    {
      AssertMapping_Variants("Test2_input.xml", "Test2_output.xml", "151521SCAC");
    }

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void UniversalShipment2IFTMBF_INTTRA_AirFlowConversion()
    {
      AssertMapping_Variants("Test2_AirFlowConversion_input.xml", "Test2_AirFlowConversion_output.xml", "151521SCAC");
    }

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void UniversalShipment2IFTMBF_INTTRA_ShipmentTypeBCNSTD()
    {
      AssertMapping_Variants("Test3_ShipmentTypeBCNSTD_input.xml", "Test3_ShipmentTypeBCNSTD_output.xml", "INTT");
    }

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void UniversalShipment2IFTMBF_INTTRA_ShipmentTypeCLDSTD()
    {
      AssertMapping_Variants("Test4_ShipmentTypeCLDSTD_input.xml", "Test4_ShipmentTypeCLDSTD_output.xml", "CMDU");
    }

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void UniversalShipment2IFTMBF_INTTRA_CLD()
    {
      AssertMapping_Variants("Test6_CLD_input.xml", "Test6_CLD_output.xml", "151521SCAC");
    }

    private void AssertMapping_Variants(string inputFile, string expectedOutputFile, string scac)
    {
      string input = filePath + inputFile;
      string expectedOutput = filePath + expectedOutputFile;

      var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockContextAccessor = MockRepository.GenerateMock<ContextAccessor>();
      var mockDataModelAccessor = MockRepository.GenerateMock<DataModelAccessor>();

      mockDateMapper.Stub(x => x.CurrentDateTime(Arg.Is("yyyyMMddHHmm"))).Return("201407040543");
      mockCodeMapper.Stub(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.INTTRA.UNH1", "@maxlength", "14")).Return("29");
      mockCodeMapper.Stub(x => x.GetRecipientCode("INTTRA_SI", "INTTRA_SI", "Shipping Instruction IFTMIN to INTTRA", "ContainerTypeToISOCode", "INTTRA Code", "45G0")).Return("45G0");
      mockCodeMapper.Stub(x => x.GetRecipientCode("INTTRA_SI", "INTTRA_SI", "Shipping Instruction IFTMIN to INTTRA", "ContainerTypeToISOCode", "INTTRA Code", "42G0")).Return("42G0");
      mockCodeMapper.Stub(x => x.GetRecipientCode("INTTRA_SI", "INTTRA_SI", "Shipping Instruction IFTMIN to INTTRA", "ContainerTypeToISOCode", "INTTRA Code", "22G0")).Return("22G0");
      mockCodeMapper.Expect(x => x.GetRecipientCode("INTTRA_SI", "INTTRA_SI", "Shipping Instruction IFTMIN to INTTRA", "Package Type", "INTTRA Code", "PCE")).Return("PS");
      mockCodeMapper.Expect(x => x.GetRecipientCode("INTTRA_SI", "INTTRA_SI", "Shipping Instruction IFTMIN to INTTRA", "Package Type", "INTTRA Code", "PKG")).Return("PK").Repeat.Times(3);
      mockCodeMapper.Expect(x => x.GetRecipientCode("INTTRA_SI", "INTTRA_SI", "Shipping Instruction IFTMIN to INTTRA", "ContainerTypeToISOCode", "INTTRA Code", "42R0")).Return("");
      mockCodeMapper.Expect(x => x.GetRecipientCode("INTTRA_SI", "INTTRA_SI", "Shipping Instruction IFTMIN to INTTRA", "ContainerTypeToISOCode", "INTTRA Code", "48K8")).Return("48T8");
      mockCodeMapper.Stub(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "NVOCC", "Is Co-load", "CLD")).Return("TRUE");
      mockCodeMapper.Stub(x => x.GetRecipientCode(Arg<string>.Is.Equal("SHIPPING_INSTRUCTION"), Arg<string>.Is.Equal("SHIPPING_INSTRUCTION"), Arg<string>.Is.Equal("OCM System Configuration"), Arg<string>.Is.Equal("NVOCC"), Arg<string>.Is.Equal("Is Co-load"),
          Arg<string>.List.OneOf(new string[] { "DRT", "AGT" }))).Return("FALSE");

      // handle 3 cases for package code:
      // package type
      mockCodeMapper.Stub(x => x.GetRecipientCode("INTTRA_SI", "INTTRA_SI", "Shipping Instruction IFTMIN to INTTRA", "Package Type", "INTTRA Code", "P_I")).Return("P_O");
      // ISO
      mockCodeMapper.Stub(x => x.GetRecipientCode("INTTRA_SI", "INTTRA_SI", "Shipping Instruction IFTMIN to INTTRA", "Package Type", "INTTRA Code", "I_I")).Return("");
      mockCodeMapper.Stub(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Package Type ISO", "Package Type", "I_I")).Return("I_O");
      // default
      mockCodeMapper.Stub(x => x.GetRecipientCode("INTTRA_SI", "INTTRA_SI", "Shipping Instruction IFTMIN to INTTRA", "Package Type", "INTTRA Code", "D_I")).Return("");
      mockCodeMapper.Stub(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Package Type ISO", "Package Type", "D_I")).Return("");
      mockCodeMapper.Stub(x => x.GetRecipientCodeUnkeyed("INTTRA_SI", "INTTRA_SI", "Shipping Instruction IFTMIN to INTTRA", "Defaults", "Package Type")).Return("D_O");

      mockDataModelAccessor.Expect(x => x.GetClientRegistrationCode("TESTSENDER__1", "BNE", "INTTRA")).Return("CGWS");
      mockDataModelAccessor.Expect(x => x.GetClientRegistrationCode(Arg<string>.Is.Equal("TESTSENDER__1"), Arg<string>.Is.Anything, Arg<string>.Is.Equal("INTTRA"))).Return("");
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("INTID", "INTTRA", "TESTSENDER__1", "CGWS"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("INTMSG", "INTTRA", "TESTSENDER__1", "29"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("GTNBRS", "CMACGM", "CGWS", "C00676795", "WTH"));
      mockContextAccessor.Stub(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("TESTSENDER__1");
      mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("INTTRA_BK");
      mockContextAccessor.Stub(x => x.GetContextProperty("InternalTrackingID", "http://cargowise.com/ehub/tracking/2010/06")).Return("");
      switch (scac)
      {
        case "151521SCAC":
          mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", "INTTRA_BK", "151521SCAA")).Return("151521SCAC").Repeat.AtLeastOnce();
          mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", "INTTRA_BK", "")).Return("");
          break;

        case "INTT":
          mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", "INTTRA_BK", "INTA")).Return("INTT").Repeat.AtLeastOnce();
          mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", "INTTRA_BK", "MSCA")).Return("MSCU");
          break;
        case "CMDU":
          mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", "INTTRA_BK", "CMDA")).Return("CMDU").Repeat.AtLeastOnce();
          break;
      }

      var extensionObjects = new Dictionary<string, object>() {
                { "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
                { "http://schemas.microsoft.com/BizTalk/2003/DateMapper", mockDateMapper },
                { "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
                { "http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor", mockDataModelAccessor },
            };
      MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.Execute<UniversalShipment2IFTMBF_INTTRA>(input, expectedOutput);
    }
  }
}
