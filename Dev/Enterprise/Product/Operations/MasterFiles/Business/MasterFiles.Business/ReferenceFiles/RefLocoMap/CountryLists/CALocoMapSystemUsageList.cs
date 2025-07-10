namespace Enterprise.MasterFiles.Business
{
	public class CALocoMapSystemUsageList : LocoMapSystemUsageList
	{
		public CALocoMapSystemUsageList()
		{
			Clear();
			AddPair(LocoMapSystemUsageList.Codes.Air, LocoMapSystemUsageList.Descriptions.Air);
			AddPair(LocoMapSystemUsageList.Codes.All, LocoMapSystemUsageList.Descriptions.All);
			AddPair(LocoMapSystemUsageList.Codes.Oth, LocoMapSystemUsageList.Descriptions.Oth);
			AddPair(LocoMapSystemUsageList.Codes.Sea, LocoMapSystemUsageList.Descriptions.Sea);
			AddPair(LocoMapSystemUsageList.Codes.Sub, LocoMapSystemUsageList.Descriptions.Sub);
		}
	}
}
