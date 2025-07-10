namespace Enterprise.MasterFiles.Business
{
	public class SGLocoMapSystemUsageList : LocoMapSystemUsageList
	{
		public SGLocoMapSystemUsageList()
		{
			Clear();
			AddPair(LocoMapSystemUsageList.Codes.CustomsPortCodeList, LocoMapSystemUsageList.Descriptions.CustomsPortCodeList);
		}
	}
}
