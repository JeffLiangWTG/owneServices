using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.eManifest.Business;
using Enterprise.Edifact;
using Enterprise.Edifact.D08A.Messages.CONTRL;

namespace Enterprise.Customs.US.eManifest.Messaging.MessageProcessors.Testing
{
	sealed class SyntaxAndServiceReportMessageWrapperTest : TestCaseWithFactory
	{
		public void TestMessageResponses()
		{
			const string messageText = @"UNH+17+CONTRL:D:03B:UN'UCI+15+8CWS:ZZ+CBP-ACE-TEST:ZZ+7'UCF+15+8CWS:ZZ+CBP-ACE-TEST:ZZ+7'UCM+11+PAXLST:D:03B:UN+7'UCM+17+CUSCAR:D:03B:UN+4'UCS+5'UCD+13+2:1'UCD+12+2:1'UCS+6'UCD+12+2:1'UCS+8+15'UCM+18+CUSCAR:D:03B:UN+4+29'UNT+11+17'UNZ+1+17'";
			var message = Factory.New<SyntaxErrorMessage>();
			message.EM_MessageText = messageText;
			var contrl = message.GetAutoEdifactMessageUsingNamedFactory(new eManifestMessageFactory(), new UNOACharacterSet()) as CONTRLMessage;
			var wrapper = new SyntaxAndServiceReportMessageWrapper(contrl);
			var responses = wrapper.MessageResponses.ToList();
			AssertEquals("MessageResponses.Count", 3, responses.Count);
			AssertMessageResponse(responses[0], "11", true, false, 0);
			AssertMessageResponse(responses[1], "17", false, true, 4);
			AssertMessageResponse(responses[2], "18", false, true, 1);
		}

		static void AssertMessageResponse(SyntaxAndServiceReportMessageWrapper.MessageResponse response, string messageNumber, bool isAcknowledgement, bool isSyntaxError, int syntaxErrorsCount)
		{
			AssertEquals("MessageNumber", messageNumber, response.MessageNumber);
			AssertEquals("IsAcknowledgement", isAcknowledgement, response.IsAcknowledgement);
			AssertEquals("IsSyntaxError", isSyntaxError, response.IsSyntaxError);
			AssertEquals("SyntaxErrors.Count", syntaxErrorsCount, response.GetSyntaxErrors(string.Empty).Count());
		}
	}
}
