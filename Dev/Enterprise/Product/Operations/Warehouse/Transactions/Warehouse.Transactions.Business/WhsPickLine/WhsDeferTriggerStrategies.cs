using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	#region WhsCheckStockOnHandIsBalancedStrategy_PickLineInsertUpdate class

	interface IWhsCheckStockOnHandIsBalancedStrategy_PickLineInsertUpdate { }

	class WhsCheckStockOnHandIsBalancedStrategy_PickLineInsertUpdate : DeferTriggerOnUpdateConditionStrategy, IWhsCheckStockOnHandIsBalancedStrategy_PickLineInsertUpdate
	{
		WhsCheckStockOnHandIsBalancedStrategy_PickLineInsertUpdate()
		{
		}

		protected override IEnumerable<SchemaColumn> ColumnsThatRequireTriggerDeferralWhenChanged
		{
			get
			{
				return new SchemaColumn[]
					{
						WhsPickLineSchema.WZ_WE_InventoryLine,
						WhsPickLineSchema.WZ_Units,
						WhsPickLineSchema.WZ_PickedDateTime
					};
			}
		}

		protected override bool ShouldDeferTrigger(BusinessObject businessEntity)
		{
			var pickLine = businessEntity as WhsPickLine;
			return base.ShouldDeferTrigger(businessEntity)
				&& (pickLine.WZ_Units > 0 || pickLine.WZ_UnitsInfo.HasChanges)
				&& (!pickLine.WZ_PickedDateTime.IsEmpty || pickLine.WZ_PickedDateTimeInfo.HasChanges);
		}
	}

	#endregion

	#region WhsCheckOrderDockDoorLocationIsMatchWithPick_DeferTriggerStrategy

	interface IWhsCheckOrderDockDoorLocationIsMatchWithPick_DeferTriggerStrategy { }

	class WhsCheckOrderDockDoorLocationIsMatchWithPick_DeferTriggerStrategy : DeferTriggerOnUpdateConditionStrategy, IWhsCheckOrderDockDoorLocationIsMatchWithPick_DeferTriggerStrategy
	{
		WhsCheckOrderDockDoorLocationIsMatchWithPick_DeferTriggerStrategy() { }

		protected override IEnumerable<SchemaColumn> ColumnsThatRequireTriggerDeferralWhenChanged => new[] { WhsDocketSchema.WD_WP };
	}

	#endregion

	#region WhsPreventChangeCriticalFieldsOnPickedOrder_OrderLineUpdate

	interface IWhsCustomCanChangeCriticalFieldsOnPickedOrderWhenImportingOrderLine { }

	class WhsCustomCanChangeCriticalFieldsOnPickedOrderWhenImportingOrderLine : DeferTriggerOnUpdateConditionStrategy, IWhsCustomCanChangeCriticalFieldsOnPickedOrderWhenImportingOrderLine
	{
		WhsCustomCanChangeCriticalFieldsOnPickedOrderWhenImportingOrderLine()
		{
		}

		protected override IEnumerable<SchemaColumn> ColumnsThatRequireTriggerDeferralWhenChanged
			=> new SchemaColumn[]
				{
					WhsDocketLineSchema.WE_OP,
					WhsDocketLineSchema.WE_ExpiryDate,
					WhsDocketLineSchema.WE_PackingDate,
					WhsDocketLineSchema.WE_PartAttrib1,
					WhsDocketLineSchema.WE_PartAttrib2,
					WhsDocketLineSchema.WE_PartAttrib3,
					WhsDocketLineSchema.WE_SerialNumber
				};

		protected override bool ShouldDeferTrigger(BusinessObject businessEntity)
		{
			var orderLine = (WhsOrderLine)businessEntity;
			var result = base.ShouldDeferTrigger(businessEntity) && IsSavingImportingData(orderLine) && orderLine.IsCustomsTransaction;
			if (result)
			{
				var order = orderLine.Order;
				result = order.WD_WP.IsValid && !order.Warehouse.WW_IsVirtualWarehouse; // Don't need to defer trigger for Virtual Warehouse, it just delete and create a new order lines (trigger runs only for update lines)
			}
			return result;
		}

		static bool IsSavingImportingData(WhsOrderLine orderLine) => orderLine.Factory is IUniversalBusinessObjectFactory;
	}

	#endregion

	#region WhsCheckPreventFinalizeIfLinesAreUnfinalized_DeferTriggerStrategy

	interface IWhsCheckPreventFinalizeIfLinesAreUnfinalized_DeferTriggerStrategy { }

	class WhsCheckPreventFinalizeIfLinesAreUnfinalized_DeferTriggerStrategy : DeferTriggerOnUpdateConditionStrategy, IWhsCheckPreventFinalizeIfLinesAreUnfinalized_DeferTriggerStrategy
	{
		WhsCheckPreventFinalizeIfLinesAreUnfinalized_DeferTriggerStrategy() { }

		protected override IEnumerable<SchemaColumn> ColumnsThatRequireTriggerDeferralWhenChanged => new[] { WhsPutawayJobSchema.WPJ_FinalizedTimeUtc };
	}

	#endregion

	#region WhsCheckDocketStatusDepartedOnPickFinalisation_DeferTriggerStrategy

	interface IWhsCheckDocketStatusDepartedOnPickFinalisation_DeferTriggerStrategy { }

	class WhsCheckDocketStatusDepartedOnPickFinalisation_DeferTriggerStrategy : DeferTriggerOnUpdateConditionStrategy, IWhsCheckDocketStatusDepartedOnPickFinalisation_DeferTriggerStrategy
	{
		WhsCheckDocketStatusDepartedOnPickFinalisation_DeferTriggerStrategy() { }

		protected override IEnumerable<SchemaColumn> ColumnsThatRequireTriggerDeferralWhenChanged => new[] { WhsPickSchema.WP_PickStatus };
	}

	#endregion

	#region IWhsCheckTotalUnitsStrategy_BOMKitReceiveLineUpdate

	interface IWhsCheckTotalUnitsStrategy_BOMKitReceiveLineUpdate { }

	class WhsCheckTotalUnitsStrategy_BOMKitReceiveLineUpdate : DeferTriggerOnUpdateConditionStrategy, IWhsCheckTotalUnitsStrategy_BOMKitReceiveLineUpdate
	{
		WhsCheckTotalUnitsStrategy_BOMKitReceiveLineUpdate()
		{
		}

		protected override IEnumerable<SchemaColumn> ColumnsThatRequireTriggerDeferralWhenChanged => new SchemaColumn[] { WhsDocketLineSchema.WE_DocketLineStatus };

		protected override bool ShouldDeferTrigger(BusinessObject businessEntity)
		{
			var docketLine = (WhsDocketLine)businessEntity;
			return base.ShouldDeferTrigger(docketLine) && docketLine.IsFinalised && docketLine.IsCreatedFromPickByBOM;
		}
	}

	#endregion

	#region IWhsCheckSerialNumberMatchWithDocketLineQuantity_DeferTriggerStrategy

	interface IWhsCheckSerialNumberMatchWithDocketLineQuantity_DeferTriggerStrategy { }

	class WhsCheckSerialNumberMatchWithDocketLineQuantity_DeferTriggerStrategy : DeferTriggerOnUpdateConditionStrategy, IWhsCheckSerialNumberMatchWithDocketLineQuantity_DeferTriggerStrategy
	{
		WhsCheckSerialNumberMatchWithDocketLineQuantity_DeferTriggerStrategy()
		{
		}

		protected override IEnumerable<SchemaColumn> ColumnsThatRequireTriggerDeferralWhenChanged
		{
			get
			{
				return new SchemaColumn[]
				{
					WhsDocketLineSchema.WE_TransactionQuantity,
					WhsDocketLineSchema.WE_DocketLineStatus
				};
			}
		}

		protected override bool ShouldDeferTrigger(BusinessObject businessEntity)
		{
			var docketLine = (WhsDocketLine)businessEntity;
			return base.ShouldDeferTrigger(docketLine) &&
				docketLine.IsFinalised &&
				WarehouseDataRegistry.Instance.EnableSchemaRedesignChanges.Value &&
				docketLine.Product.IsSerialNumberUsed(GetClient(docketLine));
		}

		static OrgHeader GetClient(WhsDocketLine docketLine)
		{
			return docketLine.Factory.GetCachedValue("IWhsCheckSerialNumberMatchWithDocketLineQuantity_DeferTriggerStrategy_Client|" + docketLine.WE_WD, () => docketLine.Docket.Client);
		}
	}

	#endregion

	#region IWhsCheckSerialNumberMatchWithPickLineUnits_DeferTriggerStrategy

	interface IWhsCheckSerialNumberMatchWithPickLineUnits_DeferTriggerStrategy { }

	class WhsCheckSerialNumberMatchWithPickLineUnits_DeferTriggerStrategy : DeferTriggerOnUpdateConditionStrategy, IWhsCheckSerialNumberMatchWithPickLineUnits_DeferTriggerStrategy
	{
		WhsCheckSerialNumberMatchWithPickLineUnits_DeferTriggerStrategy()
		{
		}

		protected override IEnumerable<SchemaColumn> ColumnsThatRequireTriggerDeferralWhenChanged
			=> new SchemaColumn[] { WhsPickLineSchema.WZ_PickedDateTime };

		protected override bool ShouldDeferTrigger(BusinessObject businessEntity)
		{
			var pickLine = (WhsPickLine)businessEntity;
			return base.ShouldDeferTrigger(pickLine) &&
				pickLine.IsPicked &&
				WarehouseDataRegistry.Instance.EnableSchemaRedesignChanges.Value;
		}
	}

	#endregion

	#region IWhsCheckTransferSerialNumberMatchWithnventory_DeferTriggerStrategy

	interface IWhsCheckTransferSerialNumberMatchWithnventory_DeferTriggerStrategy { }

	class WhsCheckTransferSerialNumberMatchWithnventory_DeferTriggerStrategy : DeferTriggerOnUpdateConditionStrategy, IWhsCheckTransferSerialNumberMatchWithnventory_DeferTriggerStrategy
	{
		WhsCheckTransferSerialNumberMatchWithnventory_DeferTriggerStrategy()
		{
		}

		protected override IEnumerable<SchemaColumn> ColumnsThatRequireTriggerDeferralWhenChanged 
			=> new SchemaColumn[] { WhsPickLineSchema.WZ_PickedDateTime };

		protected override bool ShouldDeferTrigger(BusinessObject businessEntity)
		{
			var pickLine = (WhsPickLine)businessEntity;
			return base.ShouldDeferTrigger(pickLine) &&
				pickLine.IsPicked &&
				WarehouseDataRegistry.Instance.EnableSchemaRedesignChanges.Value;
		}
	}

	#endregion
}
