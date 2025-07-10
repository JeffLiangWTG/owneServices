using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Messaging.Business.MessageProcessor;

namespace Enterprise.Customs.ServiceTasks.UniversalCustomsMessaging
{
	public static class UniversalCustomsMessagingSubscribers
	{
		public static bool HasUniversalCustomsMessagingSubscribers() =>
			messageProcessorCreatorsByApplicationCode.Value.Any();

		public static IEnumerable<string> GetApplicationCodes() =>
			messageProcessorCreatorsByApplicationCode.Value.Keys;

		public static IUniversalCustomsMessageProcessor GetMessageProcessor(string applicationCode) =>
			messageProcessorCreatorsByApplicationCode.Value.TryGetValue(applicationCode, out var messageProcessorCreator)
			? messageProcessorCreator.Create()
			: null;

		static Lazy<IReadOnlyDictionary<string, IDataCreator<IUniversalCustomsMessageProcessor>>> messageProcessorCreatorsByApplicationCode =>
			new(() =>
			{
				return UniversalCustomsDataRegistration<IUniversalCustomsMessageProcessor, UniversalCustomsMessageProcessorAttribute>.AssertUniqueApplicationCodes(
					ConfigurationProvider()
					.Concat(CustomsDataRegistry.Instance.UCMTestApplicationCodes.Value
						.Cast<UCMEDIMessageTestType>()
						.Select(testType =>
							new UniversalCustomsDataRegistration<IUniversalCustomsMessageProcessor, UniversalCustomsMessageProcessorAttribute>(
								(string)testType.ApplicationCode,
								new T1UniversalCustomsMessageProcessorCreator(testType.UCKDelayTimeInMilliseconds, testType.UCQDelayTimeInMilliseconds, testType.ShouldMessageBeProcessedInASeparateFactory))))
				).ToDictionary(x => x.ApplicationCode, x => x.Creator);
			}, LazyThreadSafetyMode.ExecutionAndPublication);

		internal static Func<IEnumerable<UniversalCustomsDataRegistration<IUniversalCustomsMessageProcessor, UniversalCustomsMessageProcessorAttribute>>> ConfigurationProvider { get; set; } =
			() => UniversalCustomsDataRegistration<IUniversalCustomsMessageProcessor, UniversalCustomsMessageProcessorAttribute>.GetAssemblyRegistrations();

		class T1UniversalCustomsMessageProcessorCreator : IDataCreator<IUniversalCustomsMessageProcessor>
		{
			public T1UniversalCustomsMessageProcessorCreator(ZInt uckDelayTimeInMilliseconds, ZInt ucqDelayTimeInMilliseconds, bool shouldMessageBeProcessedInASeparateFactory)
			{
				instance = new Lazy<IUniversalCustomsMessageProcessor>(() =>
					new T1UniversalCustomsMessageProcessor(uckDelayTimeInMilliseconds, ucqDelayTimeInMilliseconds, shouldMessageBeProcessedInASeparateFactory));
			}

			public IUniversalCustomsMessageProcessor Create() => instance.Value;

			readonly Lazy<IUniversalCustomsMessageProcessor> instance;
		}
	}
}
