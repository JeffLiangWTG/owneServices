using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Clients.TRX.Transforms.UniShip_2_EMCX12_990;
using CargoWise.eHub.Core.Transforms.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Clients.TRX.Tests
{
	[TestClass]
	public class UniShip_2_EMCX12_990Tests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniShip_2_EMCX12_990()
		{
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();

			mockCodeMapper.Expect(x => x.GetRecipientCodeUnkeyed("TRXELPELP", "TRXELPELP_E02", "EMC 990 - Send Shipments", "Defaults", "SCAC")).Return("MOAV");
			mockCodeMapper.Expect(x => x.GetRecipientCode("TRXELPELP", "TRXELPELP_E02", "EMC 990 - Send Shipments", "Shipment Status", "Action Code B104", "AD")).Return("A");
			mockContextAccessor.Expect(x => x.SetContextProperty("OverrideEDIHeader", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "true"));
			mockContextAccessor.Expect(x => x.SetContextProperty("GS03", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "MARTECSYD"));

			var extensionObjects = new Dictionary<string, object>() { 
			     { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockCodeMapper },
				 { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockContextAccessor }
			};
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);

			string sourceFile = "UniShip_2_EMCX12_990.TestFiles.UniShip_2_EMCX12_990_input.xml";
			string expectedFile = "UniShip_2_EMCX12_990.TestFiles.UniShip_2_EMCX12_990_output.xml";
			mapTester.ExecuteCompiled<UniShip_2_EMCX12_990>(sourceFile, expectedFile);

			mockCodeMapper.VerifyAllExpectations();
			mockContextAccessor.VerifyAllExpectations();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniShip_2_EMCX12_990_NoGS03()
		{
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();

			mockCodeMapper.Expect(x => x.GetRecipientCodeUnkeyed("TRXELPELP", "TRXELPELP_E02", "EMC 990 - Send Shipments", "Defaults", "SCAC")).Return("MOAV");
			mockCodeMapper.Expect(x => x.GetRecipientCode("TRXELPELP", "TRXELPELP_E02", "EMC 990 - Send Shipments", "Shipment Status", "Action Code B104", "AD")).Return("A");
			mockContextAccessor.Expect(x => x.SetContextProperty("", "", "")).IgnoreArguments().Repeat.Times(0);

			var extensionObjects = new Dictionary<string, object>() { 
			     { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockCodeMapper },
				 { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockContextAccessor }
			};
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);

			string sourceFile = "UniShip_2_EMCX12_990.TestFiles.NoOverride_Input.xml";
			string expectedFile = "UniShip_2_EMCX12_990.TestFiles.NoOverride_Output.xml";
			mapTester.ExecuteCompiled<UniShip_2_EMCX12_990>(sourceFile, expectedFile);

			mockCodeMapper.VerifyAllExpectations();
			mockContextAccessor.VerifyAllExpectations();
		}
	}
}
