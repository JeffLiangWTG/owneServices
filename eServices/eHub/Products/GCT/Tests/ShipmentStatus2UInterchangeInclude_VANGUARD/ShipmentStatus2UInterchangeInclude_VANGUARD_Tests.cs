using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.GCT.Transforms.ShipmentStatus2UInterchangeInclude_VANGUARD;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.GCT.Tests
{
  [TestClass]
  public class ShipmentStatus2UInterchangeInclude_VANGUARG_Tests
  {
    const string filePath = "ShipmentStatus2UInterchangeInclude_VANGUARD.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestShipmentStatus2UInterchangeInclude_VANGUARD()
    {
      AssertMapping("Test1_input.xml", "Test1_output.xml");
      AssertMapping("Test2_input.xml", "Test2_output.xml");
      AssertMapping("Test3_input.xml", "Test3_output.xml");
      AssertMapping("Test4_input.xml", "Test4_output.xml");
    }

    private static void AssertMapping(string inputFile, string expectedOutputFile)
    {

      string input = filePath + inputFile;
      string expectedOutput = filePath + expectedOutputFile;

      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();

      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("VANGUARD").Repeat.Any();
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", "CONTAINER_TRACKING")).Repeat.Any();
      mockDateMapper.Expect(x => x.CurrentDateTimeUTC(Arg.Is("s"))).Return("2020-05-31T09:59:30");

      mockCodeMapper.Expect(x => x.GetRecipientCode("VANGUARD", "VANGUARD", "Shipment Status from VANGUARD", "Event Type", "Event Type", "I")).Return("GIN").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("VANGUARD", "VANGUARD", "Shipment Status from VANGUARD", "Event Type", "Event Reference", "I")).Return("Cargo Received").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("VANGUARD", "VANGUARD", "Shipment Status from VANGUARD", "Event Type", "Event Parameters", "I")).Return("|Facility=CTO|Type=Container").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("VANGUARD", "VANGUARD", "Shipment Status from VANGUARD", "Event Type", "Is Estimate", "I")).Return("FALSE").Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("VANGUARD", "VANGUARD", "Shipment Status from VANGUARD", "ISOCodeToContainerType", "CW1 Code", "45G0")).Return("45G0").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("VANGUARD", "VANGUARD", "Shipment Status from VANGUARD", "ISOCodeToContainerType", "CW1 Code", "45XX")).Return("").Repeat.Any();

      var extensionObjects = new Dictionary<string, object>() {
        { "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
        { "http://schemas.microsoft.com/BizTalk/2003/DateMapper", mockDateMapper },
      };
      MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);

      mapTester.Execute<ShipmentStatus2UInterchangeInclude>(input, expectedOutput);

      mockContextAccessor.VerifyAllExpectations();
      mockCodeMapper.VerifyAllExpectations();
      mockDateMapper.VerifyAllExpectations();
    }
  }
}
