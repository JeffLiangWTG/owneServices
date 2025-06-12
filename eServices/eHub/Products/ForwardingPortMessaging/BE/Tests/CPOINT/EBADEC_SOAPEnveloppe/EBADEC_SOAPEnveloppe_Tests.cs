using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.ForwardingPortMessaging.BE.CPOINT;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using System.Collections.Generic;

namespace CargoWise.eHub.Products.ForwardingPortMessaging.BE.Tests
{
  [TestClass]
  public class EBADEC_SOAPEnveloppe_Tests
  {
    const string filePath = "CPOINT.EBADEC_SOAPEnveloppe.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestUShipment2EBADEC_SOAPEnveloppe()
    {
        AssertMapping("Test1_input_containers_ORG_DUNS.xml", "Test1_output_containers_ORG_DUNS_SOAP.xml");
        AssertMapping("Test2_input_containers_AMD_PSN.xml", "Test2_output_containers_AMD_PSN_SOAP.xml");
        AssertMapping("Test3_input_containers_WTH_EOR.xml", "Test3_output_containers_WTH_EOR_SOAP.xml");
        AssertMapping("Test4_input_vehicles_ORG_DUNS.xml", "Test4_output_vehicles_ORG_DUNS_SOAP.xml");
        AssertMapping("Test5_input_vehicles_AMD_PSN.xml", "Test5_output_vehicles_AMD_PSN_SOAP.xml");
        AssertMapping("Test6_input_vehicles_WTH_EOR.xml", "Test6_output_vehicles_WTH_EOR_SOAP.xml");
        AssertMapping("Test7_input_containers_FWSubShip.xml", "Test7_output_containers_FWSubShip_SOAP.xml");
        AssertMapping("Test8_input_vehicles_FWSubShip.xml", "Test8_output_vehicles_FWSubShip_SOAP.xml");
    }

    void AssertMapping(string inputFile, string expectedOutputFile)
    {
        var input = filePath + inputFile;
        var expectedOutput = filePath + expectedOutputFile;

        var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
        var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
        var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();

        var serviceProvider = "CPOINT";
        var destinationParty = serviceProvider + "_EBADEC1";

        mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("TESTSENDER");
        mockContextAccessor.Expect(x => x.GetContextProperty("MessageTrackingID", "http://cargowise.com/ehub/tracking/2010/06")).Return("55555");

        mockCodeMapper.Expect(x => x.GetRecipientCode("CPOINT", "CPOINT", "CPOINT System Configuration", "Connection Details", "Username", "EBADEC")).Return("USERNAME_XXX");
        mockCodeMapper.Expect(x => x.GetRecipientCode("CPOINT", "CPOINT", "CPOINT System Configuration", "Connection Details", "Password", "EBADEC")).Return("PASSWORD_XXX");

        mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CPTMSG", "CPOINT", "TESTSENDER", "55555", "CPO0000000099", "CheckPoint"));

            var extensionObjects = new Dictionary<string, object>()
        {
            { "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
            { "http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor", mockDataModelAccessor },
            { "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
        };

        var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
        mapTester.ExecuteCompiled<EBADEC_SOAPEnveloppe>(input, expectedOutput);

        mockContextAccessor.VerifyAllExpectations();
        mockDataModelAccessor.VerifyAllExpectations();
        mockCodeMapper.VerifyAllExpectations();
    }
}
}