using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	internal class USCPSCLabReportAddInfoValidationTest : BusinessObjectLookupsTestCase
	{
		public void TestCheckUS_RemarksText()
		{
			CPSCReport.US_RemarksType = "CP1";
			CPSCReport.US_RemarksText = ZString.Empty;
			AssertHasMessageErrorContaining(CPSCReport.US_RemarksTextInfo, MandatoryValidation.YouHaveNotEntered);

			CPSCReport.US_RemarksText = "123";
			AssertNoMessageErrorContaining(CPSCReport.US_RemarksTextInfo, MandatoryValidation.YouHaveNotEntered);

			CPSCReport.US_RemarksType = ZString.Empty;
			CPSCReport.US_RemarksText = ZString.Empty;
			AssertHasMessageErrorContaining(CPSCReport.US_RemarksTextInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_RemarksType()
		{
			CPSCReport.US_RemarksText = "Test";
			CPSCReport.US_RemarksType = ZString.Empty;
			AssertHasMessageErrorContaining(CPSCReport.US_RemarksTypeInfo, MandatoryValidation.YouHaveNotEntered);

			CPSCReport.US_RemarksType = "CP1";
			AssertNoMessageErrorContaining(CPSCReport.US_RemarksTypeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(CPSCReport.US_RemarksTypeInfo, ListValidation.InvalidCodeMessageError);

			CPSCReport.US_RemarksText = ZString.Empty;
			CPSCReport.US_RemarksType = ZString.Empty;
			AssertHasMessageErrorContaining(CPSCReport.US_RemarksTypeInfo, MandatoryValidation.YouHaveNotEntered);

			CPSCReport.US_RemarksType = "Tes";
			AssertHasMessageErrorContaining(CPSCReport.US_RemarksTypeInfo, ListValidation.InvalidCodeMessageError);
		}

		CPSCReport CPSCReport
		{
			get
			{
				if (report == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
					declaration.US_EnableENS = true;
					declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;

					var invoice = declaration.Invoices.AddNew();
					var invoiceLine = invoice.JobComInvoiceLines.AddNew();
					var cpscHeader = invoiceLine.CPSCHeaders.AddNew();
					var rule = cpscHeader.RuleAndLabs.AddNew();
					report = rule.ReportAndLabs.AddNew();
				}
				return report;
			}
		}
		CPSCReport report;
	}
}
