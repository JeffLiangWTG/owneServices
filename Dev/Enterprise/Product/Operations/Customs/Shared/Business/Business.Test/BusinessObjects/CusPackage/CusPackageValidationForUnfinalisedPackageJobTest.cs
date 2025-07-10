using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business.Testing;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusPackageValidationForUnfinalisedPackageJobTest : PkgPackageValidationForUnfinalisedPackageJobTest
	{
		public void TestCheckKP_MarksAndNumbers()
		{
			var cusPackageJob = Factory.New<CusPackageJob>();
			var cusPackage = cusPackageJob.Packages.AddNew();
			cusPackage.Validation.ValidateKP_MarksAndNumbers();
			AssertHasErrorContaining(cusPackage.KP_MarksAndNumbersInfo, MandatoryValidation.MustBeEntered);
		}

		public void TestCheckCustomAttribute1()
		{
			var customAttrib1 = organisation.CustomLabels.AddNew();
			customAttrib1.OT_FieldName = Core.Constants.CustomLabels.CusPackage.CustomAttribute1;
			customAttrib1.OT_IsMandatory = true;

			declaration.JE_OH_Supplier = organisation.PK;
			var packingList = declaration.LoadOrCreateCusPackingList(Factory);
			var cusPackage = packingList.PackageJob.Packages.AddNew();
			var validation = (CusPackageValidationForUnfinalisedPackageJob)cusPackage.Validation;

			CombineAssertions(() =>
			{
				validation.ValidateCustomAttribute1();
				AssertHasErrorContaining("Has Error", cusPackage.CustomAttribute1Info, MandatoryValidation.MustBeEntered);

				customAttrib1.OT_IsMandatory = false;
				validation.ValidateCustomAttribute1();
				AssertNoErrorContaining("No Error", cusPackage.CustomAttribute1Info, MandatoryValidation.MustBeEntered);
			});
		}

		public void TestCheckCustomAttribute2()
		{
			var customAttrib2 = organisation.CustomLabels.AddNew();
			customAttrib2.OT_FieldName = Core.Constants.CustomLabels.CusPackage.CustomAttribute2;
			customAttrib2.OT_IsMandatory = true;

			declaration.JE_OH_Supplier = organisation.PK;
			var packingList = declaration.LoadOrCreateCusPackingList(Factory);
			var cusPackage = packingList.PackageJob.Packages.AddNew();
			var validation = (CusPackageValidationForUnfinalisedPackageJob)cusPackage.Validation;

			CombineAssertions(() =>
			{
				validation.ValidateCustomAttribute2();
				AssertHasErrorContaining("Has Erorr", cusPackage.CustomAttribute2Info, MandatoryValidation.MustBeEntered);

				customAttrib2.OT_IsMandatory = false;
				validation.ValidateCustomAttribute2();
				AssertNoErrorContaining("No Error", cusPackage.CustomAttribute2Info, MandatoryValidation.MustBeEntered);
			});
		}

		public void TestCheckCustomDate1()
		{
			var customDate1 = organisation.CustomLabels.AddNew();
			customDate1.OT_FieldName = Core.Constants.CustomLabels.CusPackage.CustomDate1;
			customDate1.OT_IsMandatory = true;

			declaration.JE_OH_Supplier = organisation.PK;
			var packingList = declaration.LoadOrCreateCusPackingList(Factory);
			var cusPackage = packingList.PackageJob.Packages.AddNew();
			var validation = (CusPackageValidationForUnfinalisedPackageJob)cusPackage.Validation;

			CombineAssertions(() =>
			{
				validation.ValidateCustomDate1();
				AssertHasErrorContaining("Has Error", cusPackage.CustomDate1Info, MandatoryValidation.MustBeEntered);

				customDate1.OT_IsMandatory = false;
				validation.ValidateCustomDate1();
				AssertNoErrorContaining("No Error", cusPackage.CustomDate1Info, MandatoryValidation.MustBeEntered);
			});
		}

		public void TestCheckCustomDate2()
		{
			var customDate2 = organisation.CustomLabels.AddNew();
			customDate2.OT_FieldName = Core.Constants.CustomLabels.CusPackage.CustomDate2;
			customDate2.OT_IsMandatory = true;

			declaration.JE_OH_Supplier = organisation.PK;
			var packingList = declaration.LoadOrCreateCusPackingList(Factory);
			var cusPackage = packingList.PackageJob.Packages.AddNew();
			var validation = (CusPackageValidationForUnfinalisedPackageJob)cusPackage.Validation;

			CombineAssertions(() =>
			{
				validation.ValidateCustomDate2();
				AssertHasErrorContaining("Has Error", cusPackage.CustomDate2Info, MandatoryValidation.MustBeEntered);

				customDate2.OT_IsMandatory = false;
				validation.ValidateCustomDate2();
				AssertNoErrorContaining("No Error", cusPackage.CustomDate2Info, MandatoryValidation.MustBeEntered);
			});
		}

		public void TestCheckCustomDecimal1()
		{
			var customDecimal1 = organisation.CustomLabels.AddNew();
			customDecimal1.OT_FieldName = Core.Constants.CustomLabels.CusPackage.CustomDecimal1;
			customDecimal1.OT_IsMandatory = true;

			declaration.JE_OH_Supplier = organisation.PK;
			var packingList = declaration.LoadOrCreateCusPackingList(Factory);
			var cusPackage = packingList.PackageJob.Packages.AddNew();
			var validation = (CusPackageValidationForUnfinalisedPackageJob)cusPackage.Validation;

			CombineAssertions(() =>
			{
				validation.ValidateCustomDecimal1();
				AssertHasErrorContaining("Has Error", cusPackage.CustomDecimal1Info, MandatoryValidation.MustBeEntered);

				customDecimal1.OT_IsMandatory = false;
				validation.ValidateCustomDecimal1();
				AssertNoErrorContaining("No Error", cusPackage.CustomDecimal1Info, MandatoryValidation.MustBeEntered);
			});
		}

		public void TestCheckCustomDecimal2()
		{
			var customDecimal2 = organisation.CustomLabels.AddNew();
			customDecimal2.OT_FieldName = Core.Constants.CustomLabels.CusPackage.CustomDecimal2;
			customDecimal2.OT_IsMandatory = true;

			declaration.JE_OH_Supplier = organisation.PK;
			var packingList = declaration.LoadOrCreateCusPackingList(Factory);
			var cusPackage = packingList.PackageJob.Packages.AddNew();
			var validation = (CusPackageValidationForUnfinalisedPackageJob)cusPackage.Validation;

			CombineAssertions(() =>
			{
				validation.ValidateCustomDecimal2();
				AssertHasErrorContaining("Has Error", cusPackage.CustomDecimal2Info, MandatoryValidation.MustBeEntered);
				customDecimal2.OT_IsMandatory = false;
				validation.ValidateCustomDecimal2();
				AssertNoErrorContaining("No Error", cusPackage.CustomDecimal2Info, MandatoryValidation.MustBeEntered);
			});
		}

		public void TestCheckNetWeight()
		{
			var packingList = declaration.LoadOrCreateCusPackingList(Factory);
			var packageJob = packingList.PackageJob;
			var cusPackage = packageJob.Packages.AddNew();

			var item = packingList.PackableItems.AddNew();
			item.CUI_PackableQty = 10m;
			item.CUI_NetWeight = 20m;
			item.CUI_NetWeightUQ = Core.Constants.Weight.Kilograms;
			var validation = (CusPackageValidationForUnfinalisedPackageJob)cusPackage.Validation;

			CombineAssertions(() =>
			{
				cusPackage.CustomsPackItem(item, 5m);
				validation.ValidateNetWeight();
				AssertNoWarning("No warning", cusPackage.NetWeightInfo, "The sum of Packed Item Net Weight 15 KG does not balance with the Invoice Line Net Weight 10.0 KG.");

				cusPackage.NetWeight = 15m;
				validation.ValidateNetWeight();
				AssertHasWarning("Net weight mismatch warning", cusPackage.NetWeightInfo, "The sum of Packed Item Net Weight 15 KG does not balance with the Invoice Line Net Weight 10.0 KG.");
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
