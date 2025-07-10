using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.LocalCartage.Business.Testing
{
	[TestedType(typeof(CartageLegPlannerCollection))]
	public class CartageLegPlannerCollectionTest : NonPersistentBusinessObjectCollectionTestCase<CartageLegPlannerCollection>
	{
		public void TestSwapFactoryRemoveAllAndAddNew()
		{
			var provider = new CartageBehaviorStrategyProvider();
			CommonCartageBehaviorStrategyProvider.SetProvider(Factory, provider);
			var planners = GetCollectionToTest();
			AssertEquals(Factory, planners.Factory);
			AssertEquals("Ensure constructor added an element automatically", 1, planners.Count);
			var planner = planners[0];
			var cartageLeg1 = planner.CartageLegs.AddNew();
			var cartageLeg2 = planner.CartageLegs.AddNew();
			AssertEquals("Precondition", Factory, planner.Factory);
			AssertContainsExactElementsInAnyOrder("Precondition:", new[] { cartageLeg1, cartageLeg2 }, planner.CartageLegs);
			bool listChangedFired = false;
			((IBindingList)planners).ListChanged += (sender, e) =>
			{
				listChangedFired = true;
				// Copy must occur PRIOR to firing ListChanged otherwise binding will pull the
				// collections before the copy is finished -- resulting in incorrect data.
				AssertEquals("The planner should've been copied *before* the ListChanged event was fired", 1, planners.Count);
				var newPlanner = planners[0];
				AssertNotEquals(planners, newPlanner);
				AssertNotEquals(Factory, planners.Factory);
				AssertNotEquals(Factory, newPlanner.Factory);
				AssertEquals(0, planner.CartageLegs.Count);
			};
			planner.CartageLegs.Sort(JobContainerLegsSchema.Constants.JU_PickupTimeIn, System.ComponentModel.ListSortDirection.Descending);
			planners.SwapFactoryRemoveAllAndAddNew();
			AssertEquals("ListChanged is fired after SwapFactoryRemoveAllAndAddNew has run", true, listChangedFired);
			AssertEquals(1, planners.Count);
			AssertNotEquals(planner, planners[0]);
			AssertNotEquals(Factory, planners.Factory);
			AssertNotEquals(Factory, planners[0].Factory);
			AssertEquals(JobContainerLegsSchema.Constants.JU_PickupTimeIn, ((IBindingList)planner.CartageLegs).SortProperty.Name);
			AssertEquals("Provider gets passed along", provider, CommonCartageBehaviorStrategyProvider.GetCartageProvider(planners.Factory));
			Assert("Old planner must be deleted.", planner.IsDeleted);
		}

		protected override CartageLegPlannerCollection GetCollectionToTest()
		{
			return new CartageLegPlannerCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new CartageLegPlanner(Factory);
		}
	}
}
