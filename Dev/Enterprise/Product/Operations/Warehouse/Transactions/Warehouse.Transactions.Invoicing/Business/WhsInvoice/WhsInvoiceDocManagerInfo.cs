using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.Invoicing;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsInvoiceDocManagerInfo : DocManagerInfo, ISupportReadOnlyOverride
	{
		public WhsInvoiceDocManagerInfo(WhsInvoice invoice)
			: base(invoice, "WIV")
		{
		}

		protected override BusinessObject[] GetRelatedObjects()
		{
			var list = new List<BusinessObject>(base.GetRelatedObjects());
			list.AddRange(Transactions);

			return list.ToArray();
		}

		AccTransactionHeaderCollection Transactions
		{
			get
			{
				var invoice = (WhsInvoice)BusinessEntity;
				return new InvoiceLoader(invoice.Factory).GetInvoicesForUniqueRef(invoice.ET_StorageJobNumber);
			}
		}

		public override bool ReadOnly => fReadOnly;

		void ISupportReadOnlyOverride.SetReadOnly(bool readOnly)
		{
			fReadOnly = readOnly;
		}

		bool fReadOnly;
	}
}
