using System;
using System.Collections.Generic;
using System.Linq;
using FlexCel.XlsAdapter;

namespace CargoWise.RefDbRepo.TRReferenceData.Services
{
	public static class Extensions
	{
		public static string GetTrimmedStringFromCell(this XlsFile xls, int row, int col)
		{
			return xls.GetStringFromCell(row, col).Trim();
		}

		public static DateTime TruncateToMinute(this DateTime dateTime)
		{
			return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, 00);
		}

		public static TValue GetOrAddNew<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key) where TValue : new()
		{
			if (dictionary.ContainsKey(key))
			{
				return dictionary[key];
			}
			else
			{
				var newValue = new TValue();
				dictionary.Add(key, newValue);
				return newValue;
			}
		}

		public static TValue GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
			=> dictionary.TryGetValue(key, out var value) ? value : default;


		public static IEnumerable<TResult> WhereNotNull<TResult>(this IEnumerable<TResult> enumerable) => enumerable.Where(item => item != null);

		public static T[][] ToArray<T>(this IEnumerable<IEnumerable<T>> lists) => lists.Select(list => list.ToArray()).ToArray<T[]>();
	}
}
