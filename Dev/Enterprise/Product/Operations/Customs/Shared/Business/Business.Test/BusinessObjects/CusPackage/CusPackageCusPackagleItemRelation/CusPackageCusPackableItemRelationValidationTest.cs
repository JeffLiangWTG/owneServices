using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusPackageCusPackableItemRelationValidationTest : BusinessObjectValidationTestCase
	{
		CusPackage package1;
		CusPackage package2;
		CusPackageCusPackableItemRelation package1Relation;
		CusPackageCusPackableItemRelation package2Relation;

		protected override void SetUp()
		{
			base.SetUp();

			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			Factory.Save();
			var cusPackingList = declaration.LoadOrCreateCusPackingList(Factory);
			Factory.Save();
			package1 = cusPackingList.PackageJob.Packages.AddNew();
			package2 = cusPackingList.PackageJob.Packages.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var cusPackableItem = Factory.New<CusPackableItem>();
			cusPackableItem.CUI_JI = invoiceLine.PK;
			cusPackableItem.CUI_CUL = cusPackingList.PK;
			package1Relation = new CusPackageCusPackableItemRelation(package1, cusPackableItem);
			package2Relation = new CusPackageCusPackableItemRelation(package2, cusPackableItem);
		}

		public void TestCheckPackedQty()
		{
			package1Relation.PackableQuantity = 5;
			package1Relation.PackedQty = 4;

			AssertNoErrorContaining(package1Relation.PackedQtyInfo, MandatoryValidation.ValueCannotBeNegative);

			package1Relation.PackedQty = -1;
			AssertHasErrorContaining(package1Relation.PackedQtyInfo, MandatoryValidation.ValueCannotBeNegative);

			package1Relation.PackedQty = 6;
			AssertHasErrorContaining(package1Relation.PackedQtyInfo, "The total packed quantity 6 cannot exceed the packable quantity 5.");

			package1Relation.PackedQty = 2;
			package2Relation.PackedQty = 1;
			AssertNoErrors(package1Relation.PackedQtyInfo);

			package2Relation.PackedQty = 4;
			AssertHasErrorContaining(package2Relation.PackedQtyInfo, "The total packed quantity 6 cannot exceed the packable quantity 5.");

			package1Relation.PackableItem.Delete();
			AssertNoExceptionThrown(package1Relation.Validation.ValidatePackedQty);
		}

		public void TestCheckPackedQtyWhenIsPacked()
		{
			var errorMessage = "Please enter a 'Packed Qty' greater than 0.";

			package1Relation.PackableQuantity = 0;
			package1Relation.IsPacked = true;
			package1Relation.PackedQty = 0;
			package1Relation.Validation.ValidatePackedQty();
			AssertHasErrorContaining(package1Relation.PackedQtyInfo, errorMessage);

			package1Relation.PackedQty = -1;
			package1Relation.Validation.ValidatePackedQty();
			AssertHasErrorContaining(package1Relation.PackedQtyInfo, errorMessage);

			package1Relation.PackedQty = 1;
			package1Relation.Validation.ValidatePackedQty();
			AssertNoErrorContaining(package1Relation.PackedQtyInfo, errorMessage);
		}

		public void TestCheckNetWeightUQ()
		{
			var warning = "You have not entered a valid code.";
			var info = package1Relation.NetWeightUQInfo;

			package1Relation.PackableQuantity = 5m;
			package1.CustomsPackItem(package1Relation.PackableItem, 5m);
			var weightUQs = package1Relation.Lookups.WeightUQs.GetAllCodes();
			foreach (var code in weightUQs)
			{
				package1Relation.NetWeightUQ = code;
				AssertNoWarning(info, warning);
			}

			package1Relation.NetWeightUQ = "A";
			AssertHasWarning(info, warning);
		}

		public void TestCheckNetWeight()
		{
			var error = "Net Weight cannot be negative.";
			var info = package1Relation.NetWeightInfo;
			package1Relation.PackableQuantity = 5m;
			package1.CustomsPackItem(package1Relation.PackableItem, 5m);

			package1Relation.NetWeight = -1;
			AssertHasError(info, error);

			package1Relation.NetWeight = 0;
			AssertNoError(info, error);

			package1Relation.NetWeight = 1;
			AssertNoError(info, error);
		}
	}
}
