using System.Collections.Generic;
using System.Web;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.Modules;

namespace Enterprise.Tracking.Web
{
	public class ScheduleColumnProvider : GridColumnProvider
	{
		ZDateTimeColumn LongZDateTimeColumn(string headerText, string bindTo, object columnKey)
		{
			return new ZDateTimeColumn(headerText, bindTo, ZDateTimePickerFormat.Long) { ColumnKey = columnKey };
		}

		readonly bool isLookup;
		readonly string moduleName;

		public ScheduleColumnProvider(string id, bool isLookup) : base()
		{
			this.moduleName = id;
			this.isLookup = isLookup;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0004:Remove Unnecessary Cast", Justification = "ZBindToChecker requires a redundant cast")]
		protected override void CustomizeDictionaryCore()
		{
			base.CustomizeDictionaryCore();
			string jX_JV_VoyageFlight;
			string jX_DepotCutOff;
			string jX_DepotReceivalCommences;
			string jX_DepotAvailabilityDate;
			string jX_DepotStorageDate;
			string jX_FCLCutOff;
			string jX_FCLReceivalCommences;
			string jX_AvailabilityDate;
			string jX_StorageDate;

			if (moduleName == WebModuleIDs.TrackingSailingSchedules.Name)
			{
				jX_JV_VoyageFlight = Res.GetString("841f5329-4084-4421-b283-70fa13f75c0a", "Voyage");
				jX_DepotCutOff = Res.GetString("a6f34ff8-41d5-4213-b461-372da7b02703", "CFS Cut Off");
				jX_DepotReceivalCommences = Res.GetString("5012c014-80d6-4310-a656-dadbd39d2aa0", "CFS Receival Start");
				jX_DepotAvailabilityDate = Res.GetString("a56e050d-bd5f-4f39-96b0-5d447ddad991", "CFS Avail.");
				jX_DepotStorageDate = Res.GetString("a5f916a2-fb79-4ac0-b7e8-c8cd2d9c364b", "CFS Storage Start");
				jX_FCLCutOff = Res.GetString("8230677d-b12b-4a16-bc7f-2863baa5d70e", "CTO Cut Off");
				jX_FCLReceivalCommences = Res.GetString("cdf972ed-3113-4a68-a4fc-fc06b4b15ec0", "CTO Receival Start");
				jX_AvailabilityDate = Res.GetString("c3660050-81a9-45c2-b3e3-ba5808ad1f9c", "CTO Avail.");
				jX_StorageDate = Res.GetString("905279a9-2668-4ca3-a7e2-0ea9ccc8f411", "CTO Storage Start");
			}
			else if (moduleName == WebModuleIDs.TrackingFlightSchedules.Name)
			{
				jX_JV_VoyageFlight = Res.GetString("8949923f-09b4-4627-beb6-a6f00a271359", "Flight No.");
				jX_DepotCutOff = Res.GetString("3d3ae563-e35f-481d-abaf-5861f068e001", "Loose Cut Off");
				jX_DepotReceivalCommences = Res.GetString("3ff187d0-fc26-46dd-ba81-7f0beae32cd4", "Loose Rec. Start");
				jX_DepotAvailabilityDate = Res.GetString("b7a997bd-5cb2-4de3-9287-bb75c5327661", "Loose Avail.");
				jX_DepotStorageDate = Res.GetString("d2f235b1-ec02-43d3-9076-da2891785c64", "Loose Stor.");
				jX_FCLCutOff = Res.GetString("cdefd631-bab7-4913-96ae-ebfaeb09d715", "ULD Cut Off");
				jX_FCLReceivalCommences = Res.GetString("f04bf8da-8dee-4eed-bff1-6560a88875ff", "ULD Rec. Start");
				jX_AvailabilityDate = Res.GetString("5427f8c3-958a-4652-8b85-3b59f46a3bdc", "ULD Avail.");
				jX_StorageDate = Res.GetString("4138569e-15a2-46a1-be9c-b41e1f7f5ac9", "ULD Stor.");
			}
			else if (moduleName == WebModuleIDs.TrackingRoadSchedules.Name)
			{
				jX_JV_VoyageFlight = Res.GetString("7eeec976-d3a9-45c6-b7e8-f343f44b636b", "Truck Ref.");
				jX_DepotCutOff = Res.GetString("a6f34ff8-41d5-4213-b461-372da7b02703", "CFS Cut Off");
				jX_DepotReceivalCommences = Res.GetString("5012c014-80d6-4310-a656-dadbd39d2aa0", "CFS Receival Start");
				jX_DepotAvailabilityDate = Res.GetString("a56e050d-bd5f-4f39-96b0-5d447ddad991", "CFS Avail.");
				jX_DepotStorageDate = Res.GetString("a5f916a2-fb79-4ac0-b7e8-c8cd2d9c364b", "CFS Storage Start");
				jX_FCLCutOff = Res.GetString("8230677d-b12b-4a16-bc7f-2863baa5d70e", "CTO Cut Off");
				jX_FCLReceivalCommences = Res.GetString("cdf972ed-3113-4a68-a4fc-fc06b4b15ec0", "CTO Receival Start");
				jX_AvailabilityDate = Res.GetString("c3660050-81a9-45c2-b3e3-ba5808ad1f9c", "CTO Avail.");
				jX_StorageDate = Res.GetString("905279a9-2668-4ca3-a7e2-0ea9ccc8f411", "CTO Storage Start");
			}
			else if (moduleName == WebModuleIDs.TrackingRailSchedules.Name)
			{
				jX_JV_VoyageFlight = Res.GetString("bc027a1e-541a-4f2a-9962-4dcdfed61455", "Journey #");
				jX_DepotCutOff = Res.GetString("a6f34ff8-41d5-4213-b461-372da7b02703", "CFS Cut Off");
				jX_DepotReceivalCommences = Res.GetString("5012c014-80d6-4310-a656-dadbd39d2aa0", "CFS Receival Start");
				jX_DepotAvailabilityDate = Res.GetString("a56e050d-bd5f-4f39-96b0-5d447ddad991", "CFS Avail.");
				jX_DepotStorageDate = Res.GetString("a5f916a2-fb79-4ac0-b7e8-c8cd2d9c364b", "CFS Storage Start");
				jX_FCLCutOff = Res.GetString("8230677d-b12b-4a16-bc7f-2863baa5d70e", "CTO Cut Off");
				jX_FCLReceivalCommences = Res.GetString("cdf972ed-3113-4a68-a4fc-fc06b4b15ec0", "CTO Receival Start");
				jX_AvailabilityDate = Res.GetString("c3660050-81a9-45c2-b3e3-ba5808ad1f9c", "CTO Avail.");
				jX_StorageDate = Res.GetString("905279a9-2668-4ca3-a7e2-0ea9ccc8f411", "CTO Storage Start");
			}
			else
			{
				return;
			}

			ZBindToChecker.CheckBindTo((ZString)((JobSailing)null).JX_JV_VoyageFlight);
			ZHyperLinkColumn voyageFlightColumn = new ZHyperLinkColumn(jX_JV_VoyageFlight, JobSailing.Schema.JX_JV_VoyageFlight) { ColumnKey = WebTracker.Grids.TrackingSchedules.Reference };
			if (isLookup)
			{
				voyageFlightColumn.DataNavigateUrlFormatString = (NoResString)@"javascript: parent." + HttpContext.Current.Request.QueryString[ZIFramePage.OKFunctionQuery] + (NoResString)"('{0}','{1}');"; // Javascript code
				voyageFlightColumn.DataNavigateUrlFields = new string[] { JobSailing.Schema.JX_JV_VoyageFlight, "PK" };
			}
			AddToDictionaryAsRequired(voyageFlightColumn);

			if (moduleName == WebModuleIDs.TrackingSailingSchedules.Name || moduleName == WebModuleIDs.TrackingRailSchedules.Name)
			{
				string columnName = moduleName == WebModuleIDs.TrackingSailingSchedules.Name ? Res.GetString("36fcc516-6f97-4ee5-9476-d127c1111838", "Vessel") : Res.GetString("51a495a3-98f3-4ee4-b567-79269550f3ae", "Journey Name");
				ZBindToChecker.CheckBindTo((ZString)((JobSailing)null).JX_JV_NKVessel);
				AddToDictionaryAsDefault(new ZTextEditColumn(columnName, JobSailing.Schema.JX_JV_NKVessel) { ColumnKey = WebTracker.Grids.TrackingSchedules.Vessel });
			}

			ZBindToChecker.CheckBindTo((ICodeDescriptionPairList)((JobSailing)null).Lookups.Ports);
			ZBindToChecker.CheckBindTo((ZString)((JobSailing)null).JX_JA_RL_NKPortOfLoading);
			ZCodeFindBoxColumn loadPortColumn = new ZCodeFindBoxColumn(Res.GetString("38813450-4665-4c5b-b7a9-694dba7756ae", "Load Port"), JobSailing.Schema.JX_JA_RL_NKPortOfLoading, "Lookups.Ports", typeof(JobSailing)) { ColumnKey = WebTracker.Grids.TrackingSchedules.LoadPort };
			loadPortColumn.DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly;
			AddToDictionaryAsDefault(loadPortColumn);

			ZBindToChecker.CheckBindTo((ICodeDescriptionPairList)((JobSailing)null).Lookups.Ports);
			ZBindToChecker.CheckBindTo((ZString)((JobSailing)null).JX_JB_RL_NKPortOfDischarge);
			ZCodeFindBoxColumn dischargePortColumn = new ZCodeFindBoxColumn(Res.GetString("592f62d5-b5bf-499e-9c94-99d584c2cff0", "Discharge Port"), JobSailing.Schema.JX_JB_RL_NKPortOfDischarge, "Lookups.Ports", typeof(JobSailing)) { ColumnKey = WebTracker.Grids.TrackingSchedules.DischargePort };
			dischargePortColumn.DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly;
			AddToDictionaryAsDefault(dischargePortColumn);

			ZBindToChecker.CheckBindTo((ZDateTime)((JobSailing)null).JX_DepotCutOff);
			AddToDictionaryAsDefault(LongZDateTimeColumn(jX_DepotCutOff, JobSailing.Schema.JX_DepotCutOff, WebTracker.Grids.TrackingSchedules.LCLCutOff));

			ZBindToChecker.CheckBindTo((ZDateTime)((JobSailing)null).JX_JA_E_DEP);
			AddToDictionaryAsDefault(LongZDateTimeColumn(Res.GetString("f3562c6a-b3cf-41a3-ba04-b422a50fa609", "ETD"), JobSailing.Schema.JX_JA_E_DEP, WebTracker.Grids.TrackingSchedules.ETD));

			ZBindToChecker.CheckBindTo((ZDateTime)((JobSailing)null).JX_JB_E_ARV);
			AddToDictionaryAsDefault(LongZDateTimeColumn(Res.GetString("c4e85948-fc61-4ab8-abc4-18612966b193", "ETA"), JobSailing.Schema.JX_JB_E_ARV, WebTracker.Grids.TrackingSchedules.ETA));

			ZBindToChecker.CheckBindTo((ZDateTime)((JobSailing)null).JX_DepotAvailabilityDate);
			AddToDictionaryAsDefault(LongZDateTimeColumn(jX_DepotAvailabilityDate, JobSailing.Schema.JX_DepotAvailabilityDate, WebTracker.Grids.TrackingSchedules.LCLAvailabilityDate));

			ZBindToChecker.CheckBindTo((ZDateTime)((JobSailing)null).JX_JA_DocumentaryCutoff);
			AddToDictionaryAsDefault(LongZDateTimeColumn(Res.GetString("3108a34d-1c3b-464c-b496-2873f63cbad1", "Doc. Cutoff"), JobSailing.Schema.JX_JA_DocumentaryCutoff, WebTracker.Grids.TrackingSchedules.DocumentaryCutoff));

			ZBindToChecker.CheckBindTo((ZString)((JobSailing)null).JX_JV_LineName);
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("974535c9-ac82-4443-904f-b1198849eaa5", "Carrier"), JobSailing.Schema.JX_JV_LineName) { ColumnKey = WebTracker.Grids.TrackingSchedules.Carrier });

