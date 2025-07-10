using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.Business.Testing
{
	sealed class NonPersistentSplitPackItemValidationTest : BusinessObjectValidationTestCase
	{
		public void TestGoodsDescriptionAllowUnicode()
		{
			nonPersistentSplitPackItem.GoodsDescription = "engrish123";
			AssertEquals(true, nonPersistentSplitPackItem.GoodsDescription.IsWesternEuropeanOrEmpty);
			AssertEquals(false, nonPersistentSplitPackItem.GoodsDescriptionInfo.HasErrors());

			nonPersistentSplitPackItem.GoodsDescription = "abc \u069A";
			AssertEquals(false, nonPersistentSplitPackItem.GoodsDescription.IsWesternEuropeanOrEmpty);
			AssertEquals(false, nonPersistentSplitPackItem.GoodsDescriptionInfo.HasErrors());
		}

		public void TestCheckGoodsDescription()
		{
			ValidationTestHelper.AssertErrorIfNotEntered(nonPersistentSplitPackItem.GoodsDescriptionInfo);
		}

		public void TestCheckPackableQuantity_Zero()
		{
			CombineAssertions(() =>
			{
				var targetInfo = nonPersistentSplitPackItem.PackableQuantityInfo;
				nonPersistentSplitPackItem.PackableQuantity = decimal.Zero;
				AssertHasWarningContaining("Zero", targetInfo, MandatoryValidation.ValueCannotBeZero);

				nonPersistentSplitPackItem.PackableQuantity = 1;
				AssertNoWarningContaining("Value", targetInfo, MandatoryValidation.ValueCannotBeZero);
			});
		}

		public void TestCheckPackableQuantity_Negative()
		{
			ValidationTestHelper.AssertErrorIfValueIsNegative(nonPersistentSplitPackItem.PackableQuantityInfo);
		}

		public void TestCheckPackableUQ_Mandatory()
		{
			ValidationTestHelper.AssertWarningIfNotEntered(nonPersistentSplitPackItem.PackableUQInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckPackableUQ_ListValidation()
		{
			packageCusPackableItemRelation.PackableItem.CUI_JI = ZGuid.Empty;
			ValidationTestHelper.AssertErrorIfInvalidCode(nonPersistentSplitPackItem.PackableUQInfo, "XX", CargoWise.Definitions.RefPackTypeStandardUnits.Codes.Inches);
		}

		public void TestTestCheckNetWeight_Zero()
		{
			CombineAssertions(() =>
			{
				var targetInfo = nonPersistentSplitPackItem.NetWeightInfo;
				nonPersistentSplitPackItem.NetWeight = decimal.Zero;
				AssertHasWarningContaining("Zero", targetInfo, MandatoryValidation.ValueCannotBeZero);

				nonPersistentSplitPackItem.NetWeight = 1;
				AssertNoWarningContaining("Value", targetInfo, MandatoryValidation.ValueCannotBeZero);
			});
		}

		public void TestCheckNetWeight_Negative()
		{
			ValidationTestHelper.AssertErrorIfValueIsNegative(nonPersistentSplitPackItem.NetWeightInfo);
		}

		public void TestCheckNetWeightUQ_Mandatory()
		{
			ValidationTestHelper.AssertWarningIfNotEntered(nonPersistentSplitPackItem.NetWeightUQInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckNetWeightUQ_ListValidation()
		{
			packageCusPackableItemRelation.PackableItem.CUI_NetWeight = ZDecimal.Zero;
			ValidationTestHelper.AssertErrorIfInvalidCode(nonPersistentSplitPackItem.NetWeightUQInfo, "XX", Core.Constants.Weight.Kilograms);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var packingList = declaration.LoadOrCreateCusPackingList(Factory);
			var package = packingList.PackageJob.Packages.AddNew();
			packageCusPackableItemRelation = package.PackableItemRelataions.Cast<CusPackageCusPackableItemRelation>().Single();
			var splitter = new PackableItemsSplitter(package, packageCusPackableItemRelation);
			nonPersistentSplitPackItem = new NonPersistentSplitPackItem(packageCusPackableItemRelation, splitter);
		}
		CusPackageCusPackableItemRelation packageCusPackableItemRelation;
		NonPersistentSplitPackItem nonPersistentSplitPackItem;
	}
}
