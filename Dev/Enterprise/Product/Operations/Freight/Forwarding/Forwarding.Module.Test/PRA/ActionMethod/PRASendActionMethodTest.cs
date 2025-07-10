using Enterprise.Services.OperationalActions.Support;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Module.Testing
{
	[TestedType(typeof(PRASendActionMethod))]
	internal class PRASendActionMethodTest : PRABaseActionMethodTest<PRASendActionMethod>
	{
		public void TestNewApplicatorIsOfTheCorrectType()
		{
			OperationalActionMethodApplicator applicator = Method.NewApplicator(Factory, new PRASettings(Factory));
			AssertEquals(typeof(PRAMessageApplicator), applicator == null ? null : applicator.GetType());
		}

		#region Implementation

		protected override PRASendActionMethod NewMethod()
		{
			return new PRASendActionMethod();
		}

		#endregion
	}
}
