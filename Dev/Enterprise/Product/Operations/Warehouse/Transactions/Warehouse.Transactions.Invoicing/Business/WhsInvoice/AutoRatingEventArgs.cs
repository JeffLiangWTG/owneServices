using System;

namespace Enterprise.Warehouse.Transactions.Invoicing
{
	public class AutoRatingEventArgs : EventArgs
	{
		public AutoRatingEventArgs(bool hasExistingInvoice, WhsInvoice invoice)
		{
			HasExistingInvoice = hasExistingInvoice;
			Invoice = invoice;
		}

		public WhsInvoice Invoice { get; }
		public bool HasExistingInvoice { get; }
	}
}
