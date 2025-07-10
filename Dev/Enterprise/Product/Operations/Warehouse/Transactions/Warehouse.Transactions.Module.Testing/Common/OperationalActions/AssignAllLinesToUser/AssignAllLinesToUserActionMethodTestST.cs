using CargoWise.EntityFramework;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	public abstract class AssignAllLinesToUserActionMethodTest<S, T> : OperationalActionMethodTest<S>
		where T : BusinessObject, IMasterStaffAssigner
		where S : LinesUserAssignerActionMethod<T>
	{
		#region TestNameAndDescription

		public void TestNameAndDescription()
		{
			var actionMethod = NewMethod();
			var expected = GetExpectedNameAndDescription();

			AssertNotNull(expected);
			AssertEquals(expected, actionMethod.Name);
			AssertEquals(expected, actionMethod.Description);
		}

		protected abstract string GetExpectedNameAndDescription();

		#endregion

		#region TestNewGuiControlAndHasControl

		public void TestNewGuiControlAndHasControl()
		{
			var actionMethod = NewMethod();
			using (var control = actionMethod.NewGuiControl())
			{
				AssertEquals(typeof(FindUserCodeControl<T>), control.GetType());
			}
			AssertEquals(true, actionMethod.HasControl);
		}

		#endregion
	}
}
