namespace Enterprise.Freight.Forwarding.Business.AWB
{
	public abstract class ExportAWBOtherChargesCollection : Forwarding.AWB.Business.ExportAWBOtherChargesCollection
	{
		protected ExportAWBOtherChargesCollection(ExportAWBHeader master)
			: base(master)
		{
		}

		public virtual int MaxOtherChargesThatCouldFitOnPrintedAWB
		{
			get { return 15; }
		}

		public virtual int MaxOtherChargesWithMissingChargeCodeThatCouldFitOnPrintedAWB
		{
			get { return 10; }
		}

		public new ExportAWBOtherCharges this[int index]
		{
			get { return (ExportAWBOtherCharges)(Elements[index]); }
		}

		public new ExportAWBOtherCharges AddNew()
		{
			return (ExportAWBOtherCharges)base.AddNew();
		}
	}
}
