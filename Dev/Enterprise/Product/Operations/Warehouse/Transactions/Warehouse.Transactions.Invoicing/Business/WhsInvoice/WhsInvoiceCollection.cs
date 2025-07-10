using CargoWise.EntityFramework;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Invoicing
{
	public class WhsInvoiceCollection : JobStorageCollection
	{
		public WhsInvoiceCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public int CountInvoicesToInclude
		{
			get
			{
				int result = 0;
				foreach (WhsInvoice invoice in this)
				{
					if (invoice.IncludeInInvoicing)
					{
						result++;
					}
				}
				return result;
			}
		}

		#region Implementation

		public new WhsInvoice this[int index]
		{
			get { return (WhsInvoice)Elements[index]; }
		}

		public virtual new WhsInvoice AddNew()
		{
			return (WhsInvoice)base.AddNew();
		}

		public int IndexOf(WhsInvoice invoice)
		{
			return Elements.IndexOf(invoice);
		}

		#endregion

		#region Filter

		protected override ZQuery CreateAdditionalFilter()
		{
			ZQuery filter = base.CreateAdditionalFilter();
			filter.AddToFilter(JobStorageSchema.ET_StorageType, WhsInvoice.StorageType);
			return filter;
		}

		#endregion
	}
}
