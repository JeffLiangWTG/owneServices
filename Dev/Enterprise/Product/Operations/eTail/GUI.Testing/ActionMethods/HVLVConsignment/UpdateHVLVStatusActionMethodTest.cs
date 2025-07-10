using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.eTail.GUI.Testing
{
	[TestedType(typeof(UpdateHVLVStatusActionMethod))]
	public class UpdateHVLVStatusActionMethodTest : OperationalActionMethodTest<UpdateHVLVStatusActionMethod>
	{
		public void TestTestNameAndDescription()
		{
			AssertEquals("Update HVLV Status", Method.Name);
			AssertEquals("Updates status on shipment HVLV consignments", Method.Description);
		}

		public void TestHasControl()
		{
			AssertEquals("Should have control enabled", true, Method.HasControl);
		}

		public void TestNewApplicatorType()
		{
			AssertEquals(typeof(UpdateHVLVStatusApplicator), Method.NewApplicator(Factory, null).GetType());
		}

		#region Implementation

		protected override UpdateHVLVStatusActionMethod NewMethod()
		{
			return new UpdateHVLVStatusActionMethod();
		}

		#endregion
	}
}
