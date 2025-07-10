using System.Web.UI.WebControls;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Tracking.Business;
using Enterprise.Tracking.Business.ImporterSecurityFiling;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using NUnit.Framework;

namespace Enterprise.Tracking.Web.Testing
{
	[TestedType(typeof(ISFModuleColumnProvider))]
	sealed class ISFModuleColumnProviderTest : GridColumnProviderTest
	{
		protected override DataGridColumn[] GetColumnsForLayoutFixNoDynamicColumns()
		{
			return new DataGridColumn[]
			{
				TestProvider[WebTracker.Grids.TrackingImporterSecurityFilings.FirstAcceptedDate],
				TestProvider[WebTracker.Grids.TrackingImporterSecurityFilings.CreatedTime],
				TestProvider[WebTracker.Grids.TrackingImporterSecurityFilings.JobReference],
				TestProvider[WebTracker.Grids.TrackingImporterSecurityFilings.BuyingParty]
			};
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0002:Simplify Member Access", Justification = "Simplified access could change context here")]
		protected override void SetupColumnsCore()
		{
			base.SetupColumnsCore();
			AddRequiredColumn(new ZHyperLinkColumn("Job Ref.", TrackingCusISFHeader.Schema.BF_JobReference)
			{
				ColumnKey = WebTracker.Grids.TrackingImporterSecurityFilings.JobReference,
				DataNavigateUrlFormatString = TrackingConstants.RelativePath.ISFDetailsPage + "?Ref={0}", // Partial URL String
				DataNavigateUrlFields = new[] { "PK" }
			});
			AddDefaultsColumn(new ZTextEditColumn("Customs Reference", TrackingCusISFHeader.Schema.BF_CustomsReference) { ColumnKey = WebTracker.Grids.TrackingImporterSecurityFilings.CustomsReference });
			AddDefaultsColumn(new ZTextEditColumn("Importer", "Importer+OH_FullName") { ColumnKey = WebTracker.Grids.TrackingImporterSecurityFilings.Importer });
			AddDefaultsColumn(new ZTextEditColumn("House Bill", TrackingCusISFHeader.Schema.BF_HouseBill) { ColumnKey = WebTracker.Grids.TrackingImporterSecurityFilings.HouseBill });
			AddDefaultsColumn(new ZTextEditColumn("Status", TrackingCusISFHeader.Schema.BF_CustomsStatusDescription) { ColumnKey = WebTracker.Grids.TrackingImporterSecurityFilings.Status });
			AddDefaultsColumn(new ZTextEditColumn("Selling Party", "SellingParty+E2_CompanyName") { ColumnKey = WebTracker.Grids.TrackingImporterSecurityFilings.SellingParty });
			AddDefaultsColumn(new ZTextEditColumn("Buying Party", "BuyingParty+E2_CompanyName") { ColumnKey = WebTracker.Grids.TrackingImporterSecurityFilings.BuyingParty });
			AddDefaultsColumn(new ZTextEditColumn("Main Ship To Party", "MainShipToParty+E2_CompanyName") { ColumnKey = WebTracker.Grids.TrackingImporterSecurityFilings.MainShipToParty });
			AddColumn(new ZTextEditColumn("Carrier SCAC", TrackingCusISFHeader.Schema.BF_SCAC) { ColumnKey = WebTracker.Grids.TrackingImporterSecurityFilings.CarrierSCAC });
			AddColumn(new ZTextEditColumn("ID Type", TrackingCusISFHeader.Schema.BF_ImporterCodeType) { ColumnKey = WebTracker.Grids.TrackingImporterSecurityFilings.IDType });
			AddColumn(new ZTextEditColumn("Importer Id Type", TrackingCusISFHeader.Schema.BF_ImporterCodeType) { ColumnKey = WebTracker.Grids.TrackingImporterSecurityFilings.ImporterIDType });
			AddColumn(new ZTextEditColumn("Importer Identification", TrackingCusISFHeader.Schema.BF_ImporterCode) { ColumnKey = WebTracker.Grids.TrackingImporterSecurityFilings.ImporterIdentification });
			AddColumn(new ZDateTimeColumn("DOB", TrackingCusISFHeader.Schema.BF_DateOfBirth, ZDateTimePickerFormat.Short) { ColumnKey = WebTracker.Grids.TrackingImporterSecurityFilings.DateOfBirth });
			AddColumn(new ZTextEditColumn("Issue Ctry/Rgn.", TrackingCusISFHeader.Schema.BF_CountryOfIssue) { ColumnKey = WebTracker.Grids.TrackingImporterSecurityFilings.IssueCountry });
			AddColumn(new ZTextEditColumn("Unload Port", TrackingCusISFHeader.Schema.BF_RL_NKPortOfUnload) { ColumnKey = WebTracker.Grids.TrackingImporterSecurityFilings.UnloadPort });
			AddColumn(new ZTextEditColumn("Delivery Port", TrackingCusISFHeader.Schema.BF_RL_NKPlaceOfDelivery) { ColumnKey = WebTracker.Grids.TrackingImporterSecurityFilings.DeliveryPort });
			AddColumn(new ZTextEditColumn("Bond Holder", TrackingCusISFHeader.Schema.BF_BondNumberOrHolder) { ColumnKey = WebTracker.Grids.TrackingImporterSecurityFilings.BondHolder });
			AddColumn(new ZTextEditColumn("Surety Code", TrackingCusISFHeader.Schema.BF_SuretyCode) { ColumnKey = WebTracker.Grids.TrackingImporterSecurityFilings.SuretyCode });
			AddColumn(new ZTextEditColumn("Cnee. ID Type", TrackingCusISFHeader.Schema.BF_ConsigneeCodeType) { ColumnKey = WebTracker.Grids.TrackingImporterSecurityFilings.ConsigneeIDType });
			AddColumn(new ZTextEditColumn("Cnee. ID", TrackingCusISFHeader.Schema.BF_ConsigneeCode) { ColumnKey = WebTracker.Grids.TrackingImporterSecurityFilings.ConsigneeID });
			AddColumn(new ZTextEditColumn("Ocean Bill", TrackingCusISFHeader.Schema.BF_OceanBill) { ColumnKey = WebTracker.Grids.TrackingImporterSecurityFilings.OceanBill });
			AddColumn(new ZTextEditColumn("Master Bill", TrackingCusISFHeader.Schema.BF_MasterBill) { ColumnKey = WebTracker.Grids.TrackingImporterSecurityFilings.MasterBill });
			AddColumn(new ZTextEditColumn("1st US Route Vessel", "FirstUSTransport+JW_Vessel") { ColumnKey = WebTracker.Grids.TrackingImporterSecurityFilings.FirstUSRouteVessel });
			AddColumn(new ZTextEditColumn("1st US Route Voyage/Flight", "FirstUSTransport+JW_VoyageFlight") { ColumnKey = WebTracker.Grids.TrackingImporterSecurityFilings.FirstUSRouteVoyage });
			AddColumn(new ZTextEditColumn("1st US Route Load Port", "FirstUSTransport+JW_RL_NKLoadPort") { ColumnKey = WebTracker.Grids.TrackingImporterSecurityFilings.FirstUSRouteLoadPort });
			AddColumn(new ZTextEditColumn("1st US Route Discharge Port", "FirstUSTransport+JW_RL_NKDiscPort") { ColumnKey = WebTracker.Grids.TrackingImporterSecurityFilings.FirstUSRouteDischargePort });
			AddColumn(new ZDateTimeColumn("1st US Route ETD", "FirstUSTransport+JW_ETD", ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.TrackingImporterSecurityFilings.FirstUSRouteETD });
			AddColumn(new ZDateTimeColumn("1st US Route ETA", "FirstUSTransport+JW_ETA", ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.TrackingImporterSecurityFilings.FirstUSRouteETA });
			AddColumn(new ZDateTimeColumn("1st US Route ATD", "FirstUSTransport+JW_ATD", ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.TrackingImporterSecurityFilings.FirstUSRouteATD });
			AddColumn(new ZDateTimeColumn("1st US Route ATA", "FirstUSTransport+JW_ATA", ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.TrackingImporterSecurityFilings.FirstUSRouteATA });
			AddDefaultsColumn(new ZTextEditColumn("Action Reason Code", TrackingCusISFHeader.Schema.ActionReasonCodeDescription) { ColumnKey = WebTracker.Grids.TrackingImporterSecurityFilings.ActionReasonCode });
			AddColumn(new ZDateTimeColumn("First Accepted Date", TrackingCusISFHeader.Schema.BF_FirstAcceptedDate, ZDateTimePickerFormat.Short) { ColumnKey = WebTracker.Grids.TrackingImporterSecurityFilings.FirstAcceptedDate });
			AddColumn(new ZDateTimeColumn("Last Accepted Date", TrackingCusISFHeader.Schema.BF_LastAcceptedDate, ZDateTimePickerFormat.Short) { ColumnKey = WebTracker.Grids.TrackingImporterSecurityFilings.LastAcceptedDate });
			AddColumn(new ZDateTimeColumn("Created Time", TrackingCusISFHeader.Schema.BF_SystemCreateTimeUtc, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.TrackingImporterSecurityFilings.CreatedTime });
		}

		protected override GridColumnProvider GetNewTestProvider()
		{
			return new ISFModuleColumnProvider();
		}
	}
}
