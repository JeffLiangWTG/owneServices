using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.TR.Business.Declaration
{
	public class InvoiceChargeCollection : EU.Business.Declaration.InvoiceChargeCollection<InvoiceCharge>
	{
		public InvoiceChargeCollection(JobComInvoiceHeader invoice) : this(invoice, null)
		{
		}

		public InvoiceChargeCollection(JobComInvoiceHeader invoice, Func<InvoiceCharge, bool> additionalFilter) : base(invoice)
		{
			AdditionalFilter = additionalFilter;
			Rebuild();
		}

		protected override bool AllowNewCore => false;

		protected override bool AllowRemoveCore => false;

		protected override bool IsThisPartOfTheCollection(BusinessObject element) =>
			base.IsThisPartOfTheCollection(element)
			&& (AdditionalFilter == null || AdditionalFilter.Invoke((InvoiceCharge)element));

		new Func<InvoiceCharge, bool> AdditionalFilter { get; }
	}
}
