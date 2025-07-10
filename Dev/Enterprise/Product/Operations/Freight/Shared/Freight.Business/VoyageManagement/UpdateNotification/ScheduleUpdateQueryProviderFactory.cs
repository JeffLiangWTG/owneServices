using System;
using CargoWise.EntityFramework;

namespace Enterprise.Freight.Business
{
	public static class ScheduleUpdateQueryProviderFactory
	{
		public static IScheduleUpdateQueryProvider Get(BusinessObjectFactory factory)
		{
			if (factory == null)
			{
				throw new ArgumentNullException(nameof(factory));
			}

			Host host = factory.GetCachedValue<Host>();
			GetValueDelegate<IScheduleUpdateQueryProvider> getProviderDelegate = host.getProviderDelegate;

			if (getProviderDelegate == null || factory.IsInTransaction)
			{
				return new ScheduleUpdateNullQueryProvider();
			}
			else
			{
				return getProviderDelegate();
			}
		}

		public static void Set(BusinessObjectFactory factory, GetValueDelegate<IScheduleUpdateQueryProvider> getProviderDelegate)
		{
			if (factory == null)
			{
				throw new ArgumentNullException(nameof(factory));
			}

			Host host = factory.GetCachedValue<Host>();
			host.getProviderDelegate = getProviderDelegate;
		}

		class Host
		{
			public GetValueDelegate<IScheduleUpdateQueryProvider> getProviderDelegate;
		}
	}
}
