namespace Enterprise.MasterFiles.Business
{
	public class GBLocoMapSystemUsageList : AirSeaMailSystemUsageList
	{
		public GBLocoMapSystemUsageList() : base()
		{
			AddPair(LocoMapSystemUsageList.Codes.GVMS, LocoMapSystemUsageList.Descriptions.GVMS);
			AddPair(LocoMapSystemUsageList.Codes.ROA, LocoMapSystemUsageList.Descriptions.ROA);
			AddPair(LocoMapSystemUsageList.Codes.RAI, LocoMapSystemUsageList.Descriptions.RAI);
			AddPair(LocoMapSystemUsageList.Codes.ROR, LocoMapSystemUsageList.Descriptions.ROR);
		}
	}
}
