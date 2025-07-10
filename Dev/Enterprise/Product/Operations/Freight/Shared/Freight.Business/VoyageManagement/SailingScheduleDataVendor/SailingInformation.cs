namespace Enterprise.Freight.Business
{
	using CargoWise.Types;

	public class SailingInformation
	{
		public ZString Load { get; set; }
		public ZDateTime ETD { get; set; }
		public ZString Discharge { get; set; }
		public ZDateTime ETA { get; set; }
	}
}
