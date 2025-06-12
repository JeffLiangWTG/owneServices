using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.VelaTrack.Transforms.UniversalEvent2TrackingRequest;
using CargoWise.eHub.Products.VelaTrack.Transforms.UniversalEvent2TrackingRequestLegacy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.VelaTrack.Tests
{
	[TestClass]
	public class UniversalEvent2TrackingRequestLegacyTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalEvent2TrackingRequestLegacy_Console_BookingReferenceOnly()
		{
			const string sourceFile = "UniversalEvent2TrackingRequest.TestFiles.inputConsol.xml";
			const string expectedFile = "UniversalEvent2TrackingRequest.TestFiles.outputConsol.xml";
			InitilizeAndExecute("EDIDATDAU", sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalEvent2TrackingRequestLegacy_Console_BookingReferenceAndMOWB()
		{
			const string sourceFile = "UniversalEvent2TrackingRequest.TestFiles.inputConsolReferenceAndMOWB.xml";
			const string expectedFile = "UniversalEvent2TrackingRequest.TestFiles.outputConsolReferenceAndMOWB.xml";
			InitilizeAndExecute("EDIDATDAU", sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalEvent2TrackingRequestLegacy_Container()
		{
			const string sourceFile = "UniversalEvent2TrackingRequest.TestFiles.inputContainer.xml";
			const string expectedFile = "UniversalEvent2TrackingRequest.TestFiles.outputContainer.xml";
			InitilizeAndExecute("EDIDATDAU", sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalEvent2TrackingRequestLegacy_Container_SCAC()
		{
			var sourceFile = "UniversalEvent2TrackingRequest.TestFiles.inputContainerSCAC.xml";
			const string expectedFile = "UniversalEvent2TrackingRequest.TestFiles.outputContainerSCAC.xml";
			InitilizeAndExecute("EDIDATDAU", sourceFile, expectedFile);
		}

		static void InitilizeAndExecute(string sourceDestination, string inputFile, string outputFile)
		{
			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
			mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(sourceDestination);
			mockContextAccessor.Expect(x => x.SetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", "VelaTrack_CSS"));

			var extensionObjects = new Dictionary<string, object>()
			{
				{"http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockContextAccessor},
			};

			var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.Execute<UniversalEvent2TrackingRequestLegacy>(inputFile, outputFile);

			mockContextAccessor.VerifyAllExpectations();
		}
	}
}
