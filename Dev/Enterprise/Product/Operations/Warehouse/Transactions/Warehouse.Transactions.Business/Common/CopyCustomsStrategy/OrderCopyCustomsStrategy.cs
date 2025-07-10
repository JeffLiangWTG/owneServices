namespace Enterprise.Warehouse.Transactions.Business
{
	class OrderCopyCustomsStrategy : ICopyCustomsStrategy
	{
		public void CopyCustomsData(WhsDocketLine transactionLine, WhsDocketLine inventoryLine)
		{
			var customsData = transactionLine.CustomsData;
			using (customsData.SuspendUpdatingDocketLineEntryKey())
			{
				var inventoryCustomsData = inventoryLine.CustomsData;
				customsData.CopyPersistantValuesFromAnotherAttribute(inventoryCustomsData);
				customsData.WB_BondedWhsQty = inventoryCustomsData.WB_BondedWhsQty;
				customsData.SetDefaultOutwardTypeIfEmpty();
			}
		}
	}
}
