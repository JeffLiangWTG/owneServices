using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

public class JobComInvoiceLineValidationTest : BusinessObjectValidationTestCase
{
	public void TestParent()
	{
		var parent = Factory.New<JobComInvoiceLine>();
		AssertEquals(parent.Validation.Parent, parent);
	}

	public void TestCheckJI_Procedure()
	{
		var declaration = Factory.New<JobDeclaration>();
		var instruction = declaration.CustomsEntryInstructions.AddNew();
		var invoiceHeader = declaration.Invoices.AddNew();
		var line = invoiceHeader.InvoiceLines.AddNew();
		line.JI_CEI = instruction.PK;
		var messageError2 = "When procedure code starts with 71, the associated entry instruction can't have guarantee(s)";

		instruction.CEI_Procedure = "10";
		CombineAssertions(() =>
		{
			line.JI_Procedure = "4000";
			AssertNoMessageError(line.JI_ProcedureInfo, messageError2);

			var guaranteeBondDetail = instruction.Guarantees.AddNew();
			guaranteeBondDetail.PW_BondType = GuaranteeBondTypeList.Codes.Comprehensive;
			guaranteeBondDetail.PW_BondNumber = "BN";
			line.JI_Procedure = "7100";
			AssertHasMessageError(line.JI_ProcedureInfo, messageError2);
		});
	}

	#region JI_CEI

