using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business.MessagingProcess
{
	public sealed class CustomsMessagingSupporter : ICustomsMessagingSupporter
	{
		public CustomsMessagingSupporter(BusinessObject topLevelBusinessObject, ICustomsMessagingProviderFactory providerFactory)
		{
			this.topLevelBusinessObject = topLevelBusinessObject;
			this.providerFactory = providerFactory;
		}

		readonly BusinessObject topLevelBusinessObject;
		readonly ICustomsMessagingProviderFactory providerFactory;
		ICustomsMessagingProvider provider;
		IReadOnlyCollection<ICustomsMessenger> messengers;

		BusinessObject ICustomsMessagingSupporter.TopLevelBusinessObject => topLevelBusinessObject;
		IReadOnlyCollection<ICustomsMessenger> ICustomsMessagingSupporter.Messengers => GetMessengers();
		ICustomsMessagingProvider ICustomsMessagingSupporter.Provider => GetProvider();

		ICustomsMessagingProvider GetProvider()
		{
			if (provider == null)
			{
				provider = providerFactory.CreateProvider(topLevelBusinessObject);
			}

			return provider;
		}

		IReadOnlyCollection<ICustomsMessenger> GetMessengers()
		{
			if (messengers == null)
			{
				messengers = GetProvider().GetMessengers();
			}

			return messengers;
		}
	}
}
