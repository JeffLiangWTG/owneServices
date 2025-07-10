using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

class InvoiceLinePackageValidationTest : BusinessObjectValidationTestCase
{
	public void TestPackQtyValidationNotExecutedWhenNotLinked()
	{
		CombineAssertions(() =>
		{
			linePackage.PackQty = 0;
			linePackage.IsLinked = false;
			basePackage.CW_PackType = "AA";
			linePackage.Validation.ValidatePackQty();
			AssertNoNotifications("Normal Package - When package is not linked, PackQty should not be validated", linePackage.PackQtyInfo);

			basePackage.CW_PackType = "NE";
			linePackage.Validation.ValidatePackQty();
			AssertNoNotifications("BreakBulk Package - When package is not linked, PackQty should not be validated", linePackage.PackQtyInfo);

			basePackage.CW_PackType = "VO";
			linePackage.Validation.ValidatePackQty();
			AssertNoNotifications("Bulk Package - When package is not linked, PackQty should not be validated", linePackage.PackQtyInfo);
		});
	}

	public void TestCheckPackQty_NotEmptyCW_MarksAndNos()
	{
		var messageError = "You have not entered a pack quantity or marks are empty in case of goods packed together.";
		var linePackage = invoiceLine.PackagesForInvoiceLinesForBindingOnly.AddNew();
		linePackage.Package = basePackage;
		basePackage.CW_MarksAndNos = "ASD";

		CombineAssertions(() =>
		{
			foreach (var packType in bulkPackageCodeList)
			{
				basePackage.CW_PackType = packType;

				linePackage.IsLinked = false;
				linePackage.Validation.ValidatePackQty();
				AssertNoMessageError($"{packType} not linked", linePackage.PackQtyInfo, messageError);

				linePackage.IsLinked = true;
				linePackage.PackQty = 1;
				linePackage.Validation.ValidatePackQty();
				AssertNoMessageError($"{packType} linked 1", linePackage.PackQtyInfo, messageError);

				linePackage.PackQty = 0;
				linePackage.Validation.ValidatePackQty();
				AssertNoMessageError($"{packType} linked 0", linePackage.PackQtyInfo, messageError);
			}

			basePackage.CW_PackType = "AA";

			linePackage.IsLinked = false;
			linePackage.Validation.ValidatePackQty();
			AssertNoMessageError("Not bulk code, not linked", linePackage.PackQtyInfo, messageError);

			linePackage.IsLinked = true;
			linePackage.PackQty = 1;
			linePackage.Validation.ValidatePackQty();
			AssertNoMessageError("Not bulk code, linked 1", linePackage.PackQtyInfo, messageError);

			linePackage.PackQty = 0;
			linePackage.Validation.ValidatePackQty();
			AssertHasMessageError("Not bulk code, linked 0", linePackage.PackQtyInfo, messageError);

			var linePackage2 = invoiceLine.PackagesForInvoiceLinesForBindingOnly.AddNew();
			linePackage2.Package = basePackage;
			linePackage2.IsLinked = true;
			linePackage2.PackQty = 1;
			linePackage.Validation.ValidatePackQty();
			AssertNoMessageError("PackQty not empty on other invoice line", linePackage.PackQtyInfo, messageError);
			linePackage2.Validation.ValidatePackQty();
			AssertNoMessageError("PackQty not empty with empty on other invoice line", linePackage2.PackQtyInfo, messageError);
		});
	}

	public void TestCheckPackQty_EmptyCW_MarksAndNos()
	{
		var messageError = "You have not entered a pack quantity or marks are empty in case of goods packed together.";

		var linePackage = invoiceLine.PackagesForInvoiceLinesForBindingOnly.AddNew();
		linePackage.Package = basePackage;
		basePackage.CW_MarksAndNos = ZString.Empty;

		CombineAssertions(() =>
		{
			foreach (var packType in bulkPackageCodeList)
			{
				basePackage.CW_PackType = packType;

				linePackage.IsLinked = false;
				linePackage.Validation.ValidatePackQty();
				AssertNoMessageError($"{packType} not linked", linePackage.PackQtyInfo, messageError);

				linePackage.IsLinked = true;
				linePackage.PackQty = 1;
				linePackage.Validation.ValidatePackQty();
				AssertNoMessageError($"{packType} linked 1", linePackage.PackQtyInfo, messageError);

				linePackage.PackQty = 0;
				linePackage.Validation.ValidatePackQty();
				AssertNoMessageError($"{packType} linked 0", linePackage.PackQtyInfo, messageError);
			}

			basePackage.CW_PackType = "AA";

			linePackage.IsLinked = false;
			linePackage.Validation.ValidatePackQty();
			AssertNoMessageError("Not bulk code, not linked", linePackage.PackQtyInfo, messageError);

			linePackage.IsLinked = true;
			linePackage.PackQty = 1;
			linePackage.Validation.ValidatePackQty();
			AssertNoMessageError("Not bulk code, linked 1", linePackage.PackQtyInfo, messageError);

			linePackage.PackQty = 0;
			linePackage.Validation.ValidatePackQty();
			AssertHasMessageError("Not bulk code, linked 0", linePackage.PackQtyInfo, messageError);
		});
	}

	public void TestTestCheckPackQty_IsSharedGoodsPackageAllowed()
	{
		var messageError = "You have not entered a pack quantity or marks are empty in case of goods packed together.";
		basePackage.CW_MarksAndNos = ZString.Empty;
		basePackage.CW_PackType = "AA";

		var validation = new InvoiceLinePackageValidationForTest(linePackage, invoiceLine);
		validation.IsSharedGoodsPackageAllowed_Override = true;
		CombineAssertions(() =>
		{
			linePackage.IsLinked = true;
			linePackage.PackQty = 0;
			validation.ValidatePackQty();
			AssertHasMessageError("Validation for shared packages should be enabled", linePackage.PackQtyInfo, messageError);

			validation.IsSharedGoodsPackageAllowed_Override = false;
			validation.ValidatePackQty();
			AssertNoMessageError("Validation for shared packages should be disabled", linePackage.PackQtyInfo, messageError);
		});
	}

	readonly List<ZString> bulkPackageCodeList = new List<ZString>() { "VQ", "VG", "VL", "VY", "VR", "VO", "VS" };

	void SetupPackageTypes()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "A");
		helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
			"VO", "VO", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.Bulk, "");
		helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
			"NE", "NE", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.BreakBulk, "");
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations);

		Factory.Save();
	}

	protected override void SetUp()
	{
		SetupPackageTypes();

		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		basePackage = declaration.Packages.AddNew();
		header = declaration.Invoices.AddNew();
		invoiceLine = header.InvoiceLines.AddNew();
		linePackage = invoiceLine.PackagesForInvoiceLinesForBindingOnly.AddNew();
		linePackage.Package = basePackage;
		linePackage.IsLinked = true;
	}

	protected JobDeclaration declaration;
	protected JobComInvoiceHeader header;
	protected JobComInvoiceLine invoiceLine;
	protected BasePackage basePackage;
	protected BaseCusLinkPackage linePackage;

	sealed class InvoiceLinePackageValidationForTest : InvoiceLinePackageValidation
	{
		public InvoiceLinePackageValidationForTest(BaseCusLinkPackage package, BaseJobComInvoiceLine invoiceLine) : base(package, invoiceLine)
		{
		}

		protected override ZBool IsSharedGoodsPackageAllowed => IsSharedGoodsPackageAllowed_Override;

		public ZBool IsSharedGoodsPackageAllowed_Override { get; set; }
	}
}
