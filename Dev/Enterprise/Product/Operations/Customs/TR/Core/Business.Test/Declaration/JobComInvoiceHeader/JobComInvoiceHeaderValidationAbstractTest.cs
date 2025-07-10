using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Declaration.Testing
{
	[TestsSubclassesOf(typeof(JobComInvoiceHeaderValidation))]
	abstract class JobComInvoiceHeaderValidationAbstractTest : BusinessObjectValidationTestCase
	{
		public void TestCheckJZ_PaymentAmount()
		{
			invoice.ZG_CommercialPaymentCode = "1";

			CombineAssertions("Validation for JZ_PaymentAmount", () =>
			{
				invoice.JZ_PaymentAmount = ZDecimal.Zero;
				AssertHasMessageErrorContaining(invoice.JZ_PaymentAmountInfo, "Payment amount is required if payment code is filled.");

				invoice.JZ_PaymentAmount = 100;
				AssertNoMessageErrorContaining(invoice.JZ_PaymentAmountInfo, "Payment amount is required if payment code is filled.");

				invoice.ZG_CommercialPaymentCode = ZString.Empty;
				invoice.JZ_PaymentAmount = ZDecimal.Zero;
				AssertNoMessageErrorContaining(invoice.JZ_PaymentAmountInfo, "Payment amount is required if payment code is filled.");
			});
		}

		public void TestCheckJZ_PaymentNo()
		{
			invoice.JZ_PaymentNo = ZString.Empty;
			var invoiceLine = invoice.InvoiceLines.AddNew();

			CombineAssertions("Validation for JI_FormattedProcedure", () =>
			{
				invoiceLine.JI_FormattedProcedure = "4000";
				validation.ValidateJZ_PaymentNo();
				AssertHasMessageErrorContaining(invoice.JZ_PaymentNoInfo, "Payment Code field is mandatory for procedure codes starting with 4 and 71, and for the procedure codes 6121, 6123, 6323, 6771, 5100, 5121, 5171, 5191, 5300, 5321, 5353, 5358, 5371, 5391, 5800.");

				invoiceLine.JI_FormattedProcedure = "7100";
				validation.ValidateJZ_PaymentNo();
				AssertHasMessageErrorContaining(invoice.JZ_PaymentNoInfo, "Payment Code field is mandatory for procedure codes starting with 4 and 71, and for the procedure codes 6121, 6123, 6323, 6771, 5100, 5121, 5171, 5191, 5300, 5321, 5353, 5358, 5371, 5391, 5800.");

				invoiceLine.JI_FormattedProcedure = "5100";
				validation.ValidateJZ_PaymentNo();
				AssertHasMessageErrorContaining(invoice.JZ_PaymentNoInfo, "Payment Code field is mandatory for procedure codes starting with 4 and 71, and for the procedure codes 6121, 6123, 6323, 6771, 5100, 5121, 5171, 5191, 5300, 5321, 5353, 5358, 5371, 5391, 5800.");

				invoiceLine.JI_FormattedProcedure = "3151";
				invoice.JZ_PaymentNo = ZString.Empty;
				AssertNoMessageErrorContaining(invoice.JZ_PaymentNoInfo, "Payment Code field is mandatory for procedure codes starting with 4 and 71, and for the procedure codes 6121, 6123, 6323, 6771, 5100, 5121, 5171, 5191, 5300, 5321, 5353, 5358, 5371, 5391, 5800.");
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_MessageType = MessageType;
			invoice = jobDeclaration.Invoices.AddNew();
			validation = GetValidation();
		}
		protected JobDeclaration jobDeclaration;
		protected JobComInvoiceHeader invoice;
		protected JobComInvoiceHeaderValidation validation;

		protected abstract string MessageType { get; }

		protected abstract JobComInvoiceHeaderValidation GetValidation();
	}
}
