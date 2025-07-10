using System.Collections.Generic;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.US.Business
{
	/// <summary>
	/// Parent should come before its children.
	/// Two parents should be sorted on their references, Invoice Number + line number
	/// </summary>
	class InvoiceLineParentChildComparer : IComparer<JobComInvoiceLine>, IComparer<BaseJobComInvoiceLine>
	{
		public int Compare(JobComInvoiceLine x, JobComInvoiceLine y)
		{
			if (x == y)
			{
				return 0;
			}

			JobComInvoiceLine xToCompare = x;
			JobComInvoiceLine yToCompare = y;

			JobComInvoiceLine xParent = x.ParentTariffLine;
			JobComInvoiceLine yParent = y.ParentTariffLine;

			if (xParent != null && yParent == null)
			{
				if (xParent == y)
				{
					return 1;
				}

				xToCompare = xParent;
			}
			else if (xParent == null && yParent != null)
			{
				if (x == yParent)
				{
					return -1;
				}

				yToCompare = yParent;
			}
			else if (xParent != null && yParent != null && xParent != yParent)
			{
				xToCompare = xParent;
				yToCompare = yParent;
			}

			int result = 0;

			if (xToCompare.InvoiceHeader != null && yToCompare.InvoiceHeader != null)
			{
				result = new InvoiceComparer().Compare(xToCompare.InvoiceHeader, yToCompare.InvoiceHeader);

				if (result == 0)
				{
					result = xToCompare.JI_LineNo.CompareTo(yToCompare.JI_LineNo);
				}

				if (result == 0)
				{
					result = xToCompare.PK.CompareTo(yToCompare.PK);
				}
			}
			return result;
		}

		int IComparer<BaseJobComInvoiceLine>.Compare(BaseJobComInvoiceLine x, BaseJobComInvoiceLine y)
		{
			return Compare((JobComInvoiceLine)x, (JobComInvoiceLine)y);
		}
	}
}
