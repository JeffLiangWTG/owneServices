
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.SG.V4.Business
{
	public class InvoiceLineViewCollection : InvoiceLineViewCollection<JobComInvoiceLine>
	{
		public InvoiceLineViewCollection(JobDeclaration jobDeclaration)
			: base(jobDeclaration)
		{
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			JobComInvoiceLine invoiceLine = child as JobComInvoiceLine;
			if (invoiceLine != null)
			{
				if (Count > 0)
				{
					JobComInvoiceLine previousInvoiceLine = this[Count - 1];
					invoiceLine.JI_CountryOfOrigin = previousInvoiceLine.JI_CountryOfOrigin;
				}
			}
		}

		protected override bool AllowNewCore
		{
			get { return !NoAddingOrRemoving && base.AllowNewCore; }
		}

		protected override bool AllowRemoveCore
		{
			get { return !NoAddingOrRemoving && base.AllowNewCore; }
		}

		public bool NoAddingOrRemoving;
	}
}
