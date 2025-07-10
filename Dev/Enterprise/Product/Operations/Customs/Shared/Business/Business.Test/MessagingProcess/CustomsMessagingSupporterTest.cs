using System.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.MessagingProcess.Testing
{
	sealed class CustomsMessagingSupporterTest : TestCaseWithFactory
	{
		public void TestICustomsMessagingSupporter()
		{
			var topLevelBO = Factory.New<DummyBizObjWithMessages>();
			var providerFactory = new CustomsMessagingProviderFactoryForTest();
			ICustomsMessagingSupporter supporter = new CustomsMessagingSupporter(topLevelBO, providerFactory);

			CombineAssertions(() =>
			{
				AssertSame("Top Level BusinessObj", topLevelBO, supporter.TopLevelBusinessObject);
				var provider = supporter.Provider;
				AssertType<CustomsMessagingProviderImplForTest>("Provider Type", provider);
				AssertSame("Provider created once", provider, supporter.Provider);

				var messenger = supporter.Messengers.Single();
				AssertType<CustomsMessengerImplForTest>("Messenger type", messenger);
				AssertSame("Messenger Owner", topLevelBO, messenger.Owner.MessageOwner);
				var messenger2 = supporter.Messengers.Single();
				AssertSame("Messengers not recreated", messenger, messenger2);
			});
		}
	}
}
