using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.VelaTrack.Transforms.VelaTrackEdi2VelaTrackEdiEnvelope;
using CargoWise.eHub.DataAccess.Models.CodeMapsTesting;
using CargoWise.eHub.DataAccess.Sql;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.VelaTrack.Tests
{
	[TestClass]
	public class VelaTrackEdi2VelaTrackEdiEnvelopeTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestVelaTrackEdi2VelaTrackEdiEnvelope()
		{
			const string sourceFile = "VelaTrackEdi2VelaTrackEdiEnvelope.TestFiles.SEA_Input.xml";
			const string expectedFile = "VelaTrackEdi2VelaTrackEdiEnvelope.TestFiles.SEA_Output.xml";
			InitilizeAndExecute(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestVelaTrackEdi2VelaTrackEdiEnvelope_WithReference()
		{
			const string sourceFile = "VelaTrackEdi2VelaTrackEdiEnvelope.TestFiles.SEA_Input2.xml";
			const string expectedFile = "VelaTrackEdi2VelaTrackEdiEnvelope.TestFiles.SEA_Output2.xml";
			InitilizeAndExecute(sourceFile, expectedFile);
		}

		static void InitilizeAndExecute(string inputFile, string outputFile)
		{
			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();

			mockContextAccessor.Expect(x => x.SetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", "VelaTrack_CSS"));
			mockContextAccessor.Expect(x => x.SetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", "VelaTrack_CSS"));

			var extensionObjects = new Dictionary<string, object>()
			{
				{"http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockContextAccessor},
			};

			var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.Execute<VelaTrackEdi2VelaTrackEdiEnvelope>(inputFile, outputFile);

			mockContextAccessor.VerifyAllExpectations();
		}
	}
}
