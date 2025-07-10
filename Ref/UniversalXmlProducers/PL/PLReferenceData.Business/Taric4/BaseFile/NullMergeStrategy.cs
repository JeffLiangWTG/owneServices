namespace CargoWise.RefDbRepo.PLReferenceData.Business.Taric4;

sealed class NullMergeStrategy : IMergeStrategy
{
	public object Merge(object baseDataGroup, object updateDataGroup) => null;
}