			ZBindToChecker.CheckBindTo((ZBool)((JobSailing)null).JX_JV_IsChartered);
			AddToDictionary(new ZCheckBoxColumn(Res.GetString("f5911dad-6026-414c-95ff-f146bb9984de", "Chartered"), JobSailing.Schema.JX_JV_IsChartered) { ColumnKey = WebTracker.Grids.TrackingSchedules.Chartered });

			ZBindToChecker.CheckBindTo((ZDateTime)((JobSailing)null).JX_DepotReceivalCommences);
			AddToDictionary(LongZDateTimeColumn(jX_DepotReceivalCommences, JobSailing.Schema.JX_DepotReceivalCommences, WebTracker.Grids.TrackingSchedules.LCLReceivalCommences));

			ZBindToChecker.CheckBindTo((ZDateTime)((JobSailing)null).JX_DepotStorageDate);
			AddToDictionary(LongZDateTimeColumn(jX_DepotStorageDate, JobSailing.Schema.JX_DepotStorageDate, WebTracker.Grids.TrackingSchedules.LCLStorageDate));

			ZBindToChecker.CheckBindTo((ZString)((JobSailing)null).JX_ReservedMasterBill);
			AddToDictionary(new ZTextEditColumn(Res.GetString("b22dca4a-060e-419c-9359-c9b89f35c80e", "Rsrvd. Master"), JobSailing.Schema.JX_ReservedMasterBill) { ColumnKey = WebTracker.Grids.TrackingSchedules.ReservedMasterBill });

