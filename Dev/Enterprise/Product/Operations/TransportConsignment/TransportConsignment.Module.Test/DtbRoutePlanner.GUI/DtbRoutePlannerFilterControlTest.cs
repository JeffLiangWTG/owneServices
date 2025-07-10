using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportCommon.Integration;
using Enterprise.TransportConsignment.Business;
using Enterprise.TransportConsignment.Business.Testing;
using Enterprise.TransportConsignment.Integration;
using Enterprise.TransportConsignment.Module;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.TransportConsignment.GUI.Testing
{
	public class DtbRoutePlannerFilterControlTest : DtbBookingConsignmentTestCaseWithFactory
	{
		#region TestHasSearchBeenRun

		public void TestHasSearchBeenRun()
		{
			using (var form = GetNewFormWithFilterControl())
			{
				form.Show();
				AssertEquals(false, form.FilterControl.HasSearchBeenRun);

				form.FilterControl.ViewMode = DtbRoutePlannerViewMode.NextAvailable;
				AssertEquals(true, form.FilterControl.HasSearchBeenRun);
			}
		}

		#endregion

		#region TestPerformSearch_FiltersChildGrids

		public void TestPerformSearch_FiltersChildGrids()
		{
			using (var form = GetNewFormWithFilterControl())
			{
				form.FilterControl.ViewMode = DtbRoutePlannerViewMode.Pickups;
				var driver1 = Helper.CreateDriver("BOB", "BOB");
				var driver2 = Helper.CreateDriver("JIM", "JIM");
				var driver3 = Helper.CreateDriver("TOM", "TOM");
				var driver4 = Helper.CreateDriver("BEN", "BEN");
				var driversGroup = Helper.CreateDriverGroup("Drivers", driver1, driver2, driver3, driver4);
				var transportRegistry = ObjectFactory.Get<ITransportRegistry>();
				transportRegistry.TransportDriversGroup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, driversGroup.PK.ToGuid());

				var branch1 = Factory.NewWithValidTestData<GlbBranch>();
				var branch2 = Factory.NewWithValidTestData<GlbBranch>();
				var branch3 = Factory.NewWithValidTestData<GlbBranch>();
				branch1.GB_GC = GlbCompany.CurrentCompany.PK;
				branch2.GB_GC = GlbCompany.CurrentCompany.PK;
				branch3.GB_GC = GlbCompany.CurrentCompany.PK;

				driver1.GS_GB_HomeBranch = branch1.PK;
				driver2.GS_GB_HomeBranch = branch2.PK;
				driver3.GS_GB_HomeBranch = branch3.PK;
				driver4.GS_GB_HomeBranch = branch3.PK;
				Factory.Save();

				var driverFilterControl = new DtbDriverFilterControl();
				form.Controls.Add(driverFilterControl);
				form.FilterControl.AddChildFilterControl(driverFilterControl);
				form.Show();

				form.FilterControl.FirePerformSearch();
				AssertContainsExactElementsInAnyOrder(new[] { driver1.PK, driver2.PK, driver3.PK, driver4.PK }, ((GlbStaffCollection)driverFilterControl.GridCollection).Select(d => d.PK));

				var branchFilter = (ModuleGuidFilter)form.FilterControl.FilterBusinessObject[DtbDriverFilterBusinessObject.FilterConstants.Branch];
				AssertNotNull("Driver Filters should appear along the Confirmation Filters on the 'parent' FilterControl", branchFilter);

				branchFilter.IsActive = true;
				branchFilter.Property = branch3.PK;
				driverFilterControl.Visible = false;
				form.FilterControl.FirePerformSearch();
				// Should not Filter non visible Grid
				AssertContainsExactElementsInAnyOrder(new[] { driver1.PK, driver2.PK, driver3.PK, driver4.PK }, ((GlbStaffCollection)driverFilterControl.GridCollection).Select(d => d.PK));

				driverFilterControl.Visible = true;
				form.FilterControl.FirePerformSearch();
				AssertContainsExactElementsInAnyOrder(new[] { driver3.PK, driver4.PK }, ((GlbStaffCollection)driverFilterControl.GridCollection).Select(d => d.PK));
			}
		}

		#endregion

		#region TestSetViewModeWithoutRunningSearch

		public void TestSetViewModeWithoutRunningSearch()
		{
			using (var form = GetNewFormWithFilterControl())
			{
				form.Show();
				AssertEquals("Precondition", DtbRoutePlannerViewMode.None, form.FilterControl.ViewMode);
				AssertEquals("Precondition", false, form.FilterControl.HasSearchBeenRun);

				form.FilterControl.SetViewModeWithoutRunningSearch(DtbRoutePlannerViewMode.NextAvailable);
				AssertEquals(DtbRoutePlannerViewMode.NextAvailable, form.FilterControl.ViewMode);
				AssertEquals(false, form.FilterControl.HasSearchBeenRun);
			}
		}

		#endregion

		#region TestSetViewMode

		public void TestSetViewMode()
		{
			using (var form = GetNewFormWithFilterControl())
			{
				form.Show();
				form.FilterControl.ViewMode = DtbRoutePlannerViewMode.Deliveries;
				AssertGridColumnsContains(directDeliverySpecificColumnNames, form.FilterControl.Grid.Columns, contains: false);

				form.FilterControl.ViewMode = DtbRoutePlannerViewMode.Pickups;
				AssertGridColumnsContains(directDeliverySpecificColumnNames, form.FilterControl.Grid.Columns, contains: false);

				form.FilterControl.ViewMode = DtbRoutePlannerViewMode.NextAvailable;
				AssertGridColumnsContains(directDeliverySpecificColumnNames, form.FilterControl.Grid.Columns, contains: false);

				form.FilterControl.ViewMode = DtbRoutePlannerViewMode.Direct;
				AssertGridColumnsContains(directDeliverySpecificColumnNames, form.FilterControl.Grid.Columns, contains: true);
			}
		}

		void AssertGridColumnsContains(string[] expectedColumns, ZGridColumns actualColumns, bool contains)
		{
			var actualColumnNamesSet = new HashSet<string>(actualColumns.Select(c => c.ColumnName));
			var expectedColumnNamesSet = new HashSet<string>(expectedColumns);
			AssertEquals(contains, expectedColumnNamesSet.IsSubsetOf(actualColumnNamesSet));
		}

		readonly string[] directDeliverySpecificColumnNames = new[] { "DirectDeliveryAddressCompanyName", "DirectDeliveryAddressLine1", "DirectDeliveryAddressCity", "DirectDeliveryAddressState", "DirectDeliveryAddressPostCode" };

		#endregion

		#region TestCanSaveColumnLayouts

		public void TestCanSaveColumnLayouts()
		{
			using (var form = GetNewFormWithFilterControl())
			{
				AssertEquals("Should not be able to save column layouts due to there being two grids.", false, form.FilterControl.CanSaveColumnLayouts);
			}
		}

		#endregion

		#region TestPerformingAndPerformedSearch

		public void TestPerformingAndPerformedSearch()
		{
			bool performingSearchFired = false;
			bool performedSearchFired = false;

			using (var form = GetNewFormWithFilterControl())
			{
				form.FilterControl.PerformingSearch += delegate
				{
					performingSearchFired = true;
					AssertEquals(false, form.FilterControl.HasSearchBeenRun);
				};

				form.FilterControl.PerformedSearch += delegate
				{
					performedSearchFired = true;
					AssertEquals(true, form.FilterControl.HasSearchBeenRun);
				};

				form.Show();
				form.FilterControl.SetViewModeWithoutRunningSearch(DtbRoutePlannerViewMode.NextAvailable);
				form.FilterControl.FirePerformSearch();
			}

			AssertEquals(true, performingSearchFired);
			AssertEquals(true, performedSearchFired);
		}

		#endregion

		#region Implementation

		DtbRoutePlannerFilterControlFormForTest GetNewFormWithFilterControl()
		{
			return new DtbRoutePlannerFilterControlFormForTest(DtbRoutePlannerCollection.New(Factory));
		}

		#region DtbRoutePlannerFilterControlFormForTest

		class DtbRoutePlannerFilterControlFormForTest : ZForm
		{
			public DtbRoutePlannerFilterControlFormForTest(DtbRoutePlannerCollection planners)
				: base(planners)
			{
				FilterControl = new DtbRoutePlannerFilterControlForTest();
				Controls.Add(FilterControl);
			}

			public DtbRoutePlannerFilterControlForTest FilterControl
			{
				get;
				private set;
			}

			public DtbRoutePlannerCollection Planners
			{
				get { return (DtbRoutePlannerCollection)DataSource; }
			}

			public DtbRoutePlanner Planner
			{
				get { return Planners.Cast<DtbRoutePlanner>().FirstOrDefault(); }
			}
		}

		class DtbRoutePlannerFilterControlForTest : DtbRoutePlannerFilterControl
		{
			public new bool CanSaveColumnLayouts
			{
				get { return base.CanSaveColumnLayouts; }
			}

			public ZLabel ToolStripRecordsFoundLabelForTest
			{
				get { return base.ToolStripRecordsFoundLabel; }
			}
		}

		#endregion

		#endregion
	}
}
