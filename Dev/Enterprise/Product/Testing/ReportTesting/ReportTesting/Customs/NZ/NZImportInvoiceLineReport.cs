namespace Enterprise.ReportTesting.Customs.NZ
{
	using CargoWise.EntityFramework;
	using Enterprise.Customs.Business;
	using Enterprise.Customs.Module;
	using Enterprise.DocumentEngine.RuntimeOptions;
	using Enterprise.MasterFiles.Business;
	using Enterprise.ReportTesting;
	using NUnit.Framework;

	[TemplateName("NZ Import Invoice Line Report")]
	class TestNZImportInvoiceLineReport : TemplateTestCase
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

		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry("NZ");
		}

		protected override void FillReportWithDefaultValues()
		{
			base.FillReportWithDefaultValues();
			Factory.New<BaseJobDeclaration>();
			var part = Factory.New<Enterprise.Customs.Business.OrgSupplierPart>();
			part.OP_PartNum = "part";
			Factory.Save();
		}
	}

	public class TestNZInvoiceLineReportMenuSetup : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new CustomsReportModule(); }
		}

		public override string MenuName
		{
			get { return "NZ Import Invoice Line Report"; }
		}

		public override string Hint
		{
			get { return "This report shall use Invoice Lines as the primary data source. This means that one record will appear on the report for every invoice line that matches the filter criteria selected."; }
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestNZImportInvoiceLineReport();
		}

		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry("NZ");
		}
	}
}
