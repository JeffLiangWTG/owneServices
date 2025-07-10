namespace Enterprise.ReportTesting.Accounting.Netting
{
	[TemplateName("Netting - Final Payment In Out Report")]
	public class FinalPaymentInOutReportTest : TemplateTestCase
	{
	}

	[TemplateName("Netting - Position By Currency - Final")]
	public class FinalPositionByCurrencyReportTest : TemplateTestCase
	{
		protected override bool ReportRequiresColumnHeadings
		{
			get
			{
				return false;
			}
		}
	}

	[TemplateName("Netting - FX Deal - Final")]
	public class FinalFXDealReportTest : TemplateTestCase
	{
	}

	[TemplateName("Netting - Spread Results - Final")]
	public class FinalSpreadResultReportTest : TemplateTestCase
	{
		protected override bool ReportRequiresColumnHeadings
		{
			get
			{
				return false;
			}
		}
	}

	[TemplateName("Netting - Management Report")]
	public class ManagementReportTest : TemplateTestCase
	{
	}
}
