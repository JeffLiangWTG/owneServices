namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	using Enterprise.Services.OperationalActions.Support.Testing;
	using NUnit.Framework;

	[TestedType(typeof(ForceReplenishmentToPickFaceActionMethod))]
	public class ForceReplenishmentToPickFaceActionMethodTest : OperationalActionMethodTest<ForceReplenishmentToPickFaceActionMethod>
	{
		protected override ForceReplenishmentToPickFaceActionMethod NewMethod()
		{
			return new ForceReplenishmentToPickFaceActionMethod();
		}

		public void TestIsRunAgainDisabled()
		{
			Assert("IsRunAgainDisabled is true.", Method.IsRunAgainDisabled);
		}

		public void TestNewApplicatorType()
		{
			AssertEquals("Applicator Type is ForceReplenishmentToPickFaceActionMethodApplicator.", typeof(ForceReplenishmentToPickFaceActionMethodApplicator), Method.NewApplicator(Factory, null).GetType());
		}
	}
}
