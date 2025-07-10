using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.CarbonEmissions.Business.Testing;
using Enterprise.Freight.Forwarding.Routing.S8.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.Routing.S8.GUI.Test
{
	class RealTimeRoutingFilterControlTest : TestCaseWithFactory
	{
		public void TestDontSearchOnLoad()
		{
			EnvProxy.Instance.Registry.RunSearchOnEnteringAModule = true;
			bool searchRun = false;

			using (TestRealTimeRoutingForm form = new TestRealTimeRoutingForm(new RoutingManager(Factory)))
			{
				TestRealTimeRoutingFilterControl filterControl = (TestRealTimeRoutingFilterControl)form.FilterControl;
				((ModuleNkFilter)filterControl.FilterBusinessObject["Origin"]).Property = "AUSYD";
				((ModuleNkFilter)filterControl.FilterBusinessObject["Destination"]).Property = "GBLON";
				((ModuleSingleDateFilter)filterControl.FilterBusinessObject["Departure Date"]).Property1 = ZDateTime.Today;

				filterControl.PerformSearch += delegate
				{ searchRun = true; };
				form.Show();
				AssertEquals(false, searchRun);

				filterControl.Find();
				AssertEquals(true, searchRun);
			}
		}

		public void TestRoutingLinesGridBound()
		{
			using (TestRealTimeRoutingForm form = new TestRealTimeRoutingForm(new RoutingManager(Factory)))
			{
				TestRealTimeRoutingFilterControl filterControl = (TestRealTimeRoutingFilterControl)form.FilterControl;
				form.Show();
				AssertNotNull("RoutingLinesGrid.DataSource set", filterControl.RoutingLinesGrid.DataSource);
			}
		}

		public void TestFilterdGridColumns()
		{
			using (CO2eTestHelper.MockCO2eFeatureControl(true))
			{
				using (FreightDataRegistry.Instance.EnableGlobalFlightSchedulesCalculation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				using (var form = new TestRealTimeRoutingForm(new RoutingManager(Factory)))
				{
					var filterControl = (TestRealTimeRoutingFilterControl)form.FilterControl;
					form.Show();

					TestGridColumns(filterControl.FilteredGrid, "OperationDay", "Days of the Week", true);
					TestGridColumns(filterControl.FilteredGrid, "Duration", "Duration", true);
					TestGridColumns(filterControl.FilteredGrid, "Flight", "Flight", false);
					TestGridColumns(filterControl.FilteredGrid, "FlightType", "Flight Type", false);
					TestGridColumns(filterControl.FilteredGrid, "Aircraft", "Aircraft", false);
					TestGridColumns(filterControl.FilteredGrid, "Via", "Via", true);
					TestGridColumns(filterControl.FilteredGrid, "CO2EmissionTotal", "CO2e (kg/t)", true);
				}

				using (FreightDataRegistry.Instance.EnableGlobalFlightSchedulesCalculation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				using (var form = new TestRealTimeRoutingForm(new RoutingManager(Factory)))
				{
					var filterControl = (TestRealTimeRoutingFilterControl)form.FilterControl;
					form.Show();

					AssertNull(filterControl.FilteredGrid.GetColumnStyle("CO2EmissionTotal"));
				}
			}

			using (CO2eTestHelper.MockCO2eFeatureControl(false))
			using (var form = new TestRealTimeRoutingForm(new RoutingManager(Factory)))
			{
				var filterControl = (TestRealTimeRoutingFilterControl)form.FilterControl;
				form.Show();

				AssertNull(filterControl.FilteredGrid.GetColumnStyle("CO2EmissionTotal"));
			}
		}

		public void TestRoutingLinesGridColumns()
		{
			using (CO2eTestHelper.MockCO2eFeatureControl(true))
			{
				using (FreightDataRegistry.Instance.EnableGlobalFlightSchedulesCalculation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				using (var form = new TestRealTimeRoutingForm(new RoutingManager(Factory)))
				{
					var filterControl = (TestRealTimeRoutingFilterControl)form.FilterControl;
					form.Show();

					TestGridColumns(filterControl.RoutingLinesGrid, "OperationDay", "Days of the Week", true);
					TestGridColumns(filterControl.RoutingLinesGrid, "EffectiveDate", "Effective Date", true);
					TestGridColumns(filterControl.RoutingLinesGrid, "DiscontinuedDate", "Discontinued Date", true);
					TestGridColumns(filterControl.RoutingLinesGrid, "FlightType", "Flight Type", true);
					TestGridColumns(filterControl.RoutingLinesGrid, "Aircraft", "Aircraft", true);
					TestGridColumns(filterControl.RoutingLinesGrid, "CO2Emission", "CO2e (kg/t)", true);
				}

				using (FreightDataRegistry.Instance.EnableGlobalFlightSchedulesCalculation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				using (var form = new TestRealTimeRoutingForm(new RoutingManager(Factory)))
				{
					var filterControl = (TestRealTimeRoutingFilterControl)form.FilterControl;
					form.Show();

					AssertNull(filterControl.RoutingLinesGrid.GetColumnStyle("CO2Emission"));
				}
			}

			using (CO2eTestHelper.MockCO2eFeatureControl(false))
			using (var form = new TestRealTimeRoutingForm(new RoutingManager(Factory)))
			{
				var filterControl = (TestRealTimeRoutingFilterControl)form.FilterControl;
				form.Show();

				AssertNull(filterControl.RoutingLinesGrid.GetColumnStyle("CO2Emission"));
			}
		}

		#region Test Include Weekly Timetable Column

		public void TestIncludeWeeklyColumnIsIncluded()
		{
			var manager = new RoutingManager(Factory);
			manager.IncludeWeeklyTimetable = true;
			var filterBizo = new RoutingRequestFilterStripBusinessObject(manager);

			using (var control = new RealTimeRoutingFilterControl(null, filterBizo))
			{
				control.FirePerformSearch();

				var displayGrid = (ZDisplayGrid)GetControl(control, "FilteredGrid");
				var daysOfWeekColumn = displayGrid.Columns.FirstOrDefault(c => c.ColumnName == "OperationDay");

				AssertNotNull("Days Of Week Column should exist", daysOfWeekColumn);
				AssertEquals("Days Of Week Column should be mandatory", true, daysOfWeekColumn.IsMandatory);
				AssertEquals("Days Of Week Column should be visible", true, daysOfWeekColumn.IsVisible);
			}
		}

		public void TestIncludeWeeklyColumnIsExcluded()
		{
			var manager = new RoutingManager(Factory);
			manager.IncludeWeeklyTimetable = false;
			var filterBizo = new RoutingRequestFilterStripBusinessObject(manager);

			using (var control = new RealTimeRoutingFilterControl(null, filterBizo))
			{
				control.FirePerformSearch();

				var displayGrid = (ZDisplayGrid)GetControl(control, "FilteredGrid");
				var daysOfWeekColumn = displayGrid.Columns.FirstOrDefault(c => c.ColumnName == "OperationDay");

				AssertNull("Days Of Week Column should not exist", daysOfWeekColumn);
			}
		}

		Control GetControl(Control form, string name)
		{
			var matchingControls = form.Controls.Find(name, true);
			if (matchingControls.Length > 1)
			{
				Fail("More than 1 control was found with the same name. Ensure this method returns the right one. ControlName: " + name);
			}
			return matchingControls.FirstOrDefault();
		}

		#endregion

		#region Implementation

		void TestGridColumns(ZGrid grid, string columnName, string caption, bool isVisible)
		{
			var columnStyle = grid.GetColumnStyle(columnName);
			AssertNotNull(columnStyle);
			AssertNotNull(columnStyle.CaptionResourceString);
			AssertEquals(caption, columnStyle.CaptionResourceString.Caption);
			AssertEquals(isVisible, columnStyle.IsVisible);
		}

		#endregion

		#region Test Classes

		class TestRealTimeRoutingForm : RealTimeRoutingForm
		{
			public TestRealTimeRoutingForm(RoutingManager routingManager)
				: base(routingManager)
			{
			}

			protected override RealTimeRoutingFilterControl NewFilterControl()
			{
				return new TestRealTimeRoutingFilterControl(Manager.Routings, Filter);
			}
		}

		class TestRealTimeRoutingFilterControl : RealTimeRoutingFilterControl
		{
			public TestRealTimeRoutingFilterControl(RoutingResponseHeaderCollection responseHeaders, RoutingRequestFilterStripBusinessObject filterBizo)
				: base(responseHeaders, filterBizo)
			{
			}

			public new ZGrid RoutingLinesGrid
			{
				get { return base.RoutingLinesGrid; }
			}
		}

		#endregion
	}
}
