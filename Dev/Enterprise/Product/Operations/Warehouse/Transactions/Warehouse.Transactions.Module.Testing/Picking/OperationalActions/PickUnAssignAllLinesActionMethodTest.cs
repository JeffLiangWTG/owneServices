using System;
using Enterprise.Warehouse.Transactions.Business;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(PickUnAssignAllLinesActionMethod))]
	class PickUnAssignAllLinesActionMethodTest : UnAssignAllLinesActionMethodTest<PickUnAssignAllLinesActionMethod, WhsPick>
	{
		#region Implementation

		protected override PickUnAssignAllLinesActionMethod NewMethod() => new PickUnAssignAllLinesActionMethod(false);

		protected override string OperationalActionName
			=> "Un-assign Pick Lines";

		protected override Type ApplicatorType => typeof(PickUnAssignAllLinesActionMethodApplicator);

		#endregion
	}
}
