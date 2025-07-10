using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	public class PGARecapPrintSignedTest : TestCaseWithFactory
	{
		public void TestPGARecapPrintSignedMembers()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableCRL = true;
			declaration.US_EnableENS = true;
			declaration.US_CertifyCargoRelease = false;
			declaration.US_PGAExpeditedRelease = false;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			declaration.US_EntryDateElectionCode = EntryDateElectionCodeList.Codes.WeeklyEstimateFilingDate;

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.US_TSCAInd = "D";
			invoiceLine.US_TSCACertification = "+";
			invoiceLine.US_TTBInd = "D";
			var ttbLine = invoiceLine.TTBLines.AddNew();
			ttbLine.US_ProcessingCode = "A";

			var pgaRecapPrintSigned = new PGARecapPrintSigned(declaration) as IAcknowledgeAndSign;
			Assert(pgaRecapPrintSigned.US_CertifyCargoRelease);
		}
	}
}
