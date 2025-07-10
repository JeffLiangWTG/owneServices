using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.LVS.GUI.Testing
{
	[TestedType(typeof(ADDCVDAppliesActionMethod))]
	public class ADDCVDAppliesActionMethodTest : OperationalActionMethodTest<ADDCVDAppliesActionMethod>
	{
		public void TestTestNameAndDescription()
		{
			AssertEquals("ADD/CVD Applies", Method.Name);
			AssertEquals("Apply anti-dumping and countervailing on US low value consignments", Method.Description);
		}

		public void TestNewApplicatorType()
		{
			AssertEquals(typeof(ADDCVDAppliesApplicator), Method.NewApplicator(Factory, null).GetType());
		}

		#region Implementation

		protected override ADDCVDAppliesActionMethod NewMethod()
		{
			return new ADDCVDAppliesActionMethod();
		}

		#endregion
	}
}
