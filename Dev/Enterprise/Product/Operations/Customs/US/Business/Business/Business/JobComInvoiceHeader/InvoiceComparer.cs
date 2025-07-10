using System.Collections.Generic;

namespace Enterprise.Customs.US.Business
{
	/// <summary>
	/// By JZ_InvoiceDisplaySequence > JZ_InvoiceNumber > PK
	/// </summary>
	class InvoiceComparer : IComparer<IInvoiceHeader>, IComparer<JobComInvoiceHeader>
	{
		public int Compare(IInvoiceHeader x, IInvoiceHeader y)
		{
			int result = x.JZ_InvoiceDisplaySequence.CompareTo(y.JZ_InvoiceDisplaySequence);

			if (result == 0)
			{
				result = x.JZ_InvoiceNumber.CompareTo(y.JZ_InvoiceNumber);
			}

			if (result == 0)
			{
				result = x.PK.CompareTo(y.PK);
			}

			return result;
		}

		int IComparer<JobComInvoiceHeader>.Compare(JobComInvoiceHeader x, JobComInvoiceHeader y)
		{
			return Compare(x, y);
		}
	}
}
