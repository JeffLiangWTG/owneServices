using Enterprise.Warehouse.Transactions.Business;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(PickAssignAllLinesToUserActionMethod))]
	class PickAssignAllLinesToUserActionMethodTest : AssignAllLinesToUserActionMethodTest<PickAssignAllLinesToUserActionMethod, WhsPick>
	{
		protected override string GetExpectedNameAndDescription() => "Assign Pick Lines to user";

		#region TestApplicator

		public void TestApplicator()
		{
			var actionMethod = NewMethod();
			var applicator = actionMethod.NewApplicator(Factory, null);
			AssertEquals(typeof(PickAssignAllLinesToUserApplicator), applicator.GetType());
		}

		#endregion

		#region Implementation

		protected override PickAssignAllLinesToUserActionMethod NewMethod()
		{
			return new PickAssignAllLinesToUserActionMethod(false);
		}

		#endregion
	}

	[TestedType(typeof(PickAssignAllLinesToUserActionMethod))]
	class ReleaseAssignAllLinesToUserActionMethodTest : AssignAllLinesToUserActionMethodTest<PickAssignAllLinesToUserActionMethod, WhsPick>
	{
		protected override string GetExpectedNameAndDescription() => "Assign Pick Lines to user";

		#region TestApplicator

		public void TestApplicator()
		{
			var actionMethod = NewMethod();
			var applicator = actionMethod.NewApplicator(Factory, null);
			AssertEquals(typeof(ReleaseAssignAllLinesToUserApplicator), applicator.GetType());
		}

		#endregion

		#region Implementation

		protected override PickAssignAllLinesToUserActionMethod NewMethod()
		{
			return new PickAssignAllLinesToUserActionMethod(true);
		}

		#endregion
	}
}
