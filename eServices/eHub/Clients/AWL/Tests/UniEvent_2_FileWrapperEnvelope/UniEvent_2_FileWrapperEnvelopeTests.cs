using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Clients.AWL.Transforms.UniEvent_2_FileWrapperEnvelope;
using CargoWise.eHub.Core.Transforms.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Clients.AWL.Tests
{
	[TestClass]
	public class UniEvent_2_FileWrapperEnvelopeTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniEvent_2_FileWrapperEnvelope()
		{
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();

			mockCodeMapper.Expect(x => x.GetRecipientCodeUnkeyed("AWLORDORD", "AWLORDORD_GDO", "Genentech Documents: Send eDoc Documents", "Defaults", "Filename Prefix")).Return("115161_entdoc_");
			mockDateMapper.Expect(x => x.CurrentDateTime("yyyyMMddHHmmssfff")).Return("20150916132343001").Repeat.Once();

			var extensionObjects = new Dictionary<string, object>() { 
			     { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockCodeMapper },
					 { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockDateMapper }
			};
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);

			string sourceFile = "UniEvent_2_FileWrapperEnvelope.TestFiles.Input.xml";
			string expectedFile = "UniEvent_2_FileWrapperEnvelope.TestFiles.Output.xml";

			mapTester.ExecuteCompiled<UniEvent_2_FileWrapperEnvelope>(sourceFile, expectedFile);

			mockCodeMapper.VerifyAllExpectations();
			mockDateMapper.VerifyAllExpectations();
		}
	}
}
