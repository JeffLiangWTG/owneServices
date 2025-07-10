using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.MarketingManager.GUI.Testing
{
	class TradedSalesAnalysisControlTest : TestCaseWithFactory
	{
		#region MainGroupingGrid

		public void TestMainGroupingGridColumns()
		{
			var product = Factory.New<OrgSalesProduct>();
			var org = Factory.New<OrgHeader>();
			var salesHeader = new SalesHeader(org, product);
			var salesAnalysis = new TradedSalesAnalysis(salesHeader);
			salesAnalysis.IncludeJobValue = false;

			using (var control = new TradedSalesAnalysisControlForTest(salesAnalysis))
			{
				control.Show();

				AssertMainGroupingColumns(control, SalesAnalysisMainGroupingTypeList.Codes.Location, SalesAnalysisLocationGroupingTypeList.Codes.CountryToCountry,
					TradePeriodGrouping.Schema.OriginCountry, TradePeriodGrouping.Schema.DestinationCountry, TradePeriodGrouping.Schema.GrossRevenue, TradePeriodGrouping.Schema.CurrencyCode);

				AssertMainGroupingColumns(control, SalesAnalysisMainGroupingTypeList.Codes.Location, SalesAnalysisLocationGroupingTypeList.Codes.DestinationCountry,
					TradePeriodGrouping.Schema.DestinationCountry, TradePeriodGrouping.Schema.GrossRevenue, TradePeriodGrouping.Schema.CurrencyCode);

				AssertMainGroupingColumns(control, SalesAnalysisMainGroupingTypeList.Codes.Location, SalesAnalysisLocationGroupingTypeList.Codes.DestinationState,
					TradePeriodGrouping.Schema.DestinationState, TradePeriodGrouping.Schema.GrossRevenue, TradePeriodGrouping.Schema.CurrencyCode);

				AssertMainGroupingColumns(control, SalesAnalysisMainGroupingTypeList.Codes.Location, SalesAnalysisLocationGroupingTypeList.Codes.LowestToLowest,
					TradePeriodGrouping.Schema.Origin, TradePeriodGrouping.Schema.Destination, TradePeriodGrouping.Schema.GrossRevenue, TradePeriodGrouping.Schema.CurrencyCode);

				AssertMainGroupingColumns(control, SalesAnalysisMainGroupingTypeList.Codes.Location, SalesAnalysisLocationGroupingTypeList.Codes.OriginCountry,
					TradePeriodGrouping.Schema.OriginCountry, TradePeriodGrouping.Schema.GrossRevenue, TradePeriodGrouping.Schema.CurrencyCode);

				AssertMainGroupingColumns(control, SalesAnalysisMainGroupingTypeList.Codes.Location, SalesAnalysisLocationGroupingTypeList.Codes.OriginState,
					TradePeriodGrouping.Schema.OriginState, TradePeriodGrouping.Schema.GrossRevenue, TradePeriodGrouping.Schema.CurrencyCode);

				AssertMainGroupingColumns(control, SalesAnalysisMainGroupingTypeList.Codes.Location, SalesAnalysisLocationGroupingTypeList.Codes.StateToState,
					TradePeriodGrouping.Schema.OriginState, TradePeriodGrouping.Schema.DestinationState, TradePeriodGrouping.Schema.GrossRevenue, TradePeriodGrouping.Schema.CurrencyCode);

				AssertMainGroupingColumns(control, SalesAnalysisMainGroupingTypeList.Codes.Location, SalesAnalysisLocationGroupingTypeList.Codes.UnlocoToUnloco,
					TradePeriodGrouping.Schema.OriginUnloco, TradePeriodGrouping.Schema.DestinationUnloco, TradePeriodGrouping.Schema.GrossRevenue, TradePeriodGrouping.Schema.CurrencyCode);

				AssertMainGroupingColumns(control, SalesAnalysisMainGroupingTypeList.Codes.Location, SalesAnalysisLocationGroupingTypeList.Codes.Warehouse,
					TradePeriodGrouping.Schema.Warehouse, TradePeriodGrouping.Schema.WarehouseCountry, TradePeriodGrouping.Schema.GrossRevenue, TradePeriodGrouping.Schema.CurrencyCode);

				AssertMainGroupingColumns(control, SalesAnalysisMainGroupingTypeList.Codes.Location, SalesAnalysisLocationGroupingTypeList.Codes.WarehouseCountry,
					TradePeriodGrouping.Schema.WarehouseCountry, TradePeriodGrouping.Schema.GrossRevenue, TradePeriodGrouping.Schema.CurrencyCode);

				AssertMainGroupingColumns(control, SalesAnalysisMainGroupingTypeList.Codes.Service, SalesAnalysisLocationGroupingTypeList.Codes.Warehouse,
					TradePeriodGrouping.Schema.Service, TradePeriodGrouping.Schema.GrossRevenue, TradePeriodGrouping.Schema.CurrencyCode);

				AssertMainGroupingColumns(control, SalesAnalysisMainGroupingTypeList.Codes.Mode, SalesAnalysisLocationGroupingTypeList.Codes.CountryToCountry,
					TradePeriodGrouping.Schema.Mode, TradePeriodGrouping.Schema.GrossRevenue, TradePeriodGrouping.Schema.CurrencyCode);

				AssertMainGroupingColumns(control, SalesAnalysisMainGroupingTypeList.Codes.ModeAndType, SalesAnalysisLocationGroupingTypeList.Codes.CountryToCountry,
					TradePeriodGrouping.Schema.Mode, TradePeriodGrouping.Schema.Type, TradePeriodGrouping.Schema.GrossRevenue, TradePeriodGrouping.Schema.CurrencyCode);

				AssertMainGroupingColumns(control, SalesAnalysisMainGroupingTypeList.Codes.Product, SalesAnalysisLocationGroupingTypeList.Codes.Warehouse,
					TradePeriodGrouping.Schema.SupplierPartNum, TradePeriodGrouping.Schema.SupplierPartDescription, TradePeriodGrouping.Schema.GrossRevenue, TradePeriodGrouping.Schema.CurrencyCode);
			}
		}

		void AssertMainGroupingColumns(TradedSalesAnalysisControlForTest tradedSalesAnalysisControl, string mainGroupingType, string locationGroupingType, params string[] expectedColumns)
		{
			tradedSalesAnalysisControl.SalesAnalysis.MainGroupingType = mainGroupingType;
			tradedSalesAnalysisControl.SalesAnalysis.LocationGroupingType = locationGroupingType;

			AssertMultilineASCIIEquals(string.Format("Columns for Main Grouping:{0} Location Grouping:{1}", mainGroupingType, locationGroupingType),
				string.Join(System.Environment.NewLine, expectedColumns),
				string.Join(System.Environment.NewLine, tradedSalesAnalysisControl.MainGroupingGrid_Exposed.ColumnStyles.Cast<ZTextBoxColumnStyleInfo>().Where(x => !x.IsUnavailable).Select(x => x.ColumnName)));
		}

		#endregion

		public void TestValidAnalysisPeriods()
		{
			var product = Factory.New<OrgSalesProduct>();
			var org = Factory.New<OrgHeader>();
			var salesHeader = new SalesHeader(org, product);
			var salesAnalysis = new TradedSalesAnalysis(salesHeader);

			using (var control = new TradedSalesAnalysisControl(salesAnalysis))
			{
				control.SetAnalysisPeriod(SalesAnalysisPeriodList.Codes.CurrentFinancialYear);
				AssertEquals(SalesAnalysisPeriodList.Codes.CurrentFinancialYear, salesAnalysis.Period);

				control.SetAnalysisPeriod(SalesAnalysisPeriodList.Codes.CurrentMonth);
				AssertEquals(SalesAnalysisPeriodList.Codes.CurrentMonth, salesAnalysis.Period);
			}
		}

		public void TestInvalidAnalysisPeriods()
		{
			var product = Factory.New<OrgSalesProduct>();
			var org = Factory.New<OrgHeader>();
			var salesHeader = new SalesHeader(org, product);
			var salesAnalysis = new TradedSalesAnalysis(salesHeader);

			using (var control = new TradedSalesAnalysisControl(salesAnalysis))
			{
				var defaultPeriod = salesAnalysis.Period;

				control.SetAnalysisPeriod("nonemptyinvalidvalue");

				AssertEquals(defaultPeriod, salesAnalysis.Period);
			}
		}

		class TradedSalesAnalysisControlForTest : TradedSalesAnalysisControl
		{
			public TradedSalesAnalysisControlForTest(TradedSalesAnalysis salesAnalysis)
				: base(salesAnalysis)
			{
			}

			public ZGrid MainGroupingGrid_Exposed
			{
				get { return MainGroupingGrid; }
			}
		}
	}
}
