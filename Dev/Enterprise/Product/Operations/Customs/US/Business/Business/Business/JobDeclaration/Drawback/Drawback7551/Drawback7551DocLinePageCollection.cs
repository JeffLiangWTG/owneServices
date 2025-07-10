using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Interoperability", "CA1405:ComVisibleTypeBaseTypesShouldBeComVisible")]
	public class Drawback7551DocLinePageCollection : NonPersistentBusinessObjectCollection<Drawback7551DocLine>
	{
		public Drawback7551DocLinePageCollection(InvoiceLineCompleteCollection collection, BusinessObjectFactory factory)
			: base(factory)
		{
			this.collection = collection;
		}
		readonly InvoiceLineCompleteCollection collection;

		public void LoadDrawback7551DocLines(JobDeclaration declaration)
		{
			foreach (JobComInvoiceLine invoiceLine in collection)
			{
				var drawback7551DocLine = GetExistingDrawback7551DocLine(invoiceLine);
				if (declaration.US_DRWEnableMerge && drawback7551DocLine != null)
				{
					UpdateDocLineDetails(drawback7551DocLine, invoiceLine);
				}
				else
				{
					Add(new Drawback7551DocLine(invoiceLine));
				}
			}

			this.Sort<Drawback7551DocLine>(new Drawback7551DocLineComparer());
		}

		protected virtual Drawback7551DocLine GetExistingDrawback7551DocLine(JobComInvoiceLine invoiceLine)
		{
			return null;
		}

		protected virtual void UpdateDocLineDetails(Drawback7551DocLine drawback7551DocLine, JobComInvoiceLine invoiceLine)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotSupportedException("CreateNonPersistentBusinessObject is not supported by DrawbackInvoiceLinePageCollection.");
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override bool AllowRemoveCore
		{
			get
			{
				return false;
			}
		}
	}

	public class Drawback7551DocLineComparer : Comparer<Drawback7551DocLine>
	{
		public override int Compare(Drawback7551DocLine x, Drawback7551DocLine y)
		{
			int result = x.US_DRWExportDate.CompareTo(y.US_DRWExportDate);
			if (result == 0)
			{
				result = x.US_DRWExportID.CompareTo(y.US_DRWExportID);
			}
			return result;
		}

		public override bool Equals(object obj)
		{
			return obj != null && obj.GetType() == GetType();
		}

		public override int GetHashCode()
		{
			return GetType().GetHashCode();
		}
	}
}
