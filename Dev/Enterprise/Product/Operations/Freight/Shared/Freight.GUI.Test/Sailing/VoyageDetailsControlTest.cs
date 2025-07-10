using System.Drawing;
using System.Linq;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.CarbonEmissions.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.GUI.Testing
{
	sealed class VoyageDetailsControlTest : BaseFreightTest
	{
		public void TestIsArchivedDisabledWithoutSecurityRights()
		{
			Env.Security.SailingScheduleManuallyArchive.IsAllowed = false;

			using (var form = new VoyageDetailsControlTestForm(Factory.New<JobVoyage>()))
			{
				var checkBox = form.Controls.Find("IsArchivedCheckBox", true)[0];
				Assert(!checkBox.Enabled);
			}

			Env.Security.SailingScheduleManuallyArchive.IsAllowed = true;

			using (var form = new VoyageDetailsControlTestForm(Factory.New<JobVoyage>()))
			{
				var checkBox = form.Controls.Find("IsArchivedCheckBox", true)[0];
				Assert(checkBox.Enabled);
			}
		}

		public void TestSetDecimalsForExchangeRate()
		{
			GlbCompany.CurrentCompany.GC_IsReciprocal = false;
			using (VoyageDetailsControlTestForm form = new VoyageDetailsControlTestForm(Factory.New<JobVoyage>()))
			{
				form.Show();
				AssertEquals("6 decimals when current login company not flagged as 'is reciprocal'", 6, form.ExRateColumnStyle.Decimals);
			}

			GlbCompany.CurrentCompany.GC_IsReciprocal = true;
			using (VoyageDetailsControlTestForm form = new VoyageDetailsControlTestForm(Factory.New<JobVoyage>()))
			{
				form.Show();
				AssertEquals("6 decimals when current login company flagged as 'is reciprocal'", 6, form.ExRateColumnStyle.Decimals);
			}
		}

		[GuiTest]
		public void TestVoyageType()
		{
			JobVoyage voyage = Factory.NewWithValidTestData<JobVoyage>();

			voyage.JV_AirSeaRoad = Constants.TransportModes.Air;
			using (VoyageDetailsControlTestForm form = new VoyageDetailsControlTestForm(voyage))
			{
				form.Show();
				ZDropEdit dropEdit = (ZDropEdit)form.Controls.Find("jv_VoyageTypeDropEdit", true)[0];
				AssertEquals("Should be invisible", false, dropEdit.Visible);

				var aircraftTypeEdit = form.Controls.Find("jv_AircraftTypeBoundTextEdit", true).FirstOrDefault();
				AssertNotNull(aircraftTypeEdit);
				AssertEquals("Aircraft Type should be visible", true, aircraftTypeEdit.Visible);
				AssertEquals("Aircraft Type", aircraftTypeEdit.GetExtension<ILabelCaptionRenderer>().Caption);
			}

			voyage.JV_AirSeaRoad = Constants.TransportModes.Rail;
			using (VoyageDetailsControlTestForm form = new VoyageDetailsControlTestForm(voyage))
			{
				form.Show();
				ZDropEdit dropEdit = (ZDropEdit)form.Controls.Find("jv_VoyageTypeDropEdit", true)[0];
				AssertEquals("Should be invisible", false, dropEdit.Visible);

				var aircraftTypeEdit = form.Controls.Find("jv_AircraftTypeBoundTextEdit", true).FirstOrDefault();
				AssertNotNull(aircraftTypeEdit);
				AssertEquals("Aircraft Type should be invisible", false, aircraftTypeEdit.Visible);
			}

			voyage.JV_AirSeaRoad = Constants.TransportModes.Road;
			using (VoyageDetailsControlTestForm form = new VoyageDetailsControlTestForm(voyage))
			{
				form.Show();
				ZDropEdit dropEdit = (ZDropEdit)form.Controls.Find("jv_VoyageTypeDropEdit", true)[0];
				AssertEquals("Should be invisible", false, dropEdit.Visible);

				var aircraftTypeEdit = form.Controls.Find("jv_AircraftTypeBoundTextEdit", true).FirstOrDefault();
				AssertNotNull(aircraftTypeEdit);
				AssertEquals("Aircraft Type should be invisible", false, aircraftTypeEdit.Visible);
			}

			voyage.JV_AirSeaRoad = Constants.TransportModes.Sea;
			using (VoyageDetailsControlTestForm form = new VoyageDetailsControlTestForm(voyage))
			{
				form.Show();
				ZDropEdit dropEdit = (ZDropEdit)form.Controls.Find("jv_VoyageTypeDropEdit", true)[0];
				AssertEquals("Should be visible", true, dropEdit.Visible);

				var aircraftTypeEdit = form.Controls.Find("jv_AircraftTypeBoundTextEdit", true).FirstOrDefault();
				AssertNotNull(aircraftTypeEdit);
				AssertEquals("Aircraft Type should be invisible", false, aircraftTypeEdit.Visible);
			}
		}

		public void TestVoyageType_Captions()
		{
			var voyage = Factory.NewWithValidTestData<JobVoyage>();

			voyage.JV_AirSeaRoad = Constants.TransportModes.Rail;
			using (var form = new VoyageDetailsControlTestForm(voyage))
			{
				form.Show();
				var journeyNameTextBox = (ZTextBox)form.Controls.Find("jv_VesselJourneyNameTextBox", true)[0];
				AssertEquals("Full Description should match", journeyNameTextBox.CaptionResourceString.FullDescription, "The name of the Rail Journey that this Voyage will be on.");
				AssertEquals("Caption should match", journeyNameTextBox.CaptionResourceString.Caption, "Journey");

				var journeyNumberTextBox = (ZTextBox)form.Controls.Find("JV_JourneyNoBoundTextEdit", true)[0];
				AssertEquals("Full Description should match", journeyNumberTextBox.CaptionResourceString.FullDescription, "The Rail Journey reference number.");
				AssertEquals("Caption should match", journeyNumberTextBox.CaptionResourceString.Caption, "Journey No.");
			}

			voyage.JV_AirSeaRoad = Constants.TransportModes.Road;
			using (var form = new VoyageDetailsControlTestForm(voyage))
			{
				form.Show();
				var journeyNumberTextBox = (ZTextBox)form.Controls.Find("JV_TruckBoundTextEdit", true)[0];
				AssertEquals("Full Description should match", journeyNumberTextBox.CaptionResourceString.FullDescription, "The Truck Journey reference number.");
				AssertEquals("Caption should match", journeyNumberTextBox.CaptionResourceString.Caption, "Truck Ref.");
			}
		}

		public void TestVendorDataStatusLabel_OnlyVisibleWhenVendorDataIntegrationEnabled()
		{
			var voyage = Factory.NewWithValidTestData<JobVoyage>();

			MockSailingScheduleDataVendor.RegisterThisSubTypeOverride();
			MockSailingScheduleDataVendor.Instance.IsEnabled = false;
			try
			{
				using (var form = new VoyageDetailsControlTestForm(voyage))
				{
					form.Show();
					AssertEquals("No label when vendor data integration not enabled", "", form.VendorDataStatusLabel.Text);
				}
			}
			finally
			{
				MockSailingScheduleDataVendor.UnregisterThisSubTypeOverride();
				MockSailingScheduleDataVendor.Instance.IsEnabled = true;
			}
		}

		public void TestVendorDataStatusLabel_WhenVendorDataCurrent()
		{
			var voyage = Factory.NewWithValidTestData<JobVoyage>();

			MockSailingScheduleDataVendor.RegisterThisSubTypeOverride();
			try
			{
				using (var form = new VoyageDetailsControlTestForm(voyage))
				{
					MockSailingScheduleDataVendor.Instance.IsVendorDataCurrent = true;
					form.Show();
					AssertEquals("Vendor data current", form.VendorDataStatusLabel.Text);
					AssertEquals("Font colour should be normal", form.ForeColor, form.VendorDataStatusLabel.ForeColor);
				}
			}
			finally
			{
				MockSailingScheduleDataVendor.UnregisterThisSubTypeOverride();
			}
		}

		public void TestVendorDataStatusLabel_WhenVendorDataOutOfDate()
		{
			var voyage = Factory.NewWithValidTestData<JobVoyage>();

			MockSailingScheduleDataVendor.RegisterThisSubTypeOverride();
			try
			{
				using (var form = new VoyageDetailsControlTestForm(voyage))
				{
					MockSailingScheduleDataVendor.Instance.IsVendorDataCurrent = false;
					form.Show();
					AssertEquals("Vendor data out of date", form.VendorDataStatusLabel.Text);
					AssertEquals("Font colour should be Red", Color.Red, form.VendorDataStatusLabel.ForeColor);
					AssertEquals("Font should be Bold", true, form.VendorDataStatusLabel.Font.Bold);
				}
			}
			finally
			{
				MockSailingScheduleDataVendor.UnregisterThisSubTypeOverride();
			}
		}

		public void TestVendorDataStatusLabel_OnlyVisibleForSea()
		{
			var voyage = Factory.NewWithValidTestData<JobVoyage>();

			MockSailingScheduleDataVendor.RegisterThisSubTypeOverride();
			try
			{
				using (var form = new VoyageDetailsControlTestForm(voyage))
				{
					voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Air;
					form.Show();
					AssertEquals("No label if a non-sea voyage shown", "", form.VendorDataStatusLabel.Text);
				}
			}
			finally
			{
				MockSailingScheduleDataVendor.UnregisterThisSubTypeOverride();
			}
		}

		public void TestJobVoyOriginBoundGrid_ColumnExists()
		{
			var voyage = Factory.NewWithValidTestData<JobVoyage>();
			using (var form = new VoyageDetailsControlTestForm(voyage))
			{
				form.Show();
				var jobVoyOriginBoundGrid = (ZGrid)form.Controls.Find("jobVoyOriginBoundGrid", true)[0];
				AssertNotNull(jobVoyOriginBoundGrid);
				AssertGridColumn(jobVoyOriginBoundGrid, JobVoyOriginSchema.JA_S_ARV.Name, false);
				AssertGridColumn(jobVoyOriginBoundGrid, JobVoyOriginSchema.JA_S_DEP.Name, false);
			}
		}

		public void TestJobVoyDestinationBoundGrid_ColumnExists()
		{
			var voyage = Factory.NewWithValidTestData<JobVoyage>();
			using (var form = new VoyageDetailsControlTestForm(voyage))
			{
				form.Show();
				var jobVoyDestinationBoundGrid = (ZGrid)form.Controls.Find("jobVoyDestinationBoundGrid", true)[0];
				AssertNotNull(jobVoyDestinationBoundGrid);
				AssertGridColumn(jobVoyDestinationBoundGrid, JobVoyDestinationSchema.JB_S_ARV.Name, false);
				AssertGridColumn(jobVoyDestinationBoundGrid, JobVoyDestinationSchema.JB_RL_NKLastForeignPort.Name, true);
				AssertGridColumn(jobVoyDestinationBoundGrid, JobVoyDestinationSchema.JB_LastForeignPortETD.Name, true);
				AssertGridColumn(jobVoyDestinationBoundGrid, JobVoyDestinationSchema.JB_RL_NKFirstDischargePort.Name, true);
				AssertGridColumn(jobVoyDestinationBoundGrid, JobVoyDestinationSchema.JB_FirstDischargePortETA.Name, true);
			}
		}

		public void TestExRateGrid_ColumnExists()
		{
			var voyage = Factory.NewWithValidTestData<JobVoyage>();
			using (var form = new VoyageDetailsControlTestForm(voyage))
			{
				form.Show();
				var exRatesGrid = (ZGrid)form.Controls.Find("exRatesGrid", true)[0];
				AssertNotNull(exRatesGrid);
				AssertGridColumn(exRatesGrid, JobVoyageExRateSchema.E8_RX_NKExCurrency.Name, true);
				AssertGridColumn(exRatesGrid, JobVoyageExRateSchema.E8_VoyageExchangeRate.Name, true);
				AssertGridColumn(exRatesGrid, JobVoyageExRateSchema.E8_RL_NKPort.Name, true);
			}
		}

		public void TestGlobalScheduleMatchingColumnOnlyShownForAir()
		{
			var voyage = Factory.NewWithValidTestData<JobVoyage>();

			voyage.JV_AirSeaRoad = Constants.TransportModes.Rail;
			using (var form = new VoyageDetailsControlTestForm(voyage))
			{
				form.Show();
				var jobSailingBoundGrid = (SailingsGrid)form.Controls.Find("JobRailBoundGrid", true)[0];
				Assert("The voyage does not have 'Air' transport mode and so should not show the Global Schedule Matching Status (JX_OnlineScheduleStatus).", !jobSailingBoundGrid.Columns.Contains(JobSailing.Schema.JX_OnlineScheduleStatus));
			}

			voyage.JV_AirSeaRoad = Constants.TransportModes.Air;
			using (var form = new VoyageDetailsControlTestForm(voyage))
			{
				form.Show();
				var jobSailingBoundGrid = (SailingsGrid)form.Controls.Find("JobFlightBoundGrid", true)[0];
				Assert("The voyage has 'Air' transport mode and so should show the Global Schedule Matching Status (JX_OnlineScheduleStatus).", jobSailingBoundGrid.Columns.Contains(JobSailing.Schema.JX_OnlineScheduleStatus));
			}
		}

		public void TestGlobalScheduleMatchingContextMenuItemOnlyShownForAir()
		{
			var voyage = Factory.NewWithValidTestData<JobVoyage>();

			voyage.JV_AirSeaRoad = Constants.TransportModes.Rail;
			using (var form = new VoyageDetailsControlTestForm(voyage))
			{
				form.Show();
				AssertNull("The voyage does not have 'Air' transport mode and so should not show the flight validation context menu item.", form.SailingsGrid.ContextMenu.MenuItems.FindByText("Validate Flight against Global Flight Schedule"));
			}

			voyage.JV_AirSeaRoad = Constants.TransportModes.Air;
			using (var form = new VoyageDetailsControlTestForm(voyage))
			{
				form.Show();
				AssertNotNull("The voyage has 'Air' transport mode and so should show the flight validation context menu item.", form.SailingsGrid.ContextMenu.MenuItems.FindByText("Validate Flight against Global Flight Schedule"));
			}
		}

		[MasterFiles.Integration.Test.MatchAgainstOnlineFlightsInUnitTest]
		public void TestGlobalScheduleMatchingContextMenuItemFunctionality()
		{
			OnlineFlightMatchingHelperTest.RunTestWithMockedS8Matcher(() =>
			{
				var voyage = Factory.New<JobVoyage>();
				voyage.JV_VoyageFlight = "QF11";

				var origin = voyage.Origins.AddNew();
				origin.JA_RL_NKPortOfLoading = "AUSYD";
				origin.JA_E_DEP = ZDate.Today;

				var destination = voyage.Destinations.AddNew();
				destination.JB_RL_NKPortOfDischarge = "USJFK";
				destination.JB_E_ARV = ZDate.Today.AddDays(1);

				voyage.JV_AirSeaRoad = Constants.TransportModes.Air;

				Factory.Save();

				using (var form = new VoyageDetailsControlTestForm(voyage))
				{
					form.Show();

					AssertEquals("Precondition: One sailing row has been generated", voyage.Sailings.Count, 1);
					form.SailingsGrid.PerformMouseDownForTest(0, 1);

					var menuItem = form.SailingsGrid.ContextMenu.MenuItems.FindByText("Validate Flight against Global Flight Schedule");
					AssertNotNull("Precondition: Menu item is visible", menuItem);

					menuItem.PerformClick();

					AssertEquals("Flight is validated and status set", Constants.FlightScheduleStatus.Matched, voyage.Sailings[0].JX_OnlineScheduleStatus);
				}
			});
		}

		public void TestVoyageDetailsGrid_CO2e()
		{
			var voyage = Factory.NewWithValidTestData<JobVoyage>();

			using (CO2eTestHelper.MockCO2eFeatureControl(false))
			using (var form = new VoyageDetailsControlTestForm(voyage))
			{
				form.Show();
				var jobSailingBoundGrid = (ZGrid)form.Controls.Find("jobSailingBoundGrid", true)[0];
				var cO2eColumn = jobSailingBoundGrid.Columns.FirstOrDefault(x => x.ColumnName == "CO2ePerTonneInKgForBinding");
				AssertNull(cO2eColumn);
			}

			using (CO2eTestHelper.MockCO2eFeatureControl(true))
			using (var form = new VoyageDetailsControlTestForm(voyage))
			{
				form.Show();
				var jobSailingBoundGrid = (ZGrid)form.Controls.Find("jobSailingBoundGrid", true)[0];
				AssertGridColumn(jobSailingBoundGrid, "CO2ePerTonneInKgForBinding", true);
			}
		}

		public void TestJobSailingBoundGrid_ColumnExists()
		{
			var voyage = Factory.NewWithValidTestData<JobVoyage>();

			using (CO2eTestHelper.MockCO2eFeatureControl(false))
			using (var form = new VoyageDetailsControlTestForm(voyage))
			{
				form.Show();
				var jobSailingBoundGrid = (ZGrid)form.Controls.Find("jobSailingBoundGrid", true)[0];
				AssertGridColumn(jobSailingBoundGrid, "JX_UniqueReference", true);
				AssertGridColumn(jobSailingBoundGrid, "JX_JA_RL_NKPortOfLoading", true);
				AssertGridColumn(jobSailingBoundGrid, "JX_JB_RL_NKPortOfDischarge", true);
				AssertGridColumn(jobSailingBoundGrid, "JX_IsPublished", true);
				AssertGridColumn(jobSailingBoundGrid, "JX_ReservedMasterBill", true);
				AssertGridColumn(jobSailingBoundGrid, "JX_DepotReceivalCommences", true);
				AssertGridColumn(jobSailingBoundGrid, "JX_DepotCutOff", true);
				AssertGridColumn(jobSailingBoundGrid, "JX_DepotAvailabilityDate", true);
				AssertGridColumn(jobSailingBoundGrid, "JX_DepotStorageDate", true);
				AssertGridColumn(jobSailingBoundGrid, "JX_ServiceString", true);
				AssertGridColumn(jobSailingBoundGrid, "JX_ArrivalPortRouteId", false);
				AssertGridColumn(jobSailingBoundGrid, "JX_DeparturePortRouteId", false);
			}
		}

		void AssertGridColumn(ZGrid grid, string columnName, bool isVisible)
		{
			var column = grid.Columns.FirstOrDefault(x => x.ColumnName == columnName);
			AssertNotNull(column);
			AssertEquals(columnName + ": IsVisible", isVisible, column.IsVisible);
		}
	}
}
