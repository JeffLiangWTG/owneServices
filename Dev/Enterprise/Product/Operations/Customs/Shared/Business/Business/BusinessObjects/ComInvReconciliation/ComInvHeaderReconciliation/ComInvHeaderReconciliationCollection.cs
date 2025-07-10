using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public class ComInvHeaderReconciliationCollection : NonPersistentBusinessObjectCollection<ComInvHeaderReconciliation>
	{
		public ComInvHeaderReconciliationCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public ComInvHeaderReconciliation this[ComInvOrderReconciliation comInvOrder]
		{
			get
			{
				ComInvHeaderReconciliation result = null;

				foreach (ComInvHeaderReconciliation header in this)
				{
					if (header.InvoiceNumber == comInvOrder.InvoiceNumber && header.InvoiceDate == comInvOrder.InvoiceDate &&
						header.CurrencyCode == comInvOrder.JD_RX_NKOrderCurrency && header.SupplierPK == comInvOrder.SupplierPK && header.IncoTerm == comInvOrder.JD_IncoTerm)
					{
						result = header;
						break;
					}
				}

				return result;
			}
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		public void CopyInvoiceLinesPersistentValuesToAnotherFactory(BusinessObjectFactory declarationFactory)
		{
			foreach (ComInvHeaderReconciliation header in this)
			{
				header.InvoiceLines.CopyPersistentValuesToAnotherFactory(declarationFactory);
			}
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return null;
		}

		protected override BusinessObject AddNewCore()
		{
			return null;
		}
	}
}
