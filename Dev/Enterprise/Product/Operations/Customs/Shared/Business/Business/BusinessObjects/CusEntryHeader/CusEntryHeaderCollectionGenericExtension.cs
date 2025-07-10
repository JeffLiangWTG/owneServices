using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public static class CusEntryHeaderCollectionGenericExtension
	{
		public static bool Any<TSource>(this CusEntryHeaderCollection<TSource> source, Func<TSource, bool> predicate)
			where TSource : CusEntryHeader
		{
			return source.AsEnumerable<TSource>().Any(predicate);
		}

		public static bool Any<TSource>(this CusEntryHeaderCollection<TSource> source)
			where TSource : CusEntryHeader
		{
			return source.AsEnumerable<TSource>().Any();
		}
		public static bool All<TSource>(this CusEntryHeaderCollection<TSource> source, Func<TSource, bool> predicate)
			where TSource : CusEntryHeader
		{
			return source.AsEnumerable<TSource>().All(predicate);
		}

		public static int Count<TSource>(this CusEntryHeaderCollection<TSource> source, Func<TSource, bool> predicate)
			where TSource : CusEntryHeader
		{
			return source.AsEnumerable<TSource>().Count(predicate);
		}

		public static int Count<TSource>(this CusEntryHeaderCollection<TSource> source)
			where TSource : CusEntryHeader
		{
			return source.AsEnumerable<TSource>().Count();
		}

		public static IEnumerable<string> Select<TSource>(this CusEntryHeaderCollection<TSource> source, Func<TSource, string> selector)
			where TSource : CusEntryHeader
		{
			return source.AsEnumerable<TSource>().Select(selector);
		}

		public static ZDateTime MaxOrDefault<TSource>(this CusEntryHeaderCollection<TSource> source, Func<TSource, ZDateTime> selector)
			where TSource : CusEntryHeader
		{
			return source.AsEnumerable<TSource>().MaxOrDefault(selector);
		}

		public static IList<TSource> ToList<TSource>(this CusEntryHeaderCollection<TSource> source)
			where TSource : CusEntryHeader
		{
			return source.AsEnumerable<TSource>().ToList();
		}

		public static TSource First<TSource>(this CusEntryHeaderCollection<TSource> source, Func<TSource, bool> selector)
			where TSource : CusEntryHeader
		{
			return source.AsEnumerable<TSource>().First(selector);
		}

		public static TSource First<TSource>(this CusEntryHeaderCollection<TSource> source)
			where TSource : CusEntryHeader
		{
			return source.AsEnumerable<TSource>().First();
		}
	}
}
