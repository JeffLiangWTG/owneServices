using System;
using System.Collections.Generic;
using System.Linq;

namespace CargoWise.Blazor.Common
{
	public static class IEnumerableExtensions
	{
		/// <summary>
		/// Function to flatten list of objects which have an enumerable child property of the same type
		/// For example class A { public IEnumerable<A></A> Children {get;} }, works to any depth and is iterative rather than recursive
		/// </summary>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="enumerable">The enumerable that you want to flatten</param>
		/// <param name="propertyToFlattenSelector">Function to select the enumerable child property of the same type that you want to flatten</param>
		/// <returns>Flattened list of items, note that duplicted children will appear multiple times in the returned list. It's up to the caller to filter if required</returns>
		public static IEnumerable<TValue> Flatten<TValue>(this IEnumerable<TValue> enumerable, Func<TValue, IEnumerable<TValue>> propertyToFlattenSelector)
		{
			var flattenedItems = new List<TValue>();

			var stack = new Stack<TValue>(enumerable);

			while (stack.Count > 0)
			{
				var item = stack.Pop();

				var property = propertyToFlattenSelector(item);

				if (property != null)
				{
					foreach (var subMenu in propertyToFlattenSelector(item))
					{
						stack.Push(subMenu);
					}
				}

				flattenedItems.Add(item);
			}

			return flattenedItems;
		}

		/// <summary>
		/// Projects to a sequence of tuples combining the index with the original element/item
		/// </summary>
		public static IEnumerable<(TValue Element, int Index)> WithIndex<TValue>(this IEnumerable<TValue> enumerable) => enumerable.Select((element, index) => (element, index));
	}
}
