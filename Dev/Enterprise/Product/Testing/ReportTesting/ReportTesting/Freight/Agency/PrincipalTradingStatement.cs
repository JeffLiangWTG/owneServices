using System.Collections.Generic;
using Enterprise.Freight.Agency.Module;

namespace Enterprise.ReportTesting.Freight.Agency
{
	[TemplateName("Agency Principal Trading Statement")]
	public class TestAgencyPrincipalTradingStatementReport : TemplateTestCase
	{
		protected override IEnumerable<string> SheetsNotRequiringColumnHeadings
		{
			get
			{
				return new string[]
				{
					"Summary",
					"Detail"
				};
			}
		}
	}

	public class AgencyPrincipalTradingStatementReportTest : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new AgencyReports(); }
		}

		public override string Hint
		{
			get
			{
				const string result =
					@"This report compares principal income with principal expenses within a given date range across all shipping jobs.
This report allows Shipping Agencies to report financial position to their Principals.

It lists all Principal only charges posted within a given date range broken into two sections: Principal Income and Principal Expenses.

Principal Income section of this report lists all posted transactions from Bills of Lading/Bookings, Container Detentions, Voyage Accounting and Sundry Charges modules, where Principal is a creditor.

Principal Expenses section lists all posted transactions from Voyage Accounting, Sundry Charges, Bills/Bookings and Detention modules, where Principal is debtor.

The report has two templates: details and summary.

Note. In Summary report Voyage Accounting charges are summarized by Charge Sub Groups defined on the Charge Code.";

				return result;
			}
		}

		public override string MenuName
		{
			get { return "Principal Trading Statement"; }
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestAgencyPrincipalTradingStatementReport();
		}
	}
}
