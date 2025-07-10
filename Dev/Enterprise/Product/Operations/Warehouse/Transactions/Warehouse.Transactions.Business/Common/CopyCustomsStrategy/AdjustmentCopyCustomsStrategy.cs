namespace Enterprise.Warehouse.Transactions.Business
{
	class AdjustmentCopyCustomsStrategy : ICopyCustomsStrategy
	{
		public void CopyCustomsData(WhsDocketLine transactionLine, WhsDocketLine inventoryLine)
		{
			var customsData = transactionLine.CustomsData;
			using (customsData.SuspendUpdatingDocketLineEntryKey())
			{
				var inventoryCustomsData = inventoryLine.CustomsData;
				customsData.CopyPersistantValuesFromAnotherAttribute(inventoryCustomsData);
				customsData.WB_BondedWhsQty = inventoryCustomsData.WB_BondedWhsQty;
				customsData.WB_IsMainInwardsProcessedItem = false;
				customsData.WB_IsSecondaryInwardsProcessedItem = false;
			}
		}
	}
}
