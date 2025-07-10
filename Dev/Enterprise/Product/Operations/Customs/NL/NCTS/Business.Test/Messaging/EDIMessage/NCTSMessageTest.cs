using Enterprise.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

[TestedType(typeof(NCTSMessage))]
sealed class NCTSMessageTest : EDIMessageTest
{
	public void TestSetDefaultValues()
	{
		AssertEquals("EM_MessageType", NLNctsConstants.Messaging.NCT, Factory.New<NCTSMessage>().EM_MessageType);
	}
}
