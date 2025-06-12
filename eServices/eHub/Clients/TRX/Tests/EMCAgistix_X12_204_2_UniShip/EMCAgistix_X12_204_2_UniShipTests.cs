using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Clients.TRX.Transforms.EMCAgistix_X12_204_2_UniShip;
using CargoWise.eHub.Core.Transforms.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Clients.TRX.Tests
{
	[TestClass]
	public class EMCAgistix_X12_204_2_UniShipTests
	{
        MapTester mapTester;
        CodeMapper mockCodeMapper;

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestEMCX12_204_2_UniShip_AIR()
        {
            mockCodeMapper.Expect(x => x.GetRecipientCode("TRXELPELP_E01", "TRXELPELP", "EMC 204 - Receive Shipments", "Transport Mode", "AIR")).Return("AIR");
            mockCodeMapper.Expect(x => x.GetRecipientCode("TRXELPELP_E01", "TRXELPELP", "EMC 204 - Receive Shipments", "Container Mode", "Container Mode", "AIR", "2D")).Return("LSE");
            
            mapTester.ExecuteCompiled<EMCAgistix_X12_204_2_UniShip>(
                "EMCAgistix_X12_204_2_UniShip.TestFiles.EMCX12_204_2_AIR_Input.xml", 
                "EMCAgistix_X12_204_2_UniShip.TestFiles.EMCX12_204_2_AIR_Output.xml");
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestEMCX12_204_2_UniShip_LTL()
        {
            mockCodeMapper.Expect(x => x.GetRecipientCode("TRXELPELP_E01", "TRXELPELP", "EMC 204 - Receive Shipments", "Transport Mode", "LTL")).Return("ROA");
            mockCodeMapper.Expect(x => x.GetRecipientCode("TRXELPELP_E01", "TRXELPELP", "EMC 204 - Receive Shipments", "Container Mode", "Container Mode", "LTL", "3D")).Return("LTL");

            mapTester.ExecuteCompiled<EMCAgistix_X12_204_2_UniShip>(
                "EMCAgistix_X12_204_2_UniShip.TestFiles.EMCX12_204_2_LTL_Input.xml",
                "EMCAgistix_X12_204_2_UniShip.TestFiles.EMCX12_204_2_LTL_Output.xml");
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestEMCX12_204_2_UniShip_OCN_FCL()
        {
            mockCodeMapper.Expect(x => x.GetRecipientCode("TRXELPELP_E01", "TRXELPELP", "EMC 204 - Receive Shipments", "Transport Mode", "OCN")).Return("SEA");
            mockCodeMapper.Expect(x => x.GetRecipientCode("TRXELPELP_E01", "TRXELPELP", "EMC 204 - Receive Shipments", "Container Mode", "Container Mode", "OCN", "FCL")).Return("FCL");
            mockCodeMapper.Expect(x => x.GetRecipientCodeUnkeyed("TRXELPELP_E01", "TRXELPELP", "EMC 204 - Receive Shipments", "Defaults", "Inner Pack Type")).Return("BOX");

            mapTester.ExecuteCompiled<EMCAgistix_X12_204_2_UniShip>(
                "EMCAgistix_X12_204_2_UniShip.TestFiles.EMCX12_204_2_OCN_FCL_Input.xml",
                "EMCAgistix_X12_204_2_UniShip.TestFiles.EMCX12_204_2_OCN_FCL_Output.xml");
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestEMCX12_204_2_UniShip_OCN_LCL()
        {
            mockCodeMapper.Expect(x => x.GetRecipientCode("TRXELPELP_E01", "TRXELPELP", "EMC 204 - Receive Shipments", "Transport Mode", "OCN")).Return("SEA");
            mockCodeMapper.Expect(x => x.GetRecipientCode("TRXELPELP_E01", "TRXELPELP", "EMC 204 - Receive Shipments", "Container Mode", "Container Mode", "OCN", "LCL")).Return("LCL");
            
            mapTester.ExecuteCompiled<EMCAgistix_X12_204_2_UniShip>(
                "EMCAgistix_X12_204_2_UniShip.TestFiles.EMCX12_204_OCN_LCL_Input.xml",
                "EMCAgistix_X12_204_2_UniShip.TestFiles.EMCX12_204_OCN_LCL_Output.xml");
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestEMCX12_204_2_UniShip_ROA()
        {
            mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetUNLOCOFromLocation", "@result", "@city", "Scottsdale", "@state", "AZ", "@country", "US")).Return("USSTZ").Repeat.Any();
            mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetUNLOCOFromLocation", "@result", "@city", "Eldersburg", "@state", "MD", "@country", "US")).Return("USEMC").Repeat.Any();
            mockCodeMapper.Expect(x => x.GetRecipientCode("TRXELPELP_E01", "TRXELPELP", "EMC 204 - Receive Shipments", "Transport Mode", "LTL")).Return("ROA");
            mockCodeMapper.Expect(x => x.GetRecipientCode("TRXELPELP_E01", "TRXELPELP", "EMC 204 - Receive Shipments", "Container Mode", "Container Mode", "LTL", "3D")).Return("LTL");

            mapTester.ExecuteCompiled<EMCAgistix_X12_204_2_UniShip>(
                "EMCAgistix_X12_204_2_UniShip.TestFiles.EMCX12_204_ROA_MultipleReferenceTypes_Input.xml",
                "EMCAgistix_X12_204_2_UniShip.TestFiles.EMCX12_204_ROA_MultipleReferenceTypes_Output.xml");
        }

        [TestInitialize]
        public void ConfigureCodeMapper()
        {
			mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();

			mockCodeMapper.Expect(x => x.GetRecipientCodeUnkeyed("TRXELPELP_E01", "TRXELPELP", "EMC 204 - Receive Shipments", "Defaults", "Data Provider Code")).Return("EMC").Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCodeUnkeyed("TRXELPELP_E01", "TRXELPELP", "EMC 204 - Receive Shipments", "Defaults", "Local Client")).Return("XXX").Repeat.Any();
			
			mockCodeMapper.Expect(x => x.GetRecipientCodeUnkeyed("TRXELPELP_E01", "TRXELPELP", "EMC 204 - Receive Shipments", "Defaults", "Pack Type")).Return("PLT").Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode("TRXELPELP_E01", "TRXELPELP", "EMC 204 - Receive Shipments", "Unit of Measurement", "L")).Return("LB").Repeat.Any();

			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetUNLOCOFromLocation", "@result", "@city", "San Mateo", "@state", "CA", "@country", "US")).Return("USSXF").Repeat.Any();
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetUNLOCOFromLocation", "@result", "@city", "Big Tree", "@state", "NY", "@country", "US")).Return("US2BT").Repeat.Any();
            mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetUNLOCOFromLocation", "@result", "@city", "San Bum", "@state", "CA", "@country", "US")).Return("").Repeat.Any();

            var extensionObjects = new Dictionary<string, object>() {
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockCodeMapper }
			};

			mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
        }
	}
}
