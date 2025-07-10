namespace Enterprise.Freight.Forwarding.Business.AWB
{
	public class ShipmentExportAWBOtherChargesCollection : ExportAWBOtherChargesCollection
	{
		public ShipmentExportAWBOtherChargesCollection(ExportAWBHeader master) : base(master)
		{
		}

		public new ShipmentExportAWBOtherCharges this[int index]
		{
			get { return (ShipmentExportAWBOtherCharges)(Elements[index]); }
		}

		public new ShipmentExportAWBOtherCharges AddNew()
		{
			return (ShipmentExportAWBOtherCharges)base.AddNew();
		}
	}
}
