namespace Enterprise.ReportTesting.Accounting.Netting
{
	[TemplateName("Netting - ExRate And Spread Report")]
	public class ExchangeRateAndSpreadReportTest : TemplateTestCase
	{
		protected override bool ReportRequiresColumnHeadings
		{
			get
			{
				return false;
			}
		}
	}

	[TemplateName("Netting - Participants List Report")]
	public class ParticipantListReportTest : TemplateTestCase
	{
	}

	[TemplateName("Netting - Period List Report")]
	public class PeriodListReportTest : TemplateTestCase
	{
	}

	[TemplateName("Netting - Transactions")]
	public class NettingTransactionsReportTest : TemplateTestCase
	{
	}

	[TemplateName("Netting - Value Proposition Model")]
	public class NettingValuePropositionModelTest : TemplateTestCase
	{
		protected override bool ReportRequiresColumnHeadings => false;
	}
}