			ZBindToChecker.CheckBindTo((ZString)((JobSailing)null).JX_JA_DepartureBerth);
			AddToDictionary(new ZTextEditColumn(Res.GetString("a16561f0-687b-4f31-b82a-7651799461c0", "Departure Berth"), JobSailing.Schema.JX_JA_DepartureBerth) { ColumnKey = WebTracker.Grids.TrackingSchedules.DepartureBerth });

			ZBindToChecker.CheckBindTo((ZString)((JobSailing)null).JX_JB_ArrivalBerth);
			AddToDictionary(new ZTextEditColumn(Res.GetString("a4e9b123-a5c6-41c2-bc48-258e5b2276c3", "Arrival Berth"), JobSailing.Schema.JX_JB_ArrivalBerth) { ColumnKey = WebTracker.Grids.TrackingSchedules.ArrivalBerth });

			ZBindToChecker.CheckBindTo((ZString)((JobSailing)null).JX_JA_DepartureReference);
			AddToDictionary(new ZTextEditColumn(Res.GetString("5de72c61-50f0-4447-9c6b-3ab5760408fa", "Departure Ref."), JobSailing.Schema.JX_JA_DepartureReference) { ColumnKey = WebTracker.Grids.TrackingSchedules.DepartureReference });

			ZBindToChecker.CheckBindTo((ZString)((JobSailing)null).JX_JB_ArrivalReference);
			AddToDictionary(new ZTextEditColumn(Res.GetString("176a2cd0-f315-4251-8649-9eabc1675a43", "Arrival Ref."), JobSailing.Schema.JX_JB_ArrivalReference) { ColumnKey = WebTracker.Grids.TrackingSchedules.ArrivalReference });

