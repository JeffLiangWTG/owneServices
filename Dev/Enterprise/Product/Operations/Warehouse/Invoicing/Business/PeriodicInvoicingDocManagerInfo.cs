using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;

namespace Enterprise.Warehouse.Invoicing.Business
{
	public class PeriodicInvoicingDocManagerInfo(PeriodicInvoicing invoice, string docManagerCode) : DocManagerInfo(invoice, docManagerCode), ISupportReadOnlyOverride
	{
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
				var invoice = (PeriodicInvoicing)BusinessEntity;
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
