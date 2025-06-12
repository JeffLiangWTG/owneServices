using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.UniversalInterchangeSplit;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests
{
	[TestClass]
	public class UniversalInterchangeSplitPerSubShipmentTests
	{
		const string filePath = "UniversalInterchangeSplitPerSubShipment.TestFiles.";

        [TestMethod]
  		[TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void UniversalInterchangeSplitPerSubShipment()
		{
            AssertMapping("Test1_input.xml", "Test1_output.xml");
		}

        void AssertMapping(string inputFile, string expectedOutputFile)
		{
			var input = filePath + inputFile;
			var expectedOutput = filePath + expectedOutputFile;

			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();

			mockContextAccessor.Expect(x => x.SetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", "SHIPPING_INSTRUCTION"));

			var extensionObjects = new Dictionary<string, object>() { 
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockContextAccessor }
			};

			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.Execute<UniversalInterchangeSplitPerSubShipment>(input, expectedOutput);
			mockContextAccessor.VerifyAllExpectations();
		}
	}
}
