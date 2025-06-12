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
  public class IFTDGN_SOAPEnveloppe_Tests
  {
    const string filePath = "CPOINT.IFTDGN_SOAPEnveloppe.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestUShipment2IFTDGN_SOAPEnveloppe()
    {
        AssertMapping("Test1_input.xml", "Test1_output_SOAP.xml");
        AssertMapping("Test2_input_UDM_Amendment_Export_IFTDGN.xml", "Test2_output_UDM_Amendment_Export_IFTDGN_SOAP.xml");
        AssertMapping("Test3_input_UDM_WithDrawal_Export_IFTDGN.xml", "Test3_output_UDM_WithDrawal_Export_IFTDGN_SOAP.xml");
    }

    void AssertMapping(string inputFile, string expectedOutputFile)
    {
        var input = filePath + inputFile;
        var expectedOutput = filePath + expectedOutputFile;

        var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
        var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
        var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();

        var serviceProvider = "CPOINT";
        var destinationParty = serviceProvider + "_IFTDGN1";

        mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("TESTSENDER");
        mockContextAccessor.Expect(x => x.GetContextProperty("MessageTrackingID", "http://cargowise.com/ehub/tracking/2010/06")).Return("55555");
        mockCodeMapper.Expect(x => x.GetRecipientCode("CPOINT", "CPOINT", "CPOINT System Configuration", "Connection Details", "Username", "IFTDGN")).Return("USERNAME_XXX");
        mockCodeMapper.Expect(x => x.GetRecipientCode("CPOINT", "CPOINT", "CPOINT System Configuration", "Connection Details", "Password", "IFTDGN")).Return("PASSWORD_XXX");

        mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CPTMSG", "CPOINT", "TESTSENDER", "55555", "PSNPSN00000000101", "CheckPoint"));

        var extensionObjects = new Dictionary<string, object>()
        {
            { "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
            { "http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor", mockDataModelAccessor },
            { "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
        };

        var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
        mapTester.ExecuteCompiled<IFTDGN_SOAPEnveloppe>(input, expectedOutput);

        mockContextAccessor.VerifyAllExpectations();
        mockDataModelAccessor.VerifyAllExpectations();
        mockCodeMapper.VerifyAllExpectations();
    }
}
}