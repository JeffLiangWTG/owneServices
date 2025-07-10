namespace Enterprise.Customs.NZ.Business.Declaration
{
	public class CusContainerInvoiceLinePivotValidation : Customs.Business.CusContainerInvoiceLinePivotValidation
	{
		public CusContainerInvoiceLinePivotValidation(CusContainerInvoiceLinePivot parent) : base(parent)
		{
		}

		protected new CusContainerInvoiceLinePivot Parent => (CusContainerInvoiceLinePivot)base.Parent;

		protected override void CheckC2_CO()
		{
			base.CheckC2_CO();

			var invoiceLine = Parent.InvoiceLine;
			if ((invoiceLine?.Declaration?.IsTSWWriteOff ?? false) && invoiceLine.ContainersPivot.Count > 1)
			{
				Parent.C2_COInfo.AddMessageError(InvoiceLineCannotHaveMultipleWriteoffContainers);
			}
		}

		public const string InvoiceLineCannotHaveMultipleWriteoffContainers = "An Invoice Line can only link to one container in a write-off declaration.";
	}
}
