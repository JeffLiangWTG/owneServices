using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Clients.TRX.Transforms.UniShip_2_GencoX12_214;
using CargoWise.eHub.Core.Transforms.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Clients.TRX.Tests
{
	[TestClass]
	public class UniShip_2_GencoX12_214Tests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniShip_2_GencoX12_214_X1()
		{
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();

			mockCodeMapper.Expect(x => x.GetRecipientCodeUnkeyed("TRXELPELP", "TRXELPELP_G02", "Genco 214 - Send Shipment Status Events", "Defaults", "SCAC")).Return("TEST");
			mockCodeMapper.Expect(x => x.GetRecipientCodeUnkeyed("TRXELPELP", "TRXELPELP_G02", "Genco 214 - Send Shipment Status Events", "Defaults", "Reason Code AT702")).Return("NS");
			mockCodeMapper.Expect(x => x.GetRecipientCode("TRXELPELP", "TRXELPELP_G02", "Genco 214 - Send Shipment Status Events", "Shipment Status", "Status Code AT701", "X1")).Return("X1");
			mockCodeMapper.Expect(x => x.GetRecipientCode("TRXELPELP", "TRXELPELP_G02", "Genco 214 - Send Shipment Status Events", "Shipment Status", "Status Code AT703", "X1")).Return("");
			mockCodeMapper.Expect(x => x.GetRecipientCode("TRXELPELP", "TRXELPELP_G02", "Genco 214 - Send Shipment Status Events", "Shipment Status", "Status Code AT704", "X1")).Return("");
			mockCodeMapper.Expect(x => x.GetRecipientCode("TRXELPELP", "TRXELPELP_G02", "Genco 214 - Send Shipment Status Events", "Unit of Measurement", "X12 Code", "KG")).Return("K");

			var extensionObjects = new Dictionary<string, object>() { 
			     { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockCodeMapper }
			};
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);

			string sourceFile = "UniShip_2_GencoX12_214.TestFiles.X1_Input.xml";
			string expectedFile = "UniShip_2_GencoX12_214.TestFiles.X1_Output.xml";
			mapTester.ExecuteCompiled<UniShip_2_GencoX12_214>(sourceFile, expectedFile);

			mockCodeMapper.VerifyAllExpectations();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniShip_2_GencoX12_214_AG()
		{
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();

			mockCodeMapper.Expect(x => x.GetRecipientCodeUnkeyed("TRXELPELP", "TRXELPELP_G02", "Genco 214 - Send Shipment Status Events", "Defaults", "SCAC")).Return("TEST");
			mockCodeMapper.Expect(x => x.GetRecipientCode("TRXELPELP", "TRXELPELP_G02", "Genco 214 - Send Shipment Status Events", "Shipment Status", "Status Code AT701", "AG")).Return("");
			mockCodeMapper.Expect(x => x.GetRecipientCode("TRXELPELP", "TRXELPELP_G02", "Genco 214 - Send Shipment Status Events", "Shipment Status", "Status Code AT703", "AG")).Return("AB");
			mockCodeMapper.Expect(x => x.GetRecipientCode("TRXELPELP", "TRXELPELP_G02", "Genco 214 - Send Shipment Status Events", "Shipment Status", "Status Code AT704", "AG")).Return("NA");
			mockCodeMapper.Expect(x => x.GetRecipientCode("TRXELPELP", "TRXELPELP_G02", "Genco 214 - Send Shipment Status Events", "Unit of Measurement", "X12 Code", "KG")).Return("K");

			mockCodeMapper.Expect(x => x.GetStateFromUNLOCO("AUSYD")).Return("NSW");

			var extensionObjects = new Dictionary<string, object>() { 
			     { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockCodeMapper }
			};
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);

			string sourceFile = "UniShip_2_GencoX12_214.TestFiles.AG_Input.xml";
			string expectedFile = "UniShip_2_GencoX12_214.TestFiles.AG_Output.xml";
			mapTester.ExecuteCompiled<UniShip_2_GencoX12_214>(sourceFile, expectedFile);

			mockCodeMapper.VerifyAllExpectations();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniShip_2_GencoX12_214_X3()
		{
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();

			mockCodeMapper.Expect(x => x.GetRecipientCodeUnkeyed("TRXELPELP", "TRXELPELP_G02", "Genco 214 - Send Shipment Status Events", "Defaults", "SCAC")).Return("TEST");
			mockCodeMapper.Expect(x => x.GetRecipientCodeUnkeyed("TRXELPELP", "TRXELPELP_G02", "Genco 214 - Send Shipment Status Events", "Defaults", "Reason Code AT702")).Return("NS");
			mockCodeMapper.Expect(x => x.GetRecipientCode("TRXELPELP", "TRXELPELP_G02", "Genco 214 - Send Shipment Status Events", "Shipment Status", "Status Code AT701", "X3")).Return("X3");
			mockCodeMapper.Expect(x => x.GetRecipientCode("TRXELPELP", "TRXELPELP_G02", "Genco 214 - Send Shipment Status Events", "Shipment Status", "Status Code AT703", "X3")).Return("");
			mockCodeMapper.Expect(x => x.GetRecipientCode("TRXELPELP", "TRXELPELP_G02", "Genco 214 - Send Shipment Status Events", "Shipment Status", "Status Code AT704", "X3")).Return("");
			mockCodeMapper.Expect(x => x.GetRecipientCode("TRXELPELP", "TRXELPELP_G02", "Genco 214 - Send Shipment Status Events", "Unit of Measurement", "X12 Code", "KG")).Return("K");

			var extensionObjects = new Dictionary<string, object>() { 
			     { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockCodeMapper }
			};
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);

			string sourceFile = "UniShip_2_GencoX12_214.TestFiles.X3_Input.xml";
			string expectedFile = "UniShip_2_GencoX12_214.TestFiles.X3_Output.xml";
			mapTester.ExecuteCompiled<UniShip_2_GencoX12_214>(sourceFile, expectedFile);

			mockCodeMapper.VerifyAllExpectations();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniShip_2_GencoX12_214_AF()
		{
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();

			mockCodeMapper.Expect(x => x.GetRecipientCodeUnkeyed("TRXELPELP", "TRXELPELP_G02", "Genco 214 - Send Shipment Status Events", "Defaults", "SCAC")).Return("TEST");
			mockCodeMapper.Expect(x => x.GetRecipientCodeUnkeyed("TRXELPELP", "TRXELPELP_G02", "Genco 214 - Send Shipment Status Events", "Defaults", "Reason Code AT702")).Return("NS");
			mockCodeMapper.Expect(x => x.GetRecipientCode("TRXELPELP", "TRXELPELP_G02", "Genco 214 - Send Shipment Status Events", "Shipment Status", "Status Code AT701", "AF")).Return("AF");
			mockCodeMapper.Expect(x => x.GetRecipientCode("TRXELPELP", "TRXELPELP_G02", "Genco 214 - Send Shipment Status Events", "Shipment Status", "Status Code AT703", "AF")).Return("");
			mockCodeMapper.Expect(x => x.GetRecipientCode("TRXELPELP", "TRXELPELP_G02", "Genco 214 - Send Shipment Status Events", "Shipment Status", "Status Code AT704", "AF")).Return("");
			mockCodeMapper.Expect(x => x.GetRecipientCode("TRXELPELP", "TRXELPELP_G02", "Genco 214 - Send Shipment Status Events", "Unit of Measurement", "X12 Code", "KG")).Return("K");

			var extensionObjects = new Dictionary<string, object>() { 
			     { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockCodeMapper }
			};
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);

			string sourceFile = "UniShip_2_GencoX12_214.TestFiles.AF_Input.xml";
			string expectedFile = "UniShip_2_GencoX12_214.TestFiles.AF_Output.xml";
			mapTester.ExecuteCompiled<UniShip_2_GencoX12_214>(sourceFile, expectedFile);

			mockCodeMapper.VerifyAllExpectations();
		}
	}
}
