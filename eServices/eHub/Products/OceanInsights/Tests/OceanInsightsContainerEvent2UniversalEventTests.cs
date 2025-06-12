using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.OceanInsights.Transforms.OInsightsEvent2UniversalEvent;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.OceanInsights.Tests
{
  [TestClass]
  public class OceanInsightsContainerEvent2UniversalEventTests
  {
    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void OceanInsightsContainerEvent2UniversalEvent_ScheduleChanged()
    {
      const string sourceFile = "TestFiles.OceanInsightContainerEvents_schedule.xml";
      const string expectedFile = "TestFiles.OceanInsightContainerEvents_schedule_output.xml";

      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "WTG Code", "VAD")).Return("ARV");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Reference", "VAD")).Return("Final Port of Discharge");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Parameters", "VAD")).Return("Facility=CTO");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Date", "VAD")).Return("pod_vslarrival_");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Location", "VAD")).Return("pod_loc");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Default Location", "VAD")).Return("");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "ContainerTypeToISOCode", "ISO Code", "20GP")).Return("22G0");

      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Related Leg", "VAD")).Return("0");

      var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
      mockDateMapper.Expect(x => x.ConvertUTCToLocalTimeByUNLOCO("2015-08-27", "USOAK")).Return("2015-08-27T00:00:00");

      var extensionObjects = new Dictionary<string, object>()
      { 
        { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockCodeMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockDateMapper }
      };

      var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.Execute<OceanInsightsContainerEvent2UniversalEvent>(sourceFile, expectedFile);
    }

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void OceanInsightsContainerEvent2UniversalEvent_StatusChanged()
    {
      const string sourceFile = "TestFiles.OceanInsightContainerEvents_status.xml";
      const string expectedFile = "TestFiles.OceanInsightContainerEvents_status_output.xml";

      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsStatusUpdate", "Milestone Key", "14")).Return("VDL").Repeat.Times(2);
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsStatusUpdate", "Status Type", "14")).Return("Ocean Transport from Port of Loading");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsStatusUpdate", "Milestone Key", "13")).Return("CLL");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsStatusUpdate", "Status Type", "13")).Return("Waiting for departure from port of load");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsStatusUpdate", "Milestone Key", "12")).Return("CGI");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsStatusUpdate", "Status Type", "12")).Return("At port of load terminal");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsStatusUpdate", "Milestone Key", "11")).Return("");

      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "WTG Code", "VDL")).Return("DEP");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Reference", "VDL")).Return("First Port of Loading");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Parameters", "VDL")).Return("Facility=CTO");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Date", "VDL")).Return("pol_vsldeparture_").Repeat.Times(2);
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Location", "VDL")).Return("pol_loc").Repeat.Times(2);
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "WTG Code", "CLL")).Return("FLO");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Reference", "CLL")).Return("First Port of Loading");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Parameters", "CLL")).Return("Facility=CTO");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Date", "CLL")).Return("pol_loaded_").Repeat.Times(2);
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Location", "CLL")).Return("pol_loc");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "WTG Code", "CGI")).Return("GIN");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Reference", "CGI")).Return("First Port of Loading");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Parameters", "CGI")).Return("Facility=CTO");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Date", "CGI")).Return("pol_arrival_").Repeat.Times(2);
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Location", "CGI")).Return("pol_loc");

      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "ContainerTypeToISOCode", "ISO Code", "20GP")).Return("22G0").Repeat.Times(4);

      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Related Leg", "CEP")).Return("");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Related Leg", "CPS")).Return("");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Related Leg", "CGI")).Return("");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Related Leg", "CLL")).Return("1");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Related Leg", "VDL")).Return("1");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Related Leg", "VAT1")).Return("1");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Related Leg", "CDT1")).Return("1");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Related Leg", "CLT1")).Return("2");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Related Leg", "VDT1")).Return("2");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Related Leg", "VAT2")).Return("2");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Related Leg", "CDT2")).Return("2");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Related Leg", "CLT2")).Return("3");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Related Leg", "VDT2")).Return("3");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Related Leg", "VAT3")).Return("3");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Related Leg", "CDT3")).Return("3");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Related Leg", "CLT3")).Return("4");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Related Leg", "VDT3")).Return("4");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Related Leg", "VAD")).Return("0");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Related Leg", "CDD")).Return("0");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Related Leg", "CGO")).Return("");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Related Leg", "CDC")).Return("");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Related Leg", "CER")).Return("");

      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Default Location", "CPS")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Default Location", "CGI")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Default Location", "CLL")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Default Location", "VDL")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Default Location", "VAT1")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Default Location", "CDT1")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Default Location", "CLT1")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Default Location", "VDT1")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Default Location", "VAT2")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Default Location", "CDT2")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Default Location", "CLT2")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Default Location", "VDT2")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Default Location", "VAT3")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Default Location", "CDT3")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Default Location", "CLT3")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Default Location", "VDT3")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Default Location", "VAD")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Default Location", "CDD")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Default Location", "CGO")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Default Location", "CDC")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Default Location", "CEP")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Default Location", "CER")).Return("").Repeat.Any();

      var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
      mockDateMapper.Expect(x => x.ConvertUTCToLocalTimeByUNLOCO("2015-08-14T00:26:16Z", "CNSHA")).Return("2015-08-14T11:26:16");
      mockDateMapper.Expect(x => x.ConvertUTCToLocalTimeByUNLOCO("2015-08-14", "CNSHA")).Return("2015-08-14T00:00:00");
      mockDateMapper.Expect(x => x.ConvertUTCToLocalTimeByUNLOCO("2015-08-13", "CNSHA")).Return("2015-08-13T00:00:00");

      var extensionObjects = new Dictionary<string, object>()
      { 
        { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockCodeMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockDateMapper }
      };

      var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.Execute<OceanInsightsContainerEvent2UniversalEvent>(sourceFile, expectedFile);
    }
    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void OceanInsightsContainerEvent2UniversalEvent_MiscEvent()
    {
      const string sourceFile = "TestFiles.OceanInsightContainerEvents_misc.xml";
      const string expectedFile = "TestFiles.OceanInsightContainerEvents_misc_output.xml";

      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMiscEvents", "CW1 Event", "3")).Return("STU");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMiscEvents", "Location", "3")).Return("pod_loc");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMiscEvents", "Reference", "3")).Return("6 hours to ETA");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMiscEvents", "Parameters", "3")).Return("MessageType=Pre-Arrival Advice|Type=Vessel approaching POD");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMiscEvents", "Is Estimate", "3")).Return("false");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "ContainerTypeToISOCode", "ISO Code", "20ST")).Return("22G0");

      var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
      mockDateMapper.Expect(x => x.ConvertUTCToLocalTimeByUNLOCO("2015-11-11T02:03+0000", "KRPUS")).Return("2015-11-10T20:03:00");

      var extensionObjects = new Dictionary<string, object>()
      { 
        { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockCodeMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockDateMapper }
      };

      var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.Execute<OceanInsightsContainerEvent2UniversalEvent>(sourceFile, expectedFile);
    }
    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void OceanInsightsContainerEvent2UniversalEvent_MiscEvent_NotFound()
    {
      const string sourceFile = "TestFiles.OceanInsightContainerEvents_misc.xml";
      const string expectedFile = "TestFiles.EmptyInterchange.xml";

      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMiscEvents", "CW1 Event", "3")).Return("");

      var extensionObjects = new Dictionary<string, object>()
      { 
        { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockCodeMapper }
      };

      var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.Execute<OceanInsightsContainerEvent2UniversalEvent>(sourceFile, expectedFile);
    }
    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void OceanInsightsContainerEvent2UniversalEvent_Event()
    {
      const string sourceFile = "TestFiles.RealOceanInsightContainerEvents.xml";
      const string expectedFile = "TestFiles.RealOceanInsightContainerEvents_output.xml";

      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsStatusUpdate", "Milestone Key", "21")).Return("CER").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsStatusUpdate", "Status Type", "21")).Return("Tracking Completed");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "WTG Code", "CER")).Return("DHR");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Reference", "CER")).Return("");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Parameters", "CER")).Return("Facility=CY");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Date", "CER")).Return("empty_return_").Repeat.Times(2);
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Location", "CER")).Return("empty_return_loc").Repeat.Times(2);
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Default Location", "CER")).Return("dlv_loc").Repeat.Times(2);
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "ContainerTypeToISOCode", "ISO Code", "20GP")).Return("22G0").Repeat.Times(2);
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsStatusUpdate", "Milestone Key", "20")).Return("");

      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Related Leg", "CER")).Return("");

      var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
      mockDateMapper.Expect(x => x.ConvertUTCToLocalTimeByUNLOCO("2015-09-16T07:16+0000", "")).Return("2015-09-16T07:16:00");
      mockDateMapper.Expect(x => x.ConvertUTCToLocalTimeByUNLOCO("2015-09-15", "")).Return("2015-09-15T00:00:00");

      var extensionObjects = new Dictionary<string, object>()
      { 
        { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockCodeMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockDateMapper }
      };

      var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.Execute<OceanInsightsContainerEvent2UniversalEvent>(sourceFile, expectedFile);
    }
    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void OceanInsightsContainerEvent2UniversalEvent_EventWith2Leg()
    {
      const string sourceFile = "TestFiles.ContainerEventWith2Leg.xml";
      const string expectedFile = "TestFiles.ContainerEventWith2Leg_output.xml";

      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsStatusUpdate", "Milestone Key", "17")).Return("VAD").Repeat.Times(2);
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsStatusUpdate", "Status Type", "17")).Return("Waiting for discharge at port of discharge");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "WTG Code", "VAD")).Return("ARV");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Reference", "VAD")).Return("Final Port of Discharge");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Parameters", "VAD")).Return("Facility=CTO");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Date", "VAD")).Return("pod_vslarrival_").Repeat.Times(2);
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Location", "VAD")).Return("pod_loc").Repeat.Times(2);
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Default Location", "VAD")).Return("").Repeat.Times(2);
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "ContainerTypeToISOCode", "ISO Code", "40ftHighCubeContainer")).Return("").Repeat.Times(2);
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsStatusUpdate", "Milestone Key", "16")).Return("");

      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Related Leg", "VAD")).Return("0");

      var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
      mockDateMapper.Expect(x => x.ConvertUTCToLocalTimeByUNLOCO("2016-12-20T23:24+0000", "GBSOU")).Return("2016-12-20T23:24+0000");
      mockDateMapper.Expect(x => x.ConvertUTCToLocalTimeByUNLOCO("2016-12-20T21:52+0000", "GBSOU")).Return("2016-12-20T21:52+0000");

      var extensionObjects = new Dictionary<string, object>()
      { 
        { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockCodeMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockDateMapper }
      };

      var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.Execute<OceanInsightsContainerEvent2UniversalEvent>(sourceFile, expectedFile);
    }
    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void OceanInsightsContainerEvent2UniversalEvent1_Event1()
    {
      const string sourceFile = "TestFiles.Event1.xml";
      const string expectedFile = "TestFiles.Event1_output.xml";

      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();

      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "ContainerTypeToISOCode", "ISO Code", "40ftHighCubeReeferContainer")).Return("").Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsStatusUpdate", "Milestone Key", "21")).Return("CER").Repeat.Times(2);
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsStatusUpdate", "Status Type", "21")).Return("Tracking completed");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "WTG Code", "CER")).Return("DHR");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Reference", "CER")).Return("");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Parameters", "CER")).Return("Facility=CY");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Date", "CER")).Return("empty_return_").Repeat.Times(2);
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Location", "CER")).Return("empty_return_loc").Repeat.Times(2);

      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsStatusUpdate", "Milestone Key", "20")).Return("CDC");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsStatusUpdate", "Status Type", "20")).Return("Waiting for empty return");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "WTG Code", "CDC")).Return("HNV");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Reference", "CDC")).Return("");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Parameters", "CDC")).Return("Facility=Place of Delivery|Old=Ocean Carrier");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Date", "CDC")).Return("dlv_delivery_").Repeat.Times(2);
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Location", "CDC")).Return("dlv_loc").Repeat.Times(2);

      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsStatusUpdate", "Milestone Key", "19")).Return("CGO");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsStatusUpdate", "Status Type", "19")).Return("Land transport to Place of Delivery");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "WTG Code", "CGO")).Return("GOU");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Reference", "CGO")).Return("");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Parameters", "CGO")).Return("Facility=CTO");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Date", "CGO")).Return("pod_departure_").Repeat.Times(2);
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Location", "CGO")).Return("pod_loc").Repeat.Times(2);

      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsStatusUpdate", "Milestone Key", "18")).Return("CDD");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsStatusUpdate", "Status Type", "18")).Return("At discharge port terminal");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "WTG Code", "CDD")).Return("FUL");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Reference", "CDD")).Return("Final Port of Discharge");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Parameters", "CDD")).Return("Facility=CTO");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Date", "CDD")).Return("pod_discharge_").Repeat.Times(2);
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Location", "CDD")).Return("pod_loc").Repeat.Times(2);

      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsStatusUpdate", "Milestone Key", "17")).Return("VAD");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsStatusUpdate", "Status Type", "17")).Return("Waiting for discharge at port of discharge");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "WTG Code", "VAD")).Return("ARV");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Reference", "VAD")).Return("Final Port of Discharge");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Parameters", "VAD")).Return("Facility=CTO");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Date", "VAD")).Return("pod_vslarrival_").Repeat.Times(2);
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Location", "VAD")).Return("pod_loc").Repeat.Times(2);

      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsStatusUpdate", "Milestone Key", "16")).Return("VDT3,CLT3,CDT3,VAT3,VDT2,CLT2,CDT2,VAT2,VDT1,CLT1,CDT1,VAT1,VDL");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsStatusUpdate", "Status Type", "16")).Return("Ocean transport to port of discharge");

      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Date", "VDT3")).Return("tsp3_vsldeparture_").Repeat.Times(2);
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Location", "VDT3")).Return("tsp3_loc").Repeat.Times(2);

      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Date", "CLT3")).Return("tsp3_loaded_").Repeat.Times(2);
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Location", "CLT3")).Return("tsp3_loc").Repeat.Times(2);

      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Date", "CDT3")).Return("tsp3_discharge_").Repeat.Times(2);
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Location", "CDT3")).Return("tsp3_loc").Repeat.Times(2);

      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Date", "VAT3")).Return("tsp3_vslarrival_").Repeat.Times(2);
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Location", "VAT3")).Return("tsp3_loc").Repeat.Times(2);

      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Date", "VDT2")).Return("tsp2_vsldeparture_").Repeat.Times(2);
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Location", "VDT2")).Return("tsp2_loc").Repeat.Times(2);

      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Date", "CLT2")).Return("tsp2_loaded_").Repeat.Times(2);
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Location", "CLT2")).Return("tsp2_loc").Repeat.Times(2);

      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Date", "CDT2")).Return("tsp2_discharge_").Repeat.Times(2);
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Location", "CDT2")).Return("tsp2_loc").Repeat.Times(2);

      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Date", "VAT2")).Return("tsp2_vslarrival_").Repeat.Times(2);
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Location", "VAT2")).Return("tsp2_loc").Repeat.Times(2);

      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Date", "VDT1")).Return("tsp1_vsldeparture_").Repeat.Times(2);
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Location", "VDT1")).Return("tsp1_loc").Repeat.Times(2);

      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Date", "CLT1")).Return("tsp1_loaded_").Repeat.Times(2);
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Location", "CLT1")).Return("tsp1_loc").Repeat.Times(2);

      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Date", "CDT1")).Return("tsp1_discharge_").Repeat.Times(2);
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Location", "CDT1")).Return("tsp1_loc").Repeat.Times(2);

      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Date", "VAT1")).Return("tsp1_vslarrival_").Repeat.Times(2);
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Location", "VAT1")).Return("tsp1_loc").Repeat.Times(2);

      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Date", "VDL")).Return("pol_vsldeparture_");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Location", "VDL")).Return("pol_loc");

      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "WTG Code", "VDT2")).Return("DEP");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Reference", "VDT2")).Return("Transshipment Port 2");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Parameters", "VDT2")).Return("Facility=CTO");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Date", "VDT2")).Return("tsp2_vsldeparture_").Repeat.Times(2);
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Location", "VDT2")).Return("tsp2_loc").Repeat.Times(2);

      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "WTG Code", "CLT2")).Return("FLO");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Reference", "CLT2")).Return("Transshipment Port 2");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Parameters", "CLT2")).Return("Facility=CTO");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Date", "CLT2")).Return("tsp2_loaded_").Repeat.Times(2);
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Location", "CLT2")).Return("tsp2_loc").Repeat.Times(2);

      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "WTG Code", "CDT2")).Return("FUL");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Reference", "CDT2")).Return("Transshipment Port 2");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Parameters", "CDT2")).Return("Facility=CTO");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Date", "CDT2")).Return("tsp2_discharge_").Repeat.Times(2);
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Location", "CDT2")).Return("tsp2_loc").Repeat.Times(2);

      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "WTG Code", "VAT2")).Return("ARV");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Reference", "VAT2")).Return("Transshipment Port 2");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Parameters", "VAT2")).Return("Facility=CTO");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Date", "VAT2")).Return("tsp2_vslarrival_").Repeat.Times(2);
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Location", "VAT2")).Return("tsp2_loc").Repeat.Times(2);

      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "WTG Code", "VDT1")).Return("DEP");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Reference", "VDT1")).Return("Transshipment Port 1");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Parameters", "VDT1")).Return("Facility=CTO");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Date", "VDT1")).Return("tsp1_vsldeparture_").Repeat.Times(2);
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Location", "VDT1")).Return("tsp1_loc").Repeat.Times(2);

      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "WTG Code", "CLT1")).Return("FLO");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Reference", "CLT1")).Return("Transshipment Port 1");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Parameters", "CLT1")).Return("Facility=CTO");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Date", "CLT1")).Return("tsp1_loaded_").Repeat.Times(2);
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Location", "CLT1")).Return("tsp1_loc").Repeat.Times(2);

      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "WTG Code", "CDT1")).Return("FUL");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Reference", "CDT1")).Return("Transshipment Port 1");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Parameters", "CDT1")).Return("Facility=CTO");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Date", "CDT1")).Return("tsp1_discharge_").Repeat.Times(2);
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Location", "CDT1")).Return("tsp1_loc").Repeat.Times(2);

      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "WTG Code", "VAT1")).Return("ARV");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Reference", "VAT1")).Return("Transshipment Port 1");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Parameters", "VAT1")).Return("Facility=CTO");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Date", "VAT1")).Return("tsp1_vslarrival_").Repeat.Times(2);
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Location", "VAT1")).Return("tsp1_loc").Repeat.Times(2);

      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "WTG Code", "VDL")).Return("DEP");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Reference", "VDL")).Return("First Port of Loading");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Parameters", "VDL")).Return("Facility=CTO");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Date", "VDL")).Return("pol_vsldeparture_").Repeat.Times(2);
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Location", "VDL")).Return("pol_loc").Repeat.Times(2);

      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsStatusUpdate", "Milestone Key", "15")).Return("VDT3,CLT3,CDT3,VAT3,VDT2,CLT2,CDT2,VAT2,VDT1,CLT1,CDT1,VAT1");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsStatusUpdate", "Status Type", "15")).Return("In transhipment");

      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsStatusUpdate", "Milestone Key", "14")).Return("VDL");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsStatusUpdate", "Status Type", "14")).Return("Ocean transport from port of load");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "WTG Code", "VDL")).Return("DEP");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Reference", "VDL")).Return("First Port of Loading");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Parameters", "VDL")).Return("Facility=CTO");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Date", "VDL")).Return("pol_vsldeparture_").Repeat.Times(2);
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Location", "VDL")).Return("pod_loc").Repeat.Times(2);

      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsStatusUpdate", "Milestone Key", "13")).Return("CLL");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsStatusUpdate", "Status Type", "13")).Return("Waiting for departure from port of load");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "WTG Code", "CLL")).Return("FLO");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Reference", "CLL")).Return("First Port of Loading");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Parameters", "CLL")).Return("Facility=CTO");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Date", "CLL")).Return("pol_loaded_").Repeat.Times(2);
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Location", "CLL")).Return("pol_loc").Repeat.Times(2);  // Changed

      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsStatusUpdate", "Milestone Key", "12")).Return("CGI");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsStatusUpdate", "Status Type", "12")).Return("At port of load terminal");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "WTG Code", "CGI")).Return("GIN");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Reference", "CGI")).Return("First Port of Loading");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Parameters", "CGI")).Return("Facility=CTO");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Date", "CGI")).Return("pol_arrival_").Repeat.Times(2);
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Location", "CGI")).Return("pol_loc").Repeat.Times(2);  // Changed

      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsStatusUpdate", "Milestone Key", "11")).Return("CPS");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsStatusUpdate", "Status Type", "11")).Return("Land transport to port of load terminal");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "WTG Code", "CPS")).Return("HNV");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Reference", "CPS")).Return("");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Parameters", "CPS")).Return("Facility=Place of Receipt|New=Ocean Carrier");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Date", "CPS")).Return("origin_pickup_").Repeat.Times(2);
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Location", "CPS")).Return("origin_loc").Repeat.Times(2);

      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsStatusUpdate", "Milestone Key", "10")).Return("CEP");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsStatusUpdate", "Status Type", "10")).Return("Waiting for full pickup");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "WTG Code", "CEP")).Return("GOU");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Reference", "CEP")).Return("");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Parameters", "CEP")).Return("Facility=CY");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Date", "CEP")).Return("empty_pickup_").Repeat.Times(2);
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Location", "CEP")).Return("empty_pickup_loc").Repeat.Times(2);
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsStatusUpdate", "Milestone Key", "9")).Return("");

      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Related Leg", "CEP")).Return("");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Related Leg", "CPS")).Return("");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Related Leg", "CGI")).Return("");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Related Leg", "CLL")).Return("1");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Related Leg", "VDL")).Return("1");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Related Leg", "VAT1")).Return("1");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Related Leg", "CDT1")).Return("1");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Related Leg", "CLT1")).Return("2");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Related Leg", "VDT1")).Return("2");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Related Leg", "VAT2")).Return("2");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Related Leg", "CDT2")).Return("2");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Related Leg", "CLT2")).Return("3");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Related Leg", "VDT2")).Return("3");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Related Leg", "VAT3")).Return("3");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Related Leg", "CDT3")).Return("3");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Related Leg", "CLT3")).Return("4");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Related Leg", "VDT3")).Return("4");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Related Leg", "VAD")).Return("0");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Related Leg", "CDD")).Return("0");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Related Leg", "CGO")).Return("");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Related Leg", "CDC")).Return("");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Related Leg", "CER")).Return("");

      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Default Location", "CPS")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Default Location", "CGI")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Default Location", "CLL")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Default Location", "VDL")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Default Location", "VAT1")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Default Location", "CDT1")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Default Location", "CLT1")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Default Location", "VDT1")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Default Location", "VAT2")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Default Location", "CDT2")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Default Location", "CLT2")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Default Location", "VDT2")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Default Location", "VAT3")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Default Location", "CDT3")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Default Location", "CLT3")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Default Location", "VDT3")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Default Location", "VAD")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Default Location", "CDD")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Default Location", "CGO")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Default Location", "CDC")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Default Location", "CER")).Return("dlv_loc").Repeat.Times(2);
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Default Location", "CEP")).Return("origin_loc").Repeat.Times(2);

      var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
      mockDateMapper.Expect(x => x.ConvertUTCToLocalTimeByUNLOCO("2017-02-14T05:20+0000", "AUMEL")).Return("2017-02-14T05:20:00").Repeat.Any();
      mockDateMapper.Expect(x => x.ConvertUTCToLocalTimeByUNLOCO("2017-02-14", "AUMEL")).Return("2017-02-14T00:00:00").Repeat.Any();
      mockDateMapper.Expect(x => x.ConvertUTCToLocalTimeByUNLOCO("2017-02-13", "NZNPE")).Return("2017-02-13T00:00:00").Repeat.Any();
      mockDateMapper.Expect(x => x.ConvertUTCToLocalTimeByUNLOCO("2017-02-10", "NZNPE")).Return("2017-02-10T00:00:00").Repeat.Any();
      mockDateMapper.Expect(x => x.ConvertUTCToLocalTimeByUNLOCO("2017-01-26", "AUMEL")).Return("2017-01-26T00:00:00").Repeat.Any();
      mockDateMapper.Expect(x => x.ConvertUTCToLocalTimeByUNLOCO("2017-01-23", "AUMEL")).Return("2017-01-23T00:00:00").Repeat.Any();
      mockDateMapper.Expect(x => x.ConvertUTCToLocalTimeByUNLOCO("2017-01-22", "AUMEL")).Return("2017-01-22T00:00:00").Repeat.Any();
      mockDateMapper.Expect(x => x.ConvertUTCToLocalTimeByUNLOCO("2017-01-16", "SGSIN")).Return("2017-01-16T00:00:00").Repeat.Any();
      mockDateMapper.Expect(x => x.ConvertUTCToLocalTimeByUNLOCO("2017-01-13", "SGSIN")).Return("2017-01-13T00:00:00").Repeat.Any();
      mockDateMapper.Expect(x => x.ConvertUTCToLocalTimeByUNLOCO("2017-01-12", "SGSIN")).Return("2017-01-12T00:00:00").Repeat.Any();
      mockDateMapper.Expect(x => x.ConvertUTCToLocalTimeByUNLOCO("2016-12-12", "CAHAL")).Return("2016-12-12T00:00:00").Repeat.Any();
      mockDateMapper.Expect(x => x.ConvertUTCToLocalTimeByUNLOCO("2016-12-12", "NZNPE")).Return("2016-12-12T00:00:00").Repeat.Any();
      mockDateMapper.Expect(x => x.ConvertUTCToLocalTimeByUNLOCO("2016-12-06", "CAHAL")).Return("2016-12-06T00:00:00").Repeat.Any();
      mockDateMapper.Expect(x => x.ConvertUTCToLocalTimeByUNLOCO("2016-12-01", "SGSIN")).Return("2016-12-01T00:00:00").Repeat.Any();

      var extensionObjects = new Dictionary<string, object>()
      { 
        { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockCodeMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockDateMapper }
      };

      var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.Execute<OceanInsightsContainerEvent2UniversalEvent>(sourceFile, expectedFile);
    }
    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void OceanInsightsContainerEvent2UniversalEvent_TimeoutSubscription()
    {
      const string sourceFile = "TestFiles.OceanInsighSubscriptionTimeoutEvent.xml";
      const string expectedFile = "TestFiles.OceanInsighSubscriptionTimeoutEvent_output.xml";

      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();

      var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
      mockDateMapper.Expect(x => x.ConvertUTCToLocalTimeByUNLOCO("2016-07-21T10:06+0000", "")).Return("2016-07-21T10:06+0000");

      var extensionObjects = new Dictionary<string, object>()
      { 
        { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockCodeMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockDateMapper }
      };

      var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.Execute<OceanInsightsContainerEvent2UniversalEvent>(sourceFile, expectedFile);
    }


    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void OceanInsightsContainerEvent2UniversalEvent_EtaStatus8()
    {
      const string sourceFile = "TestFiles.OIContainerEventEtaStatus8.xml";
      const string expectedFile = "TestFiles.OIContainerEventEtaStatus8_output.xml";

      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMiscEvents", "CW1 Event", "8")).Return("ARV");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMiscEvents", "Location", "8")).Return("pod_loc");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMiscEvents", "Reference", "8")).Return("");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMiscEvents", "Parameters", "8")).Return("Facility=CTO");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMiscEvents", "Is Estimate", "8")).Return("true");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "ContainerTypeToISOCode", "ISO Code", "40ftStandardContainer")).Return("");

      var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
      mockDateMapper.Expect(x => x.ConvertUTCToLocalTimeByUNLOCO("2017-07-24T10:00+0000", "AUBNE")).Return("2017-07-24T10:00:00");

      var extensionObjects = new Dictionary<string, object>()
      { 
        { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockCodeMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockDateMapper }
      };

      var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.Execute<OceanInsightsContainerEvent2UniversalEvent>(sourceFile, expectedFile);
    }

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void OceanInsightsContainerEvent2UniversalEvent_EventCode110()
    {
      const string sourceFile = "TestFiles.OIContainerEventEventCode110ContainerRemoved.xml";
      const string expectedFile = "TestFiles.OIContainerEventEventCode110ContainerRemoved_output.xml";

      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();

      var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
      mockDateMapper.Expect(x => x.ConvertUTCToLocalTimeByUNLOCO("2017-02-07T10:25+0000", "")).Return("2017-02-07T10:25+0000");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "ContainerTypeToISOCode", "ISO Code", "40ftStandardContainer")).Return("");

      var extensionObjects = new Dictionary<string, object>()
      { 
        { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockCodeMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockDateMapper }
      };

      var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.Execute<OceanInsightsContainerEvent2UniversalEvent>(sourceFile, expectedFile);
    }

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void OceanInsightsContainerEvent2UniversalEvent_LifeCycleStatus4()
    {
      const string sourceFile = "TestFiles.OIContainerEventLifeCycleStatus4ContainerRemoved.xml";
      const string expectedFile = "TestFiles.OIContainerEventLifeCycleStatus4ContainerRemoved_output.xml";

      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();

      var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
      mockDateMapper.Expect(x => x.ConvertUTCToLocalTimeByUNLOCO("2017-02-07T10:25+0000", "")).Return("2017-02-07T10:25+0000");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "ContainerTypeToISOCode", "ISO Code", "40ftStandardContainer")).Return("");

      var extensionObjects = new Dictionary<string, object>()
      { 
        { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockCodeMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockDateMapper }
      };

      var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.Execute<OceanInsightsContainerEvent2UniversalEvent>(sourceFile, expectedFile);
    }

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void OceanInsightsContainerEvent2UniversalEvent_NewActualsStatus9()
    {
      const string sourceFile = "TestFiles.NewActualReceived.xml";
      const string expectedFile = "TestFiles.NewActualReceived_output.xml";

      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsStatusUpdate", "Milestone Key", "17")).Return("VAD").Repeat.Times(2);
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsStatusUpdate", "Status Type", "17")).Return("Waiting for discharge at port of discharge");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Location", "VAD")).Return("pod_loc").Repeat.Times(2);
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Related Leg", "VAD")).Return("0");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Date", "VAD")).Return("pod_vslarrival_");

      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "WTG Code", "VAD")).Return("ARV");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Reference", "VAD")).Return("");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Parameters", "VAD")).Return("Facility=CTO	");

      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsStatusUpdate", "Milestone Key", "16")).Return("VDT3,CLT3,CDT3,VAT3,VDT2,CLT2,CDT2,VAT2,VDT1,CLT1,CDT1,VAT1,VDL");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsStatusUpdate", "Status Type", "16")).Return("Ocean transport to port of discharge");

      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Date", "VDT3")).Return("tsp3_vsldeparture_").Repeat.Times(2);
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Location", "VDT3")).Return("tsp3_loc").Repeat.Times(2);
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Related Leg", "VDT3")).Return("4");

      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Date", "CLT3")).Return("tsp3_loaded_").Repeat.Times(2);
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Location", "CLT3")).Return("tsp3_loc").Repeat.Times(2);
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Related Leg", "CLT3")).Return("4");

      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Date", "CDT3")).Return("tsp3_discharge_").Repeat.Times(2);
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Location", "CDT3")).Return("tsp3_loc").Repeat.Times(2);
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Related Leg", "CDT3")).Return("3");

      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Date", "VAT3")).Return("tsp3_vslarrival_").Repeat.Times(2);
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Location", "VAT3")).Return("tsp3_loc").Repeat.Times(2);
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Related Leg", "VAT3")).Return("3");

      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Date", "VDT2")).Return("tsp2_vsldeparture_").Repeat.Times(2);
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Location", "VDT2")).Return("tsp2_loc").Repeat.Times(2);
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Related Leg", "VDT2")).Return("3");

      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Date", "CLT2")).Return("tsp2_loaded_").Repeat.Times(2);
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Location", "CLT2")).Return("tsp2_loc").Repeat.Times(2);
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Related Leg", "CLT2")).Return("3");

      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Date", "CDT2")).Return("tsp2_discharge_").Repeat.Times(2);
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Location", "CDT2")).Return("tsp2_loc").Repeat.Times(2);
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Related Leg", "CDT2")).Return("2");

      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Date", "VAT2")).Return("tsp2_vslarrival_").Repeat.Times(2);
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Location", "VAT2")).Return("tsp2_loc").Repeat.Times(2);
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Related Leg", "VAT2")).Return("2");

      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Date", "VDT1")).Return("tsp1_vsldeparture_").Repeat.Times(2);
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Location", "VDT1")).Return("tsp1_loc").Repeat.Times(2);
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Related Leg", "VDT1")).Return("2");

      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Date", "CLT1")).Return("tsp1_loaded_").Repeat.Times(2);
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Location", "CLT1")).Return("tsp1_loc").Repeat.Times(2);
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Related Leg", "CLT1")).Return("2");

      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Date", "CDT1")).Return("tsp1_discharge_").Repeat.Times(2);
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Location", "CDT1")).Return("tsp1_loc").Repeat.Times(2);
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Related Leg", "CDT1")).Return("1");

      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Date", "VAT1")).Return("tsp1_vslarrival_").Repeat.Times(2);
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Location", "VAT1")).Return("tsp1_loc").Repeat.Times(2);
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Related Leg", "VAT1")).Return("1");

      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Date", "VDL")).Return("pol_vsldeparture_");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Location", "VDL")).Return("pol_loc");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Related Leg", "VDL")).Return("1");

      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "WTG Code", "VDL")).Return("DEP");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Reference", "VDL")).Return("First Port of Loading");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Parameters", "VDL")).Return("Facility=CTO");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Date", "VDL")).Return("pol_vsldeparture_").Repeat.Times(2);
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Location", "VDL")).Return("pol_loc").Repeat.Times(2);

      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsStatusUpdate", "Milestone Key", "15")).Return("VDT3,CLT3,CDT3,VAT3,VDT2,CLT2,CDT2,VAT2,VDT1,CLT1,CDT1,VAT1");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsStatusUpdate", "Status Type", "15")).Return("In transhipment");

      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsStatusUpdate", "Milestone Key", "14")).Return("VDL");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsStatusUpdate", "Status Type", "14")).Return("Ocean transport from port of load");

      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsStatusUpdate", "Milestone Key", "13")).Return("CLL");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsStatusUpdate", "Status Type", "13")).Return("Waiting for departure from port of load");

      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Location", "CLL")).Return("pol_loc");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Related Leg", "CLL")).Return("1");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Date", "CLL")).Return("pol_loaded_");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "WTG Code", "CLL")).Return("FLO");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Reference", "CLL")).Return("First Port of Loading");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Parameters", "CLL")).Return("Facility=CTO");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Date", "CLL")).Return("pol_loaded_").Repeat.Times(2);
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Location", "CLL")).Return("pol_loc").Repeat.Times(2);  // Changed

      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsStatusUpdate", "Milestone Key", "12")).Return("CGI");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsStatusUpdate", "Status Type", "12")).Return("At port of load terminal");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "WTG Code", "CGI")).Return("GIN");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Reference", "CGI")).Return("First Port of Loading");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Parameters", "CGI")).Return("Facility=CTO");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Date", "CGI")).Return("pol_arrival_").Repeat.Times(2);
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Location", "CGI")).Return("pol_loc").Repeat.Times(2);  // Changed
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Related Leg", "CGI")).Return("");

      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsStatusUpdate", "Milestone Key", "11")).Return("CPS");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsStatusUpdate", "Status Type", "11")).Return("Land transport to port of load terminal");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "WTG Code", "CPS")).Return("HNV");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Reference", "CPS")).Return("");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Parameters", "CPS")).Return("Facility=Place of Receipt|New=Ocean Carrier");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Date", "CPS")).Return("origin_pickup_").Repeat.Times(2);
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Location", "CPS")).Return("origin_loc").Repeat.Times(2);
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Related Leg", "CPS")).Return("");

      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsStatusUpdate", "Milestone Key", "10")).Return("CEP");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsStatusUpdate", "Status Type", "10")).Return("Waiting for full pickup");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "WTG Code", "CEP")).Return("GOU");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Reference", "CEP")).Return("");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Parameters", "CEP")).Return("Facility=CY");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Date", "CEP")).Return("empty_pickup_").Repeat.Times(2);
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Location", "CEP")).Return("origin_loc").Repeat.Times(2);
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsStatusUpdate", "Milestone Key", "9")).Return("");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Related Leg", "CEP")).Return("");

      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "ContainerTypeToISOCode", "ISO Code", "40HQ")).Return("").Repeat.Times(6);

      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Default Location", "CPS")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Default Location", "CGI")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Default Location", "CLL")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Default Location", "VDL")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Default Location", "VAT1")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Default Location", "CDT1")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Default Location", "CLT1")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Default Location", "VDT1")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Default Location", "VAT2")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Default Location", "CDT2")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Default Location", "CLT2")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Default Location", "VDT2")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Default Location", "VAT3")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Default Location", "CDT3")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Default Location", "CLT3")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Default Location", "VDT3")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Default Location", "VAD")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Default Location", "CDD")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Default Location", "CGO")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Default Location", "CDC")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Default Location", "CEP")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Default Location", "CER")).Return("").Repeat.Any();

      var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
      mockDateMapper.Expect(x => x.ConvertUTCToLocalTimeByUNLOCO("2016-08-04T11:31+0000", "USHOU")).Return("2016-08-04T11:31:00").Repeat.Times(1);
      mockDateMapper.Expect(x => x.ConvertUTCToLocalTimeByUNLOCO("2016-08-04T11:11+0000", "USHOU")).Return("2016-08-04T11:11:00").Repeat.Times(1);
      mockDateMapper.Expect(x => x.ConvertUTCToLocalTimeByUNLOCO("2016-07-04", "CNSHA")).Return("2016-07-04T00:00:00").Repeat.Times(2);
      mockDateMapper.Expect(x => x.ConvertUTCToLocalTimeByUNLOCO("2016-07-01", "CNSHA")).Return("2016-07-01T00:00:00").Repeat.Times(2);
      mockDateMapper.Expect(x => x.ConvertUTCToLocalTimeByUNLOCO("2016-06-30", "CNSHA")).Return("2016-06-30T00:00:00").Repeat.Times(2);

      var extensionObjects = new Dictionary<string, object>()
      { 
        { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockCodeMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockDateMapper }
      };

      var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.Execute<OceanInsightsContainerEvent2UniversalEvent>(sourceFile, expectedFile);
    }

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void OceanInsightsContainerEvent2UniversalEvent_Fallback()
    {
      AssertMappingWithLocationFallBack("OceanInsightContainerEvents_Fallback.xml", "OceanInsightContainerEvents_Fallback_1_output.xml", "CER", "empty_return_loc", "dlv_loc", "empty_return_");
      AssertMappingWithLocationFallBack("OceanInsightContainerEvents_Fallback.xml", "OceanInsightContainerEvents_Fallback_2_output.xml", "CEP", "empty_pickup_loc", "origin_loc", "empty_pickup_");
    }

    void AssertMappingWithLocationFallBack(string input, string expectedOutput, string eventCode, string milestoneLocation, string defaultLocation, string milestoneDate)
    {
      var filePath = "TestFiles.";
      var sourceFile = filePath + input;
      var expectedFile = filePath + expectedOutput;


      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsStatusUpdate", "Milestone Key", "20")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsStatusUpdate", "Milestone Key", "21")).Return(eventCode).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsStatusUpdate", "Status Type", "21")).Return("Tracking Completed");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "WTG Code", eventCode)).Return("CER").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Reference", eventCode)).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Parameters", eventCode)).Return("Facility=CY").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Date", eventCode)).Return(milestoneDate).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "ContainerTypeToISOCode", "ISO Code", "20GP")).Return("22G0").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Related Leg", eventCode)).Return("");

      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Milestone Location", eventCode)).Return(milestoneLocation).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OceanInsights_CSS", "OceanInsights_CSS", "OceanInsights ContainerEvent to UniversalEvent", "OceanInsightsMilestones", "Default Location", eventCode)).Return(defaultLocation).Repeat.Any();

      var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
      mockDateMapper.Expect(x => x.ConvertUTCToLocalTimeByUNLOCO("2015-08-26T01:31:49.172675Z", "AAAAA")).Return("2015-08-26T00:00:00").Repeat.Any();
      mockDateMapper.Expect(x => x.ConvertUTCToLocalTimeByUNLOCO("2015-08-26T01:31:49.172675Z", "BBBBB")).Return("2015-08-26T00:00:00").Repeat.Any();
      mockDateMapper.Expect(x => x.ConvertUTCToLocalTimeByUNLOCO("2016-12-22", "AAAAA")).Return("2016-12-22").Repeat.Any();
      mockDateMapper.Expect(x => x.ConvertUTCToLocalTimeByUNLOCO("2016-12-11", "BBBBB")).Return("2016-12-11").Repeat.Any();
      var extensionObjects = new Dictionary<string, object>()
      { 
        { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockCodeMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockDateMapper }
      };

      var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.Execute<OceanInsightsContainerEvent2UniversalEvent>(sourceFile, expectedFile);
    }
  }
}
