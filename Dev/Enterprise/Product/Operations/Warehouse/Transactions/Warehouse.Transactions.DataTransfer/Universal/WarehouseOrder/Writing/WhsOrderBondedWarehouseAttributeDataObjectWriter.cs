using CargoWise.Common;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal
{
	class WhsOrderBondedWarehouseAttributeDataObjectWriter : WhsBondedWarehouseAttributeDataObjectWriter
	{
		internal WhsOrderBondedWarehouseAttributeDataObjectWriter(IDataWritingManager manager, WhsOrderLine orderLine)
			: base(manager)
		{
			OrderLine = Argument.NotNull(orderLine, nameof(orderLine));
		}

		WhsOrderLine OrderLine { get; }

		protected override CustomsEntryInfo PopulateDataObject(WhsBondedWarehouseAttribute customsDataBO)
		{
			var customsData = base.PopulateDataObject(customsDataBO);
			var entryNumber = WhsBondedWarehouseAttribute.BreakUpKey(OrderLine.WE_BondedEntryKey);
			customsData.InwardsEntryKey = entryNumber.EntryKey;
			customsData.InwardsEntryLineNumber = entryNumber.EntryLineNo;

			return customsData;
		}
	}
}
