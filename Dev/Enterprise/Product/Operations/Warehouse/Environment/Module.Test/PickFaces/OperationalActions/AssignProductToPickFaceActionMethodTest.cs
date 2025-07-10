using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Environment.Module.Testing
{
	[TestedType(typeof(AssignProductToPickFaceActionMethod))]
	public class AssignProductToPickFaceActionMethodTest : OperationalActionMethodTest<AssignProductToPickFaceActionMethod>
	{
		#region TestNewControl

		public void TestNewControl()
		{
			var actionMethod = NewMethod();
			AssertEquals("Expecting 'Has Control' flag is set correctly", true, actionMethod.HasControl);

			using (var newControl = actionMethod.NewGuiControl())
			{
				AssertNotNull(newControl);
				AssertType<AssignProductToPickFaceControl>("Expecting correct new control type", newControl);
			}
		}

		#endregion

		#region TestTypedApplicator

		public void TestTypedApplicator()
		{
			var actionMethod = NewMethod();
			var applicator = actionMethod.NewApplicator(Factory, null);

			AssertType<AssignProductToPickFaceActionMethodApplicator>("Expecting correct applicator type", applicator);
		}

		#endregion

		protected override AssignProductToPickFaceActionMethod NewMethod()
		{
			return new AssignProductToPickFaceActionMethod();
		}
	}
}
