using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Enterprise.Customs.ServiceTasks.UniversalCustomsMessaging;
using Enterprise.Messaging.Business.MessageProcessor;

namespace Enterprise.Customs.ServiceTasks
{
	public static class UCPSubscribers
	{
		public static bool HasUniversalCustomsMessagePackers() =>
			universalCustomsMessagePackers.Value.Any();

		public static IEnumerable<string> GetApplicationCodes() =>
			universalCustomsMessagePackers.Value.Keys;

		public static IUniversalCustomsEDIMessagePacker GetMessagePacker(string applicationCode) =>
			universalCustomsMessagePackers.Value.TryGetValue(applicationCode, out var messagePackerCreator)
			? messagePackerCreator.Create()
			: null;

		static Lazy<IReadOnlyDictionary<string, IDataCreator<IUniversalCustomsEDIMessagePacker>>> universalCustomsMessagePackers =>
			new(() =>
				UniversalCustomsDataRegistration<IUniversalCustomsEDIMessagePacker, UniversalCustomsEDIMessagePackerAttribute>.AssertUniqueApplicationCodes(ConfigurationProvider()).ToDictionary(x => x.ApplicationCode, x => x.Creator),
				LazyThreadSafetyMode.ExecutionAndPublication);

		internal static Func<IEnumerable<UniversalCustomsDataRegistration<IUniversalCustomsEDIMessagePacker, UniversalCustomsEDIMessagePackerAttribute>>> ConfigurationProvider { get; set; } =
			() => UniversalCustomsDataRegistration<IUniversalCustomsEDIMessagePacker, UniversalCustomsEDIMessagePackerAttribute>.GetAssemblyRegistrations();
	}
}
