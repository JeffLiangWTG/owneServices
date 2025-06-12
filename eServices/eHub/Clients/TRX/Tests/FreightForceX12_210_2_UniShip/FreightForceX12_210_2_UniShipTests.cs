using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Clients.TRX.Transforms.FreightForceX12_210_2_UniShip;
using CargoWise.eHub.Core.Transforms.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Clients.TRX.Tests
{
    [TestClass]
    public class FreightForceX12_210_2_UniShipTests
    {
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestFreightForceX12_210_2_UniShip()
		{
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();

            mockCodeMapper.Expect(x => x.GetRecipientCodeUnkeyed("TRXELPELP_F02", "TRXELPELP", "Freight Force 210 - Receive Shipment Costs", "Defaults", "Data Provider")).Return("FREIGHTFORCE").Repeat.Any();
            mockCodeMapper.Expect(x => x.GetRecipientCodeUnkeyed("TRXELPELP_F02", "TRXELPELP", "Freight Force 210 - Receive Shipment Costs", "Defaults", "Import Instruction")).Return("UpdateAndInsertIfNotFound").Repeat.Any();

            mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("TRXELPELP").Repeat.Any();

			var extensionObjects = new Dictionary<string, object>() { 
			     { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockCodeMapper }, 
					 { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockContextAccessor }
			};
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);

            string sourceFile = "FreightForceX12_210_2_UniShip.TestFiles.FreightForceX12_210_2_UniShip_input.xml";
            string expectedFile = "FreightForceX12_210_2_UniShip.TestFiles.FreightForceX12_210_2_UniShip_output.xml";
            mapTester.ExecuteCompiled<FreightForceX12_210_2_UniShip>(sourceFile, expectedFile);

            sourceFile = "FreightForceX12_210_2_UniShip.TestFiles.210_MOAV_18_20170602_1605_input.xml";
            expectedFile = "FreightForceX12_210_2_UniShip.TestFiles.210_MOAV_18_20170602_1605_output.xml";
            mapTester.ExecuteCompiled<FreightForceX12_210_2_UniShip>(sourceFile, expectedFile);

            sourceFile = "FreightForceX12_210_2_UniShip.TestFiles.210_MOAV_19_20170602_1706_input.xml";
            expectedFile = "FreightForceX12_210_2_UniShip.TestFiles.210_MOAV_19_20170602_1706_output.xml";
            mapTester.ExecuteCompiled<FreightForceX12_210_2_UniShip>(sourceFile, expectedFile);
                  
			mockCodeMapper.VerifyAllExpectations();
			mockContextAccessor.VerifyAllExpectations();
		}
    }
}
