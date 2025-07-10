using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.NO.Business.Testing;

sealed class JobComInvoiceHeaderValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckJZ_InvoiceDate()
	{
		AssertNoMessageErrors(InvoiceHeader.JZ_InvoiceDateInfo);
		InvoiceHeader.JZ_InvoiceDate = ZDate.Empty;
		InvoiceHeader.Validation.ValidateJZ_InvoiceDate();
		AssertHasMessageError(InvoiceHeader.JZ_InvoiceDateInfo, MandatoryValidation.YouHaveNotEnteredMessage(InvoiceHeader.JZ_InvoiceDateInfo.HumanReadableName));
		InvoiceHeader.JZ_InvoiceDate = ZDate.BrettsBirthday;
		InvoiceHeader.Validation.ValidateJZ_InvoiceDate();
		AssertNoMessageErrors(InvoiceHeader.JZ_InvoiceDateInfo);
	}

	public void TestCheckJZ_ValuationCode_IsValid()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		string codeType = "TRNAT";
		helper.CreateCusCodeType(codeType, "TRNAT CODE");
		helper.CreateCusCodeList(Core.Constants.CountryCodes.Norway, codeType, "01", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
		Factory.Save();
		CombineAssertions(() =>
		{
			InvoiceHeader.JZ_ValuationCode = ZString.Empty;
			AssertHasMessageError(InvoiceHeader.JZ_ValuationCodeInfo,
				MandatoryValidation.YouHaveNotEnteredMessage(InvoiceHeader.JZ_ValuationCodeInfo.HumanReadableName));
			InvoiceHeader.JZ_ValuationCode = "99";
			AssertHasMessageError(InvoiceHeader.JZ_ValuationCodeInfo, ListValidation.InvalidCodeMessageError);
			InvoiceHeader.JZ_ValuationCode = "01";
			AssertNoMessageErrors(InvoiceHeader.JZ_ValuationCodeInfo);
		});
	}

	public void TestCheckJZ_InvoiceCurrExRateType_Mandatory()
	{
		var targetInfo = InvoiceHeader.JZ_InvoiceCurrExRateInfo;
		const string messageError = "You have not entered an Exchange Rate.";
		CombineAssertions(() =>
		{
			InvoiceHeader.JZ_InvoiceCurrExRateType = ZString.Empty;
			ValidationTestHelper.AssertFieldIsNotMandatory(targetInfo, messageError, "FixedRate not ticked");

			InvoiceHeader.JZ_InvoiceCurrExRateType = ChargeExchangeRateTypeList.Codes.FixedRate;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(targetInfo, messageError, "FixedRate ticked");
		});
	}

	public void TestCheckJZ_InvoiceCurrExRateType_CannotBeNegative()
	{
		var targetInfo = InvoiceHeader.JZ_InvoiceCurrExRateInfo;
		const string messageError = "Exchange Rate cannot be negative.";

		CombineAssertions(() =>
		{
			InvoiceHeader.JZ_InvoiceCurrExRateType = ZString.Empty;
			InvoiceHeader.Validation.ValidateJZ_InvoiceCurrExRate();
			AssertEquals("Fixed Rate not ticked => ExchangeRate preset to 0", ZDecimal.Zero, InvoiceHeader.JZ_InvoiceCurrExRate);
			AssertNoMessageError("Fixed Rate not ticked => no message error", targetInfo, messageError);

			InvoiceHeader.JZ_InvoiceCurrExRateType = ChargeExchangeRateTypeList.Codes.FixedRate;
			InvoiceHeader.JZ_InvoiceCurrExRate = -1m;
			InvoiceHeader.Validation.ValidateJZ_InvoiceCurrExRate();
			AssertHasMessageError("Fixed Rate ticked => negative ExchangeRate", targetInfo, messageError);

			InvoiceHeader.JZ_InvoiceCurrExRate = 1m;
			InvoiceHeader.Validation.ValidateJZ_InvoiceCurrExRate();
			AssertNoMessageError("Fixed Rate ticked => positive ExchangeRate", targetInfo, messageError);
		});
	}

	public void TestCheckJZ_ValuationCodeInfo_WhenProcedureIs6021()
	{
		var messageErrorText = "Procedure 6021 requires transaction nature 05 or 10.";
		var entry = Declaration.CustomsEntryInstructions.AddNew();
		var invoiceLine = InvoiceHeader.JobComInvoiceLines.AddNew();
		entry.CEI_Procedure = "6021";
		invoiceLine.JI_CEI = entry.PK;
		invoiceLine.JI_Procedure = "6021";

		CombineAssertions(() =>
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(InvoiceHeader.JZ_ValuationCodeInfo, new ZString[] { "01", "02" }, new ZString[] { "05", "10" }, messageErrorText);
		});
	}

	public void TestCheckJZ_IncoTermPlace()
	{
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(InvoiceHeader.JZ_IncoTermPlaceInfo);
	}

	protected override void SetUp()
	{
		base.SetUp();
		Declaration = Factory.New<JobDeclaration>();
		InvoiceHeader = Declaration.Invoices.AddNew();
	}
	JobDeclaration Declaration { get; set; }
	JobComInvoiceHeader InvoiceHeader { get; set; }
}

