using Enterprise.MasterFiles.Module;
using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.MasterFiles.Module.Testing
{
	[TestedType(typeof(AssignWhsPutawayGroupMethod))]
	sealed class AssignWhsPutawayGroupMethodTest : OperationalActionMethodTest<AssignWhsPutawayGroupMethod>
	{
		#region TestNewApplicator

		public void TestAssignWhsPutawayGroup_NewApplicator()
		{
			AssertType<AssignWhsPutawayGroupMethodApplicator>(NewMethod().NewApplicator(Factory, null));
		}

		#endregion

		#region TestNewGuiControl

		public void TestAssignWhsPutawayGroup_NewGuiControl()
		{
			var method = NewMethod();
			AssertEquals(true, method.HasControl);
			using (var control = method.NewGuiControl())
			{
				AssertType<AssignWhsPutawayGroupControl>(control);
			}
		}

		#endregion

		#region TestNameAndDescription

		public void TestNameAndDescription()
		{
			AssertEquals("Assign Warehouse Putaway Group to Product/Warehouse parameters", NewMethod().Name);
			AssertEquals("Assign Warehouse Putaway Group to Product/Warehouse parameters", NewMethod().Description);
		}

		#endregion

		#region Implementation

		protected override AssignWhsPutawayGroupMethod NewMethod()
		{
			return new AssignWhsPutawayGroupMethod();
		}

		#endregion
	}
}
