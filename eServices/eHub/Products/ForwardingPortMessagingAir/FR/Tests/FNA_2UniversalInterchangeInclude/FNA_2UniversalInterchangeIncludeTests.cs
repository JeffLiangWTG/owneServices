using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using System.Xml.XPath;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.ForwardingPortMessagingAir.FR.CIN;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.ForwardingPortMessagingAir.FR.Tests
{
	[TestClass]
	public class FNA_2UniversalInterchangeIncludeTests
	{
		const string filePath = "FNA_2UniversalInterchangeInclude.TestFiles.";

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestFNA_2UniversalInterchangeInclude()
		{
			AssertMapping("Test1_input.xml", "Test1_output.xml", "60012855640");
			AssertMapping("Test2_input.xml", "Test2_output.xml", "");
		}

		void AssertMapping(string inputFile, string expectedOutputFile, string messageReference)
		{
			var input = filePath + inputFile;
			var expectedOutput = filePath + expectedOutputFile;

			var senderId = "CIN";
			var recipientId = "WTLDAUIIG";
			var serviceProviderMsgId = senderId.Substring(0, 3) + "MSG";

			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();

			mockDateMapper.Expect(x => x.CurrentDateTimeUTC("yyyy-MM-ddTHH:mm:ssZ")).Return("2023-08-10T14:00:00Z").Repeat.Any();
			mockDateMapper.Expect(x => x.ConvertUTCToLocalTimeByUNLOCO("2023-08-10T14:00:00Z", "FRPAR")).Return("2023-08-10T08:00:00").Repeat.Any();

			mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(senderId).Repeat.Once();
			mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(recipientId);
			mockContextAccessor.Expect(x => x.SetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", "clientID"));

			mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE_AIR", "FORWARDING_PORT_MESSAGE_AIR", "FPMA System Configuration", "Port Settings", "MSGID", senderId)).Return(serviceProviderMsgId).Repeat.Any();

			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderId, "@recipientId", "", "@ST_ID", serviceProviderMsgId, "@value", messageReference, "@referenceType", "DocumentName")).Return("Export Notification (755)").Repeat.Any();
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderId, "@recipientId", "", "@ST_ID", serviceProviderMsgId, "@value", messageReference, "@referenceType", "JobNumber")).Return("SAPAR00000016").Repeat.Any();
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderId, "@recipientId", "", "@ST_ID", serviceProviderMsgId, "@value", messageReference, "@referenceType", "ForwardingType")).Return("ForwardingShipment").Repeat.Any();
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderId, "@recipientId", "", "@ST_ID", serviceProviderMsgId, "@value", messageReference, "@referenceType", "OperationPort")).Return("FRCDG").Repeat.Any();
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedClients", "", "@senderId", senderId, "@ST_ID", serviceProviderMsgId, "@value", messageReference)).Return("clientID").Repeat.Any();

			var extensionObjects = new Dictionary<string, object>()
			{
				{"http://schemas.microsoft.com/BizTalk/2003/DateMapper", mockDateMapper},
				{ "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
			};

			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.Execute<FNA_2UniversalInterchangeInclude>(input, expectedOutput);
		}
	}
}
