using System.Collections.Generic;
using Enterprise.Customs.Module;

namespace Enterprise.ReportTesting.Customs.ZA
{
	[TemplateName("ZA Customs Entry Payment Report")]
	public class TestZACustomsEntryPaymentReportTemplate : TemplateTestCase
	{
		protected override List<string> GetExcludedSheetNames()
		{
			var sheetsToExclude = base.GetExcludedSheetNames();
			sheetsToExclude.Add("PaymentSummary");
			return sheetsToExclude;
		}
	}

	public class TestZACustomsEntryPaymentReportReport : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new CustomsReportModule(); }
		}

		public override string MenuName
		{
			get { return "Customs Entry Payment Report"; }
		}

		public override string Hint
		{
			get
			{
				return @"The report produces a listing of payments on ZA Declaration Entries.

For a specified date range the report identifies declaration payment details including job, Importer, Customs Office, Financial Account Number, Transport Document Number, and payment details.
Filter options include Payment Type, Customs Office, Financial Account Number and Importer.

Note that Accounting Date on this report is derived from DTM+202 – ACCOUNTING DATE segment of the CUSRES message received from SARS, if DTM+202 is not present then the Accounting Date falls back to the Receive Date of the last CUSRES message received.
DTM+202 is only included when the Payment Code is D or V, this means that the Accounting Date of Cash Payments fall back to the last CUSRES Receive Date
Therefore, this Report should be reviewed in conjunction with STATAC messages.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestZACustomsEntryPaymentReportTemplate();
		}
	}
}
