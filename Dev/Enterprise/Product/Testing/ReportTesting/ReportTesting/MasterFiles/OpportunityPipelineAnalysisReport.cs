using System.IO;
using CargoWise.Types;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.MarketingManager.Business;
using Enterprise.MarketingManager.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ReportTesting.MasterFiles
{
	[TemplateName("Opportunity Pipeline Analysis Report")]
	public class OpportunityPipelineAnalysisReport : TemplateTestCase
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
			Report.OptionalTemplateSheetCollection["Opportunity Summary"].Selected = true;
			Report.OptionalTemplateSheetCollection["Opportunity With Value Analysis"].Selected = true;
			FillReportWithDefaultValues();
			SetupReportData();

			SetTestColumnToFirst(0, "Recall Date");
			SetTestColumnToFirst(1, "Recall Date");

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				const int firstDataRow = 14;
				const int firstDataColumn = 5;
				excelInterface.Xls.ActiveSheet = 1;
				AssertEquals("20/May/01 23:50", excelInterface.Xls.GetStringFromCell(firstDataRow, firstDataColumn).Value);

				excelInterface.Xls.ActiveSheet = 2;
				AssertEquals("20/May/01 23:50", excelInterface.Xls.GetStringFromCell(firstDataRow, firstDataColumn).Value);
			}
		}

		#region Value Analysis Details columns visibility tests

		public void TestVADetailsColumnIsCorrectlyHiddenIfRequired_VerticalMarket()
		{
			AssertVADetailsColumnIsCorrectlyHiddenIfRequired("Vertical Market", "By Vertical Market", "Break down by Vertical Market", "Vertical Market");
		}

		public void TestVADetailsColumnIsCorrectlyHiddenIfRequired_PeriodOfActivity()
		{
			AssertVADetailsColumnIsCorrectlyHiddenIfRequired("Period of Activity", "By Period of Activity", "Break down by Period of Activity", "Period of Activity");
		}

		public void TestVADetailsColumnIsCorrectlyHiddenIfRequired_Incoterm()
		{
			AssertVADetailsColumnIsCorrectlyHiddenIfRequired("Incoterm", "By Incoterm", "Break down by Incoterm", "Incoterm");
		}

		public void TestVADetailsColumnIsCorrectlyHiddenIfRequired_Commodity()
		{
			AssertVADetailsColumnIsCorrectlyHiddenIfRequired("Commodity", "By Commodity", "Break down by Commodity", "Commodity");
		}

		public void TestVADetailsColumnIsCorrectlyHiddenIfRequired_Carrier()
		{
			AssertVADetailsColumnIsCorrectlyHiddenIfRequired("Carrier", "By Carrier", "Break down by Carrier", "Carrier");
		}

		public void TestVADetailsColumnIsCorrectlyHiddenIfRequired_Competitor()
		{
			AssertVADetailsColumnIsCorrectlyHiddenIfRequired("Competitor", "By Competitor", "Break down by Competitor", "Competitor");
		}

		public void TestVADetailsColumnIsCorrectlyHiddenIfRequired_ControllingAgent()
		{
			AssertVADetailsColumnIsCorrectlyHiddenIfRequired("Controlling Agent", "By Controlling Agent", "Break down by Controlling Agent", "Controlling Agent");
		}

		void AssertVADetailsColumnIsCorrectlyHiddenIfRequired(string columnHeadingName, string filterByCriteriaName, string filterByCriteriaOptionName, string expectedCellContent)
		{
			PrepareReportForRender();
			Report.OptionalTemplateSheetCollection["Opportunity Summary"].Selected = false;
			Report.OptionalTemplateSheetCollection["Opportunity With Value Analysis"].Selected = true;
			FillReportWithDefaultValues();
			SetupReportData();

			AssertFilterByCriteriaColumnVisibility(columnHeadingName, filterByCriteriaName, filterByCriteriaOptionName, expectedCellContent);
		}

		void AssertFilterByCriteriaColumnVisibility(string columnHeadingName, string filterByCriteriaName, string filterByCriteriaOptionName, string expectedCellContent)
		{
			SetTestColumnToFirst(1, columnHeadingName);
			AssertColumnVisibility(filterByCriteriaName, filterByCriteriaOptionName, expectedCellContent, isHideValueAnalysisFilterTicked: false, isFilterByCriteriaOptionTicked: false, shouldColumnBeHidden: true);
			AssertColumnVisibility(filterByCriteriaName, filterByCriteriaOptionName, expectedCellContent, isHideValueAnalysisFilterTicked: false, isFilterByCriteriaOptionTicked: true, shouldColumnBeHidden: false);
			AssertColumnVisibility(filterByCriteriaName, filterByCriteriaOptionName, expectedCellContent, isHideValueAnalysisFilterTicked: true, isFilterByCriteriaOptionTicked: false, shouldColumnBeHidden: true);
			AssertColumnVisibility(filterByCriteriaName, filterByCriteriaOptionName, expectedCellContent, isHideValueAnalysisFilterTicked: true, isFilterByCriteriaOptionTicked: true, shouldColumnBeHidden: true);
		}

		void AssertColumnVisibility(string filterByCriteriaName, string filterByCriteriaOptionName, string expectedCellContent, bool isHideValueAnalysisFilterTicked, bool isFilterByCriteriaOptionTicked, bool shouldColumnBeHidden)
		{
			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				var hideValueAnalysisFilter = (OptionGroup)Report.FilterCollection["Hide Value Analysis"];
				var byVerticalMarkerFilter = (OptionGroup)Report.FilterCollection[filterByCriteriaName];

				hideValueAnalysisFilter.DescriptionCodePairList["Hide Value Analysis results"].Value = isHideValueAnalysisFilterTicked;
				byVerticalMarkerFilter.DescriptionCodePairList[filterByCriteriaOptionName].Value = isFilterByCriteriaOptionTicked;

				Report.Save(stream);
				excelInterface.LoadExcelFile(stream);
				excelInterface.ActiveWorksheet = 1;

				AssertEquals(expectedCellContent, excelInterface.Xls.GetStringFromCell(15, 5).Value);
				AssertEquals(shouldColumnBeHidden, excelInterface.Xls.GetColHidden(5));

				Report.ResetCachedExcelFileForTesting();
			}
		}

		#endregion

		void SetTestColumnToFirst(int excelSheet, string columnHeadingName)
		{
			var columnHeadings = Report.ColumnHeadingManager.CurrentConfiguration.Worksheets[excelSheet].ColumnHeadings;
			var columnHeading = columnHeadings[columnHeadingName];
			columnHeading.Hidden = false;
			columnHeading.CurrentPosition = 0;
		}

		void SetupReportData()
		{
			var shipmentProduct = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment);
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			opportunity.P8_RecallDateLocal = new ZDateTime(2001, 05, 20, 23, 50, 12);
			var sales = Factory.NewWithValidTestData<OrgSales>();
			sales.OW_MP_Product = shipmentProduct.PK;
			var tradeDetail = sales.TradeDetails.AddNew();
			tradeDetail.CurrentProspectPeriod.PAS_OH_Client = org.PK;

			OrgSalesValueAssociationPivot.AddPivotIfNotExist(opportunity, tradeDetail);

			Factory.Save();
		}
	}

	public class TestOpportunityPipelineAnalysisReport : ReportTestCase
	{
		public override ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new Enterprise.Rating.Module.SalesMgrReports(); }
		}

		public override string MenuName
		{
			get { return "Opportunity Pipeline Analysis"; }
		}

		public override string Hint
		{
			get
			{
				return @"List of Opportunities with Value Analysis details.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new OpportunityPipelineAnalysisReport();
		}
	}
}
