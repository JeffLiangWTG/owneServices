using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.GCT.Transforms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.XLANGs.BaseTypes;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.GCT.Tests
{
	[TestClass]
	public class IFTSTA2UInterchange_Generic_Test
	{
		const string filePath = "IFTSTA2UInterchange_Generic.TestFiles.";

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void IFTSTA2UInterchange_Generic()
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

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void IFTSTA2UInterchange_XmlEnvelope()
		{
			var extensionObjects = GetExtensionObjects("CMACGM", null, null, "");
			var mockCodeMapper = extensionObjects["http://schemas.microsoft.com/BizTalk/2003/ScriptNS1"] as CodeMapper;
			mockCodeMapper.Expect(x => x.GetRecipientCode("CMACGM", "CMACGM", "CMACGM IFTSTA to UniversalInterchange", "Container Status", "Event Reference", "VD")).Return("test reference VD1").Repeat.Once();
			mockCodeMapper.Expect(x => x.GetRecipientCode("CMACGM", "CMACGM", "CMACGM IFTSTA to UniversalInterchange", "Container Status", "Event Reference", "VA")).Return("test reference VA1").Repeat.Twice();
			mockCodeMapper.Expect(x => x.GetRecipientCode("CMACGM", "CMACGM", "CMACGM IFTSTA to UniversalInterchange", "Container Status", "Event Reference", "UV")).Return("test reference UV1").Repeat.Once();
			mockCodeMapper.Expect(x => x.GetRecipientCode("CMACGM", "CMACGM", "CMACGM IFTSTA to UniversalInterchange", "Container Status", "Event Parameters", "VD")).Return("|Facility=CTO|Department=Carrier|Type=Repair Authorization|Reason=Fumugation|IsEstimate=true").Repeat.Once();
			mockCodeMapper.Expect(x => x.GetRecipientCode("CMACGM", "CMACGM", "CMACGM IFTSTA to UniversalInterchange", "Container Status", "Event Parameters", "VA")).Return("|Facility=CTO|Department=Carrier|Type=Repair Authorization|Reason=Fumugation|IsEstimate=false").Repeat.Twice();
			mockCodeMapper.Expect(x => x.GetRecipientCode("CMACGM", "CMACGM", "CMACGM IFTSTA to UniversalInterchange", "Container Status", "Event Parameters", "UV")).Return("|Facility=CTO").Repeat.Once();

			var inputFile = filePath + "TestPreserveInterchange_input.xml";
			var expectedOutputFile = filePath + "TestPreserveInterchange_output.xml";
			AssertMapping<XmlEnvelopeIFTSTA2UInterchange>(inputFile, expectedOutputFile, extensionObjects);
		}

		void AssertMapping(string carrierCode, string inputFile, string expectedOutputFile, string testReference, string eventParameters, string expectedUNB2 = "UNB2")// bool carrierHasCodeMapping)
		{
			var input = filePath + inputFile;
			var expectedOutput = filePath + expectedOutputFile;

			var extensionObjects = GetExtensionObjects(carrierCode, testReference, eventParameters, expectedUNB2);
			AssertMapping<IFTSTA2UInterchange_Generic>(input, expectedOutput, extensionObjects);

			using (var inputStream = new MemoryStream(Encoding.UTF8.GetBytes(string.Format(EdifactInterchangeXml, TestHelper.GetEmbeddedResourceAsString(input)))))
			using (var expectedOutputStream = TestHelper.GetEmbeddedResource(expectedOutput))
			{
				extensionObjects = GetExtensionObjects(carrierCode, testReference, eventParameters, expectedUNB2);
				AssertMapping<XmlEnvelopeIFTSTA2UInterchange>(inputStream, expectedOutputStream, extensionObjects);
			}
		}

		Dictionary<string, object> GetExtensionObjects(string carrierCode, string testReference, string eventParameters, string expectedUNB2)
		{
			var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();

			var carrierMappingName = carrierCode + " IFTSTA to UniversalInterchange";

			mockDateMapper.Expect(x => x.CurrentDateTime(Arg.Is("s"))).Return("2015-06-01T09:30:10");

			mockCodeMapper.Expect(x => x.GetRecipientCode(carrierCode, carrierCode, carrierMappingName, "Container Status", "Event Type", "VD")).Return("DEP").Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode(carrierCode, carrierCode, carrierMappingName, "Container Status", "Event Type", "VA")).Return("ARV").Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode(carrierCode, carrierCode, carrierMappingName, "Container Status", "Event Type", "UV")).Return("XXX").Repeat.Any();
			if (eventParameters != null)
			{
				mockCodeMapper.Expect(x => x.GetRecipientCode(carrierCode, carrierCode, carrierMappingName, "Container Status", "Event Parameters", "VD")).Return(eventParameters).Repeat.Any();
				mockCodeMapper.Expect(x => x.GetRecipientCode(carrierCode, carrierCode, carrierMappingName, "Container Status", "Event Parameters", "VA")).Return(eventParameters).Repeat.Any();
				mockCodeMapper.Expect(x => x.GetRecipientCode(carrierCode, carrierCode, carrierMappingName, "Container Status", "Event Parameters", "UV")).Return(eventParameters).Repeat.Any();
			}
			mockCodeMapper.Expect(x => x.GetRecipientCode(carrierCode, carrierCode, carrierMappingName, "Container Status", "Is Estimate", "VD")).Return("TRUE").Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode(carrierCode, carrierCode, carrierMappingName, "Container Status", "Is Estimate", "VA")).Return("FALSE").Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode(carrierCode, carrierCode, carrierMappingName, "Container Status", "Is Estimate", "UV")).Return("false").Repeat.Any();
			if (testReference != null)
			{
				mockCodeMapper.Expect(x => x.GetRecipientCode(carrierCode, carrierCode, carrierMappingName, "Container Status", "Event Reference", "VD")).Return(testReference).Repeat.Any();
				mockCodeMapper.Expect(x => x.GetRecipientCode(carrierCode, carrierCode, carrierMappingName, "Container Status", "Event Reference", "VA")).Return(testReference).Repeat.Any();
				mockCodeMapper.Expect(x => x.GetRecipientCode(carrierCode, carrierCode, carrierMappingName, "Container Status", "Event Reference", "UV")).Return(testReference).Repeat.Any();
			}

			mockCodeMapper.Expect(x => x.GetRecipientCode("GCTIFTSTA", "GCTIFTSTA", "Generic IFTSTA to UniversalInterchange", "Container Status", "Event Reference", "VA")).Return("test reference VA2").Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode("GCTIFTSTA", "GCTIFTSTA", "Generic IFTSTA to UniversalInterchange", "Container Status", "Event Reference", "VD")).Return("test reference VD2").Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode("GCTIFTSTA", "GCTIFTSTA", "Generic IFTSTA to UniversalInterchange", "Container Status", "Event Parameters", "VD")).Return("|Facility=CTO|Department=Carrier|Type=Repair Authorization|Reason=TestVD2B|IsEstimate=true").Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode("GCTIFTSTA", "GCTIFTSTA", "Generic IFTSTA to UniversalInterchange", "Container Status", "Event Parameters", "VA")).Return("|Facility=CTO|Department=Carrier|Type=Repair Authorization|Reason=TestVA2B|IsEstimate=false").Repeat.Any();

			mockCodeMapper.Expect(x => x.GetRecipientCode("GCTIFTSTA", "GCTIFTSTA", "Generic IFTSTA to UniversalInterchange", "SCAC", "SCAC", carrierCode, "")).Return("AAAA").Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode("GCTIFTSTA", "GCTIFTSTA", "Generic IFTSTA to UniversalInterchange", "SCAC", "SCAC", carrierCode, "CMDU")).Return("CMDU").Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode("GCTIFTSTA", "GCTIFTSTA", "Generic IFTSTA to UniversalInterchange", "SCAC", "SCAC", carrierCode, "XXXX")).Return("XXXX").Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode("GCTIFTSTA", "GCTIFTSTA", "Generic IFTSTA to UniversalInterchange", "SCAC", "SCAC", carrierCode, "UNB2")).Return(expectedUNB2).Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode("GCTIFTSTA", "GCTIFTSTA", "Generic IFTSTA to UniversalInterchange", "SCAC", "SCAC", carrierCode, "HLCU")).Return("").Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode("GCTIFTSTA", "GCTIFTSTA", "Generic IFTSTA to UniversalInterchange", "SCAC", "SCAC", carrierCode, "CMDX")).Return("").Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode("GCTIFTSTA", "GCTIFTSTA", "Generic IFTSTA to UniversalInterchange", "SCAC", "SCAC", carrierCode, "SUDU")).Return("").Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode("GCTIFTSTA", "GCTIFTSTA", "Generic IFTSTA to UniversalInterchange", "ISOCodeToContainerType", "Container Type", "22G1")).Return("22G1").Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode("GCTIFTSTA", "GCTIFTSTA", "Generic IFTSTA to UniversalInterchange", "ISOCodeToContainerType", "Container Type", "4500")).Return("45G0").Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode("GCTIFTSTA", "GCTIFTSTA", "Generic IFTSTA to UniversalInterchange", "ISOCodeToContainerType", "Container Type", "45G1")).Return("45G1").Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode("GCTIFTSTA", "GCTIFTSTA", "Generic IFTSTA to UniversalInterchange", "ISOCodeToContainerType", "Container Type", "4510")).Return("4510").Repeat.Any();

			mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(carrierCode).Repeat.Any();
			mockContextAccessor.Expect(x => x.SetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", "CONTAINER_TRACKING")).Repeat.Any();
			mockContextAccessor.Expect(x => x.GetContextProperty("UNB2_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("UNB2").Repeat.Any();

			return new Dictionary<string, object>() {
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockDateMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockCodeMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockContextAccessor }
			};
		}

		void AssertMapping<T>(string inputFile, string expectedOutputFile, Dictionary<string, object> extensionObjects) where T : TransformBase
		{
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.Execute<T>(inputFile, expectedOutputFile);

			foreach (var mockObject in extensionObjects.Values)
			{
				mockObject.VerifyAllExpectations();
			}
		}

		void AssertMapping<T>(Stream inputStream, Stream expectedOutputStream, Dictionary<string, object> extensionObjects) where T : TransformBase
		{
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.Execute<T>(inputStream, expectedOutputStream);

			foreach (var mockObject in extensionObjects.Values)
			{
				mockObject.VerifyAllExpectations();
			}
		}

		private const string EdifactInterchangeXml = @"<ins0:EdifactInterchangeXml xmlns:ins0=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006/InterchangeXML"" xmlns:ns0=""http://cargowise.com/ehub/core/2011/02"" DelimiterSetSerializedData=""39:-1:-1:43:58:63:-1:44:-1"">
  <ns0:UNB xmlns:ns0=""http://schemas.microsoft.com/Edi/EdifactServiceSchema"">
    <UNB1>
      <UNB1.1>UNOC</UNB1.1>
      <UNB1.2>1</UNB1.2>
    </UNB1>
    <UNB2>
      <UNB2.1>CCLOG_CW</UNB2.1>
      <UNB2.2>ZZZ</UNB2.2>
    </UNB2>
    <UNB3>
      <UNB3.1>CCLOG_CW</UNB3.1>
      <UNB3.2>ZZZ</UNB3.2>
    </UNB3>
    <UNB4>
      <UNB4.1>210831</UNB4.1>
      <UNB4.2>1351</UNB4.2>
    </UNB4>
    <UNB5>869912876</UNB5>
  </ns0:UNB>
  <TransactionSetGroup>
    <TransactionSet DocType=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006#EFACT_D99B_IFTSTA"">
      {0}
    </TransactionSet>
  </TransactionSetGroup>
  <ns0:UNZ xmlns:ns0=""http://schemas.microsoft.com/Edi/EdifactServiceSchema"">
    <UNZ1>3</UNZ1>
    <UNZ2>869912876</UNZ2>
  </ns0:UNZ>
</ins0:EdifactInterchangeXml>";
	}
}
