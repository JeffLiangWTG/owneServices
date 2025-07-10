using System.IO;
using Enterprise.Rating.Module;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ReportTesting.MasterFiles
{
	[TemplateName("Entity Commission Report")]
	public class EntityCommissionReportTemplateTest : TemplateTestCase
	{
		public void TestOrganizationEntityColumnsShown()
		{
			PrepareReportForRender();
			FillReportWithDefaultValues();

			using (var stream = new MemoryStream())
			using (var excelInterface = new DocumentEngine.FlexCelInterface.ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				AssertEquals("Organization Entity", excelInterface.WorkSheets[0][4, 23].ToString());
				AssertEquals("Organization Entity Name", excelInterface.WorkSheets[0][4, 24].ToString());
			}
		}

		public void TestSelectedFiltersCell_WrapTextFalse()
		{
			PrepareReportForRender();
			FillReportWithDefaultValues();

			using (var stream = new MemoryStream())
			using (var excelInterface = new DocumentEngine.FlexCelInterface.ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				Assert("Wrap Text should be false for the Selected Filters cell.", !excelInterface.WorkSheets[0].GetCellFormat(1, 1).WrapText);
			}
		}
	}

	public class EntityCommissionReportReportTest : ReportTestCase
	{
		public override ZEmbeddedModule ModuleToTest
		{
			get { return new SalesMgrReports(); }
		}

		public override string MenuName
		{
			get { return "Entity Commission Report"; }
		}

		public override string Hint
		{
			get { return @"Entity Commission Report shows a listing of all entity commissions in the system."; }
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new EntityCommissionReportTemplateTest();
		}
	}
}
