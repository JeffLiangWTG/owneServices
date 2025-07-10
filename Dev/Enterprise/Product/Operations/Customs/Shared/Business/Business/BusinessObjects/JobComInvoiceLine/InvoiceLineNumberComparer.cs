using System.Collections.Generic;

namespace Enterprise.Customs.Business
{
	public class InvoiceLineNumberComparer : IComparer<BaseJobComInvoiceLine>
	{
		#region IComparer Members
		public int Compare(BaseJobComInvoiceLine x, BaseJobComInvoiceLine y)
		{
			if (x == y)
			{
				return 0;
			}

			int result = x.JI_LineNo.CompareTo(y.JI_LineNo);
			if (result == 0)
			{
				result = x.PK.CompareTo(y.PK);
			}

			return result;
		}

		#endregion
	}
}
