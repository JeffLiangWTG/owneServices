using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.PL;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

sealed class ExitSummaryJobComInvoiceLineValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckJI_InvoiceQuantity() => CombineAssertions(() =>
	{
		invoiceLine.JI_InvoiceQuantity = -1111111111111.1111M;
		AssertHasMessageError("Negative Customs Invoice Quantity", invoiceLine.JI_InvoiceQuantityInfo, GetQuantityLengthValidationErrorMessage(invoiceLine.JI_InvoiceQuantityInfo.HumanReadableName, invoiceLine.JI_InvoiceQuantity));

		invoiceLine.JI_InvoiceQuantity = 0M;
		AssertNoNotifications("Zero Invoice Quantity", invoiceLine.JI_InvoiceQuantityInfo);

		invoiceLine.JI_InvoiceQuantity = 1111111111111.1111M;
		AssertHasMessageError("Positive Invoice Quantity", invoiceLine.JI_InvoiceQuantityInfo, GetQuantityLengthValidationErrorMessage(invoiceLine.JI_InvoiceQuantityInfo.HumanReadableName, invoiceLine.JI_InvoiceQuantity));
	});

	public void TestCheckJI_InvoiceUQ() => CombineAssertions(() =>
	{
		invoiceLine.JI_InvoiceUQ = "ZZ";
		AssertNoNotifications("Invalid Invoice Quantity UoM", invoiceLine.JI_InvoiceUQInfo);

		invoiceLine.JI_InvoiceUQ = "KG";
		AssertNoNotifications("Valid Invoice Quantity UoM", invoiceLine.JI_InvoiceUQInfo);

		invoiceLine.JI_InvoiceUQ = ZString.Empty;
		AssertNoNotifications("Empty Invoice Quantity UoM", invoiceLine.JI_InvoiceUQInfo);
	});

	public void TestCheckJI_CountryOfOrigin() => CombineAssertions(() =>
	{
		invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;
		AssertNoNotifications("Valid Country of Origin", invoiceLine.JI_CountryOfOriginInfo);

		invoiceLine.JI_CountryOfOrigin = "ZZ";
		AssertHasMessageError("Invalid Country of Origin", invoiceLine.JI_CountryOfOriginInfo, ListValidation.InvalidCodeMessageError);

		invoiceLine.JI_CountryOfOrigin = ZString.Empty;
		AssertNoNotifications("Invalid Country of Origin", invoiceLine.JI_CountryOfOriginInfo);
	});

	public void TestCheckJI_SupplementaryCode1() => CombineAssertions(() =>
	{
		const string errorMessage = "Supplementary Code must be 4 characters long if supplied";

		invoiceLine.JI_SupplementaryCode1 = "11";
		AssertHasMessageError("Invalid Supplementary Code 1", invoiceLine.JI_SupplementaryCode1Info, errorMessage);

		invoiceLine.JI_SupplementaryCode1 = ZString.Empty;
		AssertNoNotifications("Empty Supplementary Code 1", invoiceLine.JI_SupplementaryCode1Info);
	});

	public void TestCheckJI_SupplementaryCode2() => CombineAssertions(() =>
	{
		const string errorMessage = "Supplementary Code must be 4 characters long if supplied";

		invoiceLine.JI_SupplementaryCode2 = "22";
		AssertHasMessageError("Invalid Supplementary Code 2", invoiceLine.JI_SupplementaryCode2Info, errorMessage);

		invoiceLine.JI_SupplementaryCode2 = ZString.Empty;
		AssertNoNotifications("Empty Supplementary Code 2", invoiceLine.JI_SupplementaryCode2Info);
	});

	public void TestCheckJI_Procedure() => CombineAssertions(() =>
	{
		const string errorMessage = "When procedure code starts with 71, the associated entry instruction can't have guarantee(s)";

		invoiceLine.JI_Procedure = ZString.Empty;
		AssertNoNotifications("Empty Procedure Code", invoiceLine.JI_ProcedureInfo);

		invoiceLine.EntryInstruction.CEI_Procedure = "10";
		invoiceLine.JI_Procedure = "7100";
		AssertNoNotifications("Entry Instruction w/o Guarantees", invoiceLine.JI_ProcedureInfo);

		var guaranteeBondDetail = invoiceLine.EntryInstruction.Guarantees.AddNew();
		guaranteeBondDetail.PW_BondType = GuaranteeBondTypeList.Codes.Comprehensive;
		guaranteeBondDetail.PW_BondNumber = "BN";
		invoiceLine.Validation.ValidateJI_Procedure();
		AssertHasMessageError("Entry Instruction w/ Guarantees", invoiceLine.JI_ProcedureInfo, errorMessage);
	});

	public void TestCheckProcedureCodeBase() => CombineAssertions(() =>
	{
		const string errorMessage = "Requested Customs Procedure Code is invalid.";

		invoiceLine.ProcedureCodeBase = "20";
		AssertHasMessageError("Invalid Procedure Code Base", invoiceLine.ProcedureCodeBaseInfo, errorMessage);

		invoiceLine.ProcedureCodeBase = ZString.Empty;
		AssertNoNotifications("Empty Procedure Code Base", invoiceLine.ProcedureCodeBaseInfo);
	});

	public void TestCheckPreviousProcedureCode() => CombineAssertions(() =>
	{
		const string errorMessage = "Previous Customs Procedure Code is invalid.";

		invoiceLine.PreviousProcedureCode = "20";
		AssertHasMessageError("Invalid Procedure Code Base", invoiceLine.PreviousProcedureCodeInfo, errorMessage);

		invoiceLine.PreviousProcedureCode = ZString.Empty;
		AssertNoNotifications("Empty Procedure Code Base", invoiceLine.PreviousProcedureCodeInfo);
	});

	public void TestCheckJI_CustomsQuantity() => CombineAssertions(() =>
	{
		invoiceLine.JI_CustomsQuantity = -1111111111111.1111M;
		AssertHasMessageError("Negative Customs Quantity", invoiceLine.JI_CustomsQuantityInfo, GetQuantityLengthValidationErrorMessage(invoiceLine.JI_CustomsQuantityInfo.HumanReadableName, invoiceLine.JI_CustomsQuantity));

		invoiceLine.JI_CustomsQuantity = 0M;
		AssertNoNotifications("Zero Customs Quantity", invoiceLine.JI_CustomsQuantityInfo);

		invoiceLine.JI_CustomsQuantity = 1111111111111.1111M;
		AssertHasMessageError("Positive Customs Quantity", invoiceLine.JI_CustomsQuantityInfo, GetQuantityLengthValidationErrorMessage(invoiceLine.JI_CustomsQuantityInfo.HumanReadableName, invoiceLine.JI_CustomsQuantity));
	});

	public void TestCheckJI_CustomsUnitQty() => CombineAssertions(() =>
	{
		invoiceLine.JI_CustomsUnitQty = "ZZ";
		AssertHasMessageError("Invalid Customs Quantity UoM", invoiceLine.JI_CustomsUnitQtyInfo, ListValidation.InvalidCodeMessageError);

		invoiceLine.JI_CustomsUnitQty = "KG";
		AssertNoNotifications("Valid Customs Quantity UoM", invoiceLine.JI_CustomsUnitQtyInfo);

		invoiceLine.JI_CustomsUnitQty = ZString.Empty;
		AssertNoNotifications("Empty Customs Quantity UoM", invoiceLine.JI_CustomsUnitQtyInfo);
	});

	public void TestCheckJI_CustomsSecondQuantity() => CombineAssertions(() =>
	{
		invoiceLine.JI_CustomsSecondQuantity = -1111111111111.1111M;
		AssertHasMessageError("Negative Customs 2nd Quantity", invoiceLine.JI_CustomsSecondQuantityInfo, GetQuantityLengthValidationErrorMessage(invoiceLine.JI_CustomsSecondQuantityInfo.HumanReadableName, invoiceLine.JI_CustomsSecondQuantity));

		invoiceLine.JI_CustomsSecondQuantity = 0M;
		AssertNoNotifications("Zero Customs 2nd Quantity", invoiceLine.JI_CustomsSecondQuantityInfo);

		invoiceLine.JI_CustomsSecondQuantity = 1111111111111.1111M;
		AssertHasMessageError("Positive Customs 2nd Quantity", invoiceLine.JI_CustomsSecondQuantityInfo, GetQuantityLengthValidationErrorMessage(invoiceLine.JI_CustomsSecondQuantityInfo.HumanReadableName, invoiceLine.JI_CustomsSecondQuantity));
	});

	public void TestCheckJI_CustomsSecondUnitQty() => CombineAssertions(() =>
	{
		invoiceLine.JI_CustomsSecondUnitQty = "ZZ";
		AssertHasMessageError("Invalid Customs 2nd Quantity UoM", invoiceLine.JI_CustomsSecondUnitQtyInfo, ListValidation.InvalidCodeMessageError);

		invoiceLine.JI_CustomsSecondUnitQty = "KG";
		AssertNoNotifications("Valid Customs 2nd Quantity UoM", invoiceLine.JI_CustomsSecondUnitQtyInfo);

		invoiceLine.JI_CustomsUnitQty = ZString.Empty;
		AssertNoNotifications("Empty Customs 2nd Quantity UoM", invoiceLine.JI_CustomsSecondUnitQtyInfo);
	});

	public void TestCheckJI_CustomsThirdQuantity() => CombineAssertions(() =>
	{
		invoiceLine.JI_CustomsThirdQuantity = -1111111111111.1111M;
		AssertHasMessageError("Negative Customs 3rd Quantity", invoiceLine.JI_CustomsThirdQuantityInfo, GetQuantityLengthValidationErrorMessage(invoiceLine.JI_CustomsThirdQuantityInfo.HumanReadableName, invoiceLine.JI_CustomsThirdQuantity));

		invoiceLine.JI_CustomsThirdQuantity = 0M;
		AssertNoNotifications("Zero Customs 3rd Quantity", invoiceLine.JI_CustomsThirdQuantityInfo);

		invoiceLine.JI_CustomsThirdQuantity = 1111111111111.1111M;
		AssertHasMessageError("Positive Customs 3rd Quantity", invoiceLine.JI_CustomsThirdQuantityInfo, GetQuantityLengthValidationErrorMessage(invoiceLine.JI_CustomsThirdQuantityInfo.HumanReadableName, invoiceLine.JI_CustomsThirdQuantity));
	});

	public void TestCheckJI_CustomsThirdUnitQty() => CombineAssertions(() =>
	{
		invoiceLine.JI_CustomsThirdUnitQty = "ZZ";
		AssertHasMessageError("Invalid Customs 3rd Quantity UoM", invoiceLine.JI_CustomsThirdUnitQtyInfo, ListValidation.InvalidCodeMessageError);

		invoiceLine.JI_CustomsThirdUnitQty = "KG";
		AssertNoNotifications("Valid Customs 3rd Quantity UoM", invoiceLine.JI_CustomsThirdUnitQtyInfo);

		invoiceLine.JI_CustomsThirdUnitQty = ZString.Empty;
		AssertNoNotifications("Empty Customs 3rd Quantity UoM", invoiceLine.JI_CustomsThirdUnitQtyInfo);
	});

	static string GetQuantityLengthValidationErrorMessage(ZString propertyName, ZDecimal value) =>
		FormattableString.Invariant($"The number {value} is too large, the maximum number of digits allowed for {propertyName} is 16.");

	protected override void SetUp()
	{
		base.SetUp();

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = PLJobMessageTypeList.Codes.ExitSummary;
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var invoiceHeader = declaration.Invoices.AddNew();
		invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;

		SetupCustomsUQReferenceData();
	}

	void SetupCustomsUQReferenceData()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "Customs Declaration Units of Quantity");
		helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "KG", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(1));
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Poland, "Poland", helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "EUN"));
		Factory.Save();
	}

	JobComInvoiceLine invoiceLine;
}
