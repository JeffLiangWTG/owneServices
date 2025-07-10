using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TR.Business.Declaration;

namespace Enterprise.Customs.TR.Business.Testing
{
	public class JobComInvoiceLineTaxValidationTest : TestCaseWithFactory
	{
		public void TestCheckJLT_Type()
		{
			tax.JLT_Type = ZString.Empty;
			AssertHasErrorContaining(tax.JLT_TypeInfo, MandatoryValidation.MustBeEntered);

			tax.JLT_Type = "10";
			AssertNoNotifications(tax.JLT_MethodOfCalculationInfo);

			tax.JLT_Type = "ABC";
			AssertHasMessageErrorContaining(tax.JLT_TypeInfo, "The code you have selected is not in the list.");
		}

		public void TestCheckJLT_MethodOfCalculation()
		{
			tax.JLT_MethodOfCalculation = ZString.Empty;
			AssertNoNotifications(tax.JLT_MethodOfCalculationInfo);

			tax.JLT_MethodOfCalculation = "CW1";
			AssertNoNotifications(tax.JLT_MethodOfCalculationInfo);

			tax.JLT_MethodOfCalculation = "ABC";
			AssertHasMessageErrorContaining(tax.JLT_MethodOfCalculationInfo, "The code you have selected is not in the list.");
		}

		public void TestCheckJLT_MethodOfPayment()
		{
			tax.JLT_MethodOfPayment = ZString.Empty;
			AssertNoNotifications(tax.JLT_MethodOfPaymentInfo);

			tax.JLT_MethodOfPayment = "C";
			AssertNoNotifications(tax.JLT_MethodOfPaymentInfo);

			tax.JLT_MethodOfPayment = "ABC";
			AssertHasMessageErrorContaining(tax.JLT_MethodOfPaymentInfo, "The code you have selected is not in the list.");
		}

		public void TestCheckJLT_RateOverrideReasonCode()
		{
			tax.JLT_RateOverrideReasonCode = ZString.Empty;
			AssertNoNotifications(tax.JLT_RateOverrideReasonCodeInfo);

			tax.JLT_RateOverrideReasonCode = "ADD";
			AssertNoNotifications(tax.JLT_RateOverrideReasonCodeInfo);

			tax.JLT_RateOverrideReasonCode = "ABC";
			AssertHasMessageErrorContaining(tax.JLT_RateOverrideReasonCodeInfo, "The code you have selected is not in the list.");
		}

		public void TestCheckJLT_Rate()
		{
			CombineAssertions(() =>
			{
				var message = $"Please enter a '{tax.JLT_RateInfo.HumanReadableName}' greater than 0.";

				tax.JLT_Rate = -1;
				AssertHasMessageErrorContaining(tax.JLT_RateInfo, message);

				tax.JLT_Rate = 0;
				AssertHasMessageErrorContaining(tax.JLT_RateInfo, message);

				tax.JLT_Rate = 1;
				AssertNoMessageErrors(tax.JLT_RateInfo);
			});
		}

		public void TestCheckJLT_BaseValue()
		{
			CombineAssertions(() =>
			{
				var message = $"Please enter a '{tax.JLT_BaseValueInfo.HumanReadableName}' greater than 0.";

				tax.JLT_BaseValue = -1;
				AssertHasMessageErrorContaining(tax.JLT_BaseValueInfo, message);

				tax.JLT_BaseValue = 0;
				AssertHasMessageErrorContaining(tax.JLT_BaseValueInfo, message);

				tax.JLT_BaseValue = 1;
				AssertNoMessageErrors(tax.JLT_BaseValueInfo);
			});
		}

		public void TestJLT_Amount()
		{
			CombineAssertions(() =>
			{
				var message = $"Please enter a '{tax.JLT_AmountInfo.HumanReadableName}' greater than 0.";

				tax.JLT_Amount = -1;
				AssertHasMessageErrorContaining(tax.JLT_AmountInfo, message);

				tax.JLT_Amount = 0;
				AssertHasMessageErrorContaining(tax.JLT_AmountInfo, message);

				tax.JLT_Amount = 1;
				AssertNoMessageErrors(tax.JLT_AmountInfo);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			invoiceHeader = Factory.New<JobComInvoiceHeader>();
			invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			tax = invoiceLine.Taxes.AddNew();
		}

		JobComInvoiceHeader invoiceHeader;
		JobComInvoiceLine invoiceLine;
		JobComInvoiceLineTax tax;
	}
}
