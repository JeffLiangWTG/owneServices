using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.ForwardingPortMessaging.Portbase.Transforms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.ForwardingPortMessaging.Portbase.Tests
{
	[TestClass]
	public class M1992UniEventTests
	{
		const string filePath = "M1992UniEvent.TestFiles.";

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void M1992UniEventTest()
		{
			AssertMapping("Test1_AQ_input.xml", "Test1_AQ_output.xml", "AQ", "MAA");
			AssertMapping("Test2_RP_input.xml", "Test2_RP_output.xml", "RP", "MRJ");
			AssertMapping("Test3_UKN_input.xml", "Test3_UKN_output.xml", "UKN", "");
		}

		void AssertMapping(string sourceFile, string expectedFile, string responseTypeCode, string eventType)
		{
			var input = filePath + sourceFile;
			var expectedOutput = filePath + expectedFile;

			var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();

			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "PORTBASE", "@recipientId", "", "@ST_ID", "PBSMSG", "@value", "100")).Return("C00001004").Repeat.AtLeastOnce();
			mockCodeMapper.Expect(x => x.GetRecipientCode("PORTBASE", "PORTBASE", "Acknowledgement Message M199 from Portbase", "Event Type", "Event Type", responseTypeCode)).Return(eventType).Repeat.Once();
			var extensionObjects = new Dictionary<string, object>()
			{
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockDateMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockCodeMapper }
			};

			var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.Execute<M1992UniEvent>(input, expectedOutput);

			mockDateMapper.VerifyAllExpectations();
			mockCodeMapper.VerifyAllExpectations();
			}
	}
}
