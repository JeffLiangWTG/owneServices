using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	[TestedType(typeof(CalculateGreenhouseGasEmissionsActionMethod))]
	public class CalculateGreenhouseGasEmissionsActionMethodTest : OperationalActionMethodTest<CalculateGreenhouseGasEmissionsActionMethod>
	{
		public void TestNameAndDescription()
		{
			AssertEquals("Test the action method name", "Calculate Greenhouse Gas Emissions", Method.Name);
			AssertEquals("Test the action method description", "Calculate the greenhouse gas emissions for selected shipments.", Method.Description);
		}

		public void TestHasControl()
		{
			AssertEquals("Shouldn't have a control", expected: false, Method.HasControl);
		}

		public void TestNewApplicatorType()
		{
			AssertType(typeof(CalculateGreenhouseGasEmissionsApplicator), Method.NewApplicator(Factory, null));
		}

		public void TestRunWithoutResultLogging()
		{
			AssertEquals("Should run without result logging", expected: true, Method.RunWithoutResultLogging);
		}

		#region Implementation

		protected override CalculateGreenhouseGasEmissionsActionMethod NewMethod()
		{
			return new CalculateGreenhouseGasEmissionsActionMethod();
		}

		#endregion
	}
}
