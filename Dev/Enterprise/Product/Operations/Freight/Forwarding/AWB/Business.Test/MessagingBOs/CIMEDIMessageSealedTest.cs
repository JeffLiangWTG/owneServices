using CargoWise.EntityFramework.Testing;

namespace Enterprise.Freight.Forwarding.AWB.Business.Testing
{
	sealed class CIMEDIMessageSealedTest : TestCaseWithFactory
	{
		public void TestEM_FormattedMessageText()
		{
			var message = Factory.New<CIMEDIMessage>();
			message.EM_MessageText = "Something";
			AssertEquals("message.EM_FormattedMessageText", "Something", message.EM_FormattedMessageText);
			message.EM_MessageText = "Something Else";
			AssertEquals("message.EM_FormattedMessageText", "Something Else", message.EM_FormattedMessageText);
		}
	}
}
