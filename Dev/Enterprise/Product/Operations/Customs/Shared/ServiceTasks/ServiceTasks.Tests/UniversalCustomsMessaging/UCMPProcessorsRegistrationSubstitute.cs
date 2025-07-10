using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Messaging.Business.MessageProcessor;

namespace Enterprise.Customs.ServiceTasks.UniversalCustomsMessaging.Testing
{
	class UCMPProcessorsRegistrationSubstitute : IDisposable
	{
		public UCMPProcessorsRegistrationSubstitute()
			: this(Array.Empty<(string ApplicationCode, IUniversalCustomsMessageProcessor Processor)>())
		{ }

		public UCMPProcessorsRegistrationSubstitute(params (string ApplicationCode, IUniversalCustomsMessageProcessor Processor)[] messageProcessors)
		{
			configurationProviderToSubstitute = UniversalCustomsMessagingSubscribers.ConfigurationProvider;
			UniversalCustomsMessagingSubscribers.ConfigurationProvider = () =>
				messageProcessors.Select(x =>
					new UniversalCustomsDataRegistration<IUniversalCustomsMessageProcessor, UniversalCustomsMessageProcessorAttribute>(
						x.ApplicationCode,
						new UCMPProcessorInstanceProvider(x.Processor)));
		}

		public void Dispose()
		{
			UniversalCustomsMessagingSubscribers.ConfigurationProvider = configurationProviderToSubstitute;
		}

		readonly Func<IEnumerable<UniversalCustomsDataRegistration<IUniversalCustomsMessageProcessor, UniversalCustomsMessageProcessorAttribute>>> configurationProviderToSubstitute;
	}

	class UCMPProcessorInstanceProvider : IDataCreator<IUniversalCustomsMessageProcessor>
	{
		public UCMPProcessorInstanceProvider(IUniversalCustomsMessageProcessor instance)
		{
			this.instance = instance;
		}

		public IUniversalCustomsMessageProcessor Create() => instance;

		readonly IUniversalCustomsMessageProcessor instance;
	}
}
