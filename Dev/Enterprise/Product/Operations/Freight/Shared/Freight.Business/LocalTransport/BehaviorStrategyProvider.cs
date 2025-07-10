using CargoWise.EntityFramework;

namespace Enterprise.Freight.Business
{
	public class BehaviorStrategyProvider
	{
		public static BehaviorStrategyProvider GetProvider(BusinessObjectFactory factory)
		{
			var host = factory.GetCachedValue<BehaviorProviderHost>();
			if (host.provider == null)
			{
				SetProvider(factory, new BehaviorStrategyProvider());
			}
			return host.provider;
		}

		public static void SetProvider(BusinessObjectFactory factory, BehaviorStrategyProvider provider)
		{
			var host = factory.GetCachedValue<BehaviorProviderHost>();
			host.provider = provider;
		}

		class BehaviorProviderHost
		{
			public BehaviorStrategyProvider provider;
		}

		public BehaviorStrategyProvider() { }

		public virtual CommonContainerBehaviorStrategy ContainerBehaviorStrategy { get { return new CommonContainerBehaviorStrategy(); } }
	}
}
