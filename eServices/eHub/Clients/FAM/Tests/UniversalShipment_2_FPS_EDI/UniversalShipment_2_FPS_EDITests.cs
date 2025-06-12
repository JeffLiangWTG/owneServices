using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Clients.FAM.Transforms.UniversalShipment_2_FPS_EDI;
using CargoWise.eHub.Core.Transforms.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Clients.FAM.Tests
{
	[TestClass]
	public class UniversalShipment_2_FPS_EDITests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalShipment_2_FPS_EDI()
		{
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();

			mockCodeMapper.Expect(x => x.GetRecipientCodeUnkeyed("FAMFPSSIN", "FAMFPSSIN_FPS", "FPS Manifest txt - Send Consols & Shipments", "Defaults", "Pack Type")).Return("PACKAGES");
			mockCodeMapper.Expect(x => x.GetRecipientCode("FAMFPSSIN", "FAMFPSSIN_FPS", "FPS Manifest txt - Send Consols & Shipments", "INCO Term", "FOB")).Return("1").Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode("FAMFPSSIN", "FAMFPSSIN_FPS", "FPS Manifest txt - Send Consols & Shipments", "Container Size", "20")).Return("1").Repeat.Twice();
			mockCodeMapper.Expect(x => x.GetRecipientCode("FAMFPSSIN", "FAMFPSSIN_FPS", "FPS Manifest txt - Send Consols & Shipments", "Container Type", "GP")).Return("1").Repeat.Twice();
			mockCodeMapper.Expect(x => x.GetRecipientCode("FAMFPSSIN", "FAMFPSSIN_FPS", "FPS Manifest txt - Send Consols & Shipments", "Container Size", "40")).Return("2").Repeat.Twice();
			mockCodeMapper.Expect(x => x.GetRecipientCode("FAMFPSSIN", "FAMFPSSIN_FPS", "FPS Manifest txt - Send Consols & Shipments", "Container Type", "OT")).Return("2").Repeat.Twice();

			var extensionObjects = new Dictionary<string, object>() { 
			     { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockCodeMapper }
			};
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);

			string sourceFile = "UniversalShipment_2_FPS_EDI.TestFiles.UniversalShipment_2_FPS_EDI_input.xml";
			string expectedFile = "UniversalShipment_2_FPS_EDI.TestFiles.UniversalShipment_2_FPS_EDI_output.xml";
			mapTester.Execute<UniversalShipment_2_FPS_EDI>(sourceFile, expectedFile);

			mockCodeMapper.VerifyAllExpectations();
		}
	}
}
