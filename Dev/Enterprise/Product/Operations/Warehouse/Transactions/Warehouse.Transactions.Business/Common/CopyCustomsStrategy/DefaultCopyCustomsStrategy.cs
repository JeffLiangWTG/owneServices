namespace Enterprise.Warehouse.Transactions.Business
{
	class DefaultCopyCustomsStrategy : ICopyCustomsStrategy
	{
		public void CopyCustomsData(WhsDocketLine transactionLine, WhsDocketLine inventoryLine)
		{
			var customsData = transactionLine.CustomsData;
			using (customsData.SuspendUpdatingDocketLineEntryKey())
			{
				var inventoryData = inventoryLine.CustomsData;
				customsData.WB_EntryKey = inventoryData.WB_EntryKey;
				customsData.WB_EntryLineNo = inventoryData.WB_EntryLineNo;
			}
		}
	}
}
