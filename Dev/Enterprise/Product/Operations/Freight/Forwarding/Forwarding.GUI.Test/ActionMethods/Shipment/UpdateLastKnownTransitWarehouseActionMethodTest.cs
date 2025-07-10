using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	[TestedType(typeof(UpdateLastKnownTransitWarehouseActionMethod))]
	public class UpdateLastKnownTransitWarehouseActionMethodTest : OperationalActionMethodTest<UpdateLastKnownTransitWarehouseActionMethod>
	{
		public void TestNameAndDescription()
		{
			AssertEquals("Update Last Known TW Details", Method.Name);
			AssertEquals("Update last known TW details on shipments", Method.Description);
		}

		public void TestHasControl()
		{
			AssertEquals(true, Method.HasControl);
		}

		public void TestNewGuiControlIsCorrectType()
		{
			using (var control = Method.NewGuiControl())
			{
				Assert(control is UpdateLastKnownTransitWarehouseUserControl);
			}
		}

		public void TestNewApplicatorIsCorrectType()
		{
			Assert(Method.NewApplicator(Factory, null) is UpdateLastKnownTransitWarehouseApplicator);
		}

		#region Implementation

		protected override UpdateLastKnownTransitWarehouseActionMethod NewMethod() => new UpdateLastKnownTransitWarehouseActionMethod();

		#endregion
	}
}
