using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.GBCustoms.Core.BT.Tests.Transforms.TransportResponse2UniversalEvent
{
	[TestClass]
	public class TransportResponse2UniversalEventTests
	{
		const string filePath = "Transforms.TransportResponse2UniversalEvent.TestFiles.";
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TransportResponse2UniversalEvent_Declaration()
		{
			AssertMapping("GBCustoms-DIRECT", "GBCustoms", "test01_input_error.xml", "test01_output_error.xml");
			AssertMapping("GBCustomsTest-DIRECT", "GBCustomsTest", "test02_input_accepted.xml", "test02_output_accepted.xml");
			AssertMapping("GBCustoms-DIRECT", "GBCustoms", "test03_input_error.xml", "test03_output_error.xml");
			AssertMapping("GBCustoms-MCP", "GBCustoms", "test04_input_error_MCP.xml", "test04_output_error_MCP.xml");
			AssertMapping("GBCustoms-MCP", "GBCustoms", "test05_input_accepted_MCP.xml", "test05_output_accepted_MCP.xml");
			AssertMapping("GBCustoms-CNS", "GBCustoms", "test06_input_error_CNS.xml", "test06_output_error_CNS.xml");
			AssertMapping("GBCustoms-CNS", "GBCustoms", "test07_input_accepted_CNS.xml", "test07_output_accepted_CNS.xml");
		}

		void AssertMapping(string senderID, string GBCustomIDSender, string testInput, string testOutput)
		{
			var input = filePath + testInput;
			var expectedOutput = filePath + testOutput;

			var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();

			mockDateMapper.Expect(x => x.CurrentDateTimeUTC("s")).Return("2018-09-17T14:21:01").Repeat.Once();
			mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(senderID).Repeat.Once();
			mockContextAccessor.Expect(x => x.SetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", GBCustomIDSender)).Repeat.Once();

			var extensionObjects = new Dictionary<string, object>()
			{
				{ "http://schemas.microsoft.com/BizTalk/2003/DateMapper", mockDateMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor }
			};

			var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.ExecuteCompiled<BT.Transforms.TransportResponse2UniversalEvent>(input, expectedOutput);

			mockDateMapper.VerifyAllExpectations();
			mockContextAccessor.VerifyAllExpectations();
		}
	}
}
