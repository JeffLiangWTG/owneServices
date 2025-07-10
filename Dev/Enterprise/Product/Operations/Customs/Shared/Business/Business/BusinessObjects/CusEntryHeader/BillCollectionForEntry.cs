using System;
using System.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public class BillCollectionForEntry : BusinessObjectCollection<Bill>
	{
		public BillCollectionForEntry(CusEntryHeader entryHeader)
			: base(entryHeader.Factory)
		{
			this.entryHeader = entryHeader;
		}

		public override void Load()
		{
			throw new NotSupportedException("PopulateBills method should be used for creating collection");
		}

		public override void Load(ZQuery alternativeAdditionalFilter)
		{
			throw new NotSupportedException("PopulateBills method should be used for creating collection");
		}

		public virtual void PopulateBills()
		{
			this.RemoveAll();

			foreach (BaseJobComInvoiceHeader invoice in entryHeader.InvoiceHeaders)
			{
				if (invoice.Bill != null && !this.Contains(invoice.Bill))
				{
					Add(invoice.Bill);
				}
			}

			if (this.Count == 0 && declaration is BaseJobDeclaration dec)
			{
				AddRange(dec.LowestBills);
			}

			this.Sort(Bill.Schema.CU_BillUniqueCode, ListSortDirection.Ascending);
		}

		#region Implementation

		protected readonly CusEntryHeader entryHeader;
		protected BaseJobDeclaration declaration
		{
			get { return entryHeader.Declaration; }
		}
		#endregion

	}
}
