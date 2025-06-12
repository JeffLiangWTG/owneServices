using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.ForwardingPortMessaging.Portbase.Transforms;

namespace CargoWise.eHub.Products.ForwardingPortMessaging.Portbase.Tests
{
	[TestClass]
	public class TA112UniEventTests
	{
		const string filePath = "TA112UniEvent.TestFiles.";

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestPortbaceTA112EDIUniversalEvent()
		{
			TestMapping("Test1_AK_input.xml", "Test1_AK_output.xml", "AK", "IRA");
			TestMapping("Test2_NA_input.xml", "Test2_NA_output.xml", "NA", "IRJ");
			TestMapping("Test3_UKN_input.xml", "Test3_UKN_output.xml", "UKN", "");
		}

		public void TestMapping(string sourceFile, string expectedFile, string responseTypeCode, string eventType)
		{
			var input = filePath + sourceFile;
			var expectedOutput = filePath + expectedFile;

			var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();

			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "PORTBASE", "@recipientId", "", "@ST_ID", "PBSMSG", "@value", "100")).Return("C00001004").Repeat.AtLeastOnce();
			mockCodeMapper.Expect(x => x.GetRecipientCode("PORTBASE", "PORTBASE", "Technical Acknowledgement TA11 from Portbase", "Event Type", "Event Type", responseTypeCode)).Return(eventType).Repeat.Once();
			mockDateMapper.Expect(x => x.CurrentDateTime(Arg.Is("s"))).Return("2015-07-09T09:30:10").Repeat.Any();
			var extensionObjects = new Dictionary<string, object>()
			{
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockDateMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockCodeMapper }
			};

			var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.Execute<TA112UniEvent>(input, expectedOutput);

			mockDateMapper.VerifyAllExpectations();
			mockCodeMapper.VerifyAllExpectations();
		}
	}
}
