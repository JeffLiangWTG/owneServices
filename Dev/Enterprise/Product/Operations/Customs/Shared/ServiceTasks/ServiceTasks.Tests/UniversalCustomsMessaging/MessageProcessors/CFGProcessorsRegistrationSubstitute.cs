using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Messaging.Business.MessageProcessor;

namespace Enterprise.Customs.ServiceTasks.UniversalCustomsMessaging.Testing
{
	class CFGProcessorsRegistrationSubstitute : IDisposable
	{
		public CFGProcessorsRegistrationSubstitute()
			: this(Array.Empty<(string MessageType, ICFGUniversalCustomsMessageProcessor Processor)>())
		{ }

		public CFGProcessorsRegistrationSubstitute(params (string MessageType, ICFGUniversalCustomsMessageProcessor Processor)[] messageProcessors)
		{
			configurationProviderToSubstitute = CFGUniversalCustomsMessageProcessors.ConfigurationProvider;
			CFGUniversalCustomsMessageProcessors.ConfigurationProvider = () =>
				messageProcessors.Select(x =>
					new CFGUniversalCustomsDataRegistration<ICFGUniversalCustomsMessageProcessor, UniversalCustomsMessageCFGProcessorAttribute>(
						x.MessageType,
						new CFGProcessorInstanceProvider(x.Processor)));
		}

		public void Dispose()
		{
			CFGUniversalCustomsMessageProcessors.ConfigurationProvider = configurationProviderToSubstitute;
		}

		readonly Func<IEnumerable<CFGUniversalCustomsDataRegistration<ICFGUniversalCustomsMessageProcessor, UniversalCustomsMessageCFGProcessorAttribute>>> configurationProviderToSubstitute;
	}

	class CFGProcessorInstanceProvider : IDataCreator<ICFGUniversalCustomsMessageProcessor>
	{
		public CFGProcessorInstanceProvider(ICFGUniversalCustomsMessageProcessor instance)
		{
			this.instance = instance;
		}

		public ICFGUniversalCustomsMessageProcessor Create() => instance;

		readonly ICFGUniversalCustomsMessageProcessor instance;
	}
}
