using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.ServiceTasks.UniversalCustomsMessaging;
using Enterprise.Messaging.Business.MessageProcessor;

namespace Enterprise.Customs.ServiceTasks.Testing
{
	class UCPMessagePackersRegistrationSubstitute : IDisposable
	{
		public UCPMessagePackersRegistrationSubstitute()
			: this(Array.Empty<(string ApplicationCode, IUniversalCustomsEDIMessagePacker MessagePacker)>())
		{ }

		public UCPMessagePackersRegistrationSubstitute(params (string ApplicationCode, IUniversalCustomsEDIMessagePacker MessagePacker)[] messageProcessors)
		{
			configurationProviderToSubstitute = UCPSubscribers.ConfigurationProvider;
			UCPSubscribers.ConfigurationProvider = () =>
				messageProcessors.Select(x =>
					new UniversalCustomsDataRegistration<IUniversalCustomsEDIMessagePacker, UniversalCustomsEDIMessagePackerAttribute>(
						x.ApplicationCode,
						new UCPMessagePackerInstanceProvider(x.MessagePacker)));
		}

		public void Dispose()
		{
			UCPSubscribers.ConfigurationProvider = configurationProviderToSubstitute;
		}

		readonly Func<IEnumerable<UniversalCustomsDataRegistration<IUniversalCustomsEDIMessagePacker, UniversalCustomsEDIMessagePackerAttribute>>> configurationProviderToSubstitute;
	}

	class UCPMessagePackerInstanceProvider : IDataCreator<IUniversalCustomsEDIMessagePacker>
	{
		public UCPMessagePackerInstanceProvider(IUniversalCustomsEDIMessagePacker instance)
		{
			this.instance = instance;
		}

		public IUniversalCustomsEDIMessagePacker Create() => instance;

		readonly IUniversalCustomsEDIMessagePacker instance;
	}
}
