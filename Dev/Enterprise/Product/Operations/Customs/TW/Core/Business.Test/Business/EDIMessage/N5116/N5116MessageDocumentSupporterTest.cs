using Enterprise.DocumentEngineCore.DocumentSupport;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(N5116MessageDocumentSupporter))]
	sealed class N5116MessageDocumentSupporterTest : TWEDIMessageDocumentSupporterTest<N5116EDIMessage>
	{
		[ExpectNoExceptions]
		public void TestGetBODocDataProviders()
		{
			CombineAssertions("Message for N5116EDIMessage", () =>
			{
				var message = GetDocumentSupportableBusinessObject();
				var providers = message.DocumentSupporter.GetBODocDataProviders(new DataContextValue(N5116MessageDocumentSupporter.N5116MessagePair), null);
				NUnit.Framework.Assert.That(providers.Length, NUnit.Framework.Is.EqualTo(1), "Provider for N5116MessagePair");
				providers = message.DocumentSupporter.GetBODocDataProviders(new DataContextValue(".AA"), null);
				NUnit.Framework.Assert.That(providers, NUnit.Framework.Is.EqualTo(default(Enterprise.DocumentEngineCore.DocWrappers.IBODocDataProvider[])), "Provider for AA - should be [null]");
			});
		}
	}
}
