using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.DIS;

namespace Enterprise.Customs.US.DIS.Business
{
	public class DISInvoiceLineRange : AutoDISInvoiceLineRange
	{
		public DISInvoiceLineRange(DISInvoice disInvoice)
			: base(disInvoice.Factory)
		{
			this.disInvoice = disInvoice;
		}

		internal readonly DISInvoice disInvoice;

		public ZString InvoiceNumber
		{
			get { return disInvoice.InvoiceNumber; }
		}

		public IEnumerable<IDISInvoiceLine> InvoiceLines
		{
			get { return disInvoice.disDocument.HostWrapper.GetInvoiceLines(InvoiceNumber, InvoiceLineFrom, InvoiceLineTo); }
		}

		public override ZInt InvoiceLineFrom
		{
			get { return base.InvoiceLineFrom; }
			set
			{
				base.InvoiceLineFrom = value;

				if (InvoiceLineTo < InvoiceLineFrom)
				{
					InvoiceLineTo = InvoiceLineFrom;
				}
			}
		}
	}
}
