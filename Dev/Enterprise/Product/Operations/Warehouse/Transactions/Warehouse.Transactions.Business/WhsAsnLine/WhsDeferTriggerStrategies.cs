using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	#region IWhsCheckSerialNumberQuantityMatchWithAsnLine_DeferTriggerStrategy

	interface IWhsCheckSerialNumberQuantityMatchWithAsnLine_DeferTriggerStrategy { }

	class WhsCheckSerialNumberQuantityMatchWithAsnLine_DeferTriggerStrategy : DeferTriggerOnUpdateConditionStrategy, IWhsCheckSerialNumberQuantityMatchWithAsnLine_DeferTriggerStrategy
	{
		WhsCheckSerialNumberQuantityMatchWithAsnLine_DeferTriggerStrategy()
		{
		}

		protected override IEnumerable<SchemaColumn> ColumnsThatRequireTriggerDeferralWhenChanged
			=> new SchemaColumn[] { WhsAsnLineSchema.WN_Quantity };

		protected override bool ShouldDeferTrigger(BusinessObject businessEntity)
		{
			var asnLine = (WhsAsnLine)businessEntity;
			return base.ShouldDeferTrigger(asnLine) &&
				WarehouseDataRegistry.Instance.EnableSchemaRedesignChanges.Value &&
				WhsProduct.GetWhsProduct(asnLine.Factory, asnLine.WN_OP).IsSerialNumberUsed(GetClient(asnLine));
		}

		static OrgHeader GetClient(WhsAsnLine asnLine)
		{
			return asnLine.Factory.GetCachedValue("WhsDocket_Client|" + asnLine.WN_WD, () => asnLine.Docket.Client, CacheStalenessPolicy.StaleOnFactorySave);
		}
	}

	#endregion
}
