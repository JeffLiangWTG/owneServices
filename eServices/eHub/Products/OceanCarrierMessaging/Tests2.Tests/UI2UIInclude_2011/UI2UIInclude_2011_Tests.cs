using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Helper;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.UI2UIInclude_2011;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests
{
  [TestClass]
  public class UI2UIInclude_2011_Tests
  {
    const string filePath = "UI2UIInclude_2011.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestUI2UIInclude_2011()
    {
      AssertMapping("Test1_input.xml", "Test1_output_AGT.xml", "AGT");
      AssertMapping("Test1_input.xml", "Test1_output_CLD.xml", "CLD");
      AssertMapping("Test2_input.xml", "Test2_output_AGT.xml", "AGT");
      AssertMapping("Test2_input.xml", "Test2_output_CLD.xml", "CLD");

      AssertMapping("Test3_input.xml", "Test3_output.xml", "AGT");
    }

    void AssertMapping(string inputFile, string expectedOutputFile, string shipmentType)
    {
      var input = filePath + inputFile;
      var expectedOutput = filePath + expectedOutputFile;

      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockOCMHelper = MockRepository.GenerateStrictMock<OCMHelper>();

      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("ECULINE").Repeat.AtLeastOnce();

      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "MSGID", "ECULINE")).Return("ECUMSG").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "ECULINE", "@recipientId", "", "@ST_ID", "ECUMSG", "@value", "C2000176981", "@referenceType", "ShipmentType")).Return(shipmentType).Repeat.Any();

      mockOCMHelper.Expect(x => x.IsCoLoad("")).Return("FALSE").Repeat.Any();
      mockOCMHelper.Expect(x => x.IsCoLoad("AGT")).Return("FALSE").Repeat.Any();
      mockOCMHelper.Expect(x => x.IsCoLoad("CLD")).Return("TRUE").Repeat.Any();

      var extensionObjects = new Dictionary<string, object>()
      {
        {"http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper},
        {"http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor},
        {"http://schemas.microsoft.com/BizTalk/2003/OCMHelper", mockOCMHelper},
      };

      MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.Execute<UI2UIInclude_2011>(input, expectedOutput);
      //mapTester.ExecuteCompiledWithXslDebug<UI2UIInclude_2011>(input, expectedOutput, "C:\\eServices_Dev05\\eHub\\Products\\OceanCarrierMessaging\\UI2UIInclude_2011\\UI2UIInclude_2011.xsl");

      mockCodeMapper.VerifyAllExpectations();
      mockContextAccessor.VerifyAllExpectations();
      mockOCMHelper.VerifyAllExpectations();
    }
  }
}