using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.Business.Testing
{
	sealed class QuickPackItemValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckPackedQtyCanNotBeGreaterThanNotPackedQty()
		{
			AssertEquals(5m, quickPackItem.NotPackedQty);
			quickPackItem.PackedQty = 1;
			AssertNoError(quickPackItem.PackedQtyInfo, "Packed Qty cannot be greater than Not Packed Qty.");

			quickPackItem.PackedQty = 5;
			AssertNoError(quickPackItem.PackedQtyInfo, "Packed Qty cannot be greater than Not Packed Qty.");

			quickPackItem.PackedQty = 6;
			AssertHasError(quickPackItem.PackedQtyInfo, "Packed Qty cannot be greater than Not Packed Qty.");
		}

		public void TestCheckPackedQtyNotZero()
		{
			quickPackItem.Pack = ZString.Empty;
			quickPackItem.PackedQty = 0;
			AssertNoError(quickPackItem.PackedQtyInfo, "Packed Qty cannot be zero.");

			quickPackItem.Pack = "Test";
			quickPackItem.Validation.ValidatePackedQty();
			AssertHasError(quickPackItem.PackedQtyInfo, "Packed Qty cannot be zero.");

			quickPackItem.PackedQty = 1;
			AssertNoError(quickPackItem.PackedQtyInfo, "Packed Qty cannot be zero.");
		}

		public void TestCheckPackedQtyNotNegative()
		{
			quickPackItem.Pack = ZString.Empty;
			quickPackItem.PackedQty = -1;
			AssertNoError(quickPackItem.PackedQtyInfo, "Packed Qty cannot be negative.");

			quickPackItem.Pack = "Test";
			quickPackItem.Validation.ValidatePackedQty();
			AssertHasError(quickPackItem.PackedQtyInfo, "Packed Qty cannot be negative.");

			quickPackItem.PackedQty = 1;
			AssertNoError(quickPackItem.PackedQtyInfo, "Packed Qty cannot be negative.");
		}

		#region Implementation
		protected override void SetUp()
		{
			base.SetUp();

			var decl = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var invoiceForTest = decl.Invoices.AddNew();
			var line = invoiceForTest.InvoiceLines.AddNew();
			line.JI_Description = "ITEM 1";
			line.JI_InvoiceQuantity = 5;
			line.JI_InvoiceUQ = "PCE";

			var packingList = decl.LoadOrCreateCusPackingList(Factory);
			var package = packingList.PackageJob.Packages.AddNew();
			package.KP_MarksAndNumbers = "Pack #1";

			var packableItem = line.CreateNewCusPackableItem();
			packingList.PackableItems.Add(packableItem);
			quickPackItem = new QuickPackItem(packableItem);
		}

		QuickPackItem quickPackItem;

		#endregion
	}
}
