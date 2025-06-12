using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Helper;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.UShipment2UInterchangeInclude;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests
{
  [TestClass]
  public class UShipment2UInterchangeInclude_Tests
  {
    const string filePath = "UShipment2UInterchangeInclude.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestUShipment2UInterchangeInclude()
    {
      AssertMapping("Test1_input.xml", "Test1_output.xml", destinationParty: "CONTAINER_TRACKING");
      AssertMapping("Test2_input.xml", "Test2_output.xml", destinationParty: "CONTAINER_TRACKING");
      AssertMapping("Test3_input.xml", "Test3_output.xml");
      AssertMapping("Test4_input.xml", "Test4_output.xml");
      AssertMapping("Test5_input.xml", "Test5_output.xml");
      AssertMapping("Test6_input.xml", "Test6_output.xml");
    }

    void AssertMapping(string inputFile, string expectedOutputFile, string destinationParty = "CARGOWISE", string senderID = "TESTSENDER__1")
    {
      var input = filePath + inputFile;
      var expectedOutput = filePath + expectedOutputFile;

      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
      var mockOCMHelper = MockRepository.GenerateStrictMock<OCMHelper>();

      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(senderID);
      mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(destinationParty);

      mockDateMapper.Expect(x => x.CurrentDateTimeUTC("yyyy-MM-ddTHH:mm:ss.fff")).Return("2020-01-15T11:59:59.999").Repeat.Any();

      mockOCMHelper.Expect(x => x.IsCoLoad("AGT")).Return("FALSE").Repeat.Any();
      mockOCMHelper.Expect(x => x.IsCoLoad("XYZ")).Return("FALSE").Repeat.Any();
      mockOCMHelper.Expect(x => x.IsCoLoad("CLD")).Return("TRUE").Repeat.Any();

      var extensionObjects = new Dictionary<string, object>() {
        { "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/DateMapper", mockDateMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
        { "http://schemas.microsoft.com/BizTalk/2003/OCMHelper", mockOCMHelper },
      };

      var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.ExecuteCompiled<UShipment2UInterchangeInclude>(input, expectedOutput);

      mockDateMapper.VerifyAllExpectations();
      mockCodeMapper.VerifyAllExpectations();
      mockContextAccessor.VerifyAllExpectations();
      mockOCMHelper.VerifyAllExpectations();
    }
  }
}