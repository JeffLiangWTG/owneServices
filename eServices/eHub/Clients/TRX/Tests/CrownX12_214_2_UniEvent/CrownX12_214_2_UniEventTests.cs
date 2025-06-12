using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Clients.TRX.Transforms.CrownX12_214_2_UniEvent;
using CargoWise.eHub.Core.Transforms.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Clients.TRX.Tests
{
	[TestClass]
	public class CrownX12_214_2_UniEventTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestCrownX12_214_2_UniEvent()
		{
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();

			mockCodeMapper.Expect(x => x.GetRecipientCodeUnkeyed("TRXELPELP_C03", "TRXELPELP", "Crown Data 214 - Receive Shipment Status", "Defaults", "Data Provider")).Return("CROWN");

			var extensionObjects = new Dictionary<string, object>() { 
			     { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockCodeMapper }
			};
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);

			string sourceFile = "CrownX12_214_2_UniEvent.TestFiles.CrownX12_214_2_UniEvent_POD_input.xml";
			string expectedFile = "CrownX12_214_2_UniEvent.TestFiles.CrownX12_214_2_UniEvent_POD_output.xml";
			mapTester.ExecuteCompiled<CrownX12_214_2_UniEvent>(sourceFile, expectedFile);

			mockCodeMapper.VerifyAllExpectations();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestCrownX12_214_2_UniEvent_NoEventTime()
		{
			var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();

			mockDateMapper.Expect(x => x.CurrentDateTimeWithTimeZone()).Return("2016-06-29T15:51:33+10:00");
			mockCodeMapper.Expect(x => x.GetRecipientCodeUnkeyed("TRXELPELP_C03", "TRXELPELP", "Crown Data 214 - Receive Shipment Status", "Defaults", "Data Provider")).Return("CROWN");

			var extensionObjects = new Dictionary<string, object>() { 
					 { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockDateMapper },
			     { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockCodeMapper }
			};
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);

			string sourceFile = "CrownX12_214_2_UniEvent.TestFiles.CrownX12_214_2_UniEvent_OFD_input.xml";
			string expectedFile = "CrownX12_214_2_UniEvent.TestFiles.CrownX12_214_2_UniEvent_OFD_output.xml";
			mapTester.ExecuteCompiled<CrownX12_214_2_UniEvent>(sourceFile, expectedFile);

			mockDateMapper.VerifyAllExpectations();
			mockCodeMapper.VerifyAllExpectations();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestCrownX12_214_2_UniEvent_Consol()
		{
			var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();

			mockDateMapper.Expect(x => x.CurrentDateTimeWithTimeZone()).Return("2016-06-29T15:51:33+10:00");
			mockCodeMapper.Expect(x => x.GetRecipientCodeUnkeyed("TRXELPELP_C03", "TRXELPELP", "Crown Data 214 - Receive Shipment Status", "Defaults", "Data Provider")).Return("CROWN");

			var extensionObjects = new Dictionary<string, object>() {
					 { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockDateMapper },
				 { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockCodeMapper }
			};
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);

			string sourceFile = "CrownX12_214_2_UniEvent.TestFiles.Consol_Input.xml";
			string expectedFile = "CrownX12_214_2_UniEvent.TestFiles.Consol_Output.xml";
			mapTester.ExecuteCompiled<CrownX12_214_2_UniEvent>(sourceFile, expectedFile);

			mockDateMapper.VerifyAllExpectations();
			mockCodeMapper.VerifyAllExpectations();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestCrownX12_214_2_UniEvent_Consol_Fallback()
		{
			var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();

			mockDateMapper.Expect(x => x.CurrentDateTimeWithTimeZone()).Return("2016-06-29T15:51:33+10:00");
			mockCodeMapper.Expect(x => x.GetRecipientCodeUnkeyed("TRXELPELP_C03", "TRXELPELP", "Crown Data 214 - Receive Shipment Status", "Defaults", "Data Provider")).Return("CROWN");

			var extensionObjects = new Dictionary<string, object>() {
					 { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockDateMapper },
				 { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockCodeMapper }
			};
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);

			string sourceFile = "CrownX12_214_2_UniEvent.TestFiles.Consol_Fallback_Input.xml";
			string expectedFile = "CrownX12_214_2_UniEvent.TestFiles.Consol_Fallback_Output.xml";
			mapTester.ExecuteCompiled<CrownX12_214_2_UniEvent>(sourceFile, expectedFile);

			mockDateMapper.VerifyAllExpectations();
			mockCodeMapper.VerifyAllExpectations();
		}
	}
}
