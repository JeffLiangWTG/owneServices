using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Business
{
	public class InvoiceQuantityAndUnitQtyResultCollection : NonPersistentBusinessObjectCollection<InvoiceQuantityAndUnitQtyResult>
	{
		public InvoiceQuantityAndUnitQtyResultCollection(JobComInvoiceLine supporter)
			: base(supporter.Factory)
		{
			line = supporter;
		}
		readonly JobComInvoiceLine line;

		int oldHashCode = -1;

		public override int GetHashCode()
		{
			var hashCode = 0;
			if (line.InvoiceHeader is JobComInvoiceHeader header)
			{
				var invoiceLines = header.InvoiceLines.Cast<JobComInvoiceLine>();
				hashCode = invoiceLines.Count().GetHashCode();
				invoiceLines.ForEach(x => hashCode = hashCode ^ x.JI_InvoiceUQ.GetHashCode() ^ x.JI_InvoiceQuantity.GetHashCode());
			}

			return hashCode;
		}

		#region Implement
		void RebuildElements()
		{
			RemoveAll();

			if (line.InvoiceHeader is JobComInvoiceHeader header)
			{
				var invoiceLines = header.InvoiceLines.Cast<JobComInvoiceLine>();
				var invoiceLinesGroup = invoiceLines.GroupBy(c => new { c.JI_InvoiceUQ }).Select(c => (c.Key.JI_InvoiceUQ, c.Sum(value => value.JI_InvoiceQuantity)));
				foreach (var group in invoiceLinesGroup)
				{
					AddCharge(group.JI_InvoiceUQ, group.Item2);
				}
			}
		}

		public bool ShouldRebuildElements()
		{
			var hashCode = GetHashCode();
			bool result = hashCode != oldHashCode;
			if (result)
			{
				oldHashCode = hashCode;
				RebuildElements();
			}
			return result;
		}

		void AddCharge(ZString invoiceUQ, ZDecimal quantity)
		{
			var invoiceQuantityAndUnitQtyResult = (InvoiceQuantityAndUnitQtyResult)CreateNonPersistentBusinessObject();
			invoiceQuantityAndUnitQtyResult.InvoiceUQ = invoiceUQ;
			invoiceQuantityAndUnitQtyResult.InvoiceQuantity = quantity;
			Add(invoiceQuantityAndUnitQtyResult);
		}

		#endregion

		#region override

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new InvoiceQuantityAndUnitQtyResult();
		}

		#endregion
	}
}
