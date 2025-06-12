using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Clients.TRX.Transforms.CrownX12_210_2_UniShip;
using CargoWise.eHub.Core.Transforms.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Clients.TRX.Tests
{
	[TestClass]
	public class CrownX12_210_2_UniShipTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestCrownX12_210_2_UniShip()
		{
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();

			mockCodeMapper.Expect(x => x.GetRecipientCodeUnkeyed("TRXELPELP_C04", "TRXELPELP", "Crown Data 210 - Receive Shipment Costs", "Defaults", "Data Provider")).Return("CROWN");
			mockCodeMapper.Expect(x => x.GetRecipientCodeUnkeyed("TRXELPELP_C04", "TRXELPELP", "Crown Data 210 - Receive Shipment Costs", "Defaults", "Import Instruction")).Return("UpdateAndInsertIfNotFound");

			mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("TRXELPELP");

			var extensionObjects = new Dictionary<string, object>() { 
			     { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockCodeMapper }, 
					 { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockContextAccessor }
			};
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);

			string sourceFile = "CrownX12_210_2_UniShip.TestFiles.CrownX12_210_2_UniShip_input.xml";
			string expectedFile = "CrownX12_210_2_UniShip.TestFiles.CrownX12_210_2_UniShip_output.xml";
			mapTester.ExecuteCompiled<CrownX12_210_2_UniShip>(sourceFile, expectedFile);

			mockCodeMapper.VerifyAllExpectations();
			mockContextAccessor.VerifyAllExpectations();
		}

	}
}
