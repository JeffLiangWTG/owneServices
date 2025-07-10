using Enterprise.Services.OperationalActions.Support;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Module.Testing
{
	[TestedType(typeof(PRACancelActionMethod))]
	internal class PRACancelActionMethodTest : PRABaseActionMethodTest<PRACancelActionMethod>
	{
		public void TestNewApplicatorIsOfTheCorrectType()
		{
			OperationalActionMethodApplicator applicator = Method.NewApplicator(Factory, new PRASettings(Factory));
			AssertEquals(typeof(PRAMessageApplicator), applicator == null ? null : applicator.GetType());
		}

		#region Implementation

		protected override PRACancelActionMethod NewMethod()
		{
			return new PRACancelActionMethod();
		}

		#endregion
	}
}
