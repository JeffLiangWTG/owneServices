using System.Web;
using System.Web.UI.WebControls;
using Enterprise.Freight.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.Modules;
using NUnit.Framework;

namespace Enterprise.Tracking.Web.Testing
{
	[TestedType(typeof(ScheduleColumnProvider))]
	[HttpContextEnabledTest]
	sealed class ScheduleColumnProviderTest : GridColumnProviderTest
	{
		#region Test Cases

		public override void TestFixOldLayout()
		{
			isLookup = false;
			moduleName = WebModuleIDs.TrackingSailingSchedules.Name;
			TestProvider = GetNewTestProvider();
			TestProvider.CustomizeDictionary();
			SetupColumns();
			base.TestFixOldLayout();

			moduleName = WebModuleIDs.TrackingFlightSchedules.Name;
			TestProvider = GetNewTestProvider();
			TestProvider.CustomizeDictionary();
			SetupColumns();
			base.TestFixOldLayout();

			moduleName = WebModuleIDs.TrackingRoadSchedules.Name;
			TestProvider = GetNewTestProvider();
			TestProvider.CustomizeDictionary();
			SetupColumns();
			base.TestFixOldLayout();

			moduleName = WebModuleIDs.TrackingRailSchedules.Name;
			TestProvider = GetNewTestProvider();
			TestProvider.CustomizeDictionary();
			SetupColumns();
			base.TestFixOldLayout();

			isLookup = true;
			moduleName = WebModuleIDs.TrackingSailingSchedules.Name;
			TestProvider = GetNewTestProvider();
			TestProvider.CustomizeDictionary();
			SetupColumns();
			base.TestFixOldLayout();

			moduleName = WebModuleIDs.TrackingFlightSchedules.Name;
			TestProvider = GetNewTestProvider();
			TestProvider.CustomizeDictionary();
			SetupColumns();
			base.TestFixOldLayout();

			moduleName = WebModuleIDs.TrackingRoadSchedules.Name;
			TestProvider = GetNewTestProvider();
			TestProvider.CustomizeDictionary();
			SetupColumns();
			base.TestColumnKeys();

			moduleName = WebModuleIDs.TrackingRailSchedules.Name;
			TestProvider = GetNewTestProvider();
			TestProvider.CustomizeDictionary();
			SetupColumns();
			base.TestFixOldLayout();
		}

		protected override DataGridColumn[] GetColumnsForLayoutFixAllDynamicColumns()
		{
			return ExpectedColumns.ToArray();
		}

		public override void TestColumnKeys()
		{
			isLookup = false;
			moduleName = WebModuleIDs.TrackingSailingSchedules.Name;
			TestProvider = GetNewTestProvider();
			TestProvider.CustomizeDictionary();
			SetupColumns();
			base.TestColumnKeys();

			moduleName = WebModuleIDs.TrackingFlightSchedules.Name;
			TestProvider = GetNewTestProvider();
			TestProvider.CustomizeDictionary();
			SetupColumns();
			base.TestColumnKeys();

			moduleName = WebModuleIDs.TrackingRoadSchedules.Name;
			TestProvider = GetNewTestProvider();
			TestProvider.CustomizeDictionary();
			SetupColumns();
			base.TestColumnKeys();

			moduleName = WebModuleIDs.TrackingRailSchedules.Name;
			TestProvider = GetNewTestProvider();
			TestProvider.CustomizeDictionary();
			SetupColumns();
			base.TestColumnKeys();

			isLookup = true;
			moduleName = WebModuleIDs.TrackingSailingSchedules.Name;
			TestProvider = GetNewTestProvider();
			TestProvider.CustomizeDictionary();
			SetupColumns();
			base.TestColumnKeys();

			moduleName = WebModuleIDs.TrackingFlightSchedules.Name;
			TestProvider = GetNewTestProvider();
			TestProvider.CustomizeDictionary();
			SetupColumns();
			base.TestColumnKeys();

			moduleName = WebModuleIDs.TrackingRoadSchedules.Name;
			TestProvider = GetNewTestProvider();
			TestProvider.CustomizeDictionary();
			SetupColumns();
			base.TestColumnKeys();

			moduleName = WebModuleIDs.TrackingRailSchedules.Name;
			TestProvider = GetNewTestProvider();
			TestProvider.CustomizeDictionary();
			SetupColumns();
			base.TestColumnKeys();
		}

