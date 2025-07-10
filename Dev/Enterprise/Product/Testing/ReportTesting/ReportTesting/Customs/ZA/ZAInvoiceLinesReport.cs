using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Module;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.ReportTesting.Customs.ZA
{
	[TemplateName("ZA Invoice Line Report")]
	public class TestZAInvoiceLinesReportTemplate : TemplateTestCase
	{
		[ExpectNoExceptions]
		[StressTest]
		[TestDate(2006, 12, 25)]
		public void TestReportRunsWithNoExceptionWithAllColumnHeadings()
		{
			PrepareReportForRender();
			SelectAllOptionalTemplates();
			FillReportWithDefaultValues();
			var headings = Report.ColumnHeadingManager.CurrentConfiguration.Worksheets[0].ColumnHeadings;
			foreach (ColumnHeading heading in headings)
			{
				heading.Hidden = false;
			}
			RunReport();
		}

		protected override void FillReportWithDefaultValues()
		{
			base.FillReportWithDefaultValues();
			Factory.New<BaseJobDeclaration>();
			Factory.Save();
		}

		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry("ZA");
		}
	}

	public class TestZAInvoiceLinesReportMenuSetup : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new CustomsReportModule(); }
		}

		public override string MenuName
		{
			get { return "Invoice Line Report"; }
		}

		public override string Hint
		{
			get
			{
				return @"This report makes ZA declaration and invoice line information available.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestZAInvoiceLinesReportTemplate();
		}
	}
}
