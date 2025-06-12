using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.VERMAS2UShipment_1STOP;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests
{
	[TestClass]
	public class VERMAS2UniversalShipment_1STOP_Tests
	{
		const string filePath = "VERMAS2UniversalShipment_1STOP.TestFiles.";

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void VERMAS2UniversalShipment_1STOP()
		{
			AssertMapping("Test1_input.xml", "Test1_output.xml");
			AssertMapping("Test2_input.xml", "Test2_output.xml");
		}

		void AssertMapping(string inputFile, string expectedOutputFile)
		{
			var input = filePath + inputFile;
			var expectedOutput = filePath + expectedOutputFile;

			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();

			mockCodeMapper.Stub(x => x.GetRecipientCode("1STOPCSYD_OCM", "1STOPCSYD_OCM", "Verified Gross Container Weight VERMAS from 1-Stop", "ContainerTypeFromISOCode", "CW1 Code", "42G1")).Return("40GP").Repeat.Any();
			mockCodeMapper.Stub(x => x.GetRecipientCode("1STOPCSYD_OCM", "1STOPCSYD_OCM", "Verified Gross Container Weight VERMAS from 1-Stop", "ContainerTypeFromISOCode", "CW1 Code", "22G1")).Return("20GP").Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", "CMACGM_BK", "EGLA")).Return("TODO").Repeat.Any();

			var extensionObjects = new Dictionary<string, object>() { 
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockCodeMapper }
			};

			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.Execute<VERMAS2UShipment_1STOP>(input, expectedOutput);
		}
	}
}
