using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public interface IBasePackagePivotCollection : IBusinessObjectCollection
	{
		ICusPackagePivot GetRelatedPivot(BasePackage package);

		ICusPackagePivot AddPivotFor(BasePackage package);

		void DeletePivotFor(BasePackage package);
		void Reload(bool reLoadExistingRows, bool assumeRowsMissingFromQueryResultsAreDeleted = false);
	}
}
