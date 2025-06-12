using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.VirtualTransportProvider.ContainerTransportOptimization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.VirtualTransportProvider.ContainerTransportOptimization.Tests
{
	[TestClass]
	public class ContainerTransportOptimization_Tests
	{
		const string filePath = "ContainerTransportOptimization.TestFiles.";

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestContainerTransportOptimization()
		{
			AssertMapping("Test1_input.xml", "Test1_output.xml", "AUSYD", "CTO_AUSYD");
			AssertMapping("Test1_input.xml", "Test1_output.xml", "AUSYD", string.Empty);
		}

		void AssertMapping(string inputFile, string outputFile, string unloco, string destinationParty)
		{
			var input = filePath + inputFile;
			var expectedOutput = filePath + outputFile;

			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();

			mockCodeMapper.Stub(x => x.GetRecipientCode("CONTAINER_TRANSPORT_OPTIMIZATION", "CONTAINER_TRANSPORT_OPTIMIZATION", "CONTAINER_TRANSPORT_OPTIMIZATION Configuration", "Port Settings", "RecipientId", unloco)).Return(destinationParty).Repeat.Once();

			if (destinationParty != string.Empty)
			{
				mockContextAccessor.Expect(x => x.SetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", destinationParty)).Repeat.Once();
			}

			var extensionObjects = new Dictionary<string, object>() {
				{ "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
			};

			var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.Execute<ContainerTransportOptimization>(input, expectedOutput);
		}
	}
}
