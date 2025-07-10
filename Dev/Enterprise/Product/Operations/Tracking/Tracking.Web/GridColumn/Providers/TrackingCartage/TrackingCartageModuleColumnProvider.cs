using System.Collections.Generic;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	public class TrackingCartageModuleColumnProvider : GridColumnProvider
	{
		protected override void CustomizeDictionaryCore()
		{
			base.CustomizeDictionaryCore();
			AddToDictionaryAsRequired(new ZHyperLinkColumn(Res.GetString("971a5f34-5483-4ac8-8c11-a8c5f849a29a", "Job ID"), TrackingCartage.Schema.JJ_ConsignmentID)
			{
				ColumnKey = WebTracker.Grids.TrackingCartages.JobID,
				DataNavigateUrlFormatString = (UrlFormatWithAppRoot(TrackingConstants.RelativePath.CartageDetailsPage) + (NoResString)"?Ref={0}"), // Partial URL String
				DataNavigateUrlFields = new[] { "PK" }
			});

			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("2c71b4d3-8732-43de-a445-5d919afc7d00", "Type"), TrackingCartage.Schema.JJ_E3_NKJobType) { ColumnKey = WebTracker.Grids.TrackingCartages.Type });
			AddToDictionary(new ZTextEditColumn(Res.GetString("a21d14f0-05da-4bc1-ab8b-fd2c5a4d469e", "First Address"), "FirstDocAddress+AddressSummary") { ColumnKey = WebTracker.Grids.TrackingCartages.FirstAddress });
			AddToDictionary(new ZTextEditColumn(Res.GetString("126701d9-c05f-43dc-9c75-fda75f4be04e", "Second Address"), "SecondDocAddress+AddressSummary") { ColumnKey = WebTracker.Grids.TrackingCartages.SecondAddress });
			AddToDictionary(new ZTextEditColumn(Res.GetString("8bd53dc7-f91e-4f26-98eb-611b4dffdbb9", "Third Address"), "ThirdDocAddress+AddressSummary") { ColumnKey = WebTracker.Grids.TrackingCartages.ThirdAddress });
			AddToDictionary(new ZTextEditColumn(Res.GetString("276be97e-fb6c-481d-9dad-4b467cfc5acd", "Fourth Address"), "FourthDocAddress+AddressSummary") { ColumnKey = WebTracker.Grids.TrackingCartages.FourthAddress });
			AddToDictionary(new ZTextEditColumn(Res.GetString("96a398db-1a95-4d20-9c0e-a48f5125e0a1", "Local Client"), TrackingCartage.Schema.LocalClientAddressDetailed) { ColumnKey = WebTracker.Grids.TrackingCartages.LocalClient });
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("6b2d65f0-85ac-46c2-ae60-4f44f4206c48", "Ref #"), TrackingCartage.Schema.JJ_OrderReferenceNumber) { ColumnKey = WebTracker.Grids.TrackingCartages.ReferenceNumber });
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("bdcee134-d24f-428e-aacf-118ca7015ab1", "Quote #"), TrackingCartage.Schema.JJ_QuoteNumber) { ColumnKey = WebTracker.Grids.TrackingCartages.QuoteNumber });
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("b7d5d09f-4de8-478e-9db7-aad8abc3a97c", "Waybill #"), TrackingCartage.Schema.JJ_WaybillNumber) { ColumnKey = WebTracker.Grids.TrackingCartages.WaybillNumber });
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("6c39c4bd-92eb-46b6-92b1-6e33d12d14ea", "Description"), TrackingCartage.Schema.JJ_GoodsDescription) { ColumnKey = WebTracker.Grids.TrackingCartages.Description });
			AddToDictionaryAsDefault(new ZDateTimeColumn(Res.GetString("04085c3b-7c44-42fe-b5fd-3007519bbcb3", "Comp. Date"), TrackingCartage.Schema.JJ_A_JCL) { ColumnKey = WebTracker.Grids.TrackingCartages.CompDate });
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("0ad040c7-5f56-44ef-9a31-5a6a8e39704e", "Vessel"), TrackingCartage.Schema.Vessel) { ColumnKey = WebTracker.Grids.TrackingCartages.Vessel });
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("f8959c90-9274-46c6-847c-745ac24e73ec", "Voyage/Flight"), TrackingCartage.Schema.VoyageFlight) { ColumnKey = WebTracker.Grids.TrackingCartages.Voyage });
			AddToDictionaryAsDefault(new ZDateTimeColumn(Res.GetString("df3ef406-017d-48bc-8cea-f81979c9e82b", "Stor."), TrackingCartage.Schema.FCLStorageDate) { ColumnKey = WebTracker.Grids.TrackingCartages.FCLStorageDate });
			AddToDictionaryAsDefault(new ZDateTimeColumn(Res.GetString("c52022f2-cd73-4a2c-9894-c894a1627472", "Avail."), TrackingCartage.Schema.FCLAvailabilityDate) { ColumnKey = WebTracker.Grids.TrackingCartages.FCLAvailabilityDate });
			AddToDictionaryAsDefault(new ZDateTimeColumn(Res.GetString("ac819181-4f48-4581-8829-1f4104abdeea", "Rec. Comm."), TrackingCartage.Schema.FCLReceivalCommences) { ColumnKey = WebTracker.Grids.TrackingCartages.FCLReceivalCommences });
			AddToDictionaryAsDefault(new ZDateTimeColumn(Res.GetString("2df92a1e-8a33-4fa6-bb7b-259d3e96155f", "Cut Off"), TrackingCartage.Schema.FCLCutOff) { ColumnKey = WebTracker.Grids.TrackingCartages.FCLCutOff });
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("4db9a0c6-9a96-4608-a573-745e318a5c71", "Drop Mode"), TrackingCartage.Schema.JJ_DropMode) { ColumnKey = WebTracker.Grids.TrackingCartages.DropMode });
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("2c62edef-7595-4c21-8ed5-e2d0cba7a4bb", "Container"), TrackingCartage.Schema.ContainerNumber) { ColumnKey = WebTracker.Grids.TrackingCartages.ContainerNumber });
			AddToDictionary(new ZTextEditColumn(Res.GetString("ed0a5391-f3fa-4677-aa9c-fb2cbd4ce071", "Service Level"), TrackingCartage.Schema.JJ_RS_NKServiceLevel) { ColumnKey = WebTracker.Grids.TrackingCartages.ServiceLevel });
			AddToDictionary(new ZTextEditColumn(Res.GetString("36ebc1a4-938f-455a-82fc-6d8b22fbe83b", "Job Status"), "Job+JH_Status") { ColumnKey = WebTracker.Grids.TrackingCartages.JobStatus });
			AddToDictionary(new ZDateTimeColumn(Res.GetString("3c9f2c8f-d7af-48de-b43e-1ff3e150d30d", "Sailing ATA"), TrackingCartage.Schema.A_ARV) { ColumnKey = WebTracker.Grids.TrackingCartages.SailingATA });
			AddToDictionary(new ZDateTimeColumn(Res.GetString("39d182c4-3e68-4f13-80d2-c0e1c9084b78", "Sailing ATD"), TrackingCartage.Schema.A_DEP) { ColumnKey = WebTracker.Grids.TrackingCartages.SailingATD });
			AddToDictionary(new ZDateTimeColumn(Res.GetString("274c12a8-5bd4-4d09-9594-663dbe2d206e", "CFS Storage Date"), TrackingCartage.Schema.LCLStorageDate) { ColumnKey = WebTracker.Grids.TrackingCartages.LCLStorageDate });
			AddToDictionary(new ZDateTimeColumn(Res.GetString("90521778-b1be-4c39-860d-9b16dd7f2232", "CFS Availability Date"), TrackingCartage.Schema.LCLAvailabilityDate) { ColumnKey = WebTracker.Grids.TrackingCartages.LCLAvailabilityDate });
			AddToDictionary(new ZDateTimeColumn(Res.GetString("0fdf523b-90ec-4e3f-b098-335108175bc3", "CFS Receival Start"), TrackingCartage.Schema.LCLReceivalCommences) { ColumnKey = WebTracker.Grids.TrackingCartages.LCLReceivalCommences });
			AddToDictionary(new ZDateTimeColumn(Res.GetString("dce64166-7281-4c4c-add1-949f7039b148", "CFS Cutoff"), TrackingCartage.Schema.LCLCutOff) { ColumnKey = WebTracker.Grids.TrackingCartages.LCLCutOff });
			AddToDictionary(new ZDateTimeColumn(Res.GetString("160ed4c3-37d8-4b8c-84a9-cbb6731d4d7c", "Estimated Time of Arrival"), TrackingCartage.Schema.E_ARV) { ColumnKey = WebTracker.Grids.TrackingCartages.EstimatedTimeOfArrival });
			AddToDictionary(new ZDateTimeColumn(Res.GetString("4ab12cfd-85a2-4931-9b1d-b378e59aae15", "Estimated Time of Departure"), TrackingCartage.Schema.E_DEP) { ColumnKey = WebTracker.Grids.TrackingCartages.EstimatedTimeOfDeparture });
		}

		protected override List<int> GetOldColumnsOrder()
		{
			List<int> result = new List<int>();
			result.Add((int)WebTracker.Grids.TrackingCartages.JobID);
			result.Add((int)WebTracker.Grids.TrackingCartages.Type);
			result.Add((int)WebTracker.Grids.TrackingCartages.FirstAddress);
			result.Add((int)WebTracker.Grids.TrackingCartages.SecondAddress);
			result.Add((int)WebTracker.Grids.TrackingCartages.ThirdAddress);
			result.Add((int)WebTracker.Grids.TrackingCartages.FourthAddress);
			result.Add((int)WebTracker.Grids.TrackingCartages.LocalClient);
			result.Add((int)WebTracker.Grids.TrackingCartages.ReferenceNumber);
			result.Add((int)WebTracker.Grids.TrackingCartages.QuoteNumber);
			result.Add((int)WebTracker.Grids.TrackingCartages.WaybillNumber);
			result.Add((int)WebTracker.Grids.TrackingCartages.Description);
			result.Add((int)WebTracker.Grids.TrackingCartages.CompDate);
			result.Add((int)WebTracker.Grids.TrackingCartages.Vessel);
			result.Add((int)WebTracker.Grids.TrackingCartages.Voyage);
			result.Add((int)WebTracker.Grids.TrackingCartages.FCLStorageDate);
			result.Add((int)WebTracker.Grids.TrackingCartages.FCLAvailabilityDate);
			result.Add((int)WebTracker.Grids.TrackingCartages.FCLReceivalCommences);
			result.Add((int)WebTracker.Grids.TrackingCartages.FCLCutOff);
			result.Add((int)WebTracker.Grids.TrackingCartages.DropMode);
			result.Add((int)WebTracker.Grids.TrackingCartages.ContainerNumber);
			result.Add((int)WebTracker.Grids.TrackingCartages.ServiceLevel);
			result.Add((int)WebTracker.Grids.TrackingCartages.JobStatus);
			result.Add((int)WebTracker.Grids.TrackingCartages.SailingATA);
			result.Add((int)WebTracker.Grids.TrackingCartages.SailingATD);
			result.Add((int)WebTracker.Grids.TrackingCartages.LCLStorageDate);
			result.Add((int)WebTracker.Grids.TrackingCartages.LCLAvailabilityDate);
			result.Add((int)WebTracker.Grids.TrackingCartages.LCLReceivalCommences);
			result.Add((int)WebTracker.Grids.TrackingCartages.LCLCutOff);
			result.Add((int)WebTracker.Grids.TrackingCartages.EstimatedTimeOfArrival);
			result.Add((int)WebTracker.Grids.TrackingCartages.EstimatedTimeOfArrival);
			return result;
		}
	}
}
