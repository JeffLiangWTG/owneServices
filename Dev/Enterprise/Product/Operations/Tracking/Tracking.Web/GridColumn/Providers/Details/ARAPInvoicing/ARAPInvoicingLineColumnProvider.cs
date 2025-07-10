using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	public class ARAPInvoicingLineColumnProvider : GridColumnProvider
	{
		public ARAPInvoicingLineColumnProvider()
		{
		}

		protected override void CustomizeDictionaryCore()
		{
			base.CustomizeDictionaryCore();
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("e6f7e977-e9c0-4df7-becb-fb058c5cf4f9", "Description"), InvoicingLineBase.Schema.AL_Desc) { ColumnKey = WebTracker.Grids.ARAPInvoicingLine.Description });
			AddToDictionaryAsDefault(new ZDropEditColumn(Res.GetString("8b04c38d-9a15-42e3-9799-97ad9d821fb3", "Cur"), InvoicingLineBase.Schema.AL_RX_NKTransactionCurrency) { ColumnKey = WebTracker.Grids.ARAPInvoicingLine.Currency });
			AddToDictionaryAsDefault(new ZCalcEditColumn(Res.GetString("583d51c2-9ef6-4cc8-9b9c-6bc6b6aecf57", "Ex. Tax"), InvoicingLineBase.Schema.AL_OSExTaxAmount) { ColumnKey = WebTracker.Grids.ARAPInvoicingLine.ExTaxAmount });
			AddToDictionaryAsDefault(new ZCalcEditColumn(Res.GetString("31445ec7-977a-45fc-92f7-fb3fe24a16c2", "Tax"), InvoicingLineBase.Schema.AL_OSTaxAmount) { ColumnKey = WebTracker.Grids.ARAPInvoicingLine.TaxAmount });
			AddToDictionaryAsDefault(new ZCalcEditColumn(Res.GetString("ff41cfc6-807a-436b-9bf6-a70edf72567a", "Total Amount"), InvoicingLineBase.Schema.AL_OverseasTotal) { ColumnKey = WebTracker.Grids.ARAPInvoicingLine.TotalAmount });
		}
	}
}
