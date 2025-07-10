using Enterprise.Customs.US.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	public class USDeclarationModuleColumnProvider : BaseDeclarationModuleColumnProvider
	{
		protected override void AddCountrySpecificColumns()
		{
			AddToDictionary(new ZDateTimeColumn(Res.GetString("225a42a6-d102-4527-9b89-9f7e7a36f7f3", "Entry Submitted"), "Declaration.EntrySubmittedDate") { ColumnKey = WebTracker.Grids.TrackingDeclarations.EntrySubmittedDate });
			AddToDictionary(new ZTextEditColumn(Res.GetString("35f724ec-feb5-4f77-a7e6-9bc6943cf388", "Entry Status"), "Declaration.EntrySummaryStatus") { ColumnKey = WebTracker.Grids.TrackingDeclarations.EntryStatus });
			AddToDictionary(new ZTextEditColumn(Res.GetString("e361ddea-91ed-4e79-9bf9-d03aecab89c2", "Entry Status Desc."), "Declaration.EntrySummaryStatusDesc") { ColumnKey = WebTracker.Grids.TrackingDeclarations.EntryStatusDescription });
			AddToDictionary(new ZTextEditColumn(Res.GetString("52494163-2e09-4516-ad60-5a2adc332d14", "Cargo Status"), "Declaration.CargoReleaseStatus") { ColumnKey = WebTracker.Grids.TrackingDeclarations.CargoStatus });
			AddToDictionary(new ZTextEditColumn(Res.GetString("9725ee15-bf65-4449-a1e4-39ff76303e88", "Cargo Status Description"), "Declaration.CargoReleaseStatusDesc") { ColumnKey = WebTracker.Grids.TrackingDeclarations.CargoStatusDescription });
			AddToDictionary(new ZDateTimeColumn("Release Date", "Declaration.JE_EntryAuthorisationDate") { ColumnKey = WebTracker.Grids.TrackingDeclarations.ReleaseDate });
			AddToDictionary(new ZTextEditColumn("Release Status Description", "Declaration.ReleaseStatusDesc") { ColumnKey = WebTracker.Grids.TrackingDeclarations.ReleaseStatusDesc });
			AddToDictionary(new ZTextEditColumn("Entry Port", "Declaration.US_SchDEntry") { ColumnKey = WebTracker.Grids.TrackingDeclarations.EntryPort });
			AddToDictionary(new ZTextEditColumn("ENS Status Description", "Declaration.EntrySummaryStatusDesc") { ColumnKey = WebTracker.Grids.TrackingDeclarations.ENSStatusDescription });
			AddToDictionary(new ZTextEditColumn("Paperless", "Declaration.US_PaperlessEntry") { ColumnKey = WebTracker.Grids.TrackingDeclarations.Paperless });
			AddToDictionary(new ZTextEditColumn("Statement Number", "Declaration.StatementNo") { ColumnKey = WebTracker.Grids.TrackingDeclarations.StatementNumber });
			AddToDictionary(new ZTextEditColumn("Filer Code", "Declaration.EntryFilerCode") { ColumnKey = WebTracker.Grids.TrackingDeclarations.FilerCode });
			AddToDictionary(new ZTextEditColumn("Payment Type", "Declaration.US_PaymentType") { ColumnKey = WebTracker.Grids.TrackingDeclarations.PaymentType });
			AddToDictionary(new ZDateTimeColumn("Payment Due Date", "Declaration.US_PaymentDueDate") { ColumnKey = WebTracker.Grids.TrackingDeclarations.PaymentDueDate });
			AddToDictionary(new ZDateTimeColumn("Statement Paid Date", "Declaration.StatementPaidDate") { ColumnKey = WebTracker.Grids.TrackingDeclarations.StatementPaidDate });
			AddToDictionary(new ZTextEditColumn("Payment Status", "Declaration.PaymentStatus") { ColumnKey = WebTracker.Grids.TrackingDeclarations.PaymentStatus });
			AddToDictionary(new ZTextEditColumn("Entry Type", "Declaration.US_EntryType") { ColumnKey = WebTracker.Grids.TrackingDeclarations.EntryType });
			AddToDictionary(new ZTextEditColumn("Periodic Statement Month", "Declaration.US_PeriodicStatementMM") { ColumnKey = WebTracker.Grids.TrackingDeclarations.PeriodicStatementMonth });
			AddToDictionary(new ZCalcEditColumn("Total Entered Value", "Declaration." + AutoJobDeclaration.Schema.US_TotalEnteredValue) { ColumnKey = WebTracker.Grids.TrackingDeclarations.TotalEnteredValue, BindToDecimals = null, Decimals = 0 });
			AddToDictionary(new ZCalcEditColumn("Total Duty & Fees", "Declaration.TotalPayable") { ColumnKey = WebTracker.Grids.TrackingDeclarations.TotalPayable, Decimals = 2 });

			if (WebEnv.AppInstance?.SiteUser is TrackingSiteUser siteUser && siteUser.CanViewAccounts)
			{
				AddToDictionary(new ZCalcEditColumn("Total Invoiced", "Declaration.TotalInvoicedAmount") { ColumnKey = WebTracker.Grids.TrackingDeclarations.TotalInvoiced, BindToDecimals = null });
				AddToDictionary(new ZCalcEditColumn("Total Outstanding", "Declaration.TotalOutstandingAmount") { ColumnKey = WebTracker.Grids.TrackingDeclarations.TotalOutstanding, BindToDecimals = null });
			}

			AddToDictionary(new ZTextEditColumn("OGA FDA Status", "Declaration.FDAStatus") { ColumnKey = WebTracker.Grids.TrackingDeclarations.FDAStatus });
			AddToDictionary(new ZTextEditColumn("OGA FDA Status Description", "Declaration.FDAStatusDescription") { ColumnKey = WebTracker.Grids.TrackingDeclarations.FDAStatusDescription });
			AddToDictionary(new ZDateTimeColumn("Last Audit Date", "Declaration.AuditDate") { ColumnKey = WebTracker.Grids.TrackingDeclarations.AuditDate });
			AddToDictionary(new ZTextEditColumn("EI Status Desc", "Declaration.ElectronicInvoiceStatusDescription") { ColumnKey = WebTracker.Grids.TrackingDeclarations.EIStatusDesc });
			AddToDictionary(new ZTextEditColumn("IT Number", "Declaration.JE_PrimaryITNumber") { ColumnKey = WebTracker.Grids.TrackingDeclarations.ITNumber });
			AddToDictionary(new ZDateTimeColumn("Liquidation Date", "Declaration.LiquidationDate") { ColumnKey = WebTracker.Grids.TrackingDeclarations.LiquidationDate });
			AddToDictionary(new ZTextEditColumn("Recon Issue", "Declaration.US_OtherReconIndicator") { ColumnKey = WebTracker.Grids.TrackingDeclarations.ReconIssue });
			AddToDictionary(new ZTextEditColumn("Recon Issue Desc", "Declaration.OtherReconIndicatorDescription") { ColumnKey = WebTracker.Grids.TrackingDeclarations.ReconIssueDesc });
			AddToDictionary(new ZTextEditColumn("Ultimate Consignee Name", "Declaration.UltimateConsigneeName") { ColumnKey = WebTracker.Grids.TrackingDeclarations.UltimateConsigneeName });
			AddToDictionary(new ZTextEditColumn("PGA Status", "Declaration.PGAStatus") { ColumnKey = WebTracker.Grids.TrackingDeclarations.PGAStatus });
			AddToDictionary(new ZTextEditColumn("PGA Status Description", "Declaration.PGAStatusDesc") { ColumnKey = WebTracker.Grids.TrackingDeclarations.PGAStatusDescription });
			AddToDictionary(new ZTextEditColumn("Carrier SCAC", "Declaration.US_UI_NKCarrierSCAC") { ColumnKey = WebTracker.Grids.TrackingDeclarations.CarrierSCAC });
		}
	}
}
