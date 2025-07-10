using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Messaging.Business.MessageProcessor;

namespace Enterprise.Customs.ServiceTasks.UniversalCustomsMessaging.Testing
{
	class UCUInterchangeUnpackersRegistrationSubstitute : IDisposable
	{
		public UCUInterchangeUnpackersRegistrationSubstitute()
			: this(Array.Empty<(string ApplicationCode, IUniversalCustomsInterchangeUnpacker Processor)>())
		{ }

		public UCUInterchangeUnpackersRegistrationSubstitute(params (string ApplicationCode, IUniversalCustomsInterchangeUnpacker Unpacker)[] messageProcessors)
		{
			configurationProviderToSubstitute = UCUSubscribers.ConfigurationProvider;
			UCUSubscribers.ConfigurationProvider = () =>
				messageProcessors.Select(x =>
					new UniversalCustomsDataRegistration<IUniversalCustomsInterchangeUnpacker, UniversalCustomsInterchangeUnpackerAttribute>(
						x.ApplicationCode,
						new UCUInterchangeUnpackerInstanceProvider(x.Unpacker)));
		}

		public void Dispose()
		{
			UCUSubscribers.ConfigurationProvider = configurationProviderToSubstitute;
		}

		readonly Func<IEnumerable<UniversalCustomsDataRegistration<IUniversalCustomsInterchangeUnpacker, UniversalCustomsInterchangeUnpackerAttribute>>> configurationProviderToSubstitute;
	}

	class UCUInterchangeUnpackerInstanceProvider : IDataCreator<IUniversalCustomsInterchangeUnpacker>
	{
		public UCUInterchangeUnpackerInstanceProvider(IUniversalCustomsInterchangeUnpacker instance)
		{
			this.instance = instance;
		}

		public IUniversalCustomsInterchangeUnpacker Create() => instance;

		readonly IUniversalCustomsInterchangeUnpacker instance;
	}
}
