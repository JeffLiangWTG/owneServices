namespace Enterprise.Customs.US.DIS.Business
{
	public class DISPackingListDataValidation : AutoDISPackingListDataValidation
	{
		public DISPackingListDataValidation(AutoDISPackingListData bizObj)
			: base(bizObj)
		{
		}

		protected override void CheckInvoiceNumber()
		{
			base.CheckInvoiceNumber();

			if (DISDocumentValidation.HasInvalidCharacters(Parent.InvoiceNumber))
			{
				Parent.InvoiceNumberInfo.AddWarning(string.Format(DISDocumentValidation.DataHasInvalidCharacters, DISPackingListData.Schema.InvoiceNumber));
			}
		}

		protected override void CheckPackingListNumber()
		{
			base.CheckPackingListNumber();

			if (DISDocumentValidation.HasInvalidCharacters(Parent.PackingListNumber))
			{
				Parent.PackingListNumberInfo.AddWarning(string.Format(DISDocumentValidation.DataHasInvalidCharacters, DISPackingListData.Schema.PackingListNumber));
			}
		}

		protected override void CheckPurchaseOrderNumber()
		{
			base.CheckPurchaseOrderNumber();

			if (DISDocumentValidation.HasInvalidCharacters(Parent.PurchaseOrderNumber))
			{
				Parent.PurchaseOrderNumberInfo.AddWarning(string.Format(DISDocumentValidation.DataHasInvalidCharacters, DISPackingListData.Schema.PurchaseOrderNumber));
			}
		}
	}
}