		public override void TestDefaultColumns()
		{
			isLookup = false;
			moduleName = WebModuleIDs.TrackingSailingSchedules.Name;
			TestProvider = GetNewTestProvider();
			TestProvider.CustomizeDictionary();
			SetupColumns();
			base.TestDefaultColumns();

			moduleName = WebModuleIDs.TrackingFlightSchedules.Name;
			TestProvider = GetNewTestProvider();
			TestProvider.CustomizeDictionary();
			SetupColumns();
			base.TestDefaultColumns();

			moduleName = WebModuleIDs.TrackingRoadSchedules.Name;
			TestProvider = GetNewTestProvider();
			TestProvider.CustomizeDictionary();
			SetupColumns();
			base.TestDefaultColumns();

			moduleName = WebModuleIDs.TrackingRailSchedules.Name;
			TestProvider = GetNewTestProvider();
			TestProvider.CustomizeDictionary();
			SetupColumns();
			base.TestDefaultColumns();

			isLookup = true;
			moduleName = WebModuleIDs.TrackingSailingSchedules.Name;
			TestProvider = GetNewTestProvider();
			TestProvider.CustomizeDictionary();
			SetupColumns();
			base.TestDefaultColumns();

			moduleName = WebModuleIDs.TrackingFlightSchedules.Name;
			TestProvider = GetNewTestProvider();
			TestProvider.CustomizeDictionary();
			SetupColumns();
			base.TestDefaultColumns();

			moduleName = WebModuleIDs.TrackingRoadSchedules.Name;
			TestProvider = GetNewTestProvider();
			TestProvider.CustomizeDictionary();
			SetupColumns();
			base.TestDefaultColumns();

			moduleName = WebModuleIDs.TrackingRailSchedules.Name;
			TestProvider = GetNewTestProvider();
			TestProvider.CustomizeDictionary();
			SetupColumns();
			base.TestDefaultColumns();
		}

		public override void TestRequiredColumns()
		{
			isLookup = false;
			moduleName = WebModuleIDs.TrackingSailingSchedules.Name;
			TestProvider = GetNewTestProvider();
			TestProvider.CustomizeDictionary();
			SetupColumns();
			base.TestRequiredColumns();

			moduleName = WebModuleIDs.TrackingFlightSchedules.Name;
			TestProvider = GetNewTestProvider();
			TestProvider.CustomizeDictionary();
			SetupColumns();
			base.TestRequiredColumns();

			moduleName = WebModuleIDs.TrackingRoadSchedules.Name;
			TestProvider = GetNewTestProvider();
			TestProvider.CustomizeDictionary();
			SetupColumns();
			base.TestRequiredColumns();

			moduleName = WebModuleIDs.TrackingRailSchedules.Name;
			TestProvider = GetNewTestProvider();
			TestProvider.CustomizeDictionary();
			SetupColumns();
			base.TestRequiredColumns();

			isLookup = true;
			moduleName = WebModuleIDs.TrackingSailingSchedules.Name;
			TestProvider = GetNewTestProvider();
			TestProvider.CustomizeDictionary();
			SetupColumns();
			base.TestRequiredColumns();

			moduleName = WebModuleIDs.TrackingFlightSchedules.Name;
			TestProvider = GetNewTestProvider();
			TestProvider.CustomizeDictionary();
			SetupColumns();
			base.TestRequiredColumns();

			moduleName = WebModuleIDs.TrackingRoadSchedules.Name;
			TestProvider = GetNewTestProvider();
			TestProvider.CustomizeDictionary();
			SetupColumns();
			base.TestRequiredColumns();

			moduleName = WebModuleIDs.TrackingRailSchedules.Name;
			TestProvider = GetNewTestProvider();
			TestProvider.CustomizeDictionary();
			SetupColumns();
			base.TestRequiredColumns();
		}

