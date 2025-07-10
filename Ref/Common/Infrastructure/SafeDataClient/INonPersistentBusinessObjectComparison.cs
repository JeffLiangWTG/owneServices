namespace CargoWise.RefDbRepo.Common.SafeDataClient
{
	public interface INonPersistentBusinessObjectComparison
	{
		bool IsIdentical(INonPersistentBusinessObject nonPersistentObject, object persistentObject);
	}
}