	public void TestCheckJI_CEI()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		invoiceLine.JI_Procedure = "7100";
		AssertNoMessageErrors(invoiceLine.JI_CEIInfo);

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var guaranteeBondDetail = entryInstruction.Guarantees.AddNew();
		guaranteeBondDetail.PW_BondType = GuaranteeBondTypeList.Codes.Comprehensive;
		guaranteeBondDetail.PW_BondNumber = "BN";
		invoiceLine.JI_CEI = entryInstruction.PK;
		AssertHasMessageError(invoiceLine.JI_CEIInfo, "When procedure code starts with 71, the associated entry instruction can't have guarantee(s)");
	}

	public void TestCheckUniqueEntryInstructionCurrencyUsed()
	{
		var messageError = "All Invoices on an Entry Instruction must have the same '[22] Currency'.";
		var declaration = Factory.New<JobDeclaration>();
		var invoice1 = declaration.Invoices.AddNew();
		var invoiceLine1 = invoice1.InvoiceLines.AddNew();
		var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
		invoiceLine1.JI_CEI = entryInstruction1.PK;
		var invoice2 = declaration.Invoices.AddNew();
		var invoiceLine2 = invoice2.InvoiceLines.AddNew();
		invoiceLine2.JI_CEI = entryInstruction1.PK;

		invoice1.JZ_RX_NKInvoice_Currency = "PLN";
		invoice2.JZ_RX_NKInvoice_Currency = "USD";
		AssertEquals(false, entryInstruction1.UniqueCurrencyUsed);
		AssertHasMessageError(invoiceLine1.JI_CEIInfo, messageError);
		AssertHasMessageError(invoiceLine2.JI_CEIInfo, messageError);
		invoiceLine1.Validation.ValidateJI_CEI();
		invoiceLine2.Validation.ValidateJI_CEI();
		AssertHasMessageError(invoiceLine1.JI_CEIInfo, messageError);
		AssertHasMessageError(invoiceLine2.JI_CEIInfo, messageError);

		invoice2.JZ_RX_NKInvoice_Currency = "PLN";
		invoiceLine1.Validation.ValidateJI_CEI();
		invoiceLine2.Validation.ValidateJI_CEI();
		AssertEquals(true, entryInstruction1.UniqueCurrencyUsed);
		AssertNoMessageError(invoiceLine1.JI_CEIInfo, messageError);
		AssertNoMessageError(invoiceLine2.JI_CEIInfo, messageError);
	}

	public void TestCheckEntryInstructionIsNotEmpty()
	{
		var messageError = "You have not selected an Entry Instruction";
		var declaration = Factory.New<JobDeclaration>();
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

		CombineAssertions("Should not be empty", () =>
		{
			invoiceLine.JI_CEI = entryInstruction.PK;
			AssertNoMessageError("It should not have message error when Entry Instruction of InvoiceLine is not empty", invoiceLine.JI_CEIInfo, messageError);

			invoiceLine.JI_CEI = ZGuid.Empty;
			AssertHasMessageError("It should have message error when Entry Instruction of InvoiceLine is empty", invoiceLine.JI_CEIInfo, messageError);
		});
	}

	#endregion

	public void TestCheckJI_DateForDutyOverride()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		invoiceLine.JI_DateForDutyOverride = new ZDateTime(ZDateTime.Now.AddDays(-2));
		AssertNoNotifications(invoiceLine.JI_DateForDutyOverrideInfo);

		invoiceLine.JI_DateForDutyOverride = new ZDateTime(ZDateTime.Now.AddDays(3));
		AssertHasWarningContaining(invoiceLine.JI_DateForDutyOverrideInfo, "Date is in the future");
	}

	public void TestCheckZG_CalculationDate2()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		invoiceLine.JI_ValuationDateOverride = new ZDateTime(ZDateTime.Now.AddDays(-2));
		AssertNoNotifications(invoiceLine.JI_ValuationDateOverrideInfo);

		invoiceLine.JI_ValuationDateOverride = new ZDateTime(ZDateTime.Now.AddDays(3));
		AssertHasWarningContaining(invoiceLine.JI_ValuationDateOverrideInfo, "Date is in the future");
	}

	public void TestCheckProcedureCodeBase() => CombineAssertions(() =>
	{
		AddRefCusProcedures();
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.MiscellaneousCustoms;
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();

		invoiceLine.PLValidationOrNull?.ValidateProcedureCodeBase();
		AssertNoNotifications("99: ", invoiceLine.ProcedureCodeBaseInfo);

		entryInstruction.CEI_Procedure = "99";
		invoiceLine.ProcedureCodeBase = "97";
		invoiceLine.PLValidationOrNull?.ValidateProcedureCodeBase();
		AssertHasMessageError("97:", invoiceLine.ProcedureCodeBaseInfo, "Requested Customs Procedure Code is invalid.");

		invoiceLine.ProcedureCodeBase = "98";
		invoiceLine.PLValidationOrNull?.ValidateProcedureCodeBase();
		AssertHasMessageError("98: ", invoiceLine.ProcedureCodeBaseInfo, "Requested Procedure Code must be the same as the Entry Instruction CPC.");

		invoiceLine.ProcedureCodeBase = "99";
		invoiceLine.PLValidationOrNull?.ValidateProcedureCodeBase();
		AssertNoErrors("99_2: ", invoiceLine.ProcedureCodeBaseInfo);
	});

	public void TestCheckPreviousProcedureCode() => CombineAssertions(() =>
	{
		AddRefCusProcedures();
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.MiscellaneousCustoms;
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();

		invoiceLine.ProcedureCodeBase = "99";
		invoiceLine.PLValidationOrNull?.ValidatePreviousProcedureCode();
		AssertNoNotifications("99: ", invoiceLine.PreviousProcedureCodeInfo);

		invoiceLine.PreviousProcedureCode = "20";
		invoiceLine.PLValidationOrNull?.ValidatePreviousProcedureCode();
		AssertHasMessageError("20: ", invoiceLine.PreviousProcedureCodeInfo, "Previous Customs Procedure Code is invalid.");

		invoiceLine.PreviousProcedureCode = "00";
		invoiceLine.PLValidationOrNull?.ValidatePreviousProcedureCode();
		AssertNoErrors("00: ", invoiceLine.PreviousProcedureCodeInfo);
	});

	void AddRefCusProcedures()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateRefCusProcedure("PL", ZString.Empty, "98", ZString.Empty, ZString.Empty, "JJ", "EXP");
		helper.CreateRefCusProcedure("PL", ZString.Empty, "99", ZString.Empty, ZString.Empty, "AB", "EXP");
		helper.CreateRefCusProcedure("PL", ZString.Empty, "99", "00", ZString.Empty, "AB - CD", "EXP");
		helper.CreateRefCusProcedure("PL", ZString.Empty, "99", "10", ZString.Empty, "AB - GH", "EXP");
		helper.CreateRefCusProcedure("PL", ZString.Empty, "99", "00", "1V1", "AB - CD - JK", "EXP");
		helper.CreateRefCusProcedure("PL", ZString.Empty, "99", "10", "1V1", "AB - GH - JK", "EXP");
		Factory.Save();
	}

	public void TestCheckJI_CustomsQuantity()
	{
		var messageError = "[38] Customs Qty must be less or equal [35] GWT";
		var declaration = Factory.New<JobDeclaration>();
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

		CombineAssertions(() =>
		{
			AssertNoMessageError("Empty Invoice Line", invoiceLine.JI_CustomsQuantityInfo, messageError);

			invoiceLine.JI_CustomsQuantity = 10m;
			invoiceLine.JI_CustomsUnitQty = "KGM";
			invoiceLine.JI_Weight = 9m;
			invoiceLine.Validation.ValidateJI_CustomsQuantity();
			AssertHasMessageError("JI_Weight no UQ and less than JI_CustomsQuantity", invoiceLine.JI_CustomsQuantityInfo, messageError);

			invoiceLine.JI_WeightUQ = "KG";
			invoiceLine.Validation.ValidateJI_CustomsQuantity();
			AssertHasMessageError("JI_Weight less than JI_CustomsQuantity", invoiceLine.JI_CustomsQuantityInfo, messageError);

			invoiceLine.JI_Weight = 10m;
			invoiceLine.Validation.ValidateJI_CustomsQuantity();
			AssertNoMessageError("JI_Weight equal JI_CustomsQuantity", invoiceLine.JI_CustomsQuantityInfo, messageError);

			invoiceLine.JI_Weight = 11m;
			invoiceLine.Validation.ValidateJI_CustomsQuantity();
			AssertNoMessageError("JI_Weight greater than JI_CustomsQuantity", invoiceLine.JI_CustomsQuantityInfo, messageError);
		});
	}

	public void TestCheckJI_CustomsSecondUnitQty()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		const string keyMessageErrorWord = "The UOM is duplicated for the invoice line.";

		CombineAssertions(() =>
		{
			AssertNoMessageError("Empty", invoiceLine.JI_CustomsSecondUnitQtyInfo, keyMessageErrorWord);

			SetSameUnitQty(invoiceLine);
			invoiceLine.Validation.ValidateJI_CustomsSecondUnitQty();
			AssertHasMessageError("Duplicate", invoiceLine.JI_CustomsSecondUnitQtyInfo, keyMessageErrorWord);

			invoiceLine.JI_CustomsSecondUnitQty = Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.Tonne;
			AssertNoMessageError("Different", invoiceLine.JI_CustomsSecondUnitQtyInfo, keyMessageErrorWord);
		});
	}

	public void TestCheckJI_CustomsThirdUnitQty()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		const string keyMessageErrorWord = "The UOM is duplicated for the invoice line.";

		CombineAssertions(() =>
		{
			AssertNoMessageError("Empty", invoiceLine.JI_CustomsThirdUnitQtyInfo, keyMessageErrorWord);

			SetSameUnitQty(invoiceLine);
			AssertHasMessageError("Duplicate", invoiceLine.JI_CustomsThirdUnitQtyInfo, keyMessageErrorWord);

			invoiceLine.JI_CustomsThirdUnitQty = Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.Tonne;
			AssertNoMessageError("Different", invoiceLine.JI_CustomsThirdUnitQtyInfo, keyMessageErrorWord);
		});
	}

	public void TestCheckJI_CustomsFourthUnitQty()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		const string keyMessageErrorWord = "The UOM is duplicated for the invoice line.";

		CombineAssertions(() =>
		{
			AssertNoMessageError("Empty", invoiceLine.JI_CustomsFourthUnitQtyInfo, keyMessageErrorWord);

			SetSameUnitQty(invoiceLine);
			AssertHasMessageError("Duplicate", invoiceLine.JI_CustomsFourthUnitQtyInfo, keyMessageErrorWord);

			invoiceLine.JI_CustomsFourthUnitQty = Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.Tonne;
			AssertNoMessageError("Different", invoiceLine.JI_CustomsFourthUnitQtyInfo, keyMessageErrorWord);
		});
	}

	public void TestCheckJI_CustomsFifthUnitQty()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		const string keyMessageErrorWord = "The UOM is duplicated for the invoice line.";

		CombineAssertions(() =>
		{
			AssertNoMessageError("Different", invoiceLine.JI_CustomsFifthUnitQtyInfo, keyMessageErrorWord);

			SetSameUnitQty(invoiceLine);
			AssertHasMessageError("Duplicate", invoiceLine.JI_CustomsFifthUnitQtyInfo, keyMessageErrorWord);

			invoiceLine.JI_CustomsFifthUnitQty = Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.Tonne;
			AssertNoMessageError("Different", invoiceLine.JI_CustomsFifthUnitQtyInfo, keyMessageErrorWord);
		});
	}

	void SetSameUnitQty(JobComInvoiceLine invoiceLine)
	{
		invoiceLine.JI_CustomsSecondUnitQty = Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.Kilogram;
		invoiceLine.JI_CustomsThirdUnitQty = Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.Kilogram;
		invoiceLine.JI_CustomsFourthUnitQty = Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.Kilogram;
		invoiceLine.JI_CustomsFifthUnitQty = Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.Kilogram;
	}

	public void TestAdditionalProcedureCodesCheckFirst4Characters() =>
		AssertEquals(false, new JobComInvLineValidationForTest(Factory.New<JobComInvoiceLine>()).AdditionalProcedureCodesCheckFirst4Characters_Exposed);

	public void TestCheckJI_CountryOfOrigin()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		var messageError = ListValidation.InvalidCodeMessageError.ToString();
		CombineAssertions(() =>
		{
			AssertNoMessageErrorContaining("Empty declaration", invoiceLine.JI_CountryOfOriginInfo, messageError);

			invoiceLine.JI_CountryOfOrigin = "1";
			AssertHasMessageErrorContaining("Invalid code", invoiceLine.JI_CountryOfOriginInfo, messageError);

			invoiceLine.JI_CountryOfOrigin = CountryCodes.Poland;
			AssertNoMessageErrorContaining("Valid code", invoiceLine.JI_CountryOfOriginInfo, messageError);
		});
	}

	public void TestCheckJI_AdditionalSupplements() => CombineAssertions(() =>
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		var additionalSupplementaryCode = invoiceLine.AdditionalSupplementaryCodes.AddNew();

		invoiceLine.PLValidationOrNull?.ValidateJI_AdditionalSupplements();
		AssertNoNotifications("No notifications", invoiceLine.JI_AdditionalSupplementsInfo);

		additionalSupplementaryCode.CY_Code = "mmm";
		invoiceLine.PLValidationOrNull?.ValidateJI_AdditionalSupplements();
		AssertHasNotifications("There should be 1 notification", invoiceLine.JI_AdditionalSupplementsInfo);
	});

	public void TestAdditionalProcedureCodesAsString_B0001E() => CombineAssertions(() =>
	{
		const string ruleB0001E_ErrorMessage = "[B0001E] \"1H2\" Additional Procedure code is not allowed during the transition period.";

		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var invLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		invLine.JI_CEI = entryInstruction.PK;
		var additionalProcedureCode1 = invLine.AdditionalProcedureCodes.AddNew();
		var additionalProcedureCode2 = invLine.AdditionalProcedureCodes.AddNew();

		using var testContext = new FunctionalityTestContext();
		testContext.SetAESTransitionPeriod(effectiveDate: entryInstruction.CEI_DateForDuty, enabled: true);

		additionalProcedureCode1.CY_Code = "2222222";
		additionalProcedureCode2.CY_Code = "333333";
		Validate();
		AssertNoMessageError($"Inside the transition period, {additionalProcedureCode1.CY_Code} and {additionalProcedureCode2.CY_Code}", invLine.AdditionalProcedureCodesAsStringInfo, ruleB0001E_ErrorMessage);

		additionalProcedureCode2.CY_Code = "1H2";
		Validate();
		AssertHasMessageError($"Inside the transition period, {additionalProcedureCode1.CY_Code} and {additionalProcedureCode2.CY_Code}", invLine.AdditionalProcedureCodesAsStringInfo, ruleB0001E_ErrorMessage);

		testContext.SetAESTransitionPeriod(effectiveDate: entryInstruction.CEI_DateForDuty, enabled: false);
		Validate();
		AssertNoMessageError($"Outside the transition period, {additionalProcedureCode1.CY_Code} and {additionalProcedureCode2.CY_Code}", invLine.AdditionalProcedureCodesAsStringInfo, ruleB0001E_ErrorMessage);

		void Validate()
		{
			invLine.AdditionalProcedureCodes.Cast<AdditionalProcedureCode>().ForEach(x => x.Validation.ValidateCY_Code());
			invLine.Validation.ValidateAdditionalProcedureCodesAsString();
		}
	});

	public void TestHasValidPackagePivots() => AssertEquals(true, new JobComInvLineValidationForTest(Factory.New<JobComInvoiceLine>()).HasValidPackagePivotsExposed);

	class JobComInvLineValidationForTest : JobComInvoiceLineValidation
	{
		public JobComInvLineValidationForTest(JobComInvoiceLine invL) : base(invL)
		{
		}

		public ZBool AdditionalProcedureCodesCheckFirst4Characters_Exposed => AdditionalProcedureCodesCheckFirst4Characters;

		public ZBool HasValidPackagePivotsExposed => HasValidPackagePivots;
	}
}
