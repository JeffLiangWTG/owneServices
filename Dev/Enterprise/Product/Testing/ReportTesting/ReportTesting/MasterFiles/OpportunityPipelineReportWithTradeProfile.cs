using System.IO;
using CargoWise.Types;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.MasterFiles.Business;

namespace Enterprise.ReportTesting.MasterFiles
{
	[TemplateName("Opportunity Pipeline Report With Trade Profile")]
	public class OpportunityPipelineReportWithTradeProfile : TemplateTestCase
	{
		protected override void FillReportWithDefaultValues()
		{
			base.FillReportWithDefaultValues();
			Factory.New<SalesTeam>();
			Factory.Save();
		}

		public void TestRecallDate()
		{
			PrepareReportForRender();
			FillReportWithDefaultValues();
			SetupReportData();

			var columnHeadings = Report.ColumnHeadingManager.CurrentConfiguration.Worksheets[0].ColumnHeadings;
			var recallDateHeading = columnHeadings["Recall Date"];
			recallDateHeading.Hidden = false;
			recallDateHeading.CurrentPosition = 0;

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				const int firstDataRow = 13;
				const int firstDataColumn = 5;
				excelInterface.Xls.ActiveSheet = 1;
				AssertEquals("24/May/22 23:50", excelInterface.Xls.GetStringFromCell(firstDataRow, firstDataColumn).Value);
			}
		}

		void SetupReportData()
		{
			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			opportunity.P8_RecallDateLocal = new ZDateTime(2022, 05, 24, 23, 50, 12);
			Factory.Save();
		}
	}

	public class TestOpportunityPipelineReportWithTradeProfile : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new Enterprise.Rating.Module.SalesMgrReports(); }
		}

		public override string MenuName
		{
			get { return "Opportunity Pipeline With Trade Profile"; }
		}

		public override string Hint
		{
			get
			{
				return @"List of Opportunities with Trade Profile details

You are able to filter this report by Sales Rep, Sales Team, Sales Type, Closed Date, Campaign, Value, Estimated Close Date, Close Certainty, Trade Lane Origin / Destination, Trade Mode, Inco Term, Trade Status, Commodity";
			}
		}

		protected override bool ExpectedIsPublished
		{
			get { return false; }
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new OpportunityPipelineReportWithTradeProfile();
		}
	}
}
