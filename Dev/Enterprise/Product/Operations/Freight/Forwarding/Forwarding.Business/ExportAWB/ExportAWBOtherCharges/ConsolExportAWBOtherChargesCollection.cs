namespace Enterprise.Freight.Forwarding.Business.AWB
{
	public class ConsolExportAWBOtherChargesCollection : ExportAWBOtherChargesCollection
	{
		public ConsolExportAWBOtherChargesCollection(ExportAWBHeader master) : base(master)
		{
		}

		public new ConsolExportAWBOtherCharges this[int index]
		{
			get { return (ConsolExportAWBOtherCharges)(Elements[index]); }
		}

		public new ConsolExportAWBOtherCharges AddNew()
		{
			return (ConsolExportAWBOtherCharges)base.AddNew();
		}
	}
}
