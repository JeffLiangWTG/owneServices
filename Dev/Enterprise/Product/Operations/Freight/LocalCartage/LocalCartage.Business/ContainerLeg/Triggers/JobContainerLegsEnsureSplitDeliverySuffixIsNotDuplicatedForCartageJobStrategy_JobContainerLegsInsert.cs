using CargoWise.EntityFramework;

namespace Enterprise.Freight.LocalCartage.Business
{
	class JobContainerLegsEnsureSplitDeliverySuffixIsNotDuplicatedForCartageJobStrategy_JobContainerLegsInsert : IDeferTriggerConditionStrategy, IJobContainerLegsEnsureSplitDeliverySuffixIsNotDuplicatedForCartageJobStrategy_JobContainerLegsInsert
	{
		JobContainerLegsEnsureSplitDeliverySuffixIsNotDuplicatedForCartageJobStrategy_JobContainerLegsInsert()
		{
		}

		TriggerRunType IDeferTriggerConditionStrategy.RunType => TriggerRunType.InsertOrUpdate;

		bool IDeferTriggerConditionStrategy.ShouldDeferTrigger(BusinessObject businessEntity)
		{
			return (businessEntity != null && (!businessEntity.IsInDatabase));
		}
	}
}
