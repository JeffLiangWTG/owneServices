using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Packing.Business
{
	interface IPkgPackageHeaderUniqueIDPerJobStrategy_PkgPackageHeaderInsertUpdate { }

	class PkgPackageHeaderUniqueIDPerJobStrategy_PkgPackageHeaderInsertUpdate : IDeferTriggerConditionStrategy, IPkgPackageHeaderUniqueIDPerJobStrategy_PkgPackageHeaderInsertUpdate
	{
		PkgPackageHeaderUniqueIDPerJobStrategy_PkgPackageHeaderInsertUpdate()
		{
		}

		TriggerRunType IDeferTriggerConditionStrategy.RunType => TriggerRunType.InsertOrUpdate;

		bool IDeferTriggerConditionStrategy.ShouldDeferTrigger(BusinessObject businessEntity)
		{
			if (businessEntity != null)
			{
				if (businessEntity.IsInDatabase)
				{
					return businessEntity.ZPropertyInfoHash.Cast<ZPropertyInfo>().Any(p => p.Name.Equals(PkgPackageHeader.Schema.KPH_PackageID) && p.HasChanges);
				}
				else
				{
					return false;
				}
			}
			return false;
		}
	}
}
