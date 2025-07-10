using Enterprise.Services.OperationalActions.Support;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Module.Testing
{
	[TestedType(typeof(PRAReSendActionMethod))]
	internal class PRAReSendActionMethodTest : PRABaseActionMethodTest<PRAReSendActionMethod>
	{
		public void TestNewApplicatorIsOfTheCorrectType()
		{
			OperationalActionMethodApplicator applicator = Method.NewApplicator(Factory, new PRASettings(Factory));
			AssertEquals(typeof(PRAMessageApplicator), applicator == null ? null : applicator.GetType());
		}

		#region Implementation

		protected override PRAReSendActionMethod NewMethod()
		{
			return new PRAReSendActionMethod();
		}

		#endregion
	}
}
