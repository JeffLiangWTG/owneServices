using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Types;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Messaging.Business.MessageProcessor;

namespace Enterprise.Customs.ServiceTasks.UniversalCustomsMessaging
{
	public static class UCUSubscribers
	{
		public static bool HasUniversalCustomsInterchangeUnpackers() =>
			interchangeUnpackerCreatorsByApplicationCode.Value.Any();

		public static IEnumerable<string> GetApplicationCodes() =>
			interchangeUnpackerCreatorsByApplicationCode.Value.Keys;

		public static IUniversalCustomsInterchangeUnpacker GetInterchangeUnpacker(string applicationCode) =>
			interchangeUnpackerCreatorsByApplicationCode.Value.TryGetValue(applicationCode, out var interchangeUnpackerCreator)
			? interchangeUnpackerCreator.Create()
			: null;

		static Lazy<IReadOnlyDictionary<string, IDataCreator<IUniversalCustomsInterchangeUnpacker>>> interchangeUnpackerCreatorsByApplicationCode =>
			new(() =>
				UniversalCustomsDataRegistration<IUniversalCustomsInterchangeUnpacker, UniversalCustomsInterchangeUnpackerAttribute>.AssertUniqueApplicationCodes(
					ConfigurationProvider()
					.Concat(CustomsDataRegistry.Instance.UCMTestApplicationCodes.Value
						.Cast<UCMEDIMessageTestType>()
						.Select(testType =>
							new UniversalCustomsDataRegistration<IUniversalCustomsInterchangeUnpacker, UniversalCustomsInterchangeUnpackerAttribute>(
								(string)testType.ApplicationCode,
								new T1UniversalCustomsInterchangeUnpackerCreator(testType.UCUDelayTimeInMilliseconds))))
				).ToDictionary(x => x.ApplicationCode, x => x.Creator),
				LazyThreadSafetyMode.ExecutionAndPublication);

		internal static Func<IEnumerable<UniversalCustomsDataRegistration<IUniversalCustomsInterchangeUnpacker, UniversalCustomsInterchangeUnpackerAttribute>>> ConfigurationProvider { get; set; } =
			() => UniversalCustomsDataRegistration<IUniversalCustomsInterchangeUnpacker, UniversalCustomsInterchangeUnpackerAttribute>.GetAssemblyRegistrations();

		class T1UniversalCustomsInterchangeUnpackerCreator : IDataCreator<IUniversalCustomsInterchangeUnpacker>
		{
			public T1UniversalCustomsInterchangeUnpackerCreator(ZInt ucuDelayTimeInMilliseconds)
			{
				instance = new Lazy<IUniversalCustomsInterchangeUnpacker>(() =>
					new T1InterchangeUnpacker(ucuDelayTimeInMilliseconds));
			}

			public IUniversalCustomsInterchangeUnpacker Create() => instance.Value;

			readonly Lazy<IUniversalCustomsInterchangeUnpacker> instance;
		}
	}
}
