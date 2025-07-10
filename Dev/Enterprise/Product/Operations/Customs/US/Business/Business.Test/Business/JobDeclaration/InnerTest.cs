using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	public class InnerTest : TestCaseWithFactory
	{
		public void TestPackingInformation()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			AssertEquals(false, declaration.IsPackingInformationRelevant);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			AssertEquals(true, declaration.IsPackingInformationRelevant);
		}

		public void TestForInvoiceHeaderNull()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			AssertEquals(false, declaration.IsPackingInformationRelevant);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;

			JobComInvoiceLine invoiceLine = Factory.New<JobComInvoiceLine>();

			declaration.InvoiceLines.Add(invoiceLine);

			AssertNoExceptionThrown("No exception should be thrown", () => declaration.ResumeApportionment());
		}
	}
}
