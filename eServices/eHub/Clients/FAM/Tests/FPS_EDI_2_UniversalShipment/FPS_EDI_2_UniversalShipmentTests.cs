using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Clients.FAM.Transforms.FPS_EDI_2_UniversalShipment;
using CargoWise.eHub.Core.Transforms.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Clients.FAM.Tests
{
	[TestClass]
	public class FPS_EDI_2_UniversalShipmentTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestFPS_EDI_2_UniversalShipment_Sample()
		{
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();

			mockCodeMapper.Expect(x => x.GetRecipientCodeUnkeyed("FAMFPSSIN_FPS", "FAMFPSSIN", "FPS Manifest txt - Receive Consols & Shipments", "Defaults", "Data Provider")).Return("FPS");
			mockCodeMapper.Expect(x => x.GetRecipientCodeUnkeyed("FAMFPSSIN_FPS", "FAMFPSSIN", "FPS Manifest txt - Receive Consols & Shipments", "Defaults", "Container Mode")).Return("FCL");
			mockCodeMapper.Expect(x => x.GetRecipientCodeUnkeyed("FAMFPSSIN_FPS", "FAMFPSSIN", "FPS Manifest txt - Receive Consols & Shipments", "Defaults", "Transport Mode")).Return("SEA");
			mockCodeMapper.Expect(x => x.GetRecipientCodeUnkeyed("FAMFPSSIN_FPS", "FAMFPSSIN", "FPS Manifest txt - Receive Consols & Shipments", "Defaults", "Weight Unit")).Return("KG");
			mockCodeMapper.Expect(x => x.GetRecipientCodeUnkeyed("FAMFPSSIN_FPS", "FAMFPSSIN", "FPS Manifest txt - Receive Consols & Shipments", "Defaults", "Volume Unit")).Return("M3");
			mockCodeMapper.Expect(x => x.GetRecipientCode("FAMFPSSIN_FPS", "FAMFPSSIN", "FPS Manifest txt - Receive Consols & Shipments", "Payment Method", "1")).Return("PPD");
			mockCodeMapper.Expect(x => x.GetRecipientCode("FAMFPSSIN_FPS", "FAMFPSSIN", "FPS Manifest txt - Receive Consols & Shipments", "Container Size", "1")).Return("20");
			mockCodeMapper.Expect(x => x.GetRecipientCode("FAMFPSSIN_FPS", "FAMFPSSIN", "FPS Manifest txt - Receive Consols & Shipments", "Container Type", "1")).Return("GP");
			mockCodeMapper.Expect(x => x.GetRecipientCode("FAMFPSSIN_FPS", "FAMFPSSIN", "FPS Manifest txt - Receive Consols & Shipments", "Container Size", "2")).Return("40");
			mockCodeMapper.Expect(x => x.GetRecipientCode("FAMFPSSIN_FPS", "FAMFPSSIN", "FPS Manifest txt - Receive Consols & Shipments", "Container Type", "2")).Return("OT");
			mockCodeMapper.Expect(x => x.GetRecipientCode("FAMFPSSIN_FPS", "FAMFPSSIN", "FPS Manifest txt - Receive Consols & Shipments", "Branch", "Company", "MYPKG")).Return("PKG");
			mockCodeMapper.Expect(x => x.GetRecipientCode("FAMFPSSIN_FPS", "FAMFPSSIN", "FPS Manifest txt - Receive Consols & Shipments", "Branch", "Branch", "MYPKG")).Return("PKG");
			mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("FAMFPSSIN");

			var extensionObjects = new Dictionary<string, object>() { 
			     { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockCodeMapper },
					 { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockDateMapper },
					 { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockContextAccessor }
			};
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);

			string sourceFile = "FPS_EDI_2_UniversalShipment.TestFiles.FPS_EDI_2_UniversalShipment_sample_input.xml";
			string expectedFile = "FPS_EDI_2_UniversalShipment.TestFiles.FPS_EDI_2_UniversalShipment_sample_output.xml";
			mapTester.Execute<FPS_EDI_2_UniversalShipment>(sourceFile, expectedFile);

			mockCodeMapper.VerifyAllExpectations();
			mockDateMapper.VerifyAllExpectations();
			mockContextAccessor.VerifyAllExpectations();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestFPS_EDI_2_UniversalShipment_Additional()
		{
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();

			mockCodeMapper.Expect(x => x.GetRecipientCodeUnkeyed("FAMFPSSIN_FPS", "FAMFPSSIN", "FPS Manifest txt - Receive Consols & Shipments", "Defaults", "Data Provider")).Return("FPS");
			mockCodeMapper.Expect(x => x.GetRecipientCodeUnkeyed("FAMFPSSIN_FPS", "FAMFPSSIN", "FPS Manifest txt - Receive Consols & Shipments", "Defaults", "Container Mode")).Return("FCL");
			mockCodeMapper.Expect(x => x.GetRecipientCodeUnkeyed("FAMFPSSIN_FPS", "FAMFPSSIN", "FPS Manifest txt - Receive Consols & Shipments", "Defaults", "Transport Mode")).Return("SEA");
			mockCodeMapper.Expect(x => x.GetRecipientCodeUnkeyed("FAMFPSSIN_FPS", "FAMFPSSIN", "FPS Manifest txt - Receive Consols & Shipments", "Defaults", "Weight Unit")).Return("KG");
			mockCodeMapper.Expect(x => x.GetRecipientCodeUnkeyed("FAMFPSSIN_FPS", "FAMFPSSIN", "FPS Manifest txt - Receive Consols & Shipments", "Defaults", "Volume Unit")).Return("M3");
			mockCodeMapper.Expect(x => x.GetRecipientCode("FAMFPSSIN_FPS", "FAMFPSSIN", "FPS Manifest txt - Receive Consols & Shipments", "Payment Method", "2")).Return("CLT");
			mockCodeMapper.Expect(x => x.GetRecipientCode("FAMFPSSIN_FPS", "FAMFPSSIN", "FPS Manifest txt - Receive Consols & Shipments", "Container Size", "1")).Return("20");
			mockCodeMapper.Expect(x => x.GetRecipientCode("FAMFPSSIN_FPS", "FAMFPSSIN", "FPS Manifest txt - Receive Consols & Shipments", "Container Type", "1")).Return("GP");
			mockCodeMapper.Expect(x => x.GetRecipientCode("FAMFPSSIN_FPS", "FAMFPSSIN", "FPS Manifest txt - Receive Consols & Shipments", "Container Size", "2")).Return("40");
			mockCodeMapper.Expect(x => x.GetRecipientCode("FAMFPSSIN_FPS", "FAMFPSSIN", "FPS Manifest txt - Receive Consols & Shipments", "Container Type", "2")).Return("OT");
			mockCodeMapper.Expect(x => x.GetRecipientCode("FAMFPSSIN_FPS", "FAMFPSSIN", "FPS Manifest txt - Receive Consols & Shipments", "Branch", "Company", "MYPEN")).Return("PKG");
			mockCodeMapper.Expect(x => x.GetRecipientCode("FAMFPSSIN_FPS", "FAMFPSSIN", "FPS Manifest txt - Receive Consols & Shipments", "Branch", "Branch", "MYPEN")).Return("PEN");
			mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("FAMFPSTRN");

			mockDateMapper.Expect(x => x.CurrentDateWithTimeZone()).Return("2015-03-11T00:00:00+11:00");

			var extensionObjects = new Dictionary<string, object>() { 
			     { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockCodeMapper },
					 { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockDateMapper },
					 { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockContextAccessor }
			};
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);

			string sourceFile = "FPS_EDI_2_UniversalShipment.TestFiles.FPS_EDI_2_UniversalShipment_additional_input.xml";
			string expectedFile = "FPS_EDI_2_UniversalShipment.TestFiles.FPS_EDI_2_UniversalShipment_additional_output.xml";
			mapTester.Execute<FPS_EDI_2_UniversalShipment>(sourceFile, expectedFile);

			mockCodeMapper.VerifyAllExpectations();
			mockDateMapper.VerifyAllExpectations();
			mockContextAccessor.VerifyAllExpectations();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestFPS_EDI_2_UniversalShipment_MultiRecord()
		{
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();

			mockCodeMapper.Expect(x => x.GetRecipientCodeUnkeyed("FAMFPSSIN_FPS", "FAMFPSSIN", "FPS Manifest txt - Receive Consols & Shipments", "Defaults", "Data Provider")).Return("FPS");
			mockCodeMapper.Expect(x => x.GetRecipientCodeUnkeyed("FAMFPSSIN_FPS", "FAMFPSSIN", "FPS Manifest txt - Receive Consols & Shipments", "Defaults", "Container Mode")).Return("FCL");
			mockCodeMapper.Expect(x => x.GetRecipientCodeUnkeyed("FAMFPSSIN_FPS", "FAMFPSSIN", "FPS Manifest txt - Receive Consols & Shipments", "Defaults", "Transport Mode")).Return("SEA");
			mockCodeMapper.Expect(x => x.GetRecipientCodeUnkeyed("FAMFPSSIN_FPS", "FAMFPSSIN", "FPS Manifest txt - Receive Consols & Shipments", "Defaults", "Weight Unit")).Return("KG");
			mockCodeMapper.Expect(x => x.GetRecipientCodeUnkeyed("FAMFPSSIN_FPS", "FAMFPSSIN", "FPS Manifest txt - Receive Consols & Shipments", "Defaults", "Volume Unit")).Return("M3");
			mockCodeMapper.Expect(x => x.GetRecipientCode("FAMFPSSIN_FPS", "FAMFPSSIN", "FPS Manifest txt - Receive Consols & Shipments", "Payment Method", "1")).Return("PPD");
			mockCodeMapper.Expect(x => x.GetRecipientCode("FAMFPSSIN_FPS", "FAMFPSSIN", "FPS Manifest txt - Receive Consols & Shipments", "Payment Method", "2")).Return("CLT");
			mockCodeMapper.Expect(x => x.GetRecipientCode("FAMFPSSIN_FPS", "FAMFPSSIN", "FPS Manifest txt - Receive Consols & Shipments", "Container Size", "3")).Repeat.Any().Return("40");
			mockCodeMapper.Expect(x => x.GetRecipientCode("FAMFPSSIN_FPS", "FAMFPSSIN", "FPS Manifest txt - Receive Consols & Shipments", "Container Type", "1")).Repeat.Any().Return("GP");
			mockCodeMapper.Expect(x => x.GetRecipientCode("FAMFPSSIN_FPS", "FAMFPSSIN", "FPS Manifest txt - Receive Consols & Shipments", "Branch", "Company", "SGSIN")).Return("FPS");
			mockCodeMapper.Expect(x => x.GetRecipientCode("FAMFPSSIN_FPS", "FAMFPSSIN", "FPS Manifest txt - Receive Consols & Shipments", "Branch", "Branch", "SGSIN")).Return("FPS");
			mockCodeMapper.Expect(x => x.GetRecipientCode("FAMFPSSIN_FPS", "FAMFPSSIN", "FPS Manifest txt - Receive Consols & Shipments", "Branch", "Company", "MYPKG")).Return("PKG");
			mockCodeMapper.Expect(x => x.GetRecipientCode("FAMFPSSIN_FPS", "FAMFPSSIN", "FPS Manifest txt - Receive Consols & Shipments", "Branch", "Branch", "MYPKG")).Return("PKG");
			mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Repeat.Any().Return("FAMFPSSIN");

			var extensionObjects = new Dictionary<string, object>() { 
			     { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockCodeMapper },
					 { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockDateMapper },
					 { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockContextAccessor }
			};
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);

			string sourceFile = "FPS_EDI_2_UniversalShipment.TestFiles.MultiRecord_Input.xml";
			string expectedFile = "FPS_EDI_2_UniversalShipment.TestFiles.MultiRecord_Output.xml";
			mapTester.Execute<FPS_EDI_2_UniversalShipment>(sourceFile, expectedFile);

			mockCodeMapper.VerifyAllExpectations();
			mockDateMapper.VerifyAllExpectations();
			mockContextAccessor.VerifyAllExpectations();
		}
	}
}
