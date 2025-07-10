using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.TransportConsignment.Business
{
	interface IDtbConsignmentAddressEnsureUniqueSequencesWithinConsignmentDeferTriggerStrategy
	{
	}

	class DtbConsignmentAddressEnsureUniqueSequencesWithinConsignmentDeferTriggerStrategy : IDeferTriggerConditionStrategy, IDtbConsignmentAddressEnsureUniqueSequencesWithinConsignmentDeferTriggerStrategy
	{
		TriggerRunType IDeferTriggerConditionStrategy.RunType => TriggerRunType.InsertOrUpdate;

		bool IDeferTriggerConditionStrategy.ShouldDeferTrigger(BusinessObject businessEntity)
		{
			if (businessEntity != null)
			{
				if (businessEntity.IsInDatabase)
				{
					return businessEntity.ZPropertyInfoHash.Cast<ZPropertyInfo>().Any(p => p.Name.Equals(DtbConsignmentAddress.Schema.LTS_Sequence) && p.HasChanges);
				}
				else
				{
					return true;
				}
			}

			return false;
		}
	}
}
