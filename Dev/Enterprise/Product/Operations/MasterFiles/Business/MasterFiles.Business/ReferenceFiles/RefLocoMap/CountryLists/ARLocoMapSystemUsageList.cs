namespace Enterprise.MasterFiles.Business
{
	public class ARLocoMapSystemUsageList : LocoMapSystemUsageList
	{
		public ARLocoMapSystemUsageList()
		{
			Clear();
			AddPair(LocoMapSystemUsageList.Codes.CustomsPortCodeList, LocoMapSystemUsageList.Descriptions.CustomsPortCodeList);
		}
	}
}
