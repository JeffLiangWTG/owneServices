using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using NUnit.Framework;

namespace Enterprise.Tracking.Web.Testing
{
	[TestedType(typeof(ARAPInvoicingSearchColumnProvider))]
	sealed class ARAPInvoicingSearchColumnProviderTest : ARAPInvoicingColumnProviderTest
	{
		protected override void SetupColumnsCore()
		{
			base.SetupColumnsCore();
			AddDefaultsColumn(new ZTextEditColumn("Consignor", "OperationsJob+InvoicingSupporter+Consignor+OH_FullName") { ColumnKey = WebTracker.Grids.ARAPInvoicing.ConsignorName });
			AddDefaultsColumn(new ZTextEditColumn("Consignee", "OperationsJob+InvoicingSupporter+Consignee+OH_FullName") { ColumnKey = WebTracker.Grids.ARAPInvoicing.ConsigneeName });
			AddDefaultsColumn(new ZDateTimeColumn("Last Requested", InvoicingBase.Schema.LastRequestedViaWeb) { ColumnKey = WebTracker.Grids.ARAPInvoicing.LastRequested });
		}

		protected override bool SupportsOldLayoutFix
		{
			get { return false; }
		}

		protected override GridColumnProvider GetNewTestProvider()
		{
			return new ARAPInvoicingSearchColumnProvider(LoggedSiteUserForTest);
		}
	}
}
