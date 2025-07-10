namespace Enterprise.ReportTesting.Accounting
{
	using NUnit.Framework;

	[TemplateName("ARAP Compliance Sequences Book Profile")]
	public class TestComplianceSequenceBookProfileReport : TemplateTestCase
	{
		[ExpectNoExceptions]
		public void TestReportOnlyRequiredFilters()
		{
			PrepareReportForRender();
			RunReport();
		}
	}

	public class TestComplianceSequenceBookProfileReportMenuSetup : ReportTestCase
	{
		public override ZArchitecture.Modules.ZEmbeddedModule ModuleToTest => new Enterprise.MasterFiles.Module.AccountReports();

		public override string MenuName => "Compliance Sequences Book Profile";

		public override string Hint => @"This report lists Compliance Books recorded under the Maintain > Account > Compliance Sequences module.
The report is relevant to Login Companies where the Compliance Sequences module has been enabled allowing the assignation of Government Controlled Compliance Numbers to Invoices and Credit Notes issued by the company.
CargoWise Compliance Sub Types and Compliance Sequences are used to automatically assign government mandated compliance numbers and print government mandated compliance documents when an invoice or credit note transaction is identified as requiring a special compliance number. 
Use this report to review the Compliance Books configured in your Login Company.";

		protected override TemplateTestCase GetTemplateTestCase() => new TestComplianceSequenceBookProfileReport();
	}
}
