using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	[TestedType(typeof(UpdateCTStatusActionMethod))]
	public class UpdateCTStatusActionMethodTest : OperationalActionMethodTest<UpdateCTStatusActionMethod>
	{
		public void TestTestNameAndDescription()
		{
			AssertEquals("Update CT Status", Method.Name);
			AssertEquals("Updates CT status on shipments", Method.Description);
		}

		public void TestHasControl()
		{
			AssertEquals("Should have control enabled", true, Method.HasControl);
		}

		public void TestNewApplicatorType()
		{
			AssertEquals(typeof(UpdateCTStatusApplicator), Method.NewApplicator(Factory, null).GetType());
		}

		#region Implementation

		protected override UpdateCTStatusActionMethod NewMethod()
		{
			return new UpdateCTStatusActionMethod();
		}

		#endregion
	}
}
