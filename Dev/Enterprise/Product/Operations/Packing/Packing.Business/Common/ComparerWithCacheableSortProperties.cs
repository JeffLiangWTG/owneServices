using System;
using System.Collections.Generic;
using CargoWise.Common;

namespace Enterprise.Packing.Business
{
	public abstract class ComparerWithCacheableSortProperties<TObject> : IComparer<TObject>
	{
		protected ComparerWithCacheableSortProperties()
		{
		}

		public int Compare(TObject x, TObject y) => CompareCore(x, y);

		protected abstract int CompareCore(TObject x, TObject y);

		protected IDisposable CacheSortProperties()
		{
			return new DisposableAction(
				() => SortPropertiesCache = new Dictionary<(TObject, string), object>(),
				() => SortPropertiesCache = null);
		}

		protected int Compare<TValue>(TObject x, TObject y, string functionKey, Func<TObject, TValue> getPropertyValue, Func<TValue, TValue, int> compare)
		{
			var propertyX = GetCachedProperty(x, functionKey, getPropertyValue);
			var propertyY = GetCachedProperty(y, functionKey, getPropertyValue);
			return compare(propertyX, propertyY);
		}

		TValue GetCachedProperty<TValue>(TObject item, string functionKey, Func<TObject, TValue> getPropertyValue)
		{
			TValue result;

			if (SortPropertiesCache != null)
			{
				var key = (item, functionKey);

				if (!SortPropertiesCache.TryGetValue(key, out var value))
				{
					SortPropertiesCache[key] = result = getPropertyValue(item);
				}
				else
				{
					result = (TValue)value;
				}
			}
			else
			{
				result = getPropertyValue(item);
			}

			return result;
		}

		Dictionary<(TObject, string), object> SortPropertiesCache;
	}
}
