using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using RefCusCodeListTypesCodes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class ExportInvoiceLinePackageValidationTest : TestCaseWithFactory
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

	public void TestCheckRuleR0219()
	{
		var messageError = "(R0219) Either all pack quantity should be equal to '0' Or all pack quantity should be greater than '0'.";

		var basePackage2 = packageGroup.Packages.AddNew();
		basePackage2.CW_PackType = "A";
		var linePackage2 = invoiceLine.PackagesForInvoiceLinesForBindingOnly.AddNew();
		linePackage2.Package = basePackage2;

		var packageBulk = packageGroup.Packages.AddNew();
		packageBulk.CW_PackType = "VO";
		var bulkLinePackage = invoiceLine.PackagesForInvoiceLinesForBindingOnly.AddNew();
		bulkLinePackage.Package = packageBulk;

		var packageBreakBulk = packageGroup.Packages.AddNew();
		packageBreakBulk.CW_PackType = "NE";
		var breakBulkLinePackage = invoiceLine.PackagesForInvoiceLinesForBindingOnly.AddNew();
		breakBulkLinePackage.Package = packageBreakBulk;

		bulkLinePackage.IsLinked = true;
		bulkLinePackage.PackQty = 0;
		linePackage.IsLinked = true;
		linePackage.PackQty = 0;
		linePackage2.IsLinked = true;
		linePackage2.PackQty = 0;
		breakBulkLinePackage.IsLinked = true;
		breakBulkLinePackage.PackQty = 0;
		CombineAssertions(() =>
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.AESTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, false))
			{
				AssertR0219NoMessageError("all PackQty are 0");

				linePackage.PackQty = 2;
				AssertR0219HasMessageError("linePackage PackQty != 0 when at least 1 linked package PackQty is 0");

				linePackage2.PackQty = 5;
				breakBulkLinePackage.PackQty = 6;
				AssertR0219NoMessageError("Bulk package is always 0 so no message error should exist if only Bulk code is 0 when rest is not");

				linePackage2.IsLinked = false;
				breakBulkLinePackage.IsLinked = false;
				linePackage.PackQty = 0;
				AssertNoMessageError("linePackage - linked but other related packages are not linked", linePackage.PackQtyInfo, messageError);
				linePackage2.Validation.ValidatePackQty();
				AssertNoMessageError("linePackage2 - not linked", linePackage2.PackQtyInfo, messageError);
				breakBulkLinePackage.Validation.ValidatePackQty();
				AssertNoMessageError("breakBulkLinePackage - not linked", breakBulkLinePackage.PackQtyInfo, messageError);
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.AESTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, true))
			{
				linePackage.IsLinked = true;
				linePackage.PackQty = 2;
				linePackage2.IsLinked = true;
				linePackage2.PackQty = 0;
				breakBulkLinePackage.IsLinked = true;
				breakBulkLinePackage.PackQty = 0;
				AssertR0219NoMessageError("In Transition Period - R0219 should be suspended");
			}
		});

		void AssertR0219NoMessageError(ZString assertMessage)
		{
			linePackage.Validation.ValidatePackQty();
			AssertNoMessageError($"linePackage - {assertMessage}", linePackage.PackQtyInfo, messageError);
			linePackage2.Validation.ValidatePackQty();
			AssertNoMessageError($"linePackage2 - {assertMessage}", linePackage2.PackQtyInfo, messageError);
			bulkLinePackage.Validation.ValidatePackQty();
			AssertNoMessageError($"bulkLinePackage - {assertMessage}", bulkLinePackage.PackQtyInfo, messageError);
			breakBulkLinePackage.Validation.ValidatePackQty();
			AssertNoMessageError($"breakBulkLinePackage - {assertMessage}", breakBulkLinePackage.PackQtyInfo, messageError);
		}

		void AssertR0219HasMessageError(ZString assertMessage)
		{
			linePackage.Validation.ValidatePackQty();
			AssertHasMessageError($"linePackage - {assertMessage}", linePackage.PackQtyInfo, messageError);
			linePackage2.Validation.ValidatePackQty();
			AssertHasMessageError($"linePackage2 - {assertMessage}", linePackage2.PackQtyInfo, messageError);
			bulkLinePackage.Validation.ValidatePackQty();
			AssertNoMessageError($"bulkLinePackage - {assertMessage} - bulk package is excluded from R0219", bulkLinePackage.PackQtyInfo, messageError);
			breakBulkLinePackage.Validation.ValidatePackQty();
			AssertHasMessageError($"breakBulkLinePackage - {assertMessage}", breakBulkLinePackage.PackQtyInfo, messageError);
		}
	}

	public void TestCheckRuleRuleR0220()
	{
		var messageError = "[R0220] Pack Quantity ‘0’ is invalid for Pack Type NE, NF and NG";

		CombineAssertions(() =>
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.AESTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, false))
			{
				basePackage.CW_PackType = "NE";
				linePackage.PackQty = 1;
				AssertNoMessageError("Outside Transition Period - BreakBulk code PackQty is 0", linePackage.PackQtyInfo, messageError);

				linePackage.PackQty = 0;
				AssertHasMessageError("Outside Transition Period - BreakBulk code PackQty not 0", linePackage.PackQtyInfo, messageError);

				linePackage.IsLinked = false;
				AssertNoMessageError("Outside Transition Period - Not Linked", linePackage.PackQtyInfo, messageError);

				linePackage.IsLinked = true;
				basePackage.CW_PackType = "AA";
				linePackage.Validation.ValidatePackQty();
				AssertNoMessageError("Outside Transition Period - BreakBulk code PackQty not 0", linePackage.PackQtyInfo, messageError);
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.AESTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, true))
			{
				basePackage.CW_PackType = "NE";
				linePackage.PackQty = 0;
				linePackage.Validation.ValidatePackQty();
				AssertNoMessageError("In Transition Period - BreakBulk code outside transition period", linePackage.PackQtyInfo, messageError);
			}
		});
	}

	public void TestIsSharedGoodsPackageAllowed_OutsideAESTransitionPeriod()
	{
		var validation = new InvoiceLinePackageValidationForTest(linePackage, invoiceLine);
		using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.AESTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, false))
		{
			CombineAssertions(() =>
			{
				basePackage.CW_PackType = "AA";
				AssertEquals("AESTransitionPeriod is off, but package is not of type break bulk", true, validation.IsSharedGoodsPackageAllowed_Exposed);

				basePackage.CW_PackType = "NE";
				AssertEquals("AESTransitionPeriod is off, but package is of type break bulk", false, validation.IsSharedGoodsPackageAllowed_Exposed);

				basePackage.CW_PackType = "VO";
				AssertEquals("AESTransitionPeriod is off, but package is of type bulk", true, validation.IsSharedGoodsPackageAllowed_Exposed);
			});
		}
	}

	public void TestIsSharedGoodsPackageAllowed_InAESTransitionPeriod()
	{
		using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.AESTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, true))
		{
			AssertEquals("AESTransitionPeriod is On", true, new InvoiceLinePackageValidationForTest(linePackage, invoiceLine).IsSharedGoodsPackageAllowed_Exposed);
		}
	}

	void SetupPackageTypes()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypesCodes.UnitedNationsPackageTypes, "A");
		helper.CreateCusCodeListWithAttribute(RefDataGroupingCodes.UnitedNationsRecommendations, RefCusCodeListTypesCodes.UnitedNationsPackageTypes,
			"VO", "VO", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.Bulk, "");
		helper.CreateCusCodeListWithAttribute(RefDataGroupingCodes.UnitedNationsRecommendations, RefCusCodeListTypesCodes.UnitedNationsPackageTypes,
			"NE", "NE", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.BreakBulk, "");
		helper.CreateNewOrGetExistingDataGrouping(RefDataGroupingCodes.UnitedNationsRecommendations);

		Factory.Save();
	}

	protected override void SetUp()
	{
		SetupPackageTypes();

		dec = Factory.New<JobDeclaration>();
		packageGroup = dec.Bills.AddNew().PackingGroups.AddNew();
		dec.JE_MessageType = JobMessageTypeList.Codes.Export;
		basePackage = packageGroup.Packages.AddNew();
		header = dec.Invoices.AddNew();
		invoiceLine = header.InvoiceLines.AddNew();
		linePackage = invoiceLine.PackagesForInvoiceLinesForBindingOnly.AddNew();
		linePackage.Package = basePackage;
	}

	JobDeclaration dec;
	JobComInvoiceHeader header;
	JobComInvoiceLine invoiceLine;
	BasePackage basePackage;
	BaseCusLinkPackage linePackage;
	BasePackingGroup packageGroup;

	sealed class InvoiceLinePackageValidationForTest : ExportInvoiceLinePackageValidation
	{
		public InvoiceLinePackageValidationForTest(BaseCusLinkPackage package, BaseJobComInvoiceLine invoiceLine) : base(package, invoiceLine)
		{
		}

		public ZBool IsSharedGoodsPackageAllowed_Exposed => IsSharedGoodsPackageAllowed;
	}
}
