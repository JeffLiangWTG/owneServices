using Enterprise.DocumentEngineCore.DocumentSupport;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(N5110EDIMessage))]
	sealed class N5110EDIMessageTest : Enterprise.Messaging.Testing.EDIMessageTest
	{
		[ExpectNoExceptions]
		public void TestDefaultValues()
		{
			var message = Factory.New<N5110EDIMessage>();
			NUnit.Framework.Assert.That(message.EM_MessageType, NUnit.Framework.Is.EqualTo("TPC").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestDocumentSupporter()
		{
			var testMessage = Factory.New<N5110EDIMessage>();
			NUnit.Framework.Assert.That(((IDocumentSupportable)testMessage).DocumentSupporter.GetType(), NUnit.Framework.Is.EqualTo(typeof(N5110MessageDocumentSupporter)));
		}
	}
}
