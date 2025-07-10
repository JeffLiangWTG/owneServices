using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Linq.Expressions;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.Models;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.NewService
{
	public static class DataServiceExtensions
	{
		public static void WriteCheckpoint(this RefDataSet dataSet, Guid dataSetPK, ref int index, int? chunkSize)
		{
			Argument.NotNull(dataSet, nameof(dataSet));

			if (chunkSize.HasValue)
			{
				index++;
				if (index == chunkSize.Value)
				{
					index = 0;
					dataSet.Checkpoint = CheckpointHelper.Create(dataSetPK).ToString();
				}
			}
		}

		public static IEnumerable<T> DistinctByKey<T>(this IEnumerable<T> source, Func<T, Guid> getKey)
		{
			Argument.NotNull(source, nameof(source));
			Argument.NotNull(getKey, nameof(getKey));
			return source.GroupBy(getKey).Select(x => x.FirstOrDefault());
		}

		public static IEnumerable<T> NotNull<T>(this IEnumerable<T> source)
		{
			Argument.NotNull(source, nameof(source));
			return source.Where(x => x != null);
		}

		public static TTarget[] MapCollection<TTarget, TSource>(this AutoMapper.IMapper mapper, IEnumerable<TSource> sources)
		{
			return sources?.Select(x => mapper.Map<TTarget>(x)).ToArray();
		}

		public static T[] GetChunk<T>(this IQueryable<T> source, DateTime? lowerTimestamp, DateTime upperTimestamp, ICheckpoint checkpointPK, short dataSetId, Guid? startPK, Guid? endPK) where T : class, IDataSetView
		{
			Argument.NotNull(source, nameof(source));

			var result = source.FilterView(lowerTimestamp, upperTimestamp, checkpointPK, dataSetId);
			if (startPK.HasValue)
			{
				result = result.Where(x => x.RVC_ParentPK.CompareTo(startPK.Value) > 0);
			}
			if (endPK.HasValue)
			{
				result = result.Where(x => x.RVC_ParentPK.CompareTo(endPK.Value) <= 0);
			}
			return result.OrderBy(x => x.RVC_ParentPK).ToArray();
		}

		public static T[] GetChunk<T>(this IQueryable<T> source, IQueryable<RefDbVersionControl> versions, Expression<Func<T, Guid>> getDataSetPK,
			DateTime? lowerTimestamp, DateTime upperTimestamp, ICheckpoint checkpointPK) where T : class
		{
			Argument.NotNull(source, nameof(source));
			Argument.NotNull(versions, nameof(versions));
			Argument.NotNull(getDataSetPK, nameof(getDataSetPK));

			return GetChunk(source, versions, getDataSetPK, lowerTimestamp, upperTimestamp, checkpointPK, null, null);
		}

		public static T[] GetChunk<T>(this IQueryable<T> source, IQueryable<RefDbVersionControl> versions, Expression<Func<T, Guid>> getDataSetPK,
			DateTime? lowerTimestamp, DateTime upperTimestamp, ICheckpoint checkpointPK, Guid? startPK, Guid? endPK) where T : class
		{
			Argument.NotNull(source, nameof(source));
			Argument.NotNull(versions, nameof(versions));
			Argument.NotNull(getDataSetPK, nameof(getDataSetPK));

			var filteredVersions = versions.Filter(lowerTimestamp, upperTimestamp, checkpointPK);
			if (startPK.HasValue)
			{
				filteredVersions = filteredVersions.Where(x => x.RVC_ParentPK.CompareTo(startPK.Value) > 0);
			}
			if (endPK.HasValue)
			{
				filteredVersions = filteredVersions.Where(x => x.RVC_ParentPK.CompareTo(endPK.Value) <= 0);
			}
			return source.Join(
				filteredVersions,
				getDataSetPK,
				v => v.RVC_ParentPK,
				(t, v) => t
			).OrderBy(getDataSetPK)
			.ToArray();
		}

		public static IEnumerable<T> GetSetData<T>(this T[] dataChunk, Guid dataSetPK, ref int idx) where T : class, IDataSetView
		{
			Argument.NotNull(dataChunk, nameof(dataChunk));
			Argument.GreaterThanOrEqual(idx, 0, nameof(idx));

			return GetSetData(dataChunk, x => x.RVC_ParentPK, dataSetPK, ref idx);
		}

		public static IEnumerable<T> GetSetData<T>(this T[] dataChunk, Func<T, Guid> getDataSetPK, Guid dataSetPK, ref int idx, bool isDbProvider = true) where T : class
		{
			Argument.NotNull(dataChunk, nameof(dataChunk));
			Argument.NotNull(getDataSetPK, nameof(getDataSetPK));
			Argument.GreaterThanOrEqual(idx, 0, nameof(idx));

			var result = new List<T>();

			while (idx < dataChunk.Length && SqlCompareGuid(getDataSetPK(dataChunk[idx]), dataSetPK, isDbProvider) <= 0)
			{
				if (SqlCompareGuid(getDataSetPK(dataChunk[idx]), dataSetPK, isDbProvider) == 0)
				{
					result.Add(dataChunk[idx]);
				}
				idx++;
			}
			return result;
		}

		static int SqlCompareGuid(Guid guid1, Guid guid2, bool isDbProvider)
		{
			if (isDbProvider)
			{
				return ((SqlGuid)guid1).CompareTo((SqlGuid)guid2);
			}
			else
			{
				return guid1.CompareTo(guid2);
			}
		}
	}
}
