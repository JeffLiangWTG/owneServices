using System.Linq;

namespace CargoWise.RefDbRepo.PLReferenceData.Business.Taric4;

sealed class MergeStrategy<TDataGroup, TDataPoint>(originType originType) : IMergeStrategy<TDataGroup, TDataPoint>
	where TDataPoint : IDataPoint
	where TDataGroup : IDataGroup<TDataPoint>, new()
{
	public object Merge(object baseDataGroup, object updateDataGroup) => Merge((TDataGroup) baseDataGroup, (TDataGroup) updateDataGroup);

	public TDataGroup Merge(TDataGroup baseDataGroup, TDataGroup updateDataGroup)
	{
		baseDataGroup.DataPoints ??= [];
		updateDataGroup.DataPoints ??= [];
		var mergedDataPoints = baseDataGroup.DataPoints
			.Concat(updateDataGroup.DataPoints)
			.OrderBy(dataPoint => dataPoint.metainfo.transactionDate)
			.Where(dataPoint => dataPoint.metainfo.origin == originType)
			.GroupBy(dataPoint => dataPoint.hjid)
			.Select(grouping => grouping.Last())
			.Where(dataPoint => dataPoint.metainfo.opType != OpType.D);
		return new() { DataPoints = mergedDataPoints.ToArray() };
	}
}
