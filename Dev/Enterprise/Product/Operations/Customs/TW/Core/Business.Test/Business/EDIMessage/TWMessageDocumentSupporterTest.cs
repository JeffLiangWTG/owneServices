using Enterprise.DocumentEngineCore.DocumentSupport;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business
{
	[TestedType(typeof(TWMessageDocumentSupporter))]
	sealed class TWMessageDocumentSupporterTest : Enterprise.Messaging.Testing.EDIMessageTest
	{
		[ExpectNoExceptions]
		public void TestDocumentSupporter()
		{
			var message = Factory.New<TWMessageDocumentSupporter>();
			NUnit.Framework.Assert.That(((IDocumentSupportable)message).DocumentSupporter, NUnit.Framework.Is.TypeOf<TWEDIMessageDocumentSupporter>());
		}
	}
}
