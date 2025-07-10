using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.TypeProvider;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;

namespace CargoWise.RefDbRepo.NewSafeDataUpdateService
{
	public static class ExpandExtensions
	{
		public static T[] GetChunk<T>(this IQueryable<T> source, List<Guid> dataSetPKs, List<Type> expandTypeList)
			where T : class
		{
			Argument.NotNull(source, nameof(source));
			Argument.NotNull(dataSetPKs, nameof(dataSetPKs));
			Argument.NotNull(expandTypeList, nameof(expandTypeList));

			if (!expandTypeList.Contains(typeof(T)))
			{
				return new T[0];
			}

			var dataSetPKProperty = typeof(T).GetDataSetPKPropertyInfo();
			var containExp = ExpressionHelper.ContainsStructPropertyExpression<T, Guid>(dataSetPKs, dataSetPKProperty);
			var dataSetPKExp = ExpressionHelper.GetStructPropertyExpression<T, Guid>(dataSetPKProperty);
			return source.Where(containExp).OrderBy(dataSetPKExp).ToArray();
		}

		public static IEnumerable<T> GetSetData<T>(this T[] dataChunk, Func<T, Guid> getDataSetPK, Guid dataSetPK, ref int idx)
			where T : class
		{
			Argument.NotNull(dataChunk, nameof(dataChunk));
			Argument.NotNull(getDataSetPK, nameof(getDataSetPK));
			Argument.GreaterThanOrEqual(idx, 0, nameof(idx));

			var result = new List<T>();

			while (idx < dataChunk.Length && SqlCompareGuid(getDataSetPK(dataChunk[idx]), dataSetPK) <= 0)
			{
				if (SqlCompareGuid(getDataSetPK(dataChunk[idx]), dataSetPK) == 0)
				{
					result.Add(dataChunk[idx]);
				}
				idx++;
			}
			return result;
		}

		static int SqlCompareGuid(Guid guid1, Guid guid2)
		{
			return ((SqlGuid)guid1).CompareTo((SqlGuid)guid2);
		}

		public static T[] FilterToArray<T>(this IEnumerable<IGrouping<Guid, T>> source, Guid target)
		{
			return source.Where(x => x.Key == target).SelectMany(x => x).ToArray();
		}

		public static T[] FilterToArray<T>(this IEnumerable<IGrouping<Guid?, T>> source, Guid target)
		{
			return source.Where(x => x.Key == target).SelectMany(x => x).ToArray();
		}
	}
}
