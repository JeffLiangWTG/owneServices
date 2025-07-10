using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace System.Linq
{
	public static class LinqHelpers
	{
		public static bool Any<T>(this BusinessObjectCollection<T> self, Func<T, bool> predicate) where T : BusinessObject => self.Cast<T>().Any(predicate);
		public static bool ContainsAnyEndingWith<T>(this IEnumerable<T> self, Func<T, ZString> map, params string[] match) => self.Select(map).ContainsAnyEndingWith(match);
		public static bool ContainsAnyEndingWith(this IEnumerable<ZString> self, params string[] match) => self.Any(value => match.Any(value.EndsWith));
		public static T SingleOrDefault<T>(this BusinessObjectCollection<T> self, Func<T, bool> predicate) where T : BusinessObject => self.Cast<T>().SingleOrDefault(predicate);

		public static IEnumerable<TResult> SelectMany<TEntity, TResult>(this BusinessObjectCollection<TEntity> self, Func<TEntity, IEnumerable<TResult>> selector)
			where TEntity : BusinessObject => self.Cast<TEntity>().SelectMany(selector);

		public static ZDecimal Sum<TEntity>(this BusinessObjectCollection<TEntity> self, Func<TEntity, decimal> selector)
			where TEntity : BusinessObject => self.Cast<TEntity>().Sum(selector);

		public static ZInt ForEachAndCount<T>(this IEnumerable<BusinessObject> self, Action<T> action)
			where T : BusinessObject => self.Select(x => x as T).ForEachAndCount(action);
		public static ZInt ForEachAndCount<T>(this IEnumerable<T> self, Action<T> action)
			where T : BusinessObject => self.Where(x => x is not null).Count(x => { action(x); return true; });

		public static T FirstOrDefault<T>(this BusinessObjectCollection<T> self)
			where T : BusinessObject => self.Cast<T>().FirstOrDefault();

		public static T FirstOrDefault<T>(this BusinessObjectCollection<T> self, Func<T, bool> predicate)
			where T : BusinessObject => self.Cast<T>().FirstOrDefault(predicate);
	}
}
