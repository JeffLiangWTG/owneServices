namespace Enterprise.MasterFiles.Business
{
	public class CNLocoMapSystemUsageList : LocoMapSystemUsageList
	{
		public CNLocoMapSystemUsageList()
		{
			Clear();
			AddPair(LocoMapSystemUsageList.Codes.CustomsPortCodeList, LocoMapSystemUsageList.Descriptions.CustomsPortCodeList);
		}
	}
}
