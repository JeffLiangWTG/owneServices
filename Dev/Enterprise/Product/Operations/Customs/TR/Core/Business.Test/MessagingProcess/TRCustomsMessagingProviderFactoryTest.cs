using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.MessagingProcess;
using Enterprise.Customs.TR.Business.MessagingProcess;

namespace Enterprise.Customs.TR.Business.Testing.MessagingProcess
{
	sealed class TRCustomsMessagingProviderFactoryTest : TestCaseWithFactory
	{
		public void TestICustomsMessagingProviderFactory()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			var msgType = "ORG";
			var signer = TRMessageSigner.New();

			ICustomsMessagingProviderFactory providerFactory = new TRCustomsMessagingProviderFactory(ProviderForTest.ProviderFunc, msgType, signer);

			var provider = providerFactory.CreateProvider(dummy);

			AssertType<ProviderForTest>("Provider Type", provider);
			var actualProvider = provider as ProviderForTest;

			CombineAssertions(() =>
			{
				AssertSame("BO", dummy, actualProvider.BizObj);
				AssertEquals("MsgType", msgType, actualProvider.MsgType);
				AssertSame("Signer", signer, actualProvider.Signer);
			});
		}

		class ProviderForTest : ICustomsMessagingProvider
		{
			public static ICustomsMessagingProvider ProviderFunc(BusinessObject bizObj, ZString msgType, TRMessageSigner signer)
			{
				return new ProviderForTest(bizObj, msgType, signer);
			}

			public ProviderForTest(BusinessObject bizObj, ZString msgType, TRMessageSigner signer)
			{
				BizObj = bizObj;
				MsgType = msgType;
				Signer = signer;
			}
			public BusinessObject BizObj { get; }
			public ZString MsgType { get; }
			public TRMessageSigner Signer { get; }

			bool ICustomsMessagingProvider.IsInTestMode => throw new System.NotImplementedException();
			bool ICustomsMessagingProvider.EnableTestModeValidation => throw new System.NotImplementedException();
			IReadOnlyCollection<ICustomsMessenger> ICustomsMessagingProvider.GetMessengers() =>	throw new System.NotImplementedException();
		}
	}
}
