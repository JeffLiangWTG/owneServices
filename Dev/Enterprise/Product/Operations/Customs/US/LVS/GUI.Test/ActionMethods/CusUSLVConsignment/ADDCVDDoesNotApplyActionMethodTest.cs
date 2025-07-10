using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.LVS.GUI.Testing
{
	[TestedType(typeof(ADDCVDDoesNotApplyActionMethod))]
	public class ADDCVDDoesNotApplyActionMethodTest : OperationalActionMethodTest<ADDCVDDoesNotApplyActionMethod>
	{
		public void TestTestNameAndDescription()
		{
			AssertEquals("ADD/CVD Does not apply", Method.Name);
			AssertEquals("Disable anti-dumping and countervailing on US low value consignments", Method.Description);
		}

		public void TestNewApplicatorType()
		{
			AssertEquals(typeof(ADDCVDDoesNotApplyApplicator), Method.NewApplicator(Factory, null).GetType());
		}

		#region Implementation

		protected override ADDCVDDoesNotApplyActionMethod NewMethod()
		{
			return new ADDCVDDoesNotApplyActionMethod();
		}

		#endregion
	}
}
