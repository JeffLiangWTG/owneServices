using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.ForwardingPortMessagingAir.FR.CIN;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.ForwardingPortMessagingAir.FR.Tests
{
	[TestClass]
	public class XML756_2UInterchangeIncludeTest
	{
		const string filePath = "XML756_2UInterchangeInclude.TestFiles.";

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestXML756_2UInterchangeInclude()
		{
			AssertMapping("Test1_input.xml", "Test1_output.xml", "SAPAR00000016");
			AssertMapping("Test2_input.xml", "Test2_output.xml", "SAPAR00000016");
			AssertMapping("Test3_input.xml", "Test3_output.xml", "");
		}

		void AssertMapping(string inputFile, string expectedOutputFile, string jobNumber)
		{
			var input = filePath + inputFile;
			var expectedOutput = filePath + expectedOutputFile;

			var senderId = "CIN";
			var recipientId = "WTLDAUIIG";
			var serviceProviderMsgId = senderId.Substring(0, 3) + "MSG";
			var subscribedDocumentIdentifier = "44444400001";

			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();

			mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(senderId).Repeat.Once();
			mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(recipientId);

			mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE_AIR", "FORWARDING_PORT_MESSAGE_AIR", "FPMA System Configuration", "Port Settings", "MSGID", senderId)).Return(serviceProviderMsgId).Repeat.Any();

			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderId, "@recipientId", "", "@ST_ID", serviceProviderMsgId, "@value", subscribedDocumentIdentifier, "@referenceType", "DocumentName")).Return("Export Notification (755)").Repeat.Any();
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderId, "@recipientId", "", "@ST_ID", serviceProviderMsgId, "@value", subscribedDocumentIdentifier, "@referenceType", "JobNumber")).Return(jobNumber).Repeat.Any();
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderId, "@recipientId", "", "@ST_ID", serviceProviderMsgId, "@value", subscribedDocumentIdentifier, "@referenceType", "ForwardingType")).Return("ForwardingShipment").Repeat.Any();
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderId, "@recipientId", "", "@ST_ID", serviceProviderMsgId, "@value", subscribedDocumentIdentifier, "@referenceType", "OperationPort")).Return("FRCDG").Repeat.Any();

			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedClients", "", "@senderId", senderId, "@ST_ID", serviceProviderMsgId, "@value", subscribedDocumentIdentifier)).Return(recipientId).Repeat.Any();
			mockContextAccessor.Expect(x => x.SetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", recipientId));

			var extensionObjects = new Dictionary<string, object>()
			{
				{ "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
			};

			var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.ExecuteCompiled<XML756_2UInterchangeInclude>(input, expectedOutput);
		}
	}
}
