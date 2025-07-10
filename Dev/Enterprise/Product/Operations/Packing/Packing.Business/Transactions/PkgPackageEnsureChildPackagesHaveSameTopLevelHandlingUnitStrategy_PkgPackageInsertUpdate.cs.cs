using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Packing.Business
{
	interface IPkgPackageEnsureChildPackagesHaveSameTopLevelHandlingUnitStrategy_PkgPackageInsertUpdate { }

	class PkgPackageEnsureChildPackagesHaveSameTopLevelHandlingUnitStrategy_PkgPackageInsertUpdate : IDeferTriggerConditionStrategy, IPkgPackageEnsureChildPackagesHaveSameTopLevelHandlingUnitStrategy_PkgPackageInsertUpdate
	{
		PkgPackageEnsureChildPackagesHaveSameTopLevelHandlingUnitStrategy_PkgPackageInsertUpdate()
		{
		}

		TriggerRunType IDeferTriggerConditionStrategy.RunType => TriggerRunType.InsertOrUpdate;

		bool IDeferTriggerConditionStrategy.ShouldDeferTrigger(BusinessObject businessEntity)
		{
			return businessEntity != null && (!businessEntity.IsInDatabase || businessEntity.ZPropertyInfoHash[PkgPackageSchema.Constants.KP_KP_TopHandlingUnitPackage].HasChanges);
		}
	}
}
