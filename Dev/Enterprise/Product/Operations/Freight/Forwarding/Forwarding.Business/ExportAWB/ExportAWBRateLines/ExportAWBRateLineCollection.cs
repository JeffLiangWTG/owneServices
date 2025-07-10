namespace Enterprise.Freight.Forwarding.Business.AWB
{
	public class ExportAWBRateLineCollection : Forwarding.AWB.Business.ExportAWBRateLineCollection
	{
		public ExportAWBRateLineCollection(ExportAWBHeader master)
			: base(master)
		{
		}

		public new ExportAWBHeader Master
		{
			get { return (ExportAWBHeader)base.Master; }
		}

		public new ExportAWBRateLine this[int index]
		{
			get { return (ExportAWBRateLine)(Elements[index]); }
		}

		public new ExportAWBRateLine AddNew()
		{
			return (ExportAWBRateLine)base.AddNew();
		}

		public new ExportAWBRateLine this[string propertyName, object value]
		{
			get { return (ExportAWBRateLine)base[propertyName, value]; }
		}
	}
}
