using CargoWise.Common;

namespace Enterprise.Customs.US.DIS.Business
{
	public class DISInvoiceLineRangeValidation : AutoDISInvoiceLineRangeValidation
	{
		public DISInvoiceLineRangeValidation(AutoDISInvoiceLineRange bizObj)
			: base(bizObj)
		{
		}

		new DISInvoiceLineRange Parent
		{
			get { return (DISInvoiceLineRange)base.Parent; }
		}

		protected override void CheckInvoiceLineFrom()
		{
			base.CheckInvoiceLineFrom();

			if (Parent.InvoiceLineFrom <= 0)
			{
				Parent.InvoiceLineFromInfo.AddMessageError(PleaseEnterValidInvoiceLineRange);
			}
			else
			{
				var invoiceLines = Parent.disInvoice.disDocument.HostWrapper.GetInvoiceLines(Parent.InvoiceNumber, Parent.InvoiceLineFrom, Parent.InvoiceLineFrom);

				if (invoiceLines.IsNullOrEmpty())
				{
					Parent.InvoiceLineFromInfo.AddMessageError(string.Format(NoInvoiceLineWithThisLineNumber, Parent.InvoiceLineFrom, Parent.InvoiceNumber));
				}
			}
		}

		protected override void CheckInvoiceLineTo()
		{
			base.CheckInvoiceLineTo();

			if (Parent.InvoiceLineTo < 0)
			{
				Parent.InvoiceLineToInfo.AddMessageError(PleaseEnterValidInvoiceLineRange);
			}
			else if (Parent.InvoiceLineTo > 0)
			{
				if (Parent.InvoiceLineTo < Parent.InvoiceLineFrom)
				{
					Parent.InvoiceLineToInfo.AddMessageError(InvalidInvoiceLineTo);
				}
				else if (Parent.InvoiceLineTo > Parent.InvoiceLineFrom)
				{
					var invoiceLines = Parent.disInvoice.disDocument.HostWrapper.GetInvoiceLines(Parent.InvoiceNumber, Parent.InvoiceLineTo, Parent.InvoiceLineTo);

					if (invoiceLines.IsNullOrEmpty())
					{
						Parent.InvoiceLineToInfo.AddMessageError(string.Format(NoInvoiceLineWithThisLineNumber, Parent.InvoiceLineTo, Parent.InvoiceNumber));
					}
				}
			}
		}

		internal const string PleaseEnterValidInvoiceLineRange = "Please enter a valid Invoice Line #.";
		internal const string InvalidInvoiceLineTo = "Invoice Line # To should be greater than or equal to Invoice Line # From.";
		internal const string NoInvoiceLineWithThisLineNumber = "Invoice Line No. '{0}' does not exist on Invoice No. '{1}'";
	}
}
