using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.GCT.Transforms.IFTSTA2UInterchange;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.GCT.Tests
{
  [TestClass]
  public class IFTSTA2UInterchange_D00B_Test
  {
    const string filePath = "IFTSTA2UInterchange.D00B.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void IFTSTA2UInterchange()
    {
      AssertMapping("YANGMING", "Test1_input.xml", "Test1a_output.xml", "test reference VD1", "|Facility=CTO|Department=Carrier|Type=Repair Authorization|Reason=Fumugation|IsEstimate=true");
      AssertMapping("YANGMING", "Test2_input.xml", "Test2a_output.xml", "test reference VA1", "|Facility=CTO|Department=Carrier|Type=Repair Authorization|Reason=Fumugation|IsEstimate=false");
      AssertMapping("YANGMING", "Test1_input.xml", "Test1b_output.xml", "test reference VD1", "");
      AssertMapping("YANGMING", "Test2_input.xml", "Test2b_output.xml", "test reference VA1", "");
      AssertMapping("YANGMING", "Test3_input.xml", "Test3_output.xml", "Rail", "");
      AssertMapping("YANGMING", "Test4_input.xml", "Test4_output.xml", "test reference VD1", "|Facility=CTO|Department=Carrier|Type=Repair Authorization|Reason=Fumugation|IsEstimate=true");
      AssertMapping("YANGMING", "Test5_input.xml", "Test5_output.xml", "test reference VA1", "", "");
      AssertMapping("YANGMING", "Test6_input.xml", "Test6_output.xml", "test reference VD1", "", "");
      AssertMapping("YANGMING", "Test7_input.xml", "Test7_output.xml", "test reference UV1", "|Facility=CTO", "");
      AssertMapping("YANGMING", "Test8_input.xml", "Test8_output.xml", "test reference UV1", "|Facility=CTO", "");
      AssertMapping("YANGMING", "Test9_input.xml", "Test9_output.xml", "test reference UV1", "|Facility=CTO", "");
    }

		void AssertMapping(string carrierCode, string inputFile, string expectedOutputFile, string testReference, string eventParameters, string expectedUNB2 = "UNB2")// bool carrierHasCodeMapping)
    {
      var input = filePath + inputFile;
      var expectedOutput = filePath + expectedOutputFile;

      var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();

      var carrierMappingName = carrierCode + " IFTSTA to UniversalInterchange";

      mockDateMapper.Expect(x => x.CurrentDateTime(Arg.Is("s"))).Return("2015-06-01T09:30:10");

      mockCodeMapper.Expect(x => x.GetRecipientCode(carrierCode, carrierCode, carrierMappingName, "Container Status", "Event Type", "1")).Return("DEP").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode(carrierCode, carrierCode, carrierMappingName, "Container Status", "Event Type", "2")).Return("ARV").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode(carrierCode, carrierCode, carrierMappingName, "Container Status", "Event Type", "3")).Return("XXX").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode(carrierCode, carrierCode, carrierMappingName, "Container Status", "Event Parameters", "1")).Return(eventParameters).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode(carrierCode, carrierCode, carrierMappingName, "Container Status", "Event Parameters", "2")).Return(eventParameters).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode(carrierCode, carrierCode, carrierMappingName, "Container Status", "Event Parameters", "3")).Return(eventParameters).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode(carrierCode, carrierCode, carrierMappingName, "Container Status", "Is Estimate", "1")).Return("TRUE").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode(carrierCode, carrierCode, carrierMappingName, "Container Status", "Is Estimate", "2")).Return("FALSE").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode(carrierCode, carrierCode, carrierMappingName, "Container Status", "Is Estimate", "3")).Return("false").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode(carrierCode, carrierCode, carrierMappingName, "Container Status", "Event Reference", "1")).Return(testReference).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode(carrierCode, carrierCode, carrierMappingName, "Container Status", "Event Reference", "2")).Return(testReference).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode(carrierCode, carrierCode, carrierMappingName, "Container Status", "Event Reference", "3")).Return(testReference).Repeat.Any();


      mockCodeMapper.Expect(x => x.GetRecipientCode("GCTIFTSTA", "GCTIFTSTA", "Generic IFTSTA to UniversalInterchange", "Container Status", "Event Reference", "2")).Return("test reference VA2").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("GCTIFTSTA", "GCTIFTSTA", "Generic IFTSTA to UniversalInterchange", "Container Status", "Event Reference", "1")).Return("test reference VD2").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("GCTIFTSTA", "GCTIFTSTA", "Generic IFTSTA to UniversalInterchange", "Container Status", "Event Parameters", "1")).Return("|Facility=CTO|Department=Carrier|Type=Repair Authorization|Reason=TestVD2B|IsEstimate=true").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("GCTIFTSTA", "GCTIFTSTA", "Generic IFTSTA to UniversalInterchange", "Container Status", "Event Parameters", "2")).Return("|Facility=CTO|Department=Carrier|Type=Repair Authorization|Reason=TestVA2B|IsEstimate=false").Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("GCTIFTSTA", "GCTIFTSTA", "Generic IFTSTA to UniversalInterchange", "SCAC", "SCAC", carrierCode, "")).Return("AAAA").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("GCTIFTSTA", "GCTIFTSTA", "Generic IFTSTA to UniversalInterchange", "SCAC", "SCAC", carrierCode, "CMDU")).Return("CMDU").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("GCTIFTSTA", "GCTIFTSTA", "Generic IFTSTA to UniversalInterchange", "SCAC", "SCAC", carrierCode, "XXXX")).Return("XXXX").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("GCTIFTSTA", "GCTIFTSTA", "Generic IFTSTA to UniversalInterchange", "SCAC", "SCAC", carrierCode, "UNB2")).Return(expectedUNB2).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("GCTIFTSTA", "GCTIFTSTA", "Generic IFTSTA to UniversalInterchange", "SCAC", "SCAC", carrierCode, "HLCU")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("GCTIFTSTA", "GCTIFTSTA", "Generic IFTSTA to UniversalInterchange", "SCAC", "SCAC", carrierCode, "CMDX")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("GCTIFTSTA", "GCTIFTSTA", "Generic IFTSTA to UniversalInterchange", "SCAC", "SCAC", carrierCode, "SUDU")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("GCTIFTSTA", "GCTIFTSTA", "Generic IFTSTA to UniversalInterchange", "ISOCodeToContainerType", "Container Type", "1")).Return("22G1").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("GCTIFTSTA", "GCTIFTSTA", "Generic IFTSTA to UniversalInterchange", "ISOCodeToContainerType", "Container Type", "2")).Return("45G0").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("GCTIFTSTA", "GCTIFTSTA", "Generic IFTSTA to UniversalInterchange", "ISOCodeToContainerType", "Container Type", "3")).Return("45G1").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("GCTIFTSTA", "GCTIFTSTA", "Generic IFTSTA to UniversalInterchange", "ISOCodeToContainerType", "Container Type", "4")).Return("4510").Repeat.Any();

      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(carrierCode).Repeat.Any();
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", "CONTAINER_TRACKING")).Repeat.Any();
      mockContextAccessor.Expect(x => x.GetContextProperty("UNB2_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("UNB2").Repeat.Any();

      var extensionObjects = new Dictionary<string, object>() {
        { "http://schemas.microsoft.com/BizTalk/2003/DateMapper", mockDateMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor }
      };

      MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.Execute<IFTSTA2UInterchange_D00B>(input, expectedOutput);

      mockDateMapper.VerifyAllExpectations();
      mockCodeMapper.VerifyAllExpectations();
      mockContextAccessor.VerifyAllExpectations();
    }
  }
}
