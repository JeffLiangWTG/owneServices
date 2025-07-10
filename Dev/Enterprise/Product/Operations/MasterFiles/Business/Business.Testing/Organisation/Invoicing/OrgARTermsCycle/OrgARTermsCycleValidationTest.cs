using CargoWise.EntityFramework.Testing;
using Enterprise.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgARTermsCycleValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckP5_ToDay()
		{
			arTerms.PY_InvoiceTerm = Constants.InvoiceTerms.MonthsFromInvoiceCycleDate;
			arTermsCycle.P5_ToDay = 0;
			AssertHasError(arTermsCycle.P5_ToDayInfo, "The value must be between 1 and 31.");

			arTermsCycle.P5_ToDay = 1;
			AssertNoErrors(arTermsCycle.P5_ToDayInfo);

			arTermsCycle.P5_ToDay = 32;
			AssertHasError(arTermsCycle.P5_ToDayInfo, "The value must be between 1 and 31.");

			arTermsCycle.P5_ToDay = 31;
			AssertNoErrors(arTermsCycle.P5_ToDayInfo);

			arTerms.ARTermsCycles.AddNew();
			arTermsCycle.RunPreSaveValidation();
			AssertHasError(arTermsCycle.P5_ToDayInfo, "Terms cycle with the same 'To Day' value already exists.");
		}

		public void TestCheckP5_PaymentDay()
		{
			arTermsCycle.P5_PaymentDay = 0;
			AssertHasError(arTermsCycle.P5_PaymentDayInfo, "The value must be between 1 and 31.");

			arTermsCycle.P5_PaymentDay = 1;
			AssertNoError(arTermsCycle.P5_PaymentDayInfo, "The value must be between 1 and 31.");

			arTermsCycle.P5_PaymentDay = 32;
			AssertHasError(arTermsCycle.P5_PaymentDayInfo, "The value must be between 1 and 31.");

			arTermsCycle.P5_PaymentDay = 31;
			AssertNoErrors(arTermsCycle.P5_PaymentDayInfo);

			arTerms.PY_InvoiceTerm = Constants.InvoiceTerms.MonthsFromInvoiceCycleDate;
			arTerms.PY_InvoiceDays = 0;
			arTermsCycle.P5_ToDay = 20;
			arTermsCycle.P5_PaymentDay = 19;
			AssertNoErrors(arTermsCycle.P5_PaymentDayInfo);

			arTermsCycle.P5_PaymentDay = 20;
			AssertNoErrors(arTermsCycle.P5_PaymentDayInfo);

			arTermsCycle.P5_PaymentDay = 21;
			AssertNoErrors(arTermsCycle.P5_PaymentDayInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			arTerms = Factory.New<OrgARTerms>();
			arTermsCycle = arTerms.ARTermsCycles.AddNew();
		}

		OrgARTerms arTerms;
		OrgARTermsCycle arTermsCycle;
	}
}
