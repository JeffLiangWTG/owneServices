namespace Enterprise.MasterFiles.Business
{
	public class FRLocoMapSystemUsageList : LocoMapSystemUsageList
	{
		public FRLocoMapSystemUsageList()
		{
			Clear();
			AddPair(Codes.PCS, Descriptions.PCS);
		}
	}
}
