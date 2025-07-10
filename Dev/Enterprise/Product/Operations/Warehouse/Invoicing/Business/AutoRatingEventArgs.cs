using System;

namespace Enterprise.Warehouse.Invoicing.Business
{
	public class AutoRatingEventArgs : EventArgs
	{
		public AutoRatingEventArgs(bool hasExistingInvoice, PeriodicInvoicing invoice)
		{
			HasExistingInvoice = hasExistingInvoice;
			Invoice = invoice;
		}

		public PeriodicInvoicing Invoice { get; }
		public bool HasExistingInvoice { get; }
	}
}
