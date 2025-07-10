using Enterprise.Freight.Agency.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Module.Testing
{
	[TestedType(typeof(ReleaseOrderWithdrawActionMethod))]
	internal class ReleaseOrderWithdrawActionMethodTest : ReleaseOrderActionMethodTest<ReleaseOrderWithdrawActionMethod>
	{
		public void TestNewApplicatorIsOfTheCorrectType()
		{
			var applicator = Method.NewApplicator(Factory, new ReleaseImportOrderSettings());
			AssertEquals(typeof(WithdrawNZReleaseOrderActionMethodApplicator), applicator.GetType());
		}

		#region Implementation
		protected override ReleaseOrderWithdrawActionMethod NewMethod()
		{
			return new ReleaseOrderWithdrawActionMethod();
		}
		#endregion
	}
}
