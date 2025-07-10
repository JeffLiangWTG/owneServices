using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using NUnit.Framework;

namespace Enterprise.Tracking.Web.Testing
{
	[TestedType(typeof(ARAPInvoicingLineColumnProvider))]
	sealed class ARAPInvoicingLineColumnProviderTest : GridColumnProviderTest
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0002:Simplify Member Access", Justification = "Simplified access could change context here")]
		protected override void SetupColumnsCore()
		{
			base.SetupColumnsCore();
			AddDefaultsColumn(new ZTextEditColumn("Description", InvoicingLineBase.Schema.AL_Desc) { ColumnKey = WebTracker.Grids.ARAPInvoicingLine.Description });
			AddDefaultsColumn(new ZDropEditColumn("Cur", InvoicingLineBase.Schema.AL_RX_NKTransactionCurrency) { ColumnKey = WebTracker.Grids.ARAPInvoicingLine.Currency });
			AddDefaultsColumn(new ZCalcEditColumn("Ex. Tax", InvoicingLineBase.Schema.AL_OSExTaxAmount) { ColumnKey = WebTracker.Grids.ARAPInvoicingLine.ExTaxAmount });
			AddDefaultsColumn(new ZCalcEditColumn("Tax", InvoicingLineBase.Schema.AL_OSTaxAmount) { ColumnKey = WebTracker.Grids.ARAPInvoicingLine.TaxAmount });
			AddDefaultsColumn(new ZCalcEditColumn("Total Amount", InvoicingLineBase.Schema.AL_OverseasTotal) { ColumnKey = WebTracker.Grids.ARAPInvoicingLine.TotalAmount });
		}

		protected override bool SupportsOldLayoutFix
		{
			get { return false; }
		}

		protected override GridColumnProvider GetNewTestProvider()
		{
			return new ARAPInvoicingLineColumnProvider();
		}
	}
}