			ZBindToChecker.CheckBindTo((ZDateTime)((JobSailing)null).JX_JA_A_DEP);
			AddToDictionary(LongZDateTimeColumn(Res.GetString("59bfd53e-6055-4e76-926a-931ea5161872", "ATD"), JobSailing.Schema.JX_JA_A_DEP, WebTracker.Grids.TrackingSchedules.ATD));

			ZBindToChecker.CheckBindTo((ZDateTime)((JobSailing)null).JX_JB_A_ARV);
			AddToDictionary(LongZDateTimeColumn(Res.GetString("95e5eba9-7afe-4024-9593-f34a04849162", "ATA"), JobSailing.Schema.JX_JB_A_ARV, WebTracker.Grids.TrackingSchedules.ATA));

			ZBindToChecker.CheckBindTo((ZBool)((JobSailing)null).JX_JB_IsTranship);
			AddToDictionary(new ZCheckBoxColumn(Res.GetString("acdddf1d-0981-4f8e-ae49-e4f996f68afc", "T/ship"), JobSailing.Schema.JX_JB_IsTranship) { ColumnKey = WebTracker.Grids.TrackingSchedules.Tranship });

			ZBindToChecker.CheckBindTo((ZString)((JobSailing)null).JX_JV_VoyageType);
			AddToDictionary(new ZTextEditColumn(Res.GetString("7bba79f4-a468-40e3-98b8-91f6ee4bae7b", "Type"), JobSailing.Schema.JX_JV_VoyageType) { ColumnKey = WebTracker.Grids.TrackingSchedules.Type });

