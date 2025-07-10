using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.TransportConsignment.Business.Testing
{
	[TestedType(typeof(DtbRoutePlannerCollection))]
	sealed class DtbRoutePlannerCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DtbRoutePlannerCollection>
	{
		#region TestSwapFactoryRemoveAllAndAddNew

		[TestDate(2013, 1, 1)]
		public void TestSwapFactoryRemoveAllAndAddNew()
		{
			var runSheet = Helper.CreateRunSheet();

			var planners = DtbRoutePlannerCollection.New(Factory);
			AssertEquals("Precondition", Factory, planners.Factory);
			AssertEquals("Should contain one RoutePlanner", 1, planners.Count);

			var planner = planners[0];
			planner.CurrentDay = RunSheetDay.Today;
			AssertEquals("Precondition", Factory, planner.Factory);
			AssertContainsExactElementsInAnyOrder("Precondition:", new[] { runSheet }, planner.RunSheetsFilteredForBinding);

			bool listChangedFired = false;
			((IBindingList)planners).ListChanged += (sender, e) =>
			{
				listChangedFired = true;

				// Copy must occur PRIOR to firing ListChanged otherwise binding will pull the
				// collections before the copy is finished -- resulting in incorrect data in grids.
				AssertEquals(1, planners.Count);
				var newPlanner = planners[0];
				AssertNotEquals(planner, newPlanner);
				AssertNotEquals(Factory, planners.Factory);
				AssertNotEquals(Factory, newPlanner.Factory);
				AssertEquals("Ensure view mode has been copied", RunSheetDay.Today, newPlanner.CurrentDay);
				AssertContainsExactElementsInAnyOrder(new[] { runSheet }, planner.RunSheetsFilteredForBinding);
			};

			planners.SwapFactoryRemoveAllAndAddNew();
			AssertEquals(true, listChangedFired);
		}

		#endregion

		#region Implementation

		protected override DtbRoutePlannerCollection GetCollectionToTest()
		{
			return DtbRoutePlannerCollection.New(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new DtbRoutePlanner(Factory);
		}

		TransportBookingConsignmentTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingConsignmentTestHelper(Factory)); }
		}

		TransportBookingConsignmentTestHelper helper;

		#endregion
	}
}
