using System;
using CargoWise.EntityFramework;

namespace Enterprise.Freight.Common.Business
{
	public static class SailingManagerQueryProviderFactory
	{
		public static ISailingManagerQueryProvider Get(BusinessObjectFactory factory)
		{
			if (factory == null)
			{
				throw new ArgumentNullException(nameof(factory));
			}

			ISailingManagerQueryProvider queryProvider = factory.GetCachedValue<Holder>().holder;
			if (queryProvider == null || factory.IsInTransaction)
			{
				queryProvider = new DefaultSailingManagerQueryProvider();
			}

			return queryProvider;
		}

		public static void Set(BusinessObjectFactory factory, ISailingManagerQueryProvider provider)
		{
			if (factory == null)
			{
				throw new ArgumentNullException(nameof(factory));
			}

			factory.GetCachedValue<Holder>().holder = provider;
		}

		class Holder
		{
			public ISailingManagerQueryProvider holder;
		}
	}
}
