using Enterprise.DocumentEngineCore.DocumentSupport;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(N5111EDIMessage))]
	sealed class N5111EDIMessageTest : Enterprise.Messaging.Testing.EDIMessageTest
	{
		[ExpectNoExceptions]
		public void TestDocumentSupporter()
		{
			var testMessage = Factory.New<N5111EDIMessage>();
			NUnit.Framework.Assert.That(((IDocumentSupportable)testMessage).DocumentSupporter.GetType(), NUnit.Framework.Is.EqualTo(typeof(N5111MessageDocumentSupporter)));
		}
	}
}
