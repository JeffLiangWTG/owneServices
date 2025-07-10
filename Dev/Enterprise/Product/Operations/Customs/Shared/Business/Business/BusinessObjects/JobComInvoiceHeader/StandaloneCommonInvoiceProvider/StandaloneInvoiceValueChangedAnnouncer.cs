using System;

namespace Enterprise.Customs.Business
{
	public class StandaloneInvoiceValueChangedAnnouncer : IDisposable, IInvoicesProviderValueChangedAnnouncer
	{
		public StandaloneInvoiceValueChangedAnnouncer(BaseJobComInvoiceHeader invoice)
		{
			this.invoice = invoice;
			invoice.JZ_MessageTypeInfo.ValueChanged += new EventHandler(ValueChanged);
		}

		protected readonly BaseJobComInvoiceHeader invoice;

		public event EventHandler OnValueChanged;

		protected void ValueChanged(object sender, EventArgs e)
		{
			if (OnValueChanged != null)
			{
				OnValueChanged(sender, e);
			}
		}

		#region IDisposable Members

		public virtual void Dispose()
		{
			invoice.JZ_MessageTypeInfo.ValueChanged -= new EventHandler(ValueChanged);
		}

		#endregion
	}
}
