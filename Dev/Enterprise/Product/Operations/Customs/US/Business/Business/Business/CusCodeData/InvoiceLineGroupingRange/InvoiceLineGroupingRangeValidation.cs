namespace Enterprise.Customs.US.Business
{
	public class InvoiceLineGroupingRangeValidation : Customs.Business.CusCodeDataValidation
	{
		public InvoiceLineGroupingRangeValidation(InvoiceLineGroupingRange parent)
			: base(parent)
		{
		}

		public new InvoiceLineGroupingRange Parent
		{
			get { return (InvoiceLineGroupingRange)base.Parent; }
		}

		protected override void CheckCY_Code()
		{
		}

		protected override void CheckCY_Type()
		{
		}
	}
}
