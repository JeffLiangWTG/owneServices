using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MessagingProcess;

namespace Enterprise.Customs.TR.Business.MessagingProcess
{
	public class TRCustomsMessagingProviderFactory : ICustomsMessagingProviderFactory
	{
		public TRCustomsMessagingProviderFactory(Func<BusinessObject, ZString, TRMessageSigner, ICustomsMessagingProvider> createProviderFunc, ZString messageType, TRMessageSigner signer = null)
		{
			this.createProviderFunc = createProviderFunc;
			this.messageType = messageType;
			this.signer = signer;
		}
		readonly Func<BusinessObject, ZString, TRMessageSigner, ICustomsMessagingProvider> createProviderFunc;
		readonly ZString messageType;
		readonly TRMessageSigner signer;

		ICustomsMessagingProvider ICustomsMessagingProviderFactory.CreateProvider(BusinessObject businessObject) => createProviderFunc(businessObject, messageType, signer);
	}
}
