using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Core.Transforms.Helper.Testing;
using CargoWise.eHub.Products.VelaTrack.Transforms.UniversalEvent2TrackingRequest;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.VelaTrack.Tests
{
	[TestClass]
	public class UniversalEvent2TrackingRequestTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalEvent2TrackingRequest_Console_BookingReferenceOnly()
		{
			const string sourceFile = "UniversalEvent2TrackingRequest.TestFiles.inputConsol.xml";
			const string expectedFile = "UniversalEvent2TrackingRequest.TestFiles.outputConsol.xml";
			InitilizeAndExecute("EDIDATDAU", sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalEvent2TrackingRequest_Console_BookingReferenceAndMOWB()
		{
			const string sourceFile = "UniversalEvent2TrackingRequest.TestFiles.inputConsolReferenceAndMOWB.xml";
			const string expectedFile = "UniversalEvent2TrackingRequest.TestFiles.outputConsolReferenceAndMOWB.xml";
			InitilizeAndExecute("EDIDATDAU", sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalEvent2TrackingRequest_Container()
		{
			const string sourceFile = "UniversalEvent2TrackingRequest.TestFiles.inputContainer.xml";
			const string expectedFile = "UniversalEvent2TrackingRequest.TestFiles.outputContainer.xml";
			InitilizeAndExecute("EDIDATDAU", sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalEvent2TrackingRequest_Container_SCAC()
		{
			var sourceFile = "UniversalEvent2TrackingRequest.TestFiles.inputContainerSCAC.xml";
			const string expectedFile = "UniversalEvent2TrackingRequest.TestFiles.outputContainerSCAC.xml";
			InitilizeAndExecute("EDIDATDAU", sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalEvent2TrackingRequest_Console_WithReference()
		{
			const string sourceFile = "UniversalEvent2TrackingRequest.TestFiles.inputConsolWithReference.xml";
			const string expectedFile = "UniversalEvent2TrackingRequest.TestFiles.outputConsolWithReference.xml";
			InitilizeAndExecute("EDIDATDAU", sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalEvent2TrackingRequest_Container_WithReference()
		{
			const string sourceFile = "UniversalEvent2TrackingRequest.TestFiles.inputContainerWithReference.xml";
			const string expectedFile = "UniversalEvent2TrackingRequest.TestFiles.outputContainerWithReference.xml";
			InitilizeAndExecute("HYEIKBDNZ", sourceFile, expectedFile);
		}

		static void InitilizeAndExecute(string sourceDestination, string inputFile, string outputFile)
		{
			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
			mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(sourceDestination);

			var extensionObjects = new Dictionary<string, object>()
			{
				{"http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockContextAccessor},
			};

			var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.Execute<UniversalEvent2TrackingRequest>(inputFile, outputFile);

			mockContextAccessor.VerifyAllExpectations();
		}
	}
}
