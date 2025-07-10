using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.DIS.Business
{
	public class DISInvoiceValidation : AutoDISInvoiceValidation
	{
		public DISInvoiceValidation(AutoDISInvoice bizObj)
			: base(bizObj)
		{
		}

		new DISInvoice Parent
		{
			get { return (DISInvoice)base.Parent; }
		}

		protected override void CheckInvoiceNumber()
		{
			base.CheckInvoiceNumber();

			ListValidation.MessageErrorIfInvalidCode(Parent.InvoiceNumberInfo, Parent.DefaultInvoiceList);

			if (Parent.InvoiceNumber.IsEmpty && Parent.InvoiceLineRanges.Count > 0)
			{
				Parent.InvoiceNumberInfo.AddMessageError(PleaseEnterInvoiceNumber);
			}
		}

		internal const string PleaseEnterInvoiceNumber = "Please enter an invoice number. You have specified invoice line ranges without an invoice number.";
	}
}
