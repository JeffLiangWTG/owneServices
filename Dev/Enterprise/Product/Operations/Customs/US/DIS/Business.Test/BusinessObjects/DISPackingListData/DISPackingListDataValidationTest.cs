using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.DIS.Business.Testing
{
	sealed class DISPackingListDataValidationTest : TestCaseWithFactory
	{
		public void TestCheckInvalidCharatersOnData()
		{
			var msgPackingListNumber = string.Format(DISDocumentValidation.DataHasInvalidCharacters, DISPackingListData.Schema.PackingListNumber);
			var msgInvoiceNumber = string.Format(DISDocumentValidation.DataHasInvalidCharacters, DISPackingListData.Schema.InvoiceNumber);
			var msgPurchaseOrderNumber = string.Format(DISDocumentValidation.DataHasInvalidCharacters, DISPackingListData.Schema.PurchaseOrderNumber);
			var packingData = new DISPackingListData(Factory);
			AssertNoWarningContaining(packingData.PackingListNumberInfo, msgPackingListNumber);
			AssertNoWarningContaining(packingData.InvoiceNumberInfo, msgInvoiceNumber);
			AssertNoWarningContaining(packingData.PurchaseOrderNumberInfo, msgPurchaseOrderNumber);
			packingData.InvoiceNumber = "BBÉCCÉDD";
			AssertHasWarningContaining(packingData.InvoiceNumberInfo, msgInvoiceNumber);
			packingData.PackingListNumber = "DÉAÉRÉRÉN";
			AssertHasWarningContaining(packingData.PackingListNumberInfo, msgPackingListNumber);
			packingData.PurchaseOrderNumber = "AÉZÉSÉYÉX";
			AssertHasWarningContaining(packingData.PurchaseOrderNumberInfo, msgPurchaseOrderNumber);
		}
	}
}
