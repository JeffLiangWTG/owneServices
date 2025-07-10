using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.eTail.GUI.Testing
{
	[TestedType(typeof(UpdateHVLVItemStatusActionMethod))]
	public class UpdateHVLVItemStatusActionMethodTest : OperationalActionMethodTest<UpdateHVLVItemStatusActionMethod>
	{
		public void TestTestNameAndDescription()
		{
			AssertEquals("Update HVLV Item Status", Method.Name);
			AssertEquals("Updates HVLV item statuses on HVLV consignments", Method.Description);
		}

		public void TestHasControl()
		{
			AssertEquals("Should have control enabled", true, Method.HasControl);
		}

		public void TestNewApplicatorType()
		{
			AssertEquals("New applicator should be of type UpdateHVLVItemStatusApplicator", typeof(UpdateHVLVItemStatusApplicator), Method.NewApplicator(Factory, null).GetType());
		}

		#region Implementation

		protected override UpdateHVLVItemStatusActionMethod NewMethod()
		{
			return new UpdateHVLVItemStatusActionMethod();
		}

		#endregion
	}
}
