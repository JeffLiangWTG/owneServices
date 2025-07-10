using CargoWise.Common;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal
{
	class WhsDynamicWorkOrderBondedWarehouseAttributeDataObjectWriter : WhsBondedWarehouseAttributeDataObjectWriter
	{
		internal WhsDynamicWorkOrderBondedWarehouseAttributeDataObjectWriter(IDataWritingManager manager, WhsDynamicWorkOrderLine workOrderLine)
			: base(manager)
		{
			WorkOrderLine = Argument.NotNull(workOrderLine, nameof(workOrderLine));
		}

		WhsDynamicWorkOrderLine WorkOrderLine { get; }

		protected override CustomsEntryInfo PopulateDataObject(WhsBondedWarehouseAttribute customsDataBO)
		{
			var customsData = new CustomsEntryInfo();
			if (WorkOrderLine.WE_WE_ParentDocketLine.IsEmpty)
			{
				customsData.IsMainInwardsProcessedItem = customsDataBO.WB_IsMainInwardsProcessedItem;
				customsData.IsSecondaryInwardsProcessedItem = customsDataBO.WB_IsSecondaryInwardsProcessedItem;
			}
			else
			{
				var entryNumber = WhsBondedWarehouseAttribute.BreakUpKey(WorkOrderLine.WE_BondedEntryKey);
				customsData.InwardsEntryKey = entryNumber.EntryKey;
				customsData.InwardsEntryLineNumber = entryNumber.EntryLineNo;
			}

			return customsData;
		}
	}
}
