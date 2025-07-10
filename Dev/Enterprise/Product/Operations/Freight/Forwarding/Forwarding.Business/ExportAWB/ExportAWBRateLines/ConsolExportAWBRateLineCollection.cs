namespace Enterprise.Freight.Forwarding.Business.AWB
{
	public class ConsolExportAWBRateLineCollection : ExportAWBRateLineCollection
	{
		public ConsolExportAWBRateLineCollection(ExportAWBHeader master)
			: base(master)
		{
		}

		public new ConsolExportAWBRateLine this[int index]
		{
			get { return (ConsolExportAWBRateLine)(Elements[index]); }
		}

		public new ConsolExportAWBRateLine AddNew()
		{
			return (ConsolExportAWBRateLine)base.AddNew();
		}
	}
}
