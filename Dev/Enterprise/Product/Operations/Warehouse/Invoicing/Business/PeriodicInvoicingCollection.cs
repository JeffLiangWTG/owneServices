using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Invoicing.Business
{
	public class PeriodicInvoicingCollection(BusinessObjectFactory factory, string storageType) : JobStorageCollection(factory)
	{
		public int CountInvoicesToInclude
		{
			get
			{
				int result = 0;
				foreach (PeriodicInvoicing invoice in this)
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

		public new PeriodicInvoicing this[int index]
		{
			get { return (PeriodicInvoicing)Elements[index]; }
		}

		public virtual new PeriodicInvoicing AddNew()
		{
			return (PeriodicInvoicing)base.AddNew();
		}

		protected override BusinessObject AddNewCore()
		{
			var invoicing = (PeriodicInvoicing)base.AddNewCore();
			invoicing.ET_StorageType = storageType;
			return invoicing;
		}

		protected override BusinessObject CreateInitialisedBusinessObjectFromRow(DataRow row)
		{
			var invoicing = (PeriodicInvoicing)base.CreateInitialisedBusinessObjectFromRow(row);
			invoicing.ET_StorageType = storageType;
			return invoicing;
		}

		public int IndexOf(PeriodicInvoicing invoice)
		{
			return Elements.IndexOf(invoice);
		}

		#endregion

		#region Filter

		protected override ZQuery CreateAdditionalFilter()
		{
			ZQuery filter = base.CreateAdditionalFilter();
			filter.AddToFilter(JobStorageSchema.ET_StorageType, storageType);
			return filter;
		}

		#endregion
	}
}
