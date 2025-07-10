using Enterprise.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.UY.Manifest.Business.Testing
{
	[TestedType(typeof(UYMessage))]
	class UYMessageTest : EDIMessageTest
	{
		public void TestMessageDefaults()
		{
			var message = Factory.New<UYMessage>();
			AssertEquals(Enterprise.Messaging.Business.EDIInterchange.ApplicationCodes.UYCustoms, message.EM_ApplicationCode);
		}
	}
}
