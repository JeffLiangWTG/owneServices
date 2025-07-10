namespace Enterprise.ReportTesting.Accounting.Netting
{
	[TemplateName("Netting - Trial Payment In Out Report")]
	public class TrialPaymentInOutReportTest : TemplateTestCase
	{
	}

	[TemplateName("Netting - Position By Currency")]
	public class TrialPositionByCurrencyReportTest : TemplateTestCase
	{
		protected override bool ReportRequiresColumnHeadings
		{
			get
			{
				return false;
			}
		}
	}

	[TemplateName("Netting - Pre FX Deal")]
	public class PreFXDealReportTest : TemplateTestCase
	{
	}

	[TemplateName("Netting - FX Deal")]
	public class TrialFXDealReportTest : TemplateTestCase
	{
	}

	[TemplateName("Netting - Spread Results")]
	public class TrialSpreadResultReportTest : TemplateTestCase
	{
		protected override bool ReportRequiresColumnHeadings
		{
			get
			{
				return false;
			}
		}
	}
}
