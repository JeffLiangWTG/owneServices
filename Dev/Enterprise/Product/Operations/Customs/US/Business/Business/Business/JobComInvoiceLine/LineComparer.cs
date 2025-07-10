using System.Collections.Generic;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.US.Business
{
	public class LineComparer : IComparer<BaseJobComInvoiceLine>
	{
		#region IComparer Members

		public int Compare(object x, object y)
		{
			var lineX = (IInvoiceLine)x;
			var lineY = (IInvoiceLine)y;

			int result = 0;

			if (lineX.InvoiceHeader is IInvoiceHeader xInvoiceHeader && lineY.InvoiceHeader is IInvoiceHeader yInvoiceHeader)
			{
				result = new InvoiceComparer().Compare(xInvoiceHeader, yInvoiceHeader);
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

		#endregion

		int IComparer<BaseJobComInvoiceLine>.Compare(BaseJobComInvoiceLine x, BaseJobComInvoiceLine y)
		{
			return Compare(x, y);
		}
	}
}
