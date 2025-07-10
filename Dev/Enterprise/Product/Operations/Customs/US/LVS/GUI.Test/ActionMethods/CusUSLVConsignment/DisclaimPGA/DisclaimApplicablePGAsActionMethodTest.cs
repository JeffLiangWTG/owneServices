using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.LVS.GUI.Testing
{
	[TestedType(typeof(DisclaimApplicablePGAsActionMethod))]
	public class DisclaimApplicablePGAsActionMethodTest : OperationalActionMethodTest<DisclaimApplicablePGAsActionMethod>
	{
		public void TestNameAndDescription()
		{
			AssertEquals("Disclaim Applicable PGAs", Method.Name);
			AssertEquals("Disclaim PGAs for all selected Low Value bills", Method.Description);
		}

		public void TestNewApplicatorType()
		{
			AssertEquals(typeof(DisclaimApplicablePGAsApplicator), Method.NewApplicator(Factory, null).GetType());
		}

		#region Implementation

		protected override DisclaimApplicablePGAsActionMethod NewMethod()
		{
			return new DisclaimApplicablePGAsActionMethod();
		}

		#endregion
	}
}
