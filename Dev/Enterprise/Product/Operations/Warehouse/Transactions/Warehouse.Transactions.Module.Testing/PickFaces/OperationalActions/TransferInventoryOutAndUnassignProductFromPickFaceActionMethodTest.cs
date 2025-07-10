using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(TransferInventoryOutAndUnassignProductFromPickFaceActionMethod))]
	public class TransferInventoryOutAndUnassignProductFromPickFaceActionMethodTest : OperationalActionMethodTest<TransferInventoryOutAndUnassignProductFromPickFaceActionMethod>
	{
		protected override TransferInventoryOutAndUnassignProductFromPickFaceActionMethod NewMethod()
		{
			return new TransferInventoryOutAndUnassignProductFromPickFaceActionMethod();
		}

		public void TestIsRunAgainDisabled()
		{
			Assert("IsRunAgainDisabled is true.", Method.IsRunAgainDisabled);
		}

		public void TestNewApplicatorType()
		{
			AssertEquals("Applicator Type is TransferInventoryOutAndUnassignProductFromPickFaceActionMethodApplicator.", typeof(TransferInventoryOutAndUnassignProductFromPickFaceActionMethodApplicator), Method.NewApplicator(Factory, null).GetType());
		}
	}
}
