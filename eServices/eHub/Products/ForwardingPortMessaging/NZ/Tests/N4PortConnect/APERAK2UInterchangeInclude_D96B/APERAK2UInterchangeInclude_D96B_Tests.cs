using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.ForwardingPortMessaging.NZ.N4PortConnect;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.ForwardingPortMessaging.NZ.Tests
{
	[TestClass]
	public class APERAK2UInterchangeInclude_D96B_Tests
  {
		const string filePath = "N4PortConnect.APERAK2UInterchangeInclude_D96B.TestFiles.";

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestAPERAK2UInterchangeInclude_D96B()
		{
			AssertMapping("Test1_input_accepted.xml", "Test1_output_accepted.xml", "N4", "N4RMSG", "N4R0000000006", "Pre-Advice Export Notificaiton (NZ)", "WTH", "AP", "NZLYT", "ForwardingConsol_C00001386", "MWA");
			AssertMapping("Test2_input_rejected.xml", "Test2_output_rejected.xml", "N4", "N4RMSG", "N4R0000000006", "Pre-Advice Export Notificaiton (NZ)", "WTH", "RE", "NZLYT", "ForwardingConsol_C00001386", "MRJ");
		}

		void AssertMapping(string sourceFile, string expectedFile, string senderID, string serviceProviderMsgID, string messageReference,
								string subscribedDocumentName, string subscribedPurpose, string responseCode, string subscribedOperationPort, string subscribedForwardingType, string eventType)
		{
			var input = filePath + sourceFile;
			var expectedOutput = filePath + expectedFile;

			var eventReference = eventType == "MWA" ? "MWR accepted" : string.Empty;

			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();

			mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(senderID).Repeat.AtLeastOnce();
			mockContextAccessor.Expect(x => x.SetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", "HYEBNEUAT"));

			mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Port Settings", "MSGID", senderID)).Return(serviceProviderMsgID);
			mockCodeMapper.Expect(x => x.GetRecipientCode("FPMAPERAK", "FPMAPERAK", "FPM APERAK Configuration", "Event Type", "Event Reference", responseCode, subscribedPurpose)).Return(eventReference).Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode("FPMAPERAK", "FPMAPERAK", "FPM APERAK Configuration", "Event Type", "Event Type", responseCode, subscribedPurpose)).Return(eventType).Repeat.Any();
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedClients", "", "@senderId", senderID, "@ST_ID", serviceProviderMsgID, "@value", messageReference)).Return("HYEBNEUAT").Repeat.Any();

			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", serviceProviderMsgID, "@value", messageReference, "@referenceType", "PreAdvice")).Return("MSCU5565665_NZLYT_HYEBNEUAT").Repeat.Any();
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", serviceProviderMsgID, "@value", messageReference, "@referenceType", "DocumentName")).Return(subscribedDocumentName).Repeat.Any();
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", serviceProviderMsgID, "@value", messageReference, "@referenceType", "Purpose")).Return(subscribedPurpose).Repeat.Any();
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", serviceProviderMsgID, "@value", messageReference, "@referenceType", "OperationPort")).Return(subscribedOperationPort).Repeat.Any();
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", serviceProviderMsgID, "@value", messageReference, "@referenceType", "ForwardingType")).Return(subscribedForwardingType).Repeat.Any();

			mockDateMapper.Expect(x => x.CurrentDateTime("yyyy-MM-ddTHH:mm:ss")).Return("2024-01-05T14:48:00").Repeat.Any();


			var extensionObjects = new Dictionary<string, object>()
			{
				{ "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/DateMapper", mockDateMapper }
			};

			var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.ExecuteCompiled<APERAK2UInterchangeInclude_D96B>(input, expectedOutput);

			mockContextAccessor.VerifyAllExpectations();
			mockCodeMapper.VerifyAllExpectations();
		}
	}
}
