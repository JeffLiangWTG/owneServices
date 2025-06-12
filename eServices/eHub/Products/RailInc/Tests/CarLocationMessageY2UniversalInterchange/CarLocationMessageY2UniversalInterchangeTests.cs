using System;
using System.IO;
using System.Reflection;
using System.Xml.Linq;
using CargoWise.eHub.Products.RailInc.Transforms.CarLocationMessageY2UniversalInterchange;
using Microsoft.BizTalk.TestTools.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using System.Collections.Generic;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.RailInc.Tests
{
  [TestClass]
  public class CarLocationMessageY2UniversalInterchangeTests
  {
    const string filePath = "CarLocationMessageY2UniversalInterchange.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestCarLocationMessageY2UniversalInterchange()
    {
      AssertMapping("Text.CVHFLEET_Input.xml", "Text.CVHFLEET_Output.xml", eventTypeCode: "W", eventType: "RLS", eventParameters: "Location=Sighting");
      AssertMapping("Text.BNSRL1_Input.xml", "Text.BNSRL1_Output.xml", eventTypeCode: "Y", eventType: "DRC", eventParameters: "Facility=Rail Terminal|Location=Sighting");
      AssertMapping("Text.XX_Input.xml", "Text.XX_Output.xml");
      AssertMapping("Text.XX_Input.xml", "Text.XX_Output_C_NotMapped.xml", eventType: "NotMapped");
      AssertMapping("ETADestinationEventCodeA_Input.xml", "ETADestinationEventCodeA_Output.xml", requiredETA: false);
      AssertMapping("ETADestinationEventCodeBlank_Input.xml", "ETADestinationEventCodeBlank_Output.xml", requiredETA: false);
      AssertMapping("ETADestinationEventCodeA_Input.xml", "ETADestinationEventCodeA_Output_ContainerTracking.xml", requiredETA: false, destinationParty: "CONTAINER_TRACKING");
      AssertMapping("ETADestinationEventCodeA_Input.xml", "ETADestinationEventCodeA_Output_Unlocode.xml", requiredETA: false, unlocode: "USMCO");
    }

    void AssertMapping(string sourceFile, string expectedFile, string eventTypeCode = "C", string eventType = "ARV", string eventParameters = "Facility=Initial Rail Terminal|Location=Sighting",
                       bool requiredETA = true, string destinationParty = "MIQMCIMKC", string unlocode = "")
    {

      var input = filePath + sourceFile;
      var expectedOutput = filePath + expectedFile;

      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();

      mockCodeMapper.Expect(x => x.GetRecipientCode("RAILINCFC", "RAILINCFC", "RailSight - Receive CLM messages", "Railinc Code", "Event Type", eventTypeCode)).Return(eventType).Repeat.Times(1);
      if (!eventType.Equals("NotMapped"))
      {
        mockCodeMapper.Expect(x => x.GetRecipientCode("RAILINCFC", "RAILINCFC", "RailSight - Receive CLM messages", "Railinc Code", "Event Parameters", eventTypeCode)).Return(eventParameters).Repeat.Times(1);
        mockCodeMapper.Expect(x => x.GetRecipientCode("RAILINCFC", "RAILINCFC", "RailSight - Receive CLM messages", "Railinc Code", "Event Reference", eventTypeCode)).Return("").Repeat.Times(1);
      }

      if (requiredETA)
      {
        mockCodeMapper.Expect(x => x.GetRecipientCode("RAILINCFC", "RAILINCFC", "RailSight - Receive CLM messages", "Railinc Code", "Event Type", "ETA")).Return("ARV").Repeat.Times(1);
        mockCodeMapper.Expect(x => x.GetRecipientCode("RAILINCFC", "RAILINCFC", "RailSight - Receive CLM messages", "Railinc Code", "Event Parameters", "ETA")).Return("Facility=Transit Rail Terminal|Location=ETA").Repeat.Times(1);
        mockCodeMapper.Expect(x => x.GetRecipientCode("RAILINCFC", "RAILINCFC", "RailSight - Receive CLM messages", "Railinc Code", "Event Reference", "ETA")).Return("Rail").Repeat.Times(1);
      }

      mockContextAccessor.Expect(x => x.GetContextProperty("OverrideEmailSubject", "http://cargowise.com/ehub/processing/2010/06")).Return("FOOBAR").Repeat.Once();
      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("RAILINCFC").Repeat.Once();
      mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(destinationParty).Repeat.AtLeastOnce();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedClients", "", "@senderId", "RAILINCFC", "@ST_ID", "RLC", "@value", "FOOBAR|C00008244_0")).Return("HYEBNEUAT").Repeat.Any();

      if (!destinationParty.Equals("CONTAINER_TRACKING"))
      {
        mockContextAccessor.Expect(x => x.SetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", "HYEBNEUAT"));
      }
      else
      {
        mockCodeMapper.Expect(x => x.GetRecipientCode("RAILINCFC", "RAILINCFC", "RailSight - Receive CLM messages", "Unlocode", "Unlocode", "439900000")).Return(unlocode).Repeat.AtLeastOnce();
      }

      var extensionObjects = new Dictionary<string, object>() {
        { "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
      };

      MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.Execute<CarLocationMessageY2UniversalInterchange>(input, expectedOutput);
      mockCodeMapper.VerifyAllExpectations();
    }
  }
}
