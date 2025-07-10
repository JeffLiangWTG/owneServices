namespace Enterprise.Freight.Forwarding.Business.AWB
{
	public class ShipmentExportAWBRateLineCollection : ExportAWBRateLineCollection
	{
		public ShipmentExportAWBRateLineCollection(ExportAWBHeader master)
			: base(master)
		{
		}

		public new ShipmentExportAWBRateLine this[int index]
		{
			get { return (ShipmentExportAWBRateLine)(Elements[index]); }
		}

		public new ShipmentExportAWBRateLine AddNew()
		{
			return (ShipmentExportAWBRateLine)base.AddNew();
		}
	}
}
