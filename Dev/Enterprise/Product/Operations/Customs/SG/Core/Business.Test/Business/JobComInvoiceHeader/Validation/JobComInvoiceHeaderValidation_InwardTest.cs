using CargoWise.EntityFramework;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public abstract class JobComInvoiceHeaderValidation_InwardTest : JobComInvoiceHeaderValidationTest
	{
		public void TestInvoiceAmount()
		{
			Validation.ValidateJZ_InvoiceAmount();
			AssertEquals("Invoice Amount can be zero for SG", true, InvoiceHeader.JZ_InvoiceAmountInfo.HasWarning("You have not entered an Invoice Amount."));
			InvoiceHeader.JZ_InvoiceAmount = 10m;
			Validation.ValidateJZ_InvoiceAmount();
			AssertEquals(false, InvoiceHeader.JZ_InvoiceAmountInfo.HasWarning("You have not entered an Invoice Amount."));
		}

		public void TestValidateIncoTerm()
		{
			InvoiceHeader.JZ_IncoTerm = "";
			Validation.ValidateJZ_IncoTerm();
			AssertEquals("Inco Term can be empty for SG", false, InvoiceHeader.JZ_IncoTermInfo.HasMessageErrors());
			InvoiceHeader.JZ_IncoTerm = "ABC";
			Validation.ValidateJZ_IncoTerm();
			AssertEquals("Invalid code list validation", true, InvoiceHeader.JZ_IncoTermInfo.HasMessageErrors());
			AssertEquals("Invalid code message error", true, InvoiceHeader.JZ_IncoTermInfo.HasMessageError("Please enter a valid Incoterm."));
			InvoiceHeader.JZ_IncoTerm = UnitPriceTermTypeCodeList.Codes.CFR;
			Validation.ValidateJZ_IncoTerm();
			AssertEquals(false, InvoiceHeader.JZ_IncoTermInfo.HasMessageErrors());
		}
	}
}