			ZBindToChecker.CheckBindTo((ZDateTime)((JobSailing)null).JX_JA_CTOCutOff);
			AddToDictionary(LongZDateTimeColumn(jX_FCLCutOff, JobSailing.Schema.JX_JA_CTOCutOff, WebTracker.Grids.TrackingSchedules.FCLCutOff));

			ZBindToChecker.CheckBindTo((ZDateTime)((JobSailing)null).JX_JA_CTOReceivalCommences);
			AddToDictionary(LongZDateTimeColumn(jX_FCLReceivalCommences, JobSailing.Schema.JX_JA_CTOReceivalCommences, WebTracker.Grids.TrackingSchedules.FCLReceivalCommences));

			ZBindToChecker.CheckBindTo((ZDateTime)((JobSailing)null).JX_JB_CTOAvailabilityDate);
			AddToDictionary(LongZDateTimeColumn(jX_AvailabilityDate, JobSailing.Schema.JX_JB_CTOAvailabilityDate, WebTracker.Grids.TrackingSchedules.FCLAvailabilityDate));

			ZBindToChecker.CheckBindTo((ZDateTime)((JobSailing)null).JX_JB_CTOStorageDate);
			AddToDictionary(LongZDateTimeColumn(jX_StorageDate, JobSailing.Schema.JX_JB_CTOStorageDate, WebTracker.Grids.TrackingSchedules.FCLStorageDate));
		}

		protected override List<int> GetOldColumnsOrder()
		{
			List<int> result = new List<int>();
			result.Add((int)WebTracker.Grids.TrackingSchedules.Reference);
			if (moduleName == WebModuleIDs.TrackingSailingSchedules.Name || moduleName == WebModuleIDs.TrackingRailSchedules.Name)
			{
				result.Add((int)WebTracker.Grids.TrackingSchedules.Vessel);
			}
			result.Add((int)WebTracker.Grids.TrackingSchedules.LoadPort);
			result.Add((int)WebTracker.Grids.TrackingSchedules.DischargePort);
			result.Add((int)WebTracker.Grids.TrackingSchedules.LCLCutOff);
			result.Add((int)WebTracker.Grids.TrackingSchedules.ETD);
			result.Add((int)WebTracker.Grids.TrackingSchedules.ETA);
			result.Add((int)WebTracker.Grids.TrackingSchedules.LCLAvailabilityDate);
			result.Add((int)WebTracker.Grids.TrackingSchedules.DocumentaryCutoff);
			result.Add((int)WebTracker.Grids.TrackingSchedules.Carrier);
			result.Add((int)WebTracker.Grids.TrackingSchedules.Chartered);
			result.Add((int)WebTracker.Grids.TrackingSchedules.LCLReceivalCommences);
			result.Add((int)WebTracker.Grids.TrackingSchedules.LCLStorageDate);
			result.Add((int)WebTracker.Grids.TrackingSchedules.ReservedMasterBill);
			result.Add((int)WebTracker.Grids.TrackingSchedules.DepartureBerth);
			result.Add((int)WebTracker.Grids.TrackingSchedules.ArrivalBerth);
			result.Add((int)WebTracker.Grids.TrackingSchedules.DepartureReference);
			result.Add((int)WebTracker.Grids.TrackingSchedules.ArrivalReference);
			result.Add((int)WebTracker.Grids.TrackingSchedules.ATD);
			result.Add((int)WebTracker.Grids.TrackingSchedules.ATA);
			result.Add((int)WebTracker.Grids.TrackingSchedules.Tranship);
			result.Add((int)WebTracker.Grids.TrackingSchedules.Type);
			result.Add((int)WebTracker.Grids.TrackingSchedules.FCLCutOff);
			result.Add((int)WebTracker.Grids.TrackingSchedules.FCLReceivalCommences);
			result.Add((int)WebTracker.Grids.TrackingSchedules.FCLAvailabilityDate);
			result.Add((int)WebTracker.Grids.TrackingSchedules.FCLStorageDate);
			return result;
		}
	}
}
