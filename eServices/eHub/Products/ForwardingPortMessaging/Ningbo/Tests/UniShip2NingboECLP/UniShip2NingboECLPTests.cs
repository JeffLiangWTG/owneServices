using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.ForwardingPortMessaging.Ningbo.Transforms.UniShip2NingboECLP;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.ForwardingPortMessaging.Ningbo.Tests
{
  [TestClass]
  public class UniShip2NingboECLPTests
  {
    const string filePath = "UniShip2NingboECLP.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestUniShip2NingboECLP()
    {
      AssertMapping1("Test1_input.xml", "Test1_output.xml");
      AssertMapping2("Test1_output.xml", "Test1_output(Cleanup).xml");
      AssertMapping1("Test2_input.xml", "Test2_output.xml");
    }

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestUniShip2NingboECLPCharCleanup()
    {
      var input = filePath + "Test3Cleanup_input.xml";
      var expectedOutput = filePath + "Test3Cleanup_output.xml";
      var mapTester = new MapTester(Assembly.GetExecutingAssembly());
       mapTester.Execute<ECLP2ECLP_CharCleanup>(input, expectedOutput);
    }

    void AssertMapping1(string inputFile, string expectedOutputFile)
    {
      var input = filePath + inputFile;
      var expectedOutput = filePath + expectedOutputFile;

      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
      var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();

      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("TESTSENDER__1");
      mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("NGBMSG_EP1");
      mockContextAccessor.Expect(x => x.GetContextProperty("InternalTrackingID", "http://cargowise.com/ehub/tracking/2010/06")).Return("36ACCF3C-893F-40CE-BA01-1D8CBE2DF678");
      mockDataModelAccessor.Expect(x => x.GetClientRegistrationCode("TESTSENDER__1", "SHA", "NGBEDI")).Return("DHLNGB");
      mockContextAccessor.Stub(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "ECLP_DHLNGB_3"));

      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("NGBMSG", "NGBEDI_EP1", "TESTSENDER__1", "3", "Shipping Order", "DocumentName"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("NGBMSG", "NGBEDI_EP1", "TESTSENDER__1", "3", "ForwardingShipment", "ForwardingType"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("NGBMSG", "NGBEDI_EP1", "TESTSENDER__1", "3", "Shipment", "SubMessageType"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("NGBMSG", "NGBEDI_EP1", "TESTSENDER__1", "36ACCF3C-893F-40CE-BA01-1D8CBE2DF678", "3"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("NGBMSG", "NGBEDI_EP1", "TESTSENDER__1", "C00681448", "TGMU6548454"));

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.ForwardingPortMessaging.Transforms.NGBEDI.ECLP", "@maxlength", "14")).Return("3");
      mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "SCAC", "Output Code", "NGBMSG_EP1", "KKLU")).Return("KKLU");
      mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "ContainerTypeToISOCode", "Carrier Code", "25G0")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "ContainerTypeToISOCode", "Carrier Code", "20RE")).Return("20RE").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("NGBEDI", "NGBEDI", "NGBEDI Provider Configuration", "ContainerTypeToISOCode", "NGBEDI Code", "25G0")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("NGBEDI", "NGBEDI", "NGBEDI Provider Configuration", "ContainerTypeToISOCode", "NGBEDI Code", "20RE")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("NGBEDI", "NGBEDI", "NGBEDI Provider Configuration", "Package Type", "NGBEDI Code", "PLT")).Return("");
      mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Package Type ISO", "Package Type", "PLT")).Return("PT");

      var extensionObjects = new Dictionary<string, object>(){
        { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockCodeMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockDateMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS3", mockContextAccessor },
        { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS4", mockDataModelAccessor },
      };

      var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.Execute<UniShip2NingboECLP>(input, expectedOutput);

      mockDateMapper.VerifyAllExpectations();
      mockCodeMapper.VerifyAllExpectations();
      mockContextAccessor.VerifyAllExpectations();
    }

    void AssertMapping2(string inputFile, string expectedOutputFile)
    {
      var input = filePath + inputFile;
      var expectedOutput = filePath + expectedOutputFile;

      var mapTester = new MapTester(Assembly.GetExecutingAssembly());
      mapTester.Execute<ECLP2ECLP_CharCleanup>(input, expectedOutput);
    }
  }
}
