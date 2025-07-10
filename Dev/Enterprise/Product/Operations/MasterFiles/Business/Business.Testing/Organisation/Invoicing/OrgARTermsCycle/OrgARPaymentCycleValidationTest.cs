using CargoWise.EntityFramework.Testing;
using Enterprise.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgARPaymentCycleValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckP5_ToDay()
		{
			arPaymentCycle.P5_ToDay = 0;
			AssertHasError(arPaymentCycle.P5_ToDayInfo, "The value must be 1 or greater.");

			arPaymentCycle.P5_ToDay = 1;
			AssertNoErrors(arPaymentCycle.P5_ToDayInfo);

			arTerms.ARPaymentCycles.AddNew();
			arPaymentCycle.RunPreSaveValidation();
			AssertHasError(arPaymentCycle.P5_ToDayInfo, "Payment cycle with the same 'Cycle #' already exists.");
		}

		public void TestCheckP5_PaymentDay()
		{
			arPaymentCycle.P5_PaymentDay = 0;
			AssertHasError(arPaymentCycle.P5_PaymentDayInfo, "The value must be between 1 and 31.");

			arPaymentCycle.P5_PaymentDay = 1;
			AssertNoError(arPaymentCycle.P5_PaymentDayInfo, "The value must be between 1 and 31.");

			arPaymentCycle.P5_PaymentDay = 32;
			AssertHasError(arPaymentCycle.P5_PaymentDayInfo, "The value must be between 1 and 31.");

			arPaymentCycle.P5_PaymentDay = 31;
			AssertNoErrors(arPaymentCycle.P5_PaymentDayInfo);

			arTerms.ARPaymentCycles.AddNew();
			arPaymentCycle.P5_PaymentDay = 1;
			arPaymentCycle.RunPreSaveValidation();
			AssertHasError(arPaymentCycle.P5_PaymentDayInfo, "Payment cycle with the same 'Payment Day' value already exists.");
		}

		protected override void SetUp()
		{
			base.SetUp();
			arTerms = Factory.New<OrgARTerms>();
			arTerms.PY_InvoiceTerm = Constants.InvoiceTerms.TermDaysAndDebtorPaymentCycle;
			arPaymentCycle = arTerms.ARPaymentCycles.AddNew();
		}

		OrgARTerms arTerms;
		OrgARPaymentCycle arPaymentCycle;
	}
}
