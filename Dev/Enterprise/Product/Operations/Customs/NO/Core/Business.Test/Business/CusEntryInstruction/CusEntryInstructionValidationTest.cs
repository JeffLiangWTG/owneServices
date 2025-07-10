using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(CusEntryInstructionValidation))]
sealed class CusEntryInstructionValidationTest : BusinessObjectValidationTestCase
{
	public void TestConstructor() => CombineAssertions(() =>
	{
		var parent = instruction;
		AssertArgumentExceptionThrown<ArgumentNullException>("When parent is null", nameof(parent), () => _ = new CusEntryInstructionValidation(null));
		AssertNoExceptionThrown("happy path", () => _ = new CusEntryInstructionValidation(parent));
	});

	public void TestStyleListImport()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(instruction.CEI_StyleInfo, "1", "4");
	}

	public void TestStyleListExport()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(instruction.CEI_StyleInfo, "4", "1");
	}

	public void TestDeclarationSubStyleListImport()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(instruction.CEI_SubStyleInfo, "A", "N");
	}

	public void TestDeclarationSubStyleListExport()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(instruction.CEI_SubStyleInfo, "C", "N");
	}

	[TestDate(2025, 3, 11)]
	public void TestCEI_DateForDuty_ShouldOnlyAllowFutureDatesForDigitoll() => CombineAssertions(() =>
	{
		const string messageError = "Date ahead in time is only allowed on a Digitoll or Direct declaration (Goods number char 7-8 = DT or D).";
		var instructionMock = Factory.NewMoq<CusEntryInstruction>();
		var today = ZDateTime.Today;
		var tomorrow = ZDateTime.Today.AddDays(1);

		instructionMock.SetupGetHasDigitollGoodsNumber(true);
		instructionMock.Object.CEI_DateForDuty = today;
		AssertNoMessageErrorContaining("When CEI_DateForDuty is today and GoodsNumber is Digitoll", instructionMock.Object.CEI_DateForDutyInfo, messageError);

		instructionMock.SetupGetHasDigitollGoodsNumber(false);
		instructionMock.Object.Validation.ValidateCEI_DateForDuty();
		AssertNoMessageErrorContaining("When CEI_DateForDuty is today and GoodsNumber is NOT Digitoll", instructionMock.Object.CEI_DateForDutyInfo, messageError);

		instructionMock.Object.CEI_DateForDuty = tomorrow;
		AssertHasMessageErrorContaining("When CEI_DateForDuty is tomorrow and GoodsNumber is NOT Digitoll", instructionMock.Object.CEI_DateForDutyInfo, messageError);

		instructionMock.SetupGetHasDigitollGoodsNumber(true);
		instructionMock.Object.Validation.ValidateCEI_DateForDuty();
		AssertNoMessageErrorContaining("When CEI_DateForDuty is tomorrow and GoodsNumber is Digitoll", instructionMock.Object.CEI_DateForDutyInfo, messageError);
	});

	[TestDate(2025, 3, 11)]
	public void TestCEI_DateForDuty_ShouldOnlyAllowFutureDatesMaximumFiveDaysAhead() => CombineAssertions(() =>
	{
		const string messageError = "Date can be maximum 5 days ahead in time.";
		var instructionMock = Factory.NewMoq<CusEntryInstruction>();
		var futureFifthDayAtEndOfDay = instructionMock.Object.CEI_DateForDuty = ZDateTime.Today.EndOfDay().AddDays(5);
		var futureSixthDayAtStartOfDay = futureFifthDayAtEndOfDay.AddSeconds(1);

		instructionMock.SetupGetHasDigitollGoodsNumber(false);
		instructionMock.Object.CEI_DateForDuty = futureSixthDayAtStartOfDay;
		AssertNoMessageErrorContaining("When CEI_DateForDuty is 6 days in future and GoodsNumber is NOT Digitoll", instructionMock.Object.CEI_DateForDutyInfo, messageError);

		instructionMock.SetupGetHasDigitollGoodsNumber(true);
		instructionMock.Object.Validation.ValidateCEI_DateForDuty();
		AssertHasMessageErrorContaining("When CEI_DateForDuty is 6 days in future and GoodsNumber is Digitoll", instructionMock.Object.CEI_DateForDutyInfo, messageError);

		instructionMock.Object.CEI_DateForDuty = futureFifthDayAtEndOfDay;
		AssertNoMessageErrorContaining("When CEI_DateForDuty is 5 days in future and GoodsNumber is Digitoll", instructionMock.Object.CEI_DateForDutyInfo, messageError);
	});

	public void TestDescriptionImport()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(instruction.CEI_DescriptionInfo);
	}

	public void TestDescriptionExport()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(instruction.CEI_DescriptionInfo);
	}

	public void TestProcedureImport()
	{
		RefCusProcedureHelper.CreateRefCusProcedureList(Factory);

		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		instruction.CEI_Style = "4";
		ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(instruction.CEI_ProcedureInfo, "1111", "4052");
	}

	public void TestValidateForRowNotification_LinkedInvoiceHeaders_JZ_IncoTerm()
	{
		AssertValidateForRowNotification_LinkedInvoiceHeaders((JobComInvoiceHeader x) => x.JZ_IncoTermInfo, "Incoterm Codes");
	}

	public void TestValidateForRowNotification_LinkedInvoiceHeaders_JZ_ValuationCode()
	{
		AssertValidateForRowNotification_LinkedInvoiceHeaders((JobComInvoiceHeader x) => x.JZ_ValuationCodeInfo, "Natures of Transaction");
	}

	void AssertValidateForRowNotification_LinkedInvoiceHeaders(Func<JobComInvoiceHeader, ZPropertyInfo> propertyInfoFunc, string name)
	{
		CombineAssertions(() =>
		{
			var message = $"Invoice Headers with different {name} are linked to this Entry Instruction.";
			IZType value1 = (ZString)"X";
			IZType value2 = (ZString)"Y";

			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
			var validation = instruction.Validation;
			var invoiceHeader = declaration.Invoices.AddNew();
			propertyInfoFunc(invoiceHeader).Value = value1;
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;

			var invoiceHeader2 = declaration.Invoices.AddNew();
			propertyInfoFunc(invoiceHeader2).Value = value2;
			var invoiceLine2 = invoiceHeader2.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = instruction.PK;
			instruction.RemoveRowError(message);
			validation.VerifyIfInvoiceHeadersHaveDistinctMergeValues();
			AssertHasRowError("Entry Instruction has more than one Invoice Header with a different Code", instruction, message);

			propertyInfoFunc(invoiceHeader2).Value = value1;
			instruction.RemoveRowError(message);
			validation.VerifyIfInvoiceHeadersHaveDistinctMergeValues();
			AssertNoRowError("Entry Instruction doesn't has more than one Invoice Header with a different Code", instruction, message);

			var invoiceHeader3 = declaration.Invoices.AddNew();
			propertyInfoFunc(invoiceHeader3).Value = value2;
			var invoiceLine3 = invoiceHeader3.InvoiceLines.AddNew();
			invoiceLine3.JI_CEI = ZGuid.Empty;
			instruction.RemoveRowError(message);
			validation.VerifyIfInvoiceHeadersHaveDistinctMergeValues();
			AssertNoRowError("Other header doesn't link to Entry Instruction", instruction, message);
		});
	}

	public void TestVerifyIfInvoiceHeadersHaveDistinctMergeValuesIsCalledByValidateAll()
	{
		var messageErrorText = "Invoice Headers with different Incoterm Codes are linked to this Entry Instruction.";

		var invoice1 = declaration.Invoices.AddNew();
		invoice1.JZ_IncoTerm = "EXW";
		var invoiceLine1 = invoice1.InvoiceLines.AddNew();
		invoiceLine1.JI_CEI = instruction.PK;

		var invoice2 = declaration.Invoices.AddNew();
		invoice2.JZ_IncoTerm = "FOB";
		var invoiceLine2 = invoice2.InvoiceLines.AddNew();
		invoiceLine2.JI_CEI = instruction.PK;

		AssertNoRowErrors("Pre-condition", instruction);
		instruction.Validation.ValidateAll();
		AssertHasRowError("Should have error", instruction, messageErrorText);
	}

	public void TestCEI_PackageCount_CannotBeEmpty()
	{
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(instruction.CEI_PackageCountInfo);
	}

	public void TestCEI_PackageCount_CannotBeNegative()
	{
		ValidationTestHelper.AssertValueCannotBeNegativeMessageError(instruction.CEI_PackageCountInfo);
	}

	public void TestCEI_PackageCount_SumShouldMatchTotal()
	{
		const int totalNoOfPieces = 10;
		var expectedWarning = $"The sum of number of Units on all Entry Instructions should be equal to {totalNoOfPieces}, the Number of Units on Declaration Header.";
		var instruction2 = declaration.CustomsEntryInstructions.AddNew();

		CombineAssertions(() =>
		{
			declaration.JE_TotalNoOfPieces = totalNoOfPieces;

			instruction2.Validation.ValidateCEI_PackageCount();
			AssertNoWarning("When no input given to the CEI_PackageCount", instruction2.CEI_PackageCountInfo, expectedWarning);

			instruction.CEI_PackageCount = 5;
			instruction2.CEI_PackageCount = 3;
			instruction2.Validation.ValidateCEI_PackageCount();
			AssertHasWarning("When sum of all CEI_PackageCount is not equal to the JE_TotalNoOfPieces", instruction2.CEI_PackageCountInfo, expectedWarning);

			instruction2.CEI_PackageCount = 5;
			instruction2.Validation.ValidateCEI_PackageCount();
			AssertNoWarning("When sum of all CEI_PackageCount is equal to the JE_TotalNoOfPieces", instruction2.CEI_PackageCountInfo, expectedWarning);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		instruction = declaration.CustomsEntryInstructions.AddNew();
	}
	JobDeclaration declaration;
	CusEntryInstruction instruction;
}
