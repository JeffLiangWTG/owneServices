using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	public class ARAPInvoicingSearchColumnProvider : ARAPInvoicingColumnProvider
	{
		public ARAPInvoicingSearchColumnProvider(TrackingSiteUser siteUser)
			: base(siteUser)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0004:Remove Unnecessary Cast", Justification = "ZBindToChecker requires a redundant cast")]
		protected override void CustomizeDictionaryCore()
		{
			base.CustomizeDictionaryCore();
			ZBindToChecker.CheckBindTo((ZString)((InvoicingBase)null).OperationsJob.InvoicingSupporter.Consignor.OH_FullName);
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("d643ab7c-d32e-43c9-a887-40af126cb8db", "Consignor"), "OperationsJob+InvoicingSupporter+Consignor+OH_FullName") { ColumnKey = WebTracker.Grids.ARAPInvoicing.ConsignorName });

			ZBindToChecker.CheckBindTo((ZString)((InvoicingBase)null).OperationsJob.InvoicingSupporter.Consignee.OH_FullName);
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("481afe7b-a00c-4e0f-92f6-15f5720dde02", "Consignee"), "OperationsJob+InvoicingSupporter+Consignee+OH_FullName") { ColumnKey = WebTracker.Grids.ARAPInvoicing.ConsigneeName });

			ZBindToChecker.CheckBindTo((ZDateTime)((InvoicingBase)null).LastRequestedViaWeb);
			AddToDictionaryAsDefault(new ZDateTimeColumn(Res.GetString("e1a04042-7479-4306-8fc7-edcab46f0413", "Last Requested"), InvoicingBase.Schema.LastRequestedViaWeb) { ColumnKey = WebTracker.Grids.ARAPInvoicing.LastRequested });
		}
	}
}
