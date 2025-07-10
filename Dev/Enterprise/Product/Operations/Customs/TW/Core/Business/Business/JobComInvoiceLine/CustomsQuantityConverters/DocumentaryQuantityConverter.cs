namespace Enterprise.Customs.TW.Business
{
	public class DocumentaryQuantityConverter : BaseCustomsQuantityConverter
	{
		public DocumentaryQuantityConverter(JobComInvoiceLine invoiceLine)
		: base(invoiceLine, invoiceLine.AddInfoChild.TWL_DocumentaryQtyInfo, invoiceLine.AddInfoChild.TWL_DocumentaryUQInfo)
		{ }
		protected override void CalculateConversionFactor()
		{
			SetDocumentaryUnitToInvoiceUnitIfReasonable();
			base.CalculateConversionFactor();
		}
		void SetDocumentaryUnitToInvoiceUnitIfReasonable()
		{
			var invoiceUnit = InvoiceLine.JI_InvoiceUQ;
			var documentaryUnit = customsUnitOfQuantityInfo.Value;
			if (!invoiceUnit.IsEmpty && documentaryUnit.IsEmpty)
			{
				customsUnitOfQuantityInfo.SetValueFromString(invoiceUnit);
			}
		}
	}
}
