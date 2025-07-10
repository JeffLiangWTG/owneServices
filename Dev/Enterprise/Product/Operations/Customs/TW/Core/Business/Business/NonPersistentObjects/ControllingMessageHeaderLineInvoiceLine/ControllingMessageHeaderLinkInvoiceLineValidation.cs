namespace Enterprise.Customs.TW.Business
{
	public class ControllingMessageHeaderLinkInvoiceLineValidation : AutoControllingMessageHeaderLinkInvoiceLineValidation
	{
		public ControllingMessageHeaderLinkInvoiceLineValidation(AutoControllingMessageHeaderLinkInvoiceLine parent) : base(parent)
		{
		}

		public new ControllingMessageHeaderLinkInvoiceLine Parent => (ControllingMessageHeaderLinkInvoiceLine)base.Parent;

		protected override void CheckLink()
		{
			base.CheckLink();
			var parent = Parent;
			if (parent.Link && parent.InvoicelinePK.IsValid && parent.ControllingMessageHeader is CusTWControllingMessageHeader header && !header.CheckAssignECFAHeaderToLines())
			{
				parent.LinkInfo.AddMessageError(ValidationConstants.CusTWControllingMessageHeader.CanNotAssignECFAHeaderToLines);
			}
		}
	}
}
