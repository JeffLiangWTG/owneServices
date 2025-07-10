namespace Enterprise.Freight.OnlineSailingSchedules.ServiceModel
{
	public class Voyage
	{
		public string Code { get; set; }
		public Carrier Operator { get; set; }
		public TradeLane TradeLane { get; set; }
		public Vessel Vessel { get; set; }
	}
}
