namespace Enterprise.ReportTesting.Customs.US
{
	using Enterprise.Customs.Module;
	using Enterprise.MasterFiles.Business;
	using Enterprise.ReportTesting;

	[TemplateName("Import MID Report")]
	public class TestUSImportMIDReport : TemplateTestCase
	{
		public void TestReportSql()
		{
			PrepareReportForRender();
			AssertEquals("ReportData: SELECT <ReportData.SelectList> FROM USMIDs(<CurrentCompany>, <Job Registered On.FromDateUtc>, <Job Registered On.ToNextDateUtc>, <Importer>, <Declaration Branch.Pks>, <Importer of Record>)",
				Report.Analyser.ReportSQLSources[0].TableNameAndSelectStatement);
		}

		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry("US");
		}
	}

	public class TestUSImportMIDReportMenuSetup : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new CustomsReportModule(); }
		}

		public override string MenuName
		{
			get { return "Import Supplier/Manufacturer Report"; }
		}

		public override string Hint
		{
			get { return "This report makes Manufacturer ID information available for import (IMP, IMX, MSC) declarations."; }
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestUSImportMIDReport();
		}

		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry("US");
		}
	}
}
