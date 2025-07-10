using System;

namespace Enterprise.Customs.Business
{
	public class InvoiceStructureChangedAnnouncer : IDisposable
	{
		public InvoiceStructureChangedAnnouncer(IInvoicesProvider invoiceProvider)
		{
			this.invoiceProvider = invoiceProvider;
			InvoiceStructureChangeEvent.AddInvoiceStructureChangedEventHandler(invoiceProvider.Factory, new EventHandler(invoiceStructureChangedServiceProvider_InvoiceStructureChanged));
		}

		readonly IInvoicesProvider invoiceProvider;

		public bool IsDirty
		{
			get { return isDirty; }
		}
		bool isDirty;

		public void ClearDirtyStatus()
		{
			isDirty = false;
		}

		#region Implementation

		void MarkAsDirty()
		{
			isDirty = true;
		}

		void invoiceStructureChangedServiceProvider_InvoiceStructureChanged(object sender, EventArgs e)
		{
			MarkAsDirty();
		}

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			InvoiceStructureChangeEvent.RemoveInvoiceStructureChangedEventHandler(invoiceProvider.Factory, new EventHandler(invoiceStructureChangedServiceProvider_InvoiceStructureChanged));
		}

		#endregion
	}
}
