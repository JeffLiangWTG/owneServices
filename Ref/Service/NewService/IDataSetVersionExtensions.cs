using System;
using System.Linq;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.NewService
{
	public static class IDataSetVersionExtensions
	{
		public static IQueryable<RefDbVersionControl> Filter(this IQueryable<RefDbVersionControl> source, DateTime? lowerTimestamp, DateTime upperTimestamp, ICheckpoint checkpoint)
		{
			Argument.NotNull(source, nameof(source));

			var result = source.Where(x => (!lowerTimestamp.HasValue || lowerTimestamp.Value < x.RVC_LastUpdatedUTC) && x.RVC_LastUpdatedUTC <= upperTimestamp);
			if (checkpoint != null)
			{
				result = checkpoint.Filter(result);
			}
			return result;
		}

		public static IQueryable<T> FilterView<T>(this IQueryable<T> source, DateTime? lowerTimestamp, DateTime upperTimestamp, ICheckpoint checkpoint, short dataSetId) where T : class, IDataSetView
		{
			Argument.NotNull(source, nameof(source));

			var result = source.Where(x => (!lowerTimestamp.HasValue || lowerTimestamp.Value < x.RVC_LastUpdatedUTC) && x.RVC_LastUpdatedUTC <= upperTimestamp && x.RVC_DataSetId == dataSetId);
			if (checkpoint != null)
			{
				result = checkpoint.FilterView(result);
			}
			return result;
		}

		public static DateTime GetLastUpdatedUTC(this RefDataSetInformation dataSetInformation)
		{
			Argument.NotNull(dataSetInformation, nameof(dataSetInformation));
			return dataSetInformation.RDS_LastUpdatedUTC ?? DateTime.MinValue;
		}

		public static DateTime GetMaxLastUpdatedUTC(this IQueryable<RefDataSetInformation> source)
		{
			Argument.NotNull(source, nameof(source));
			return source.Max(x => x.RDS_LastUpdatedUTC) ?? DateTime.MinValue;
		}
	}
}
