using Enterprise.DocumentEngineCore.DocumentSupport;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(N5116EDIMessage))]
	sealed class N5116EDIMessageTest : Enterprise.Messaging.Testing.EDIMessageTest
	{
		[ExpectNoExceptions]
		public void TestDocumentSupporter()
		{
			var testMessage = Factory.New<N5116EDIMessage>();
			NUnit.Framework.Assert.That(((IDocumentSupportable)testMessage).DocumentSupporter.GetType(), NUnit.Framework.Is.EqualTo(typeof(N5116MessageDocumentSupporter)));
		}
	}
}
