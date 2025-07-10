using System;
using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Declaration.Testing
{
	[TestedType(typeof(JobComInvoiceHeaderValidation))]
	class JobComInvoiceHeaderValidationTest : JobComInvoiceHeaderValidationAbstractTest
	{
		protected override string MessageType => MessageTypeList.Codes.MiscellaneousCustoms;

		protected override JobComInvoiceHeaderValidation GetValidation() => new JobComInvoiceHeaderValidation(invoice);

		protected Type GetTypeForTest()
		{
			return typeof(JobComInvoiceHeaderValidation);
		}

		public void TestCheckJZ_RelatedIndicator()
		{
			var header = Factory.New<JobDeclaration>();

			CombineAssertions(() =>
			{
				var invoice1 = header.Invoices.AddNew();
				invoice1.JZ_RelatedIndicator = RelationCodeList.Codes.N;
				AssertNoMessageErrorContaining(invoice1.JZ_RelatedIndicatorInfo, "Seller and Buyer relations are different on invoices");

				var invoice2 = header.Invoices.AddNew();
				invoice2.JZ_RelatedIndicator = RelationCodeList.Codes.Y;
				AssertHasMessageErrorContaining(invoice2.JZ_RelatedIndicatorInfo, "Seller and Buyer relations are different on invoices");

				var invoice3 = header.Invoices.AddNew();
				invoice3.JZ_RelatedIndicator = "A";
				AssertHasMessageErrorContaining(invoice3.JZ_RelatedIndicatorInfo, "The code you have selected is not in the list.");
			});
		}

		public void TestCheckJZ_RX_NKInvoice_Currency()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var anotherInvoice = declaration.Invoices.AddNew();

			var message = "All Invoices must have the same Currency.";

			anotherInvoice.JZ_RX_NKInvoice_Currency = "EUR";
			invoice.JZ_RX_NKInvoice_Currency = "TRY";
			AssertHasMessageError("Has message when another invoice has different Currency.", invoice.JZ_RX_NKInvoice_CurrencyInfo, message);

			invoice.JZ_RX_NKInvoice_Currency = "EUR";
			AssertNoMessageError("No message when another invoice has same Currency.", invoice.JZ_RX_NKInvoice_CurrencyInfo, message);
		}

		public void TestValidateSingleInvoice()
		{
			var declaration = Factory.New<JobDeclaration>();
			var errorMessage = "You can enter only one invoice for a declaration.";
			CombineAssertions("Validate Single Invoice", () =>
			{
				var firstInvoice = declaration.Invoices.AddNew();
				firstInvoice.Validation.ValidateAll();
				AssertNoRowMessageError("First Invoice", firstInvoice, errorMessage);

				var secondInvoice = declaration.Invoices.AddNew();
				secondInvoice.Validation.ValidateAll();
				AssertHasRowMessageError("Second Invoice", secondInvoice, errorMessage);
			});
		}
	}
}
