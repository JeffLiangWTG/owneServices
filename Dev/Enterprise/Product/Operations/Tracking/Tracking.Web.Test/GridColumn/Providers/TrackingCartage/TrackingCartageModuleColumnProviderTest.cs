using System.Web.UI.WebControls;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using NUnit.Framework;

namespace Enterprise.Tracking.Web.Testing
{
	[TestedType(typeof(TrackingCartageModuleColumnProvider))]
	sealed class TrackingCartageModuleColumnProviderTest : GridColumnProviderTest
	{
		#region Implementation

		protected override DataGridColumn[] GetColumnsForLayoutFixNoDynamicColumns()
		{
			return new DataGridColumn[]
			{
				TestProvider[WebTracker.Grids.TrackingCartages.SailingATD],
				TestProvider[WebTracker.Grids.TrackingCartages.FirstAddress],
				TestProvider[WebTracker.Grids.TrackingCartages.ServiceLevel],
				TestProvider[WebTracker.Grids.TrackingCartages.EstimatedTimeOfArrival],
				TestProvider[WebTracker.Grids.TrackingCartages.JobID]
			};
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0002:Simplify Member Access", Justification = "Simplified access could change context here")]
		protected override void SetupColumnsCore()
		{
			base.SetupColumnsCore();
			AddRequiredColumn(new ZHyperLinkColumn("Job ID", TrackingCartage.Schema.JJ_ConsignmentID)
			{
				ColumnKey = WebTracker.Grids.TrackingCartages.JobID,
				DataNavigateUrlFormatString = TrackingConstants.RelativePath.CartageDetailsPage + "?Ref={0}", // Partial URL String
				DataNavigateUrlFields = new[] { "PK" }
			});

			AddDefaultsColumn(new ZTextEditColumn("Type", TrackingCartage.Schema.JJ_E3_NKJobType) { ColumnKey = WebTracker.Grids.TrackingCartages.Type });
			AddColumn(new ZTextEditColumn("First Address", "FirstDocAddress+AddressSummary") { ColumnKey = WebTracker.Grids.TrackingCartages.FirstAddress });
			AddColumn(new ZTextEditColumn("Second Address", "SecondDocAddress+AddressSummary") { ColumnKey = WebTracker.Grids.TrackingCartages.SecondAddress });
			AddColumn(new ZTextEditColumn("Third Address", "ThirdDocAddress+AddressSummary") { ColumnKey = WebTracker.Grids.TrackingCartages.ThirdAddress });
			AddColumn(new ZTextEditColumn("Fourth Address", "FourthDocAddress+AddressSummary") { ColumnKey = WebTracker.Grids.TrackingCartages.FourthAddress });
			AddColumn(new ZTextEditColumn("Local Client", TrackingCartage.Schema.LocalClientAddressDetailed) { ColumnKey = WebTracker.Grids.TrackingCartages.LocalClient });
			AddDefaultsColumn(new ZTextEditColumn("Ref #", TrackingCartage.Schema.JJ_OrderReferenceNumber) { ColumnKey = WebTracker.Grids.TrackingCartages.ReferenceNumber });
			AddDefaultsColumn(new ZTextEditColumn("Quote #", TrackingCartage.Schema.JJ_QuoteNumber) { ColumnKey = WebTracker.Grids.TrackingCartages.QuoteNumber });
			AddDefaultsColumn(new ZTextEditColumn("Waybill #", TrackingCartage.Schema.JJ_WaybillNumber) { ColumnKey = WebTracker.Grids.TrackingCartages.WaybillNumber });
			AddDefaultsColumn(new ZTextEditColumn("Description", TrackingCartage.Schema.JJ_GoodsDescription) { ColumnKey = WebTracker.Grids.TrackingCartages.Description });
			AddDefaultsColumn(new ZDateTimeColumn("Comp. Date", TrackingCartage.Schema.JJ_A_JCL) { ColumnKey = WebTracker.Grids.TrackingCartages.CompDate });
			AddDefaultsColumn(new ZTextEditColumn("Vessel", TrackingCartage.Schema.Vessel) { ColumnKey = WebTracker.Grids.TrackingCartages.Vessel });
			AddDefaultsColumn(new ZTextEditColumn("Voyage/Flight", TrackingCartage.Schema.VoyageFlight) { ColumnKey = WebTracker.Grids.TrackingCartages.Voyage });
			AddDefaultsColumn(new ZDateTimeColumn("Stor.", TrackingCartage.Schema.FCLStorageDate) { ColumnKey = WebTracker.Grids.TrackingCartages.FCLStorageDate });
			AddDefaultsColumn(new ZDateTimeColumn("Avail.", TrackingCartage.Schema.FCLAvailabilityDate) { ColumnKey = WebTracker.Grids.TrackingCartages.FCLAvailabilityDate });
			AddDefaultsColumn(new ZDateTimeColumn("Rec. Comm.", TrackingCartage.Schema.FCLReceivalCommences) { ColumnKey = WebTracker.Grids.TrackingCartages.FCLReceivalCommences });
			AddDefaultsColumn(new ZDateTimeColumn("Cut Off", TrackingCartage.Schema.FCLCutOff) { ColumnKey = WebTracker.Grids.TrackingCartages.FCLCutOff });
			AddDefaultsColumn(new ZTextEditColumn("Drop Mode", TrackingCartage.Schema.JJ_DropMode) { ColumnKey = WebTracker.Grids.TrackingCartages.DropMode });
			AddDefaultsColumn(new ZTextEditColumn("Container", TrackingCartage.Schema.ContainerNumber) { ColumnKey = WebTracker.Grids.TrackingCartages.ContainerNumber });
			AddColumn(new ZTextEditColumn("Service Level", TrackingCartage.Schema.JJ_RS_NKServiceLevel) { ColumnKey = WebTracker.Grids.TrackingCartages.ServiceLevel });
			AddColumn(new ZTextEditColumn("Job Status", "Job+JH_Status") { ColumnKey = WebTracker.Grids.TrackingCartages.JobStatus });
			AddColumn(new ZDateTimeColumn("Sailing ATA", TrackingCartage.Schema.A_ARV) { ColumnKey = WebTracker.Grids.TrackingCartages.SailingATA });
			AddColumn(new ZDateTimeColumn("Sailing ATD", TrackingCartage.Schema.A_DEP) { ColumnKey = WebTracker.Grids.TrackingCartages.SailingATD });
			AddColumn(new ZDateTimeColumn("CFS Storage Date", TrackingCartage.Schema.LCLStorageDate) { ColumnKey = WebTracker.Grids.TrackingCartages.LCLStorageDate });
			AddColumn(new ZDateTimeColumn("CFS Availability Date", TrackingCartage.Schema.LCLAvailabilityDate) { ColumnKey = WebTracker.Grids.TrackingCartages.LCLAvailabilityDate });
			AddColumn(new ZDateTimeColumn("CFS Receival Start", TrackingCartage.Schema.LCLReceivalCommences) { ColumnKey = WebTracker.Grids.TrackingCartages.LCLReceivalCommences });
			AddColumn(new ZDateTimeColumn("CFS Cutoff", TrackingCartage.Schema.LCLCutOff) { ColumnKey = WebTracker.Grids.TrackingCartages.LCLCutOff });
			AddColumn(new ZDateTimeColumn("Estimated Time of Arrival", TrackingCartage.Schema.E_ARV) { ColumnKey = WebTracker.Grids.TrackingCartages.EstimatedTimeOfArrival });
			AddColumn(new ZDateTimeColumn("Estimated Time of Departure", TrackingCartage.Schema.E_DEP) { ColumnKey = WebTracker.Grids.TrackingCartages.EstimatedTimeOfDeparture });
		}

		protected override GridColumnProvider GetNewTestProvider()
		{
			return new TrackingCartageModuleColumnProvider();
		}

		#endregion
	}
}
