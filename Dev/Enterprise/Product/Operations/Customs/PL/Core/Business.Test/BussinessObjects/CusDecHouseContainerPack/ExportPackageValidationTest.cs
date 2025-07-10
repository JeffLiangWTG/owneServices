using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using JobDeclaration = Enterprise.Customs.PL.Business.Declaration.JobDeclaration;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class ExportPackageValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckCW_PackQtyNotInAESTransitionPeriod()
	{
		const string errorMessage = "At least one Package line under Invoice line must have Package Quantity greater than 0 for same Package Type (non BULK+BREAKBULK) and Shipping Marks.";
		SetupPackTypes();
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		var package = declaration.Packages.AddNew();
		var linePackage = CreateNewLinkedPackLine(declaration, package);
		CombineAssertions(() =>
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.AESTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, false))
			{
				package.CW_PackType = "AB";
				linePackage.PackQty = 0;
				package.Validation.ValidateCW_PackQty();
				AssertHasMessageError("Not in AESTransitionPeriod, Non-empty Pack Type, all pack lines are empty", package.CW_PackQtyInfo, errorMessage);
				package.CW_PackType = "VQ";
				package.Validation.ValidateCW_PackQty();
				AssertNoMessageError("Not in AESTransitionPeriod, Empty (BULK) Pack Type", package.CW_PackQtyInfo, errorMessage);
				package.CW_PackType = "NE";
				package.Validation.ValidateCW_PackQty();
				AssertNoMessageError("Not in AESTransitionPeriod, Empty (BREAKBULK) Pack Type", package.CW_PackQtyInfo, errorMessage);
				package.CW_PackType = "AB";
				var linePackage2 = CreateNewLinkedPackLine(declaration, package);
				linePackage2.PackQty = 2;
				package.Validation.ValidateCW_PackQty();
				AssertNoMessageError("Not in AESTransitionPeriod, Non-empty Pack Type, exists pack line with quantity > 0", package.CW_PackQtyInfo, errorMessage);
			}
		});
	}

	public void TestCheckCW_PackQtyInAESTransitionPeriod()
	{
		const string errorMessage = "At least one Package line under Invoice line must have Package Quantity greater than 0 for same Package Type (non BULK+BREAKBULK) and Shipping Marks.";
		SetupPackTypes();
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		var package = declaration.Packages.AddNew();
		var linePackage = CreateNewLinkedPackLine(declaration, package);
		CombineAssertions(() =>
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.AESTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, true))
			{
				package.CW_PackType = "AB";
				linePackage.PackQty = 0;
				package.Validation.ValidateCW_PackQty();
				AssertEquals("In AESTransitionPeriod, Non-empty Pack Type, all pack lines are empty", false, package.CW_PackQtyInfo.HasMessageError(errorMessage));
			}
		});
	}

	void SetupPackTypes()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "Description");
		helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
			"VQ", "VQ", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.Bulk, "");
		helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
			"NE", "NE", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.BreakBulk, "");
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations);
		Factory.Save();
	}

	BaseCusLinkPackage CreateNewLinkedPackLine(JobDeclaration declaration, Package package)
	{
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		return invoiceLine.PackagesForInvoiceLinesForBindingOnly.Cast<InvoiceLineCusLinkPackage>().First(x => x.Package == package);
	}
}
