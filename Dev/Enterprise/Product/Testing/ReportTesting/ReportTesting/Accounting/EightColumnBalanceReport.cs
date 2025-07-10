#region Test
#if DEBUG
namespace Enterprise.ReportTesting.Accounting
{
	using NUnit.Framework;

	[TemplateName("Eight Column Balance Report")]
	public class TestEightColumnBalanceReport : TemplateTestCase
	{
		protected override bool ReportRequiresColumnHeadings => false;

		[ExpectNoExceptions]
		public void TestReportOnlyRequiredFilters()
		{
			PrepareReportForRender();
			SelectAllOptionalTemplates();
			RunReport();
		}
	}
}

#endif
#endregion
