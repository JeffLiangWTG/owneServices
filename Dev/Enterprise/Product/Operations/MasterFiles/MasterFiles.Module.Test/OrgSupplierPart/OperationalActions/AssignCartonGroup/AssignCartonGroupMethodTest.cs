using Enterprise.MasterFiles.Module;
using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.MasterFiles.Module.Testing
{
	[TestedType(typeof(AssignCartonGroupMethod))]
	sealed class AssignCartonGroupMethodTest : OperationalActionMethodTest<AssignCartonGroupMethod>
	{
		#region TestNewApplicator_RealTest

		public void TestNewApplicator_RealTest()
		{
			AssertType<AssignCartonGroupMethodApplicator>(NewMethod().NewApplicator(Factory, null));
		}

		#endregion

		#region TestNewGuiControl_RealTest

		public void TestNewGuiControl_RealTest()
		{
			var method = NewMethod();
			AssertEquals(true, method.HasControl);
			using (var control = method.NewGuiControl())
			{
				AssertType<AssignCartonGroupControl>(control);
			}
		}

		#endregion

		#region TestNameAndDescription

		public void TestNameAndDescription()
		{
			AssertEquals("Assign Carton Group to Product Relationships", NewMethod().Name);
			AssertEquals("Assign Carton Group to Product Relationships", NewMethod().Description);
		}

		#endregion

		#region Implementation

		protected override AssignCartonGroupMethod NewMethod()
		{
			return new AssignCartonGroupMethod();
		}

		#endregion
	}
}
