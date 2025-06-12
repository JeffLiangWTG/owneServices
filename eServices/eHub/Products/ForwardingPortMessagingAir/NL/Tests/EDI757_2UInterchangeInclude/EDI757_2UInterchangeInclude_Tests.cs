using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.ForwardingPortMessagingAir.NL.CARGONAUT;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.ForwardingPortMessagingAir.NL.Tests
{
	[TestClass]
	public class EDI757_2UInterchangeInclude_Tests
	{
		const string filePath = "EDI757_2UInterchangeInclude.TestFiles.";

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test_XML757_2UInterchangeInclude()
		{
			AssertMapping("Test1_input.xml", "Test1_output.xml", "20230243699");
			AssertMapping("Test2_input.xml", "Test2_output.xml", "");
			AssertMapping("Test3_input.xml", "Test3_output.xml", "20240243699");
		}

		void AssertMapping(string inputFile, string expectedOutputFile, string reference)
		{
			var input = filePath + inputFile;
			var expectedOutput = filePath + expectedOutputFile;

			var recipientID = "recipientId111";
			var senderId = "CARGONAUT";
			var serviceProviderMsgId = "CGNMSG";
			var subscribedJobNumber = reference == "20230243699" ? "SAPAR00000016" : string.Empty;

			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
			var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();

			mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(senderId).Repeat.Any();

			mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE_AIR", "FORWARDING_PORT_MESSAGE_AIR", "FPMA System Configuration", "Port Settings", "MSGID", senderId)).Return(serviceProviderMsgId).Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE_AIR", "FORWARDING_PORT_MESSAGE_AIR", "FPMA System Configuration", "Port Settings", "Name", senderId)).Return(senderId).Repeat.Any();

			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderId, "@recipientId", "", "@ST_ID", serviceProviderMsgId, "@value", reference, "@referenceType", "JobNumber")).Return(subscribedJobNumber).Repeat.Any();
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderId, "@recipientId", "", "@ST_ID", serviceProviderMsgId, "@value", reference, "@referenceType", "DocumentName")).Return("Export Notification (755)").Repeat.Any();
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderId, "@recipientId", "", "@ST_ID", serviceProviderMsgId, "@value", reference, "@referenceType", "ForwardingType")).Return("ForwardingShipment").Repeat.Any();
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderId, "@recipientId", "", "@ST_ID", serviceProviderMsgId, "@value", reference, "@referenceType", "OperationPort")).Return("FRCDG").Repeat.Any();

			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedClients", "", "@senderId", senderId, "@ST_ID", serviceProviderMsgId, "@value", reference)).Return(recipientID).Repeat.Any();
			mockContextAccessor.Expect(x => x.SetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", recipientID)).Repeat.Any();

			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CGNTRC", recipientID, senderId, subscribedJobNumber + "_" + reference + "_0000099", "MAA", "STU-757")).Repeat.Any();

			var extensionObjects = new Dictionary<string, object>()
			{
				{ "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor", mockDataModelAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
			};

			var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.Execute<EDI757_2UInterchangeInclude>(input, expectedOutput);

			mockContextAccessor.VerifyAllExpectations();
			mockDataModelAccessor.VerifyAllExpectations();
			mockCodeMapper.VerifyAllExpectations();
		}
	}
}
