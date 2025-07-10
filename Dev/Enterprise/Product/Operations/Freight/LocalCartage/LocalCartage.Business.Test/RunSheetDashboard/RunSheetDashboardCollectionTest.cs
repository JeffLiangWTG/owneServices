using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Freight.LocalCartage.Business.Testing
{
	[TestedType(typeof(RunSheetDashboardCollection))]
	public class RunSheetDashboardCollectionTest : NonPersistentBusinessObjectCollectionTestCase<RunSheetDashboardCollection>
	{
		public void TestNew()
		{
			var dashboards = RunSheetDashboardCollection.New(Factory);
			AssertEquals("Precondition", Factory, dashboards.Factory);
			AssertEquals("Should contain one RunSheetDashboard", 1, dashboards.Count);
		}

		[TestDate(2013, 1, 1)]
		public void TestSwapFactoryRemoveAllAndAddNew()
		{
			var runSheet = Factory.New<CommonWorkSheet>();
			runSheet.EY_StartTime = ZDateTime.Today;
			runSheet.EY_EndTime = ZDateTime.Today.AddDays(1);
			var dashboards = RunSheetDashboardCollection.New(Factory);
			var dashboard = dashboards[0];
			dashboard.DateRangeFilter = "Today";
			AssertEquals("Precondition", Factory, dashboard.Factory);
			AssertContainsExactElementsInAnyOrder("Precondition:", new[] { runSheet }, dashboard.RunSheets);
			bool listChangedFired = false;
			((IBindingList)dashboards).ListChanged += (sender, e) =>
			{
				listChangedFired = true;
				// Copy must occur PRIOR to firing ListChanged otherwise binding will pull the
				// collections before the copy is finished -- resulting in incorrect data.
				AssertEquals(1, dashboards.Count);
				var newDashboard = dashboards[0];
				AssertNotEquals(dashboard, newDashboard);
				AssertNotEquals(Factory, dashboards.Factory);
				AssertNotEquals(Factory, newDashboard.Factory);
				AssertEquals("Ensure DateRange has been copied", "Today", newDashboard.DateRangeFilter);
				AssertContainsExactElementsInAnyOrder(new[] { runSheet }, dashboard.RunSheets);
			};
			dashboards.SwapFactoryRemoveAllAndAddNew();
			AssertEquals(true, listChangedFired);
		}

		protected override RunSheetDashboardCollection GetCollectionToTest()
		{
			return RunSheetDashboardCollection.New(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new RunSheetDashboard(Factory);
		}
	}
}
