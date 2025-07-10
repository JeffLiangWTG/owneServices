using Enterprise.Freight.Agency.Business;
using Enterprise.Services.OperationalActions.Support;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Module.Testing
{
	[TestedType(typeof(EIDOWithdrawActionMethod))]
	internal class EIDOWithdrawActionMethodTest : EIDOBaseActionMethodTest<EIDOWithdrawActionMethod>
	{
		public void TestNewApplicatorIsOfTheCorrectType()
		{
			OperationalActionMethodApplicator applicator = Method.NewApplicator(Factory, new ReleaseImportOrderSettings());
			AssertEquals(typeof(EIDOSendCancellationApplicator), applicator == null ? null : applicator.GetType());
		}

		#region Implementation
		protected override EIDOWithdrawActionMethod NewMethod()
		{
			return new EIDOWithdrawActionMethod();
		}
		#endregion
	}
}
