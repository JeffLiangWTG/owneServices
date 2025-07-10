using CargoWise.EntityFramework;

namespace Enterprise.Customs.TW.Business
{
	public class StorageAndShippingConditionJobComInvLineRefsValidation : Customs.Business.JobComInvLineRefsValidation
	{
		public StorageAndShippingConditionJobComInvLineRefsValidation(StorageAndShippingConditionJobComInvLineRefs parent)
			: base(parent)
		{
		}

		new StorageAndShippingConditionJobComInvLineRefs Parent => (StorageAndShippingConditionJobComInvLineRefs)base.Parent;

		protected override void CheckJG_ReferenceNumber()
		{
			base.CheckJG_ReferenceNumber();

			var parentInvoiceLine = Parent.InvoiceLine;
			if (parentInvoiceLine != null && parentInvoiceLine.IsForCAHeaderCD)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JG_ReferenceNumberInfo);
			}
		}
	}
}
