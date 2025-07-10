using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(UnmarkProductsAsBarcodedMethodApplicator))]
	sealed class UnmarkProductsAsBarcodedMethodApplicatorTest : OperationalActionMethodApplicatorTest
	{
		#region TestAction

		public void TestAction()
		{
			var product1 = Factory.NewWithValidTestData<OrgSupplierPart>();
			product1.OP_PartNum = "P1";
			var product2 = Factory.NewWithValidTestData<OrgSupplierPart>();
			product2.OP_PartNum = "P2";
			product2.OP_IsBarcoded = false;
			var product3 = Factory.NewWithValidTestData<OrgSupplierPart>();
			product3.OP_PartNum = "P3";
			var barcode = product3.PartBarcodes.AddNew();
			barcode.PH_Barcode = "111";
			barcode.PH_F3_NKPackType = "UNT";

			var expectedLogText = @"INFO: Product P1 successfully unmarked as barcoded.
WARNING: Product P2 skipped - it is already non-barcoded.
WARNING: Product P3 could not be unmarked as barcoded because of the following error(s):
Error - PH_Barcode: Non barcoded products should not have barcodes defined.
Error - OP_IsBarcoded: Non barcoded products should not have barcodes defined.
";
			ApplyApplicator(new[] { product1, product2, product3 }, expectedLogText);

			AssertEquals(false, product1.OP_IsBarcoded);
			AssertEquals(false, product2.OP_IsBarcoded);
			AssertEquals(true, product3.OP_IsBarcoded);
		}

		#endregion

		protected override BusinessObject GetNewBusinessObject()
		{
			return new UnmarkProductsAsBarcodedMethodApplicator("test", Factory);
		}
	}
}
