using CargoWise.EntityFramework.Testing;
using Enterprise.MarketingManager.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business.Testing
{
	sealed class TradedSalesAnalysisValidationTest : BusinessObjectValidationTestCase
	{
		public void TestAnalysisPeriod()
		{
			var org = Factory.New<OrgHeader>();
			var salesHeader = new SalesHeader(org, Factory.New<OrgSalesProduct>());
			var analysis = new TradedSalesAnalysis(salesHeader);

			analysis.Period = "XXX";
			AssertListValidationInvalidCodeError(analysis.PeriodInfo, true);

			analysis.Period = SalesAnalysisPeriodList.Codes.CurrentMonth;
			AssertListValidationInvalidCodeError(analysis.PeriodInfo, false);
		}

		public void TestMainGroupingType()
		{
			var org = Factory.New<OrgHeader>();
			var salesHeader = new SalesHeader(org, Factory.New<OrgSalesProduct>());
			var analysis = new TradedSalesAnalysis(salesHeader);

			analysis.MainGroupingType = "XXX";
			AssertListValidationInvalidCodeError(analysis.MainGroupingTypeInfo, true);

			analysis.MainGroupingType = SalesAnalysisMainGroupingTypeList.Codes.ModeAndType;
			AssertListValidationInvalidCodeError(analysis.MainGroupingTypeInfo, false);
		}

		public void TestLocationGroupingType()
		{
			var product = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment);
			var org = Factory.New<OrgHeader>();
			var salesHeader = new SalesHeader(org, product);
			var analysis = new TradedSalesAnalysis(salesHeader);

			analysis.LocationGroupingType = "XXX";
			AssertListValidationInvalidCodeError(analysis.LocationGroupingTypeInfo, true);

			analysis.LocationGroupingType = SalesAnalysisLocationGroupingTypeList.Codes.CountryToCountry;
			AssertListValidationInvalidCodeError(analysis.LocationGroupingTypeInfo, false);
		}
	}
}
