using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.ForwardingPortMessaging.Ningbo.Transforms.UShp2ETRM_NGBEDI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.ForwardingPortMessaging.Ningbo.Tests
{
  [TestClass]
  public class UShp2ETRM_NGBEDI_Tests
  {
    const string filePath = "UShp2ETRM_NGBEDI.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void UShipment2TerminalRelease_NGBEDI()
    {
      AssertMapping("Test1_input.xml", "Test1_output.xml");   //One SubShipment, Payment Method - ELS
      AssertMapping("Test2_input.xml", "Test2_output.xml");   //One SubShipments(Ports), Payment Method - CCX/PPD
      AssertMapping("Test4_input.xml", "Test4_output.xml");   //One SubShipments(Ports), Multiple PackingLine
    }

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TesteManifest2eManifestCharCleanup()
    {
      var input = filePath + "Test3Cleanup_input.xml";
      var expectedOutput = filePath + "Test3Cleanup_output.xml";
      var mapTester = new MapTester(Assembly.GetExecutingAssembly());
      mapTester.Execute<ETRM2ETRM_CharCleanup>(input, expectedOutput);
    }

    void AssertMapping(string inputFile, string expectedOutputFile)
    {
      var input = filePath + inputFile;
      var expectedOutput = filePath + expectedOutputFile;

      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
      var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
      var mockStringMapper = MockRepository.GenerateStrictMock<StringMapper>();

      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("TESTSENDER__1");
      mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("NGBEDI_TR1");
      mockDataModelAccessor.Expect(x => x.GetClientRegistrationCode("TESTSENDER__1", "SHA", "NGBEDI")).Return("ESPS");
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("NGBMSG", "NGBEDI_TR1", "TESTSENDER__1", "3", "Terminal Release", "DocumentName")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("NGBMSG", "NGBEDI_TR1", "TESTSENDER__1", "3", "ForwardingShipment", "ForwardingType")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("NGBMSG", "NGBEDI_TR1", "TESTSENDER__1", "3", "Shipment", "SubMessageType")).Repeat.Any();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.ForwardingPortMessaging.Ningbo.Transforms.NGBEDI.TerminalRelease", "@maxlength", "14")).Return("3");

      mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "SCAC", "Output Code", "NGBEDI_TR1", "MAEU")).Return("SCAC001").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Release Type", "Output Code", "NGBEDI_TR1", "BOL")).Return("BOL").Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("NGBEDI", "NGBEDI", "NGBEDI Provider Configuration", "ContainerTypeToISOCode", "NGBEDI Code", "22G0")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("NGBEDI", "NGBEDI", "NGBEDI Provider Configuration", "ContainerTypeToISOCode", "NGBEDI Code", "20G0")).Return("20G0").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("NGBEDI", "NGBEDI", "NGBEDI Provider Configuration", "Package Type", "NGBEDI Code", "PLT")).Return("PT").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("NGBEDI", "NGBEDI", "NGBEDI Provider Configuration", "Shipping Agent Code", "Output Code", "TESTSENDER__1", "")).Return("DHLNGB").Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "ContainerTypeToISOCode", "Carrier Code", "22G0")).Return("22G0").Repeat.Any();
      mockContextAccessor.Stub(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "ETRM_ESPS_3"));

      var extensionObjects = new Dictionary<string, object>() { 
        { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockCodeMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockDateMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockStringMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS3", mockContextAccessor },
        { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS4", mockDataModelAccessor },
      };

      var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.Execute<UShp2ETRM_NGBEDI>(input, expectedOutput);
      //mapTester.ExecuteCompiledWithXslDebug<UShipment2TerminalRelease_NGBEDI>(input, expectedOutput, @"C:\eServices-Dev3\ehub\Products\ForwardingPortMessaging\Ningbo\UShipment2TerminalRelease\UShipment2TerminalRelease_NGBEDI.xsl");

      mockDateMapper.VerifyAllExpectations();
      mockCodeMapper.VerifyAllExpectations();
      mockContextAccessor.VerifyAllExpectations();
    }
  }
}