using System;
using Enterprise.Edifact;

namespace Enterprise.Customs.NZ.Business.MessageProcessors.Testing
{
	using NUnit.Framework;

	public class MessageFactoriesTest : TestCase
	{
		public void TestNZOutwardReportResponseMessage()
		{
			CheckMessage(NZOutwardReportResponseMessage, typeof(Edifact.D03A.Messages.CUSRES.CUSRESMessage));
		}

		protected void CheckMessage(string messageText, Type expectedMessageType)
		{
			MessageFactory messageFactory = NzEdifactMessageFactory.NZCMessageFactory;
			object message = messageFactory.GetMessage(new UNOACharacterSet(), messageText.Replace("\r", "").Replace("\n", ""));
			if (message != null)
			{
				AssertEquals("Message Type", expectedMessageType, message.GetType());
			}
			else
			{
				Fail("MessageFactory returned null for message");
			}
		}

		const string NZOutwardReportResponseMessage = @"
UNH+3024+CUSRES:D:03A:UN+C00001001'
BGM+963+00000000'
GEI+6+841:120:143'
ERP+001:080'
ERC+677::143'
UNT+6+3024'";
	}
}
