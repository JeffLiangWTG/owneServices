using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Transit.Business
{
	public interface IWhsItemPackageStateOverpackHUAndChildPackagesHaveSameRCNandDCNDeferTriggerStrategy
	{
	}

	class WhsItemPackageStateOverpackHUAndChildPackagesHaveSameRCNandDCNDeferTriggerStrategy : IDeferTriggerConditionStrategy, IWhsItemPackageStateOverpackHUAndChildPackagesHaveSameRCNandDCNDeferTriggerStrategy
	{
		WhsItemPackageStateOverpackHUAndChildPackagesHaveSameRCNandDCNDeferTriggerStrategy()
		{
		}

		public TriggerRunType RunType => TriggerRunType.InsertOrUpdate;

		public bool ShouldDeferTrigger(BusinessObject businessEntity) => true;
	}
}
