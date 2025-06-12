using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.GCT.Transforms.CWTG;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.GCT.Tests
{
  [TestClass]
  public class TrackAndTrace2UInterchangeInclude_Test
  {
    const string filePath = "CWTG.TrackAndTrace.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestTrackAndTrace2UInterchangeInclude_Test()
    {
      AssertMapping("Test1_Import_input.xml", "Test1_Import_output.xml");
      AssertMapping("Test2_Export_input.xml", "Test2_Export_output.xml");
      AssertMapping("Test3_ImportAndExport_input.xml", "Test3_ImportAndExport_output.xml");
      AssertMapping("Test4_ImportAndExport_input.xml", "Test4_ImportAndExport_output.xml");
    }

    private static void AssertMapping(string inputFile, string expectedOutputFile)
    {
      string input = filePath + inputFile;
      string expectedOutput = filePath + expectedOutputFile;

      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();

      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("CWTG").Repeat.Any();
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", "CONTAINER_TRACKING")).Repeat.Any();
      mockDateMapper.Expect(x => x.CurrentDateTimeUTC(Arg.Is("s"))).Return("2017-10-24T03:50:32");

      mockCodeMapper.Expect(x => x.GetRecipientCode(Arg<string>.Is.Equal("CWTG"), Arg<string>.Is.Equal("CWTG"), Arg<string>.Is.Equal("CWTG Provider Configuration"), Arg<string>.Is.Equal("Event Type"), Arg<string>.Is.Equal("Event Type"), Arg<string>.Is.Equal("booking_confirmed"), Arg<string>.Is.Anything)).Return("BKC").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode(Arg<string>.Is.Equal("CWTG"), Arg<string>.Is.Equal("CWTG"), Arg<string>.Is.Equal("CWTG Provider Configuration"), Arg<string>.Is.Equal("Event Type"), Arg<string>.Is.Equal("Event Type"), Arg<string>.Is.Equal("cargo_pickup"), Arg<string>.Is.Anything)).Return("HNV").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode(Arg<string>.Is.Equal("CWTG"), Arg<string>.Is.Equal("CWTG"), Arg<string>.Is.Equal("CWTG Provider Configuration"), Arg<string>.Is.Equal("Event Type"), Arg<string>.Is.Equal("Event Type"), Arg<string>.Is.Equal("cargo_received"), Arg<string>.Is.Anything)).Return("GIN").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode(Arg<string>.Is.Equal("CWTG"), Arg<string>.Is.Equal("CWTG"), Arg<string>.Is.Equal("CWTG Provider Configuration"), Arg<string>.Is.Equal("Event Type"), Arg<string>.Is.Equal("Event Type"), Arg<string>.Is.Equal("cargo_packed"), Arg<string>.Is.Anything)).Return("PKC").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode(Arg<string>.Is.Equal("CWTG"), Arg<string>.Is.Equal("CWTG"), Arg<string>.Is.Equal("CWTG Provider Configuration"), Arg<string>.Is.Equal("Event Type"), Arg<string>.Is.Equal("Event Type"), Arg<string>.Is.Equal("custom_clearance"), Arg<string>.Is.Anything)).Return("CLR").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode(Arg<string>.Is.Equal("CWTG"), Arg<string>.Is.Equal("CWTG"), Arg<string>.Is.Equal("CWTG Provider Configuration"), Arg<string>.Is.Equal("Event Type"), Arg<string>.Is.Equal("Event Type"), Arg<string>.Is.Equal("cargo_launched"), Arg<string>.Is.Anything)).Return("FLO").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode(Arg<string>.Is.Equal("CWTG"), Arg<string>.Is.Equal("CWTG"), Arg<string>.Is.Equal("CWTG Provider Configuration"), Arg<string>.Is.Equal("Event Type"), Arg<string>.Is.Equal("Event Type"), Arg<string>.Is.Equal("vessel_sailed"), Arg<string>.Is.Anything)).Return("DEP").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode(Arg<string>.Is.Equal("CWTG"), Arg<string>.Is.Equal("CWTG"), Arg<string>.Is.Equal("CWTG Provider Configuration"), Arg<string>.Is.Equal("Event Type"), Arg<string>.Is.Equal("Event Type"), Arg<string>.Is.Equal("bl_issued"), Arg<string>.Is.Anything)).Return("STU").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode(Arg<string>.Is.Equal("CWTG"), Arg<string>.Is.Equal("CWTG"), Arg<string>.Is.Equal("CWTG Provider Configuration"), Arg<string>.Is.Equal("Event Type"), Arg<string>.Is.Equal("Event Type"), Arg<string>.Is.Equal("vessel_arrived"), Arg<string>.Is.Anything)).Return("ARV").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode(Arg<string>.Is.Equal("CWTG"), Arg<string>.Is.Equal("CWTG"), Arg<string>.Is.Equal("CWTG Provider Configuration"), Arg<string>.Is.Equal("Event Type"), Arg<string>.Is.Equal("Event Type"), Arg<string>.Is.Equal("cargo_unpacked"), Arg<string>.Is.Anything)).Return("UPC").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode(Arg<string>.Is.Equal("CWTG"), Arg<string>.Is.Equal("CWTG"), Arg<string>.Is.Equal("CWTG Provider Configuration"), Arg<string>.Is.Equal("Event Type"), Arg<string>.Is.Equal("Event Type"), Arg<string>.Is.Equal("do_released"), Arg<string>.Is.Anything)).Return("RLS").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode(Arg<string>.Is.Equal("CWTG"), Arg<string>.Is.Equal("CWTG"), Arg<string>.Is.Equal("CWTG Provider Configuration"), Arg<string>.Is.Equal("Event Type"), Arg<string>.Is.Equal("Event Type"), Arg<string>.Is.Equal("cargo_released"), Arg<string>.Is.Anything)).Return("GOU").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode(Arg<string>.Is.Equal("CWTG"), Arg<string>.Is.Equal("CWTG"), Arg<string>.Is.Equal("CWTG Provider Configuration"), Arg<string>.Is.Equal("Event Type"), Arg<string>.Is.Equal("Event Type"), Arg<string>.Is.Equal("cargo_delivered"), Arg<string>.Is.Anything)).Return("HNV").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode(Arg<string>.Is.Equal("CWTG"), Arg<string>.Is.Equal("CWTG"), Arg<string>.Is.Equal("CWTG Provider Configuration"), Arg<string>.Is.Equal("Event Type"), Arg<string>.Is.Equal("Event Type"), Arg<string>.Is.Equal("fake"), Arg<string>.Is.Anything)).Return("").Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode(Arg<string>.Is.Equal("CWTG"), Arg<string>.Is.Equal("CWTG"), Arg<string>.Is.Equal("CWTG Provider Configuration"), Arg<string>.Is.Equal("Event Type"), Arg<string>.Is.Equal("Event Parameters"), Arg<string>.Is.Equal("booking_confirmed"), Arg<string>.Is.Anything)).Return("|Department=Carrier|Type=NVOCC").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode(Arg<string>.Is.Equal("CWTG"), Arg<string>.Is.Equal("CWTG"), Arg<string>.Is.Equal("CWTG Provider Configuration"), Arg<string>.Is.Equal("Event Type"), Arg<string>.Is.Equal("Event Parameters"), Arg<string>.Is.Equal("cargo_pickup"), Arg<string>.Is.Anything)).Return("|Facility=Place Of Receipt|New=Carrier").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode(Arg<string>.Is.Equal("CWTG"), Arg<string>.Is.Equal("CWTG"), Arg<string>.Is.Equal("CWTG Provider Configuration"), Arg<string>.Is.Equal("Event Type"), Arg<string>.Is.Equal("Event Parameters"), Arg<string>.Is.Equal("cargo_received"), Arg<string>.Is.Anything)).Return("|Facility=CFS").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode(Arg<string>.Is.Equal("CWTG"), Arg<string>.Is.Equal("CWTG"), Arg<string>.Is.Equal("CWTG Provider Configuration"), Arg<string>.Is.Equal("Event Type"), Arg<string>.Is.Equal("Event Parameters"), Arg<string>.Is.Equal("cargo_packed"), Arg<string>.Is.Anything)).Return("|Facility=CFS|Type=Container").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode(Arg<string>.Is.Equal("CWTG"), Arg<string>.Is.Equal("CWTG"), Arg<string>.Is.Equal("CWTG Provider Configuration"), Arg<string>.Is.Equal("Event Type"), Arg<string>.Is.Equal("Event Parameters"), Arg<string>.Is.Equal("custom_clearance"), Arg<string>.Is.Anything)).Return("|Department=Carrier|Type=Cargo").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode(Arg<string>.Is.Equal("CWTG"), Arg<string>.Is.Equal("CWTG"), Arg<string>.Is.Equal("CWTG Provider Configuration"), Arg<string>.Is.Equal("Event Type"), Arg<string>.Is.Equal("Event Parameters"), Arg<string>.Is.Equal("cargo_launched"), Arg<string>.Is.Anything)).Return("|Facility=CTO").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode(Arg<string>.Is.Equal("CWTG"), Arg<string>.Is.Equal("CWTG"), Arg<string>.Is.Equal("CWTG Provider Configuration"), Arg<string>.Is.Equal("Event Type"), Arg<string>.Is.Equal("Event Parameters"), Arg<string>.Is.Equal("vessel_sailed"), Arg<string>.Is.Anything)).Return("|Facility=CTO").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode(Arg<string>.Is.Equal("CWTG"), Arg<string>.Is.Equal("CWTG"), Arg<string>.Is.Equal("CWTG Provider Configuration"), Arg<string>.Is.Equal("Event Type"), Arg<string>.Is.Equal("Event Parameters"), Arg<string>.Is.Equal("bl_issued"), Arg<string>.Is.Anything)).Return("|Department=Carrier|Type=Bill of Lading Issued").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode(Arg<string>.Is.Equal("CWTG"), Arg<string>.Is.Equal("CWTG"), Arg<string>.Is.Equal("CWTG Provider Configuration"), Arg<string>.Is.Equal("Event Type"), Arg<string>.Is.Equal("Event Parameters"), Arg<string>.Is.Equal("vessel_arrived"), Arg<string>.Is.Anything)).Return("|Facility=CTO").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode(Arg<string>.Is.Equal("CWTG"), Arg<string>.Is.Equal("CWTG"), Arg<string>.Is.Equal("CWTG Provider Configuration"), Arg<string>.Is.Equal("Event Type"), Arg<string>.Is.Equal("Event Parameters"), Arg<string>.Is.Equal("cargo_unpacked"), Arg<string>.Is.Anything)).Return("|Facility=CFS|Type=Container").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode(Arg<string>.Is.Equal("CWTG"), Arg<string>.Is.Equal("CWTG"), Arg<string>.Is.Equal("CWTG Provider Configuration"), Arg<string>.Is.Equal("Event Type"), Arg<string>.Is.Equal("Event Parameters"), Arg<string>.Is.Equal("do_released"), Arg<string>.Is.Anything)).Return("|Department=Carrier|Type=Delivery Order").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode(Arg<string>.Is.Equal("CWTG"), Arg<string>.Is.Equal("CWTG"), Arg<string>.Is.Equal("CWTG Provider Configuration"), Arg<string>.Is.Equal("Event Type"), Arg<string>.Is.Equal("Event Parameters"), Arg<string>.Is.Equal("cargo_released"), Arg<string>.Is.Anything)).Return("|Facility=CFS|Old=Carrier").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode(Arg<string>.Is.Equal("CWTG"), Arg<string>.Is.Equal("CWTG"), Arg<string>.Is.Equal("CWTG Provider Configuration"), Arg<string>.Is.Equal("Event Type"), Arg<string>.Is.Equal("Event Parameters"), Arg<string>.Is.Equal("cargo_delivered"), Arg<string>.Is.Anything)).Return("|Facility=Place Of Delivery|Old=Carrier").Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode(Arg<string>.Is.Equal("CWTG"), Arg<string>.Is.Equal("CWTG"), Arg<string>.Is.Equal("CWTG Provider Configuration"), Arg<string>.Is.Equal("Event Type"), Arg<string>.Is.Equal("Event Reference"), Arg<string>.Is.Anything, Arg<string>.Is.Anything)).Return("REF").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode(Arg<string>.Is.Equal("CWTG"), Arg<string>.Is.Equal("CWTG"), Arg<string>.Is.Equal("CWTG Provider Configuration"), Arg<string>.Is.Equal("Event Type"), Arg<string>.Is.Equal("Is Estimate"), Arg<string>.Is.Anything, Arg<string>.Is.Anything)).Return("FALSE").Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("CWTG", "CWTG", "CWTG Provider Configuration", "ISOCodeToContainerType", "CW1 Code", "40HC")).Return("40HC").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("CWTG", "CWTG", "CWTG Provider Configuration", "ISOCodeToContainerType", "CW1 Code", "45XX")).Return("").Repeat.Any();

      var extensionObjects = new Dictionary<string, object>() {
                { "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
                { "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
                { "http://schemas.microsoft.com/BizTalk/2003/DateMapper", mockDateMapper },
            };
      MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);

      mapTester.Execute<TrackAndTrace2UInterchangeInclude>(input, expectedOutput);
    }
  }
}
