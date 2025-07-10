using System;
using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public static class FactoryExtensions
	{
		public static TContract GetValue<TContract>(this BusinessObjectFactory factory) where TContract : class
		{
			Argument.NotNull(factory, "factory");

			LazyValueProvider<TContract> lazy = factory.GetCachedValue<LazyValueProvider<TContract>>();
			return lazy.Value;
		}

		public static void SetValue<TContract, UImplementation>(this BusinessObjectFactory factory)
			where TContract : class
			where UImplementation : class, TContract, new()
		{
			SetValue<TContract>(factory, () => new UImplementation());
		}

		public static void SetValue<TContract>(this BusinessObjectFactory factory, Func<TContract> provider) where TContract : class
		{
			Argument.NotNull(factory, "factory");

			LazyValueProvider<TContract> lazy = factory.GetCachedValue<LazyValueProvider<TContract>>();
			lazy.ValueProvider = provider;
		}

		public static void RemoveValue<TContract>(this BusinessObjectFactory factory) where TContract : class
		{
			LazyValueProvider<TContract> lazy = factory.GetCachedValue<LazyValueProvider<TContract>>();
			lazy.ValueProvider = () => null;
		}

		public static T ImportFromAnotherFactorySafe<T>(this BusinessObjectFactory factory, T businessObject) where T : BusinessObject
		{
			if (businessObject != null)
			{
				if (businessObject.Factory == factory)
				{
					return businessObject;
				}

				ZQuery query = new ZQuery(businessObject.PKSchemaColumn, businessObject.PK);
				query.FetchOnlyFromLocalCache = true;

				return (T)factory.LoadTop1(businessObject.GetType(), query) ?? (T)factory.ImportFromAnotherFactory(businessObject);
			}

			return null;
		}

		public static T GetCached<T>(this BusinessObjectFactory factory, ref CachedProperty<T> cachedProperty, GetValueDelegate<T> getValueDelegate)
		{
			if (cachedProperty == null)
			{
				cachedProperty = new CachedProperty<T>(factory, getValueDelegate);
			}

			return cachedProperty.Value;
		}
	}
}
