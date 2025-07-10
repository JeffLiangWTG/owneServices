namespace Enterprise.ReportTesting.Accounting
{
	using System.Collections.Generic;
	using Enterprise.Accounting.Module;

	[TemplateName("AR Outstanding Charge Lines in Local Currency")]
	public class AROutstandingChargeLinesInLocalCurrencyTemplateTest : TemplateTestCase
	{
		protected override List<string> GetExcludedSheetNames()
		{
			return new List<string> { "Group by Charge Code" };
		}
	}

	public abstract class AROutstandingChargeLinesInLocalCurrencyReportTest : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new ReceivReports(); }
		}

		public override string MenuName
		{
			get { return "Outstanding Charge Lines in Local Currency"; }
		}

		public override string Hint
		{
			get
			{
				return @"Use this report to identify the local amount currently outstanding for transaction line on outstanding Receivables Invoice, Credit Note, and Adjustment Note transactions.
Only INV, CRD and ADJ transaction lines are reported.  Other outstanding transaction types (eg. Unmatched Receipts) are ignored.
Note:  When part paid transactions were matched at a line level, this report will accurately identify the outstanding balance for each transaction line.
When part paid transactions have not been matched at a line level, each line within the outstanding transaction will be treated as fully outstanding.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new AROutstandingChargeLinesInLocalCurrencyTemplateTest();
		}
	}
}
