namespace Enterprise.MasterFiles.Business
{
	public class AirSeaMailSystemUsageList : LocoMapSystemUsageList
	{
		public AirSeaMailSystemUsageList()
		{
			Clear();
			AddPair(LocoMapSystemUsageList.Codes.Air, LocoMapSystemUsageList.Descriptions.Air);
			AddPair(LocoMapSystemUsageList.Codes.Mail, LocoMapSystemUsageList.Descriptions.Mail);
			AddPair(LocoMapSystemUsageList.Codes.Sea, LocoMapSystemUsageList.Descriptions.Sea);
		}
	}
}
