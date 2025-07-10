using Enterprise.DocumentEngineCore.DocumentSupport;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(N5111MessageDocumentSupporter))]
	sealed class N5111MessageDocumentSupporterTest : TWEDIMessageDocumentSupporterTest<N5111EDIMessage>
	{
		[ExpectNoExceptions]
		public void TestGetBODocDataProviders()
		{
			CombineAssertions("Message for N5111EDIMessage", () =>
			{
				var message = GetDocumentSupportableBusinessObject();
				var providers = message.DocumentSupporter.GetBODocDataProviders(new DataContextValue(N5111MessageDocumentSupporter.N5111MessagePair), null);
				NUnit.Framework.Assert.That(providers.Length, NUnit.Framework.Is.EqualTo(1), "Provider for N5111MessagePair");
				providers = message.DocumentSupporter.GetBODocDataProviders(new DataContextValue(".AA"), null);
				NUnit.Framework.Assert.That(providers, NUnit.Framework.Is.EqualTo(default(Enterprise.DocumentEngineCore.DocWrappers.IBODocDataProvider[])), "Provider for AA - should be [null]");
			});
		}
	}
}
