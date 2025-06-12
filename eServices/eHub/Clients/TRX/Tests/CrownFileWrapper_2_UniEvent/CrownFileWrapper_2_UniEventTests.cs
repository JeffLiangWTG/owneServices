using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Clients.TRX.Transforms.CrownFileWrapper_2_UniEvent;
using CargoWise.eHub.Core.Transforms.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Clients.TRX.Tests
{
	[TestClass]
	public class CrownFileWrapper_2_UniEventTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestCrownFileWrapper_2_UniEvent()
		{
			var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();

			mockDateMapper.Expect(x => x.CurrentDateTimeWithTimeZone()).Return("2016-07-06T09:57:05+10:00");
			mockCodeMapper.Expect(x => x.GetRecipientCodeUnkeyed("TRXELPELP_C05", "TRXELPELP", "Crown Data PDF & TIFF - Receive eDocs", "Defaults", "Event Code")).Return("DDI");
			mockCodeMapper.Expect(x => x.GetRecipientCodeUnkeyed("TRXELPELP_C05", "TRXELPELP", "Crown Data PDF & TIFF - Receive eDocs", "Defaults", "Event Reference")).Return("TEST");
			mockCodeMapper.Expect(x => x.GetRecipientCodeUnkeyed("TRXELPELP_C05", "TRXELPELP", "Crown Data PDF & TIFF - Receive eDocs", "Defaults", "Document Type")).Return("POD");
			mockCodeMapper.Expect(x => x.GetRecipientCodeUnkeyed("TRXELPELP_C05", "TRXELPELP", "Crown Data PDF & TIFF - Receive eDocs", "Defaults", "Document Description")).Return("Proof Of Delivery");
			mockContextAccessor.Expect(x => x.GetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06")).Return("/Hello/Yo/Doc_S54615268_20160726.pdf");

			var extensionObjects = new Dictionary<string, object>() { 
					 { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockDateMapper },
			     { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockCodeMapper },
					 { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockContextAccessor }
			};
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);

			string sourceFile = "CrownFileWrapper_2_UniEvent.TestFiles.CrownFileWrapper_2_UniEvent_input.xml";
			string expectedFile = "CrownFileWrapper_2_UniEvent.TestFiles.CrownFileWrapper_2_UniEvent_output.xml";
			mapTester.ExecuteCompiled<CrownFileWrapper_2_UniEvent>(sourceFile, expectedFile);

			mockDateMapper.VerifyAllExpectations();
			mockCodeMapper.VerifyAllExpectations();
			mockContextAccessor.VerifyAllExpectations();
		}
	}
}
