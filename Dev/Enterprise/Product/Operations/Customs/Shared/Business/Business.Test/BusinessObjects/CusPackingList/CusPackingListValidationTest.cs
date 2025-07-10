using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusPackingListValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCUL_CustomAttribute1()
		{
			var customAttrib1 = organisation.CustomLabels.AddNew();
			customAttrib1.OT_FieldName = Core.Constants.CustomLabels.CustomsPackingList.CustomAttribute1;
			customAttrib1.OT_IsMandatory = true;

			declaration.JE_OH_Supplier = organisation.PK;
			var packingList = declaration.LoadOrCreateCusPackingList(Factory);

			CombineAssertions(() =>
			{
				packingList.Validation.ValidateCUL_CustomAttribute1();
				AssertHasErrorContaining("Has Error", packingList.CUL_CustomAttribute1Info, MandatoryValidation.MustBeEntered);

				customAttrib1.OT_IsMandatory = false;
				packingList.Validation.ValidateCUL_CustomAttribute1();
				AssertNoErrorContaining("No Error", packingList.CUL_CustomAttribute1Info, MandatoryValidation.MustBeEntered);
			});
		}

		public void TestCheckCUL_CustomAttribute2()
		{
			var customAttrib2 = organisation.CustomLabels.AddNew();
			customAttrib2.OT_FieldName = Core.Constants.CustomLabels.CustomsPackingList.CustomAttribute2;
			customAttrib2.OT_IsMandatory = true;

			declaration.JE_OH_Supplier = organisation.PK;
			var packingList = declaration.LoadOrCreateCusPackingList(Factory);
			CombineAssertions(() =>
			{
				packingList.Validation.ValidateCUL_CustomAttribute2();
				AssertHasErrorContaining("Has Erorr", packingList.CUL_CustomAttribute2Info, MandatoryValidation.MustBeEntered);

				customAttrib2.OT_IsMandatory = false;
				packingList.Validation.ValidateCUL_CustomAttribute2();
				AssertNoErrorContaining("No Error", packingList.CUL_CustomAttribute2Info, MandatoryValidation.MustBeEntered);
			});
		}

		public void TestCheckCUL_CustomDate1()
		{
			var customDate1 = organisation.CustomLabels.AddNew();
			customDate1.OT_FieldName = Core.Constants.CustomLabels.CustomsPackingList.CustomDate1;
			customDate1.OT_IsMandatory = true;

			declaration.JE_OH_Supplier = organisation.PK;
			var packingList = declaration.LoadOrCreateCusPackingList(Factory);
			CombineAssertions(() =>
			{
				packingList.Validation.ValidateCUL_CustomDate1();
				AssertHasErrorContaining("Has Error", packingList.CUL_CustomDate1Info, MandatoryValidation.MustBeEntered);

				customDate1.OT_IsMandatory = false;
				packingList.Validation.ValidateCUL_CustomDate1();
				AssertNoErrorContaining("No Error", packingList.CUL_CustomDate1Info, MandatoryValidation.MustBeEntered);
			});
		}

		public void TestCheckCUL_CustomDate2()
		{
			var customDate2 = organisation.CustomLabels.AddNew();
			customDate2.OT_FieldName = Core.Constants.CustomLabels.CustomsPackingList.CustomDate2;
			customDate2.OT_IsMandatory = true;

			declaration.JE_OH_Supplier = organisation.PK;
			var packingList = declaration.LoadOrCreateCusPackingList(Factory);
			CombineAssertions(() =>
			{
				packingList.Validation.ValidateCUL_CustomDate2();
				AssertHasErrorContaining("Has Error", packingList.CUL_CustomDate2Info, MandatoryValidation.MustBeEntered);

				customDate2.OT_IsMandatory = false;
				packingList.Validation.ValidateCUL_CustomDate2();
				AssertNoErrorContaining("No Error", packingList.CUL_CustomDate2Info, MandatoryValidation.MustBeEntered);
			});
		}

		public void TestCheckCUL_CustomDecimal1()
		{
			var customDecimal1 = organisation.CustomLabels.AddNew();
			customDecimal1.OT_FieldName = Core.Constants.CustomLabels.CustomsPackingList.CustomDecimal1;
			customDecimal1.OT_IsMandatory = true;

			declaration.JE_OH_Supplier = organisation.PK;
			var packingList = declaration.LoadOrCreateCusPackingList(Factory);
			CombineAssertions(() =>
			{
				packingList.Validation.ValidateCUL_CustomDecimal1();
				AssertHasErrorContaining("Has Error", packingList.CUL_CustomDecimal1Info, MandatoryValidation.MustBeEntered);

				customDecimal1.OT_IsMandatory = false;
				packingList.Validation.ValidateCUL_CustomDecimal1();
				AssertNoErrorContaining("No Error", packingList.CUL_CustomDecimal1Info, MandatoryValidation.MustBeEntered);
			});
		}

		public void TestCheckCUL_CustomDecimal2()
		{
			var customDecimal2 = organisation.CustomLabels.AddNew();
			customDecimal2.OT_FieldName = Core.Constants.CustomLabels.CustomsPackingList.CustomDecimal2;
			customDecimal2.OT_IsMandatory = true;

			declaration.JE_OH_Supplier = organisation.PK;
			var packingList = declaration.LoadOrCreateCusPackingList(Factory);
			CombineAssertions(() =>
			{
				packingList.Validation.ValidateCUL_CustomDecimal2();
				AssertHasErrorContaining("Has Error", packingList.CUL_CustomDecimal2Info, MandatoryValidation.MustBeEntered);

				customDecimal2.OT_IsMandatory = false;
				packingList.Validation.ValidateCUL_CustomDecimal2();
				AssertNoErrorContaining("No Error", packingList.CUL_CustomDecimal2Info, MandatoryValidation.MustBeEntered);
			});
		}

		public void TestCheckTotalNetWeight()
		{
			var packingList = declaration.LoadOrCreateCusPackingList(Factory);

			var item1 = packingList.PackableItems.AddNew();
			item1.CUI_PackableQty = 10m;
			item1.CUI_NetWeight = 20m;
			item1.CUI_NetWeightUQ = Core.Constants.Weight.Kilograms;

			var item2 = packingList.PackableItems.AddNew();
			item2.CUI_PackableQty = 20m;
			item2.CUI_NetWeight = 10m;
			item2.CUI_NetWeightUQ = Core.Constants.Weight.Kilograms;

			CombineAssertions(() =>
			{
				packingList.PackageJob.Packages.AddNew().CustomsPackItem(item1, 2m);
				packingList.PackageJob.Packages.AddNew().CustomsPackItem(item2, 4m);
				packingList.Validation.ValidateTotalNetWeight();
				AssertHasWarning("Has Warning", packingList.TotalNetWeightInfo, "The sum of all Invoice Line Net Weight 30 KG does not balance with the Packing List total Net Weight 6 KG.");

				packingList.PackageJob.Packages.AddNew().CustomsPackItem(item1, 8m);
				packingList.PackageJob.Packages.AddNew().CustomsPackItem(item2, 16m);
				packingList.Validation.ValidateTotalNetWeight();
				AssertNoWarning("No Warning", packingList.TotalNetWeightInfo, "The sum of all Invoice Line Net Weight 30 KG does not balance with the Packing List total Net Weight 6 KG.");
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			organisation = Factory.New<OrgHeader>();
			organisation.FillWithValidTestData();

			declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_OH_Supplier = organisation.PK;
		}

		OrgHeader organisation;
		BaseJobDeclaration declaration;
	}
}
