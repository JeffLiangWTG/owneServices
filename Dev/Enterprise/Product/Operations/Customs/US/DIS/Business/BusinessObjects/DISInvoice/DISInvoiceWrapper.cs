using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.DIS;

namespace Enterprise.Customs.US.DIS.Business
{
	class DISInvoiceWrapper : IDISInvoice
	{
		public DISInvoiceWrapper(DISInvoice invoice, DISHostWrapper wrapper)
		{
			this.invoice = invoice;
			this.wrapper = wrapper;
		}

		readonly DISInvoice invoice;
		readonly DISHostWrapper wrapper;

		IEnumerable<IDISInvoiceLine> IDISInvoice.InvoiceLines
		{
			get
			{
				if (invoice.InvoiceLineRanges.Count > 0)
				{
					foreach (DISInvoiceLineRange lineRange in invoice.InvoiceLineRanges)
					{
						var lineWrapper = new DISInvoiceLineRangeWrapper(lineRange, wrapper);

						foreach (IDISInvoiceLine invoiceLine in lineWrapper.InvoiceLines)
						{
							yield return invoiceLine;
						}
					}
				}
				else
				{
					foreach (var invoiceLine in wrapper.GetAllInvoiceLines(invoice.InvoiceNumber))
					{
						yield return invoiceLine;
					}
				}
			}
		}

		ZString IDISInvoice.InvoiceNumber
		{
			get { return invoice.InvoiceNumber; }
		}

		DISInvoiceType IDISInvoice.InvoiceType
		{
			get { return DISInvoiceType.CommercialInvoice; }
		}
	}
}
