using System.Collections.Generic;

namespace Enterprise.Customs.US.Business
{
	public class InvoiceLineComparer : Comparer<JobComInvoiceLine>
	{
		public InvoiceLineComparer()
		{
		}

		public override int Compare(JobComInvoiceLine x, JobComInvoiceLine y)
		{
			var xToCompare = x.ParentTariffLine ?? x;
			var yToCompare = y.ParentTariffLine ?? y;
			if (xToCompare == yToCompare)//share the same parent or same object
			{
				xToCompare = x;
				yToCompare = y;
			}

			var lineX = xToCompare;
			var lineY = yToCompare;

			int result = 0;

			if (lineX.InvoiceHeader is JobComInvoiceHeader xInvoiceHeader && lineY.InvoiceHeader is JobComInvoiceHeader yInvoiceHeader)
			{
				string valueX = xInvoiceHeader.JZ_InvoiceNumber.IsEmpty ? xInvoiceHeader.PK.ToString() : xInvoiceHeader.JZ_InvoiceNumber.ToString();
				string valueY = yInvoiceHeader.JZ_InvoiceNumber.IsEmpty ? yInvoiceHeader.PK.ToString() : yInvoiceHeader.JZ_InvoiceNumber.ToString();

				result = valueX.CompareTo(valueY);
			}

			if (result == 0)
			{
				result = lineX.JI_LineNo.CompareTo(lineY.JI_LineNo);
			}

			if (result == 0)
			{
				result = lineX.PK.CompareTo(lineY.PK);
			}

			return result;
		}
	}
}
