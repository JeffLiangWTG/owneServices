using CargoWise.EntityFramework;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.LocalCartage.Business
{
	public class CommonCartageBehaviorStrategyProvider : BehaviorStrategyProvider
	{
		public CommonCartageBehaviorStrategyProvider() { }

		public static CommonCartageBehaviorStrategyProvider GetCartageProvider(BusinessObjectFactory factory)
		{
			var host = factory.GetCachedValue<CommonCartageBehaviorStrategyHost>();
			if (host.provider == null)
			{
				SetProvider(factory, new CommonCartageBehaviorStrategyProvider());
			}

			return host.provider;
		}

		public static void SetProvider(BusinessObjectFactory factory, CommonCartageBehaviorStrategyProvider provider)
		{
			var host = factory.GetCachedValue<CommonCartageBehaviorStrategyHost>();
			host.provider = provider;

			BehaviorStrategyProvider.SetProvider(factory, provider);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Scope = "type")]
		class CommonCartageBehaviorStrategyHost
		{
			public CommonCartageBehaviorStrategyProvider provider;
		}

		public virtual CommonBookedCtgMoveBehaviorStrategy BookedCtgMoveBehaviorStrategy
		{
			get { return new CommonBookedCtgMoveBehaviorStrategy(); }
		}

		public virtual CommonCartageBehaviorStrategy CartageBehaviorStrategy
		{
			get { return new StandaloneCartageBehaviorStrategy(); }
		}

		public virtual CommonCartageBehaviorStrategy InternalCartageBehaviorStrategy
		{
			get { return new InternalCartageBehaviorStrategy(); }
		}

		public virtual CommonCartageLegBehaviorStrategy CartageLegBehaviorStrategy
		{
			get { return new CommonCartageLegBehaviorStrategy(); }
		}

		public virtual CommonWorkSheetBehaviorStrategy WorkSheetBehaviorStrategy
		{
			get { return new CommonWorkSheetBehaviorStrategy(); }
		}

		public override CommonContainerBehaviorStrategy ContainerBehaviorStrategy
		{
			get { return new CartageContainerBehaviorStrategy(); }
		}
	}
}
