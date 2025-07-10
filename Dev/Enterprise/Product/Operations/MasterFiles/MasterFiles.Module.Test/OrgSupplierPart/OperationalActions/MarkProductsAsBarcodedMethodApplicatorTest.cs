using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(MarkProductsAsBarcodedMethodApplicator))]
	sealed class MarkProductsAsBarcodedMethodApplicatorTest : OperationalActionMethodApplicatorTest
	{
		#region TestAction

		public void TestAction()
		{
			var product1 = Factory.NewWithValidTestData<OrgSupplierPart>();
			product1.OP_PartNum = "P1";
			var product2 = Factory.NewWithValidTestData<OrgSupplierPart>();
			product2.OP_PartNum = "P2";
			product2.OP_IsBarcoded = false;

			var expectedLogText = @"WARNING: Product P1 skipped - it is already barcoded.
INFO: Product P2 marked as barcoded successfully.
";
			ApplyApplicator(new[] { product1, product2 }, expectedLogText);

			AssertEquals(true, product1.OP_IsBarcoded);
			AssertEquals(true, product2.OP_IsBarcoded);
		}

		#endregion

		protected override BusinessObject GetNewBusinessObject()
		{
			return new MarkProductsAsBarcodedMethodApplicator("test", Factory);
		}
	}
}
