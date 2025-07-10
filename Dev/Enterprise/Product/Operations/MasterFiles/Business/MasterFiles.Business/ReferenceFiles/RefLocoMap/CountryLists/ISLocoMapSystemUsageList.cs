namespace Enterprise.MasterFiles.Business
{
	public class ISLocoMapSystemUsageList : LocoMapSystemUsageList
	{
		public ISLocoMapSystemUsageList()
		{
			Clear();
			AddPair(LocoMapSystemUsageList.Codes.CustomsOfficeCode, LocoMapSystemUsageList.Descriptions.CustomsOfficeCode);
		}
	}
}

