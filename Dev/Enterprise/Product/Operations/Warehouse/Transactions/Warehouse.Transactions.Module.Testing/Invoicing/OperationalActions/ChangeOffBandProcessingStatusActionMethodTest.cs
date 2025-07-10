using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(ChangeOffBandProcessingStatusActionMethod))]
	public class ChangeOffBandProcessingStatusActionMethodTest : OperationalActionMethodTest<ChangeOffBandProcessingStatusActionMethod>
	{
		protected override ChangeOffBandProcessingStatusActionMethod NewMethod() => new ChangeOffBandProcessingStatusActionMethod();

		#region TestHasControl

		public void TestHasControl()
		{
			AssertEquals("Has control.", true, Method.HasControl);
		}

		#endregion

		#region TestSelectOffBandProcessingStatusControl

		public void TestSelectOffBandProcessingStatusControl()
		{
			using (var control = Method.NewGuiControl())
			{
				AssertEquals("Should return customs control.", typeof(SelectOffBandProcessingStatusControl), control.GetType());
			}
		}

		#endregion

		#region TestNewApplicatorType

		public void TestNewApplicatorType()
		{
			AssertEquals("Applicator Type is ChangeOffBandProcessingStatusActionMethodApplicator.", typeof(ChangeOffBandProcessingStatusActionMethodApplicator), Method.NewApplicator(Factory, null).GetType());
		}

		#endregion

		#region TestNameAndDescription

		public void TestNameAndDescription()
		{
			AssertEquals("Name.", "Change Off Band Processing Status", Method.Name);
			AssertEquals("Description.", "Change Off Band Processing Status", Method.Description);
		}

		#endregion
	}
}
