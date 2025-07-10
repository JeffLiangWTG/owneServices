using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business.ConcurrencyResolver
{
	public interface IConcurrencyDeleteAndUpdateResolver<T>
		where T : BusinessObject
	{
		ConcurrencyResolutionResult<T> DeleteDuplicateAndUpdateOriginalWhenRequired(T original, T duplicate);
	}
}
