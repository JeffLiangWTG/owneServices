namespace CargoWise.RefDbRepo.PLReferenceData.Business.Taric4;

interface IMergeStrategy
{
	object Merge(object baseDataGroup, object updateDataGroup);
}

interface IMergeStrategy<TDataGroup, TDataPoint> : IMergeStrategy
	where TDataPoint : IDataPoint
	where TDataGroup : IDataGroup<TDataPoint>, new()
{
	TDataGroup Merge(TDataGroup baseDataGroup, TDataGroup updateDataGroup);
}