		public override void TestUniqueColumns()
		{
			isLookup = false;
			moduleName = WebModuleIDs.TrackingSailingSchedules.Name;
			TestProvider = GetNewTestProvider();
			TestProvider.CustomizeDictionary();
			SetupColumns();
			base.TestUniqueColumns();

			moduleName = WebModuleIDs.TrackingFlightSchedules.Name;
			TestProvider = GetNewTestProvider();
			TestProvider.CustomizeDictionary();
			SetupColumns();
			base.TestUniqueColumns();

			moduleName = WebModuleIDs.TrackingRoadSchedules.Name;
			TestProvider = GetNewTestProvider();
			TestProvider.CustomizeDictionary();
			SetupColumns();
			base.TestUniqueColumns();

			moduleName = WebModuleIDs.TrackingRailSchedules.Name;
			TestProvider = GetNewTestProvider();
			TestProvider.CustomizeDictionary();
			SetupColumns();
			base.TestUniqueColumns();

			isLookup = true;
			moduleName = WebModuleIDs.TrackingSailingSchedules.Name;
			TestProvider = GetNewTestProvider();
			TestProvider.CustomizeDictionary();
			SetupColumns();
			base.TestUniqueColumns();

			moduleName = WebModuleIDs.TrackingFlightSchedules.Name;
			TestProvider = GetNewTestProvider();
			TestProvider.CustomizeDictionary();
			SetupColumns();
			base.TestUniqueColumns();

			moduleName = WebModuleIDs.TrackingRoadSchedules.Name;
			TestProvider = GetNewTestProvider();
			TestProvider.CustomizeDictionary();
			SetupColumns();
			base.TestUniqueColumns();

			moduleName = WebModuleIDs.TrackingRailSchedules.Name;
			TestProvider = GetNewTestProvider();
			TestProvider.CustomizeDictionary();
			SetupColumns();
			base.TestUniqueColumns();
		}

		#endregion

