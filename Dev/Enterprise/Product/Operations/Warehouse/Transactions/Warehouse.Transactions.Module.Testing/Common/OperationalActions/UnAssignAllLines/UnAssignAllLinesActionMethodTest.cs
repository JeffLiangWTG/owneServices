using System;
using CargoWise.EntityFramework;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	public abstract class UnAssignAllLinesActionMethodTest<TActionMethod, TAssigner> : OperationalActionMethodTest<TActionMethod>
		where TAssigner : BusinessObject, IMasterStaffAssigner
		where TActionMethod : LinesUserAssignerActionMethod<TAssigner>
	{
		#region TestNameAndDescription

		public void TestNameAndDescription()
		{
			var actionMethod = NewMethod();
			AssertEquals(OperationalActionName, actionMethod.Name);
			AssertEquals(OperationalActionName, actionMethod.Description);
		}

		#endregion

		#region TestApplicator

		public void TestApplicator()
		{
			var actionMethod = NewMethod();
			var applicator = actionMethod.NewApplicator(Factory, null);
			AssertEquals(ApplicatorType, applicator.GetType());
		}

		#endregion

		#region OperationalActionName

		protected abstract string OperationalActionName { get; }

		#endregion

		#region ApplicatorType

		protected abstract Type ApplicatorType { get; }

		#endregion
	}
}
