using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class AssignedJobComInvLineRefsValidationTest : BusinessObjectValidationTestCase
	{
		public void TestParent()
		{
			var parent = Factory.New<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew().AssignedJobComInvLineRefsCollection.AddNew();
			AssertEquals("Parent", parent, parent.Validation.Parent);
		}

		public void TestCheckJG_ReferenceNumber()
		{
			var invoiceLine = Factory.New<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew();
			var assignedNumber1 = invoiceLine.AssignedJobComInvLineRefsCollection.AddNew();
			var assignedNumber2 = invoiceLine.AssignedJobComInvLineRefsCollection.AddNew();
			assignedNumber1.JG_ReferenceNumber = "XXX";
			assignedNumber2.JG_ReferenceNumber = "XXX";
			AssertHasMessageError(assignedNumber2.JG_ReferenceNumberInfo, "Assigned Number cannot be duplicated.");
			assignedNumber2.JG_ReferenceNumber = "DDD";
			AssertNoMessageErrors(assignedNumber2.JG_ReferenceNumberInfo);
			invoiceLine.AssignedJobComInvLineRefsCollection.RemoveAndDeleteAll();
			invoiceLine.JI_EPTDigit1 = "A";
			invoiceLine.JI_EPTDigit2 = "0";
			invoiceLine.JI_EPTDigit3 = "1";
			for (int i = 0; i < 10; i++)
			{
				var assignedNumber = invoiceLine.AssignedJobComInvLineRefsCollection.AddNew();
				assignedNumber.JG_ReferenceNumber = "XXX";
				if (i >= 9)
				{
					AssertHasRowMessageErrorContaining(invoiceLine, ValidationConstants.InvoiceLine.MoreThan10AssignedNumbersPerInvoiceLine);
				}
				else
				{
					AssertNoRowMessageErrorContaining(invoiceLine, ValidationConstants.InvoiceLine.MoreThan10AssignedNumbersPerInvoiceLine);
				}
			}
		}
	}
}