		ZDateTimeColumn LongZDateTimeColumn(string headerText, string bindTo, object columnKey)
		{
			return new ZDateTimeColumn(headerText, bindTo, ZDateTimePickerFormat.Long) { ColumnKey = columnKey };
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0002:Simplify Member Access", Justification = "Simplified access could change context here")]
		protected override void SetupColumnsCore()
		{
			base.SetupColumnsCore();
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
				jX_JV_VoyageFlight = "Voyage";
				jX_DepotCutOff = "CFS Cut Off";
				jX_DepotReceivalCommences = "CFS Receival Start";
				jX_DepotAvailabilityDate = "CFS Avail.";
				jX_DepotStorageDate = "CFS Storage Start";
				jX_FCLCutOff = "CTO Cut Off";
				jX_FCLReceivalCommences = "CTO Receival Start";
				jX_AvailabilityDate = "CTO Avail.";
				jX_StorageDate = "CTO Storage Start";
			}
			else if (moduleName == WebModuleIDs.TrackingFlightSchedules.Name)
			{
				jX_JV_VoyageFlight = "Flight No.";
				jX_DepotCutOff = "Loose Cut Off";
				jX_DepotReceivalCommences = "Loose Rec. Start";
				jX_DepotAvailabilityDate = "Loose Avail.";
				jX_DepotStorageDate = "Loose Stor.";
				jX_FCLCutOff = "ULD Cut Off";
				jX_FCLReceivalCommences = "ULD Rec. Start";
				jX_AvailabilityDate = "ULD Avail.";
				jX_StorageDate = "ULD Stor.";
			}
			else if (moduleName == WebModuleIDs.TrackingRoadSchedules.Name)
			{
				jX_JV_VoyageFlight = "Truck Ref.";
				jX_DepotCutOff = "CFS Cut Off";
				jX_DepotReceivalCommences = "CFS Receival Start";
				jX_DepotAvailabilityDate = "CFS Avail.";
				jX_DepotStorageDate = "CFS Storage Start";
				jX_FCLCutOff = "CTO Cut Off";
				jX_FCLReceivalCommences = "CTO Receival Start";
				jX_AvailabilityDate = "CTO Avail.";
				jX_StorageDate = "CTO Storage Start";
			}
			else if (moduleName == WebModuleIDs.TrackingRailSchedules.Name)
			{
				jX_JV_VoyageFlight = "Journey #";
				jX_DepotCutOff = "CFS Cut Off";
				jX_DepotReceivalCommences = "CFS Receival Start";
				jX_DepotAvailabilityDate = "CFS Avail.";
				jX_DepotStorageDate = "CFS Storage Start";
				jX_FCLCutOff = "CTO Cut Off";
				jX_FCLReceivalCommences = "CTO Receival Start";
				jX_AvailabilityDate = "CTO Avail.";
				jX_StorageDate = "CTO Storage Start";
			}
			else
			{
				return;
			}

			ZHyperLinkColumn voyageFlightColumn = new ZHyperLinkColumn(jX_JV_VoyageFlight, JobSailing.Schema.JX_JV_VoyageFlight) { ColumnKey = WebTracker.Grids.TrackingSchedules.Reference };
			if (isLookup)
			{
				voyageFlightColumn.DataNavigateUrlFormatString = @"javascript: parent." + HttpContext.Current.Request.QueryString[ZIFramePage.OKFunctionQuery] + "('{0}','{1}');"; // Javascript code
				voyageFlightColumn.DataNavigateUrlFields = new string[] { JobSailing.Schema.JX_JV_VoyageFlight, "PK" };
			}
			AddRequiredColumn(voyageFlightColumn);

			if (moduleName == WebModuleIDs.TrackingSailingSchedules.Name || moduleName == WebModuleIDs.TrackingRailSchedules.Name)
			{
				AddDefaultsColumn(new ZTextEditColumn(moduleName == WebModuleIDs.TrackingSailingSchedules.Name ? "Vessel" : "Journey Name", JobSailing.Schema.JX_JV_NKVessel) { ColumnKey = WebTracker.Grids.TrackingSchedules.Vessel });
			}

			AddDefaultsColumn(new ZCodeFindBoxColumn("Load Port", JobSailing.Schema.JX_JA_RL_NKPortOfLoading, "Lookups.Ports", typeof(JobSailing))
			{
				ColumnKey = WebTracker.Grids.TrackingSchedules.LoadPort,
				DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly
			});

			AddDefaultsColumn(new ZCodeFindBoxColumn("Discharge Port", JobSailing.Schema.JX_JB_RL_NKPortOfDischarge, "Lookups.Ports", typeof(JobSailing))
			{
				ColumnKey = WebTracker.Grids.TrackingSchedules.DischargePort,
				DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly
			});

			AddDefaultsColumn(LongZDateTimeColumn(jX_DepotCutOff, JobSailing.Schema.JX_DepotCutOff, WebTracker.Grids.TrackingSchedules.LCLCutOff));
			AddDefaultsColumn(LongZDateTimeColumn("ETD", JobSailing.Schema.JX_JA_E_DEP, WebTracker.Grids.TrackingSchedules.ETD));
			AddDefaultsColumn(LongZDateTimeColumn("ETA", JobSailing.Schema.JX_JB_E_ARV, WebTracker.Grids.TrackingSchedules.ETA));
			AddDefaultsColumn(LongZDateTimeColumn(jX_DepotAvailabilityDate, JobSailing.Schema.JX_DepotAvailabilityDate, WebTracker.Grids.TrackingSchedules.LCLAvailabilityDate));
			AddDefaultsColumn(LongZDateTimeColumn("Doc. Cutoff", JobSailing.Schema.JX_JA_DocumentaryCutoff, WebTracker.Grids.TrackingSchedules.DocumentaryCutoff));
			AddDefaultsColumn(new ZTextEditColumn("Carrier", JobSailing.Schema.JX_JV_LineName) { ColumnKey = WebTracker.Grids.TrackingSchedules.Carrier });
			AddColumn(new ZCheckBoxColumn("Chartered", JobSailing.Schema.JX_JV_IsChartered) { ColumnKey = WebTracker.Grids.TrackingSchedules.Chartered });
			AddColumn(LongZDateTimeColumn(jX_DepotReceivalCommences, JobSailing.Schema.JX_DepotReceivalCommences, WebTracker.Grids.TrackingSchedules.LCLReceivalCommences));
			AddColumn(LongZDateTimeColumn(jX_DepotStorageDate, JobSailing.Schema.JX_DepotStorageDate, WebTracker.Grids.TrackingSchedules.LCLStorageDate));
			AddColumn(new ZTextEditColumn("Rsrvd. Master", JobSailing.Schema.JX_ReservedMasterBill) { ColumnKey = WebTracker.Grids.TrackingSchedules.ReservedMasterBill });
			AddColumn(new ZTextEditColumn("Departure Berth", JobSailing.Schema.JX_JA_DepartureBerth) { ColumnKey = WebTracker.Grids.TrackingSchedules.DepartureBerth });
			AddColumn(new ZTextEditColumn("Arrival Berth", JobSailing.Schema.JX_JB_ArrivalBerth) { ColumnKey = WebTracker.Grids.TrackingSchedules.ArrivalBerth });
			AddColumn(new ZTextEditColumn("Departure Ref.", JobSailing.Schema.JX_JA_DepartureReference) { ColumnKey = WebTracker.Grids.TrackingSchedules.DepartureReference });
			AddColumn(new ZTextEditColumn("Arrival Ref.", JobSailing.Schema.JX_JB_ArrivalReference) { ColumnKey = WebTracker.Grids.TrackingSchedules.ArrivalReference });
			AddColumn(LongZDateTimeColumn("ATD", JobSailing.Schema.JX_JA_A_DEP, WebTracker.Grids.TrackingSchedules.ATD));
			AddColumn(LongZDateTimeColumn("ATA", JobSailing.Schema.JX_JB_A_ARV, WebTracker.Grids.TrackingSchedules.ATA));
			AddColumn(new ZCheckBoxColumn("T/ship", JobSailing.Schema.JX_JB_IsTranship) { ColumnKey = WebTracker.Grids.TrackingSchedules.Tranship });
			AddColumn(new ZTextEditColumn("Type", JobSailing.Schema.JX_JV_VoyageType) { ColumnKey = WebTracker.Grids.TrackingSchedules.Type });
			AddColumn(LongZDateTimeColumn(jX_FCLCutOff, JobSailing.Schema.JX_JA_CTOCutOff, WebTracker.Grids.TrackingSchedules.FCLCutOff));
			AddColumn(LongZDateTimeColumn(jX_FCLReceivalCommences, JobSailing.Schema.JX_JA_CTOReceivalCommences, WebTracker.Grids.TrackingSchedules.FCLReceivalCommences));
			AddColumn(LongZDateTimeColumn(jX_AvailabilityDate, JobSailing.Schema.JX_JB_CTOAvailabilityDate, WebTracker.Grids.TrackingSchedules.FCLAvailabilityDate));
			AddColumn(LongZDateTimeColumn(jX_StorageDate, JobSailing.Schema.JX_JB_CTOStorageDate, WebTracker.Grids.TrackingSchedules.FCLStorageDate));
		}

		protected override GridColumnProvider GetNewTestProvider()
		{
			return new ScheduleColumnProvider(moduleName, isLookup);
		}

		protected override void SetUp()
		{
			isLookup = false;
			moduleName = WebModuleIDs.TrackingSailingSchedules.Name;
			base.SetUp();
		}

		bool isLookup;
		string moduleName;
	}
}
