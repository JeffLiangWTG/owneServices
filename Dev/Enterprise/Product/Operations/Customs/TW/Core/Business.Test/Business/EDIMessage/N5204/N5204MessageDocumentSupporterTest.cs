using Enterprise.DocumentEngineCore.DocumentSupport;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(N5204MessageDocumentSupporter))]
	sealed class N5204MessageDocumentSupporterTest : TWEDIMessageDocumentSupporterTest<N5204EDIMessage>
	{
		[ExpectNoExceptions]
		public void TestGetBODocDataProviders()
		{
			CombineAssertions("Message for N5204EDIMessage", () =>
			{
				var message = GetDocumentSupportableBusinessObject();
				var providers = message.DocumentSupporter.GetBODocDataProviders(new DataContextValue(N5204MessageDocumentSupporter.N5204MessagePair), null);
				NUnit.Framework.Assert.That(providers.Length, NUnit.Framework.Is.EqualTo(1), "Provider for N5204MessagePair");
				providers = message.DocumentSupporter.GetBODocDataProviders(new DataContextValue(".AA"), null);
				NUnit.Framework.Assert.That(providers, NUnit.Framework.Is.EqualTo(default(Enterprise.DocumentEngineCore.DocWrappers.IBODocDataProvider[])), "Provider for AA - should be [null]");
			});
		}
	}
}
