namespace Enterprise.MasterFiles.Business
{
	public class TRLocoMapSystemUsageList : LocoMapSystemUsageList
	{
		public TRLocoMapSystemUsageList()
		{
			Clear();
			AddPair(LocoMapSystemUsageList.Codes.CustomsPortCodeList, LocoMapSystemUsageList.Descriptions.CustomsPortCodeList);
		}
	}
}
