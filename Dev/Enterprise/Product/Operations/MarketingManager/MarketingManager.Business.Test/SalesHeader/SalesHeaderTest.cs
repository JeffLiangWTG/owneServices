using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MarketingManager.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(SalesHeader))]
	sealed class SalesHeaderTest : NonPersistentBusinessObjectTestCase
	{
		#region Properties

		public void TestSalesProductCode()
		{
			var salesProduct = Factory.New<OrgSalesProduct>();
			salesProduct.MP_Code = "XXX";

			var org = Factory.New<OrgHeader>();
			var salesHeader = new SalesHeader(org, salesProduct);
			AssertEquals("XXX", salesHeader.SalesProductCode);
		}

		public void TestTotalCurrencyCode()
		{
			var audCompany = Factory.NewWithValidTestData<GlbCompany>();
			audCompany.GC_RX_NKLocalCurrency = "AUD";
			var usdCompany = Factory.NewWithValidTestData<GlbCompany>();
			usdCompany.GC_RX_NKLocalCurrency = "USD";
			var audBranch = Factory.NewWithValidTestData<GlbBranch>();
			audBranch.GB_GC = audCompany.PK;
			var usdBranch = Factory.NewWithValidTestData<GlbBranch>();
			usdBranch.GB_GC = usdCompany.PK;
			var salesProduct = Factory.NewWithValidTestData<OrgSalesProduct>();
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, audBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var org = Factory.New<OrgHeader>();
				var header = new SalesHeader(org, salesProduct);
				AssertEquals("AUD", header.TotalCurrencyCode);
			}

			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, usdBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var org = Factory.New<OrgHeader>();
				var header = new SalesHeader(org, salesProduct);
				AssertEquals("USD", header.TotalCurrencyCode);
			}
		}

		[TestDate(2015, 5, 5)]
		public void TestTradedAnnualTotal()
		{
			var product = Factory.New<OrgSalesProduct>();
			product.MP_Code = "CW1";
			product.MP_Name = "CargoWiseOne";
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var sales1 = org.SalesCollection.AddNew();
			sales1.OW_MP_Product = product.PK;
			sales1.OW_IsTraded = true;
			var tradeDetail1 = sales1.TradeDetails.AddNew();
			var tradePeriod1 = tradeDetail1.TradedPeriods.AddNew();
			tradePeriod1.PAS_Period = new ZDate(2015, 1, 1);
			tradePeriod1.PAS_OH_Client = org.PK;
			var tradeValue1 = tradePeriod1.TradeValues.AddNew();
			tradeValue1.PAV_GC = Env.CurrentCompanyPK;
			tradeValue1.PAV_Revenue = 100;
			tradeValue1.PAV_RX_NKCurrency = "AUD";

			var sales2 = org.SalesCollection.AddNew();
			sales2.OW_MP_Product = product.PK;
			sales2.OW_IsTraded = true;
			var tradeDetail2 = sales2.TradeDetails.AddNew();
			var tradePeriod2 = tradeDetail2.TradedPeriods.AddNew();
			tradePeriod2.PAS_Period = new ZDate(2015, 2, 1);
			tradePeriod2.PAS_OH_Client = org.PK;
			var tradeValue2 = tradePeriod2.TradeValues.AddNew();
			tradeValue2.PAV_GC = Env.CurrentCompanyPK;
			tradeValue2.PAV_Revenue = 100;
			tradeValue2.PAV_RX_NKCurrency = "USD";

			var sales3 = org.SalesCollection.AddNew();
			sales3.OW_MP_Product = product.PK;
			sales3.OW_IsTraded = true;
			var tradeDetail3 = sales3.TradeDetails.AddNew();
			var tradePeriod3 = tradeDetail3.TradedPeriods.AddNew();
			tradePeriod3.PAS_Period = new ZDate(2014, 6, 1);
			tradePeriod3.PAS_OH_Client = org.PK;
			var tradeValue3 = tradePeriod3.TradeValues.AddNew();
			tradeValue3.PAV_GC = Env.CurrentCompanyPK;
			tradeValue3.PAV_Revenue = 100;
			tradeValue3.PAV_RX_NKCurrency = "AUD";

			var currentPeriodSales = org.SalesCollection.AddNew();
			currentPeriodSales.OW_MP_Product = product.PK;
			currentPeriodSales.OW_IsTraded = true;
			var currentPeriodTradeDetail = currentPeriodSales.TradeDetails.AddNew();
			var currentPeriodTradePeriod = currentPeriodTradeDetail.TradedPeriods.AddNew();
			currentPeriodTradePeriod.PAS_Period = new ZDate(2015, 5, 1);
			currentPeriodTradePeriod.PAS_OH_Client = org.PK;
			var currentPeriodTradeValue = currentPeriodTradePeriod.TradeValues.AddNew();
			currentPeriodTradeValue.PAV_GC = Env.CurrentCompanyPK;
			currentPeriodTradeValue.PAV_Revenue = 100;
			currentPeriodTradeValue.PAV_RX_NKCurrency = "USD";

			var usdExRate = Factory.New<RefExchangeRate>();
			usdExRate.RE_RX_NKExCurrency = "USD";
			usdExRate.RE_StartDate = new ZDateTime(2015, 1, 1);
			usdExRate.RE_ExpiryDate = new ZDateTime(2016, 1, 1);
			usdExRate.RE_SellRate = 2;
			usdExRate.RE_ExRateType = Enterprise.Core.Constants.ExchangeRateTypes.Code.SellRate;

			Factory.Save();

			var salesHeader = new SalesHeader(org, product);
			salesHeader.RevenueDisplayOption = SalesHeader.RevenueDisplay.PerAnnum;
			AssertEquals(100m + 100m / 2m + 100m, salesHeader.TradedAnnualTotal);

			var helper = new AccountingPeriodTestHelper(Factory);
			helper.PostPeriodsForEntireYear(2015);

			salesHeader.RevenueDisplayOption = SalesHeader.RevenueDisplay.FinancialYear;
			AssertEquals(100m + 100m / 2m + 100m / 2m, salesHeader.TradedAnnualTotal);

			var newCompany = Factory.NewWithValidTestData<GlbCompany>();
			newCompany.GC_Code = "DDD";
			Factory.Save();

			salesHeader.CompanyFilter = newCompany.PK;
			salesHeader.RevenueDisplayOption = SalesHeader.RevenueDisplay.PerAnnum;
			AssertEquals("Company filter should not apply to traded values", 100m + 100m / 2m + 100m, salesHeader.TradedAnnualTotal);
			salesHeader.RevenueDisplayOption = SalesHeader.RevenueDisplay.FinancialYear;
			AssertEquals("Company filter should not apply to traded values", 100m + 100m / 2m + 100m / 2m, salesHeader.TradedAnnualTotal);
		}

		[TestDate(2015, 5, 5)]
		public void TestTradedAnnualTEUTotalQuantity()
		{
			var product = Factory.New<OrgSalesProduct>();
			product.MP_Code = "CW1";
			product.MP_Name = "CargoWiseOne";
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var sales1 = org.SalesCollection.AddNew();
			sales1.OW_MP_Product = product.PK;
			sales1.OW_IsTraded = true;
			var tradeDetail1 = sales1.TradeDetails.AddNew();
			var tradePeriod1 = tradeDetail1.TradedPeriods.AddNew();
			tradePeriod1.PAS_TEUQuantity = 10m;
			tradePeriod1.PAS_Period = new ZDate(2015, 1, 1);
			tradePeriod1.PAS_OH_Client = org.PK;
			var tradeValue1 = tradePeriod1.TradeValues.AddNew();
			tradeValue1.PAV_GC = Env.CurrentCompanyPK;
			tradeValue1.PAV_Revenue = 100;
			tradeValue1.PAV_RX_NKCurrency = "AUD";

			var sales2 = org.SalesCollection.AddNew();
			sales2.OW_MP_Product = product.PK;
			sales2.OW_IsTraded = true;
			var tradeDetail2 = sales2.TradeDetails.AddNew();
			var tradePeriod2 = tradeDetail2.TradedPeriods.AddNew();
			tradePeriod2.PAS_TEUQuantity = 20m;
			tradePeriod2.PAS_Period = new ZDate(2015, 2, 1);
			tradePeriod2.PAS_OH_Client = org.PK;
			var tradeValue2 = tradePeriod2.TradeValues.AddNew();
			tradeValue2.PAV_GC = Env.CurrentCompanyPK;
			tradeValue2.PAV_Revenue = 100;
			tradeValue2.PAV_RX_NKCurrency = "USD";

			var sales3 = org.SalesCollection.AddNew();
			sales3.OW_MP_Product = product.PK;
			sales3.OW_IsTraded = true;
			var tradeDetail3 = sales3.TradeDetails.AddNew();
			var tradePeriod3 = tradeDetail3.TradedPeriods.AddNew();
			tradePeriod3.PAS_TEUQuantity = 30m;
			tradePeriod3.PAS_Period = new ZDate(2014, 6, 1);
			tradePeriod3.PAS_OH_Client = org.PK;
			var tradeValue3 = tradePeriod3.TradeValues.AddNew();
			tradeValue3.PAV_GC = Env.CurrentCompanyPK;
			tradeValue3.PAV_Revenue = 100;
			tradeValue3.PAV_RX_NKCurrency = "AUD";

			var currentPeriodSales = org.SalesCollection.AddNew();
			currentPeriodSales.OW_MP_Product = product.PK;
			currentPeriodSales.OW_IsTraded = true;
			var currentPeriodTradeDetail = currentPeriodSales.TradeDetails.AddNew();
			var currentPeriodTradePeriod = currentPeriodTradeDetail.TradedPeriods.AddNew();
			currentPeriodTradePeriod.PAS_TEUQuantity = 40m;
			currentPeriodTradePeriod.PAS_Period = new ZDate(2015, 5, 1);
			currentPeriodTradePeriod.PAS_OH_Client = org.PK;
			var currentPeriodTradeValue = currentPeriodTradePeriod.TradeValues.AddNew();
			currentPeriodTradeValue.PAV_GC = Env.CurrentCompanyPK;
			currentPeriodTradeValue.PAV_Revenue = 100;
			currentPeriodTradeValue.PAV_RX_NKCurrency = "USD";

			var usdExRate = Factory.New<RefExchangeRate>();
			usdExRate.RE_RX_NKExCurrency = "USD";
			usdExRate.RE_StartDate = new ZDateTime(2015, 1, 1);
			usdExRate.RE_ExpiryDate = new ZDateTime(2016, 1, 1);
			usdExRate.RE_SellRate = 2;
			usdExRate.RE_ExRateType = Enterprise.Core.Constants.ExchangeRateTypes.Code.SellRate;

			Factory.Save();

			var salesHeader = new SalesHeader(org, product);
			salesHeader.RevenueDisplayOption = SalesHeader.RevenueDisplay.PerAnnum;

			AssertEquals("sales1 + sales2 + sales3. currentPeriodTradePeriod is out of range.", 10m + 20m + 30m, salesHeader.TradedAnnualTEUTotalQuantity);

			var helper = new AccountingPeriodTestHelper(Factory);
			helper.PostPeriodsForEntireYear(2015);

			salesHeader.RevenueDisplayOption = SalesHeader.RevenueDisplay.FinancialYear;
			AssertEquals("sales1 + sales2 + currentPeriodTradePeriod. sales3 is out of range", 10m + 20m + 40m, salesHeader.TradedAnnualTEUTotalQuantity);

			var newCompany = Factory.NewWithValidTestData<GlbCompany>();
			newCompany.GC_Code = "DDD";
			Factory.Save();

			salesHeader.CompanyFilter = newCompany.PK;
			salesHeader.RevenueDisplayOption = SalesHeader.RevenueDisplay.PerAnnum;
			AssertEquals("Company filter should not apply to traded values", 10m + 20m + 30m, salesHeader.TradedAnnualTEUTotalQuantity);
			salesHeader.RevenueDisplayOption = SalesHeader.RevenueDisplay.FinancialYear;
			AssertEquals("Company filter should not apply to traded values", 10m + 20m + 40m, salesHeader.TradedAnnualTEUTotalQuantity);
		}

		[TestDate(2017, 2, 1)]
		public void TestTotalEstimatedValue()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var product = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, "SHP");

			var sales1 = org.SalesCollection.AddNew();
			sales1.OW_MP_Product = product.PK;
			sales1.OW_IsTraded = false;
			sales1.OW_OH_Primary = org.PK;

			var tradeDetail1 = sales1.TradeDetails.AddNew();
			tradeDetail1.ProspectDetail.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.Yearly;
			tradeDetail1.CurrentProspectPeriod.PAS_RX_NKCurrency = "AUD";
			tradeDetail1.CurrentProspectPeriod.PAS_EstimatedProfit = 1000m;

			var sales2 = org.SalesCollection.AddNew();
			sales2.OW_MP_Product = product.PK;
			sales2.OW_IsTraded = false;
			sales2.OW_OH_Primary = org.PK;

			var tradeDetail2 = sales2.TradeDetails.AddNew();
			tradeDetail2.ProspectDetail.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.Yearly;
			tradeDetail2.CurrentProspectPeriod.PAS_RX_NKCurrency = "USD";
			tradeDetail2.CurrentProspectPeriod.PAS_EstimatedProfit = 1000m;

			var usdExRate = Factory.New<RefExchangeRate>();
			usdExRate.RE_RX_NKExCurrency = "USD";
			usdExRate.RE_StartDate = new ZDateTime(2017, 1, 1);
			usdExRate.RE_ExpiryDate = new ZDateTime(2018, 1, 1);
			usdExRate.RE_SellRate = 2;
			usdExRate.RE_ExRateType = Enterprise.Core.Constants.ExchangeRateTypes.Code.SellRate;

			Factory.Save();

			var salesHeader = new SalesHeader(org, product);
			salesHeader.CompanyFilter = ZGuid.Empty;
			AssertEquals(1000m + 1000m / 2m, salesHeader.TotalEstimatedAnnualValue);
			AssertEquals((1000m + 1000m / 2m) / 12, salesHeader.TotalEstimatedMonthlyAverage);
		}

		[TestDate(2017, 3, 2)]
		public void TestCommittedValue()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var opp = org.SalesOpportunities.AddNew();
			var product = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, "SHP");

			var sales1 = org.SalesCollection.AddNew();
			sales1.OW_MP_Product = product.PK;
			sales1.OW_IsTraded = false;
			sales1.OW_OH_Primary = org.PK;

			var tradeDetail1 = sales1.TradeDetails.AddNew();
			tradeDetail1.PA_Status = OpportunityTradeStatus.Codes.Successful;
			tradeDetail1.ProspectDetail.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.Monthly;
			tradeDetail1.ProspectPeriodStart = new ZDate(2017, 1, 1);
			tradeDetail1.ProspectPeriodEnd = new ZDate(2017, 3, 1);
			tradeDetail1.CurrentProspectPeriod.PAS_RX_NKCurrency = "AUD";
			tradeDetail1.CurrentProspectPeriod.PAS_EstimatedProfit = 100m;

			var sales2 = org.SalesCollection.AddNew();
			sales2.OW_MP_Product = product.PK;
			sales2.OW_IsTraded = false;
			sales2.OW_OH_Primary = org.PK;

			var tradeDetail2a = sales2.TradeDetails.AddNew();
			tradeDetail2a.PA_Status = OpportunityTradeStatus.Codes.Successful;
			tradeDetail2a.ProspectDetail.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.Monthly;
			tradeDetail2a.ProspectPeriodStart = new ZDate(2017, 1, 1);
			tradeDetail2a.ProspectPeriodEnd = new ZDate(2017, 6, 1);
			tradeDetail2a.CurrentProspectPeriod.PAS_RX_NKCurrency = "USD";
			tradeDetail2a.CurrentProspectPeriod.PAS_EstimatedProfit = 100m;

			var tradeDetail2b = sales2.TradeDetails.AddNew();
			tradeDetail2b.PA_Status = OpportunityTradeStatus.Codes.Active;
			tradeDetail2b.ProspectDetail.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.Monthly;
			tradeDetail2b.CurrentProspectPeriod.PAS_RX_NKCurrency = "USD";
			tradeDetail2b.CurrentProspectPeriod.PAS_EstimatedProfit = 200m;

			opp.AssociatedTradeLanesPivots.AddPivotFor(tradeDetail1);
			opp.AssociatedTradeLanesPivots.AddPivotFor(tradeDetail2a);
			opp.AssociatedTradeLanesPivots.AddPivotFor(tradeDetail2b);

			var usdExRate = Factory.New<RefExchangeRate>();
			usdExRate.RE_RX_NKExCurrency = "USD";
			usdExRate.RE_StartDate = new ZDateTime(2016, 1, 1);
			usdExRate.RE_ExpiryDate = new ZDateTime(2018, 1, 1);
			usdExRate.RE_SellRate = 2;
			usdExRate.RE_ExRateType = Enterprise.Core.Constants.ExchangeRateTypes.Code.SellRate;

			Factory.Save();

			var helper = new AccountingPeriodTestHelper(Factory);
			helper.PostPeriodsForEntireYear(2017);

			var salesHeader = new SalesHeader(org, product);
			salesHeader.CompanyFilter = ZGuid.Empty;

			CombineAssertions(() =>
			{
				AssertEquals("Committed annual value", 600m, salesHeader.CommittedAnnualValue);
				AssertEquals("Committed monthly value", 50m, salesHeader.CommittedMonthlyValue);
				AssertEquals("Committed current financial year value", 600m, salesHeader.CommittedCurrentFinancialYearValue);
				AssertEquals("Committed current financial year to date value", 450m, salesHeader.CommittedCurrentFinancialYearToDateValue);
				AssertEquals("Committed trailing 12 months value", 300m, salesHeader.CommittedTrailing12MonthsValue);
				AssertEquals("Forecast current financial year value", 300m, salesHeader.ForecastCurrentFinancialYearValue);
			});
		}

		[TestDate(2017, 3, 2)]
		public void TestCommittedTEUQuantity()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var opp = org.SalesOpportunities.AddNew();
			var product = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, "SHP");

			var sales1 = org.SalesCollection.AddNew();
			sales1.OW_MP_Product = product.PK;
			sales1.OW_IsTraded = false;
			sales1.OW_OH_Primary = org.PK;

			var tradeDetail1 = sales1.TradeDetails.AddNew();
			tradeDetail1.PA_Status = OpportunityTradeStatus.Codes.Successful;
			tradeDetail1.ProspectDetail.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.Monthly;
			tradeDetail1.CurrentProspectPeriod.PAS_TEUQuantity = 2m;
			tradeDetail1.CurrentProspectPeriod.PAS_RepeatsMnth = 4m;
			tradeDetail1.ProspectPeriodStart = new ZDate(2017, 1, 1);
			tradeDetail1.ProspectPeriodEnd = new ZDate(2017, 3, 1);
			tradeDetail1.CurrentProspectPeriod.PAS_RX_NKCurrency = "AUD";
			tradeDetail1.CurrentProspectPeriod.PAS_EstimatedProfit = 100m;

			var sales2 = org.SalesCollection.AddNew();
			sales2.OW_MP_Product = product.PK;
			sales2.OW_IsTraded = false;
			sales2.OW_OH_Primary = org.PK;

			var tradeDetail2a = sales2.TradeDetails.AddNew();
			tradeDetail2a.PA_Status = OpportunityTradeStatus.Codes.Successful;
			tradeDetail2a.ProspectDetail.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.Monthly;
			tradeDetail2a.CurrentProspectPeriod.PAS_TEUQuantity = 2m;
			tradeDetail2a.CurrentProspectPeriod.PAS_RepeatsMnth = 4m;
			tradeDetail2a.ProspectPeriodStart = new ZDate(2017, 1, 1);
			tradeDetail2a.ProspectPeriodEnd = new ZDate(2017, 6, 1);
			tradeDetail2a.CurrentProspectPeriod.PAS_RX_NKCurrency = "USD";
			tradeDetail2a.CurrentProspectPeriod.PAS_EstimatedProfit = 100m;

			var tradeDetail2b = sales2.TradeDetails.AddNew();
			tradeDetail2b.PA_Status = OpportunityTradeStatus.Codes.Active;
			tradeDetail2b.ProspectDetail.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.Monthly;
			tradeDetail2b.CurrentProspectPeriod.PAS_TEUQuantity = 2m;
			tradeDetail2b.CurrentProspectPeriod.PAS_RepeatsMnth = 4m;
			tradeDetail2b.CurrentProspectPeriod.PAS_RX_NKCurrency = "USD";
			tradeDetail2b.CurrentProspectPeriod.PAS_EstimatedProfit = 200m;

			opp.AssociatedTradeLanesPivots.AddPivotFor(tradeDetail1);
			opp.AssociatedTradeLanesPivots.AddPivotFor(tradeDetail2a);
			opp.AssociatedTradeLanesPivots.AddPivotFor(tradeDetail2b);

			var usdExRate = Factory.New<RefExchangeRate>();
			usdExRate.RE_RX_NKExCurrency = "USD";
			usdExRate.RE_StartDate = new ZDateTime(2016, 1, 1);
			usdExRate.RE_ExpiryDate = new ZDateTime(2018, 1, 1);
			usdExRate.RE_SellRate = 2;
			usdExRate.RE_ExRateType = Enterprise.Core.Constants.ExchangeRateTypes.Code.SellRate;

			Factory.Save();

			var helper = new AccountingPeriodTestHelper(Factory);
			helper.PostPeriodsForEntireYear(2017);

			var salesHeader = new SalesHeader(org, product);
			salesHeader.CompanyFilter = ZGuid.Empty;

			CombineAssertions(() =>
			{
				AssertEquals("Committed current financial year TEU value", 72m, salesHeader.CommittedCurrentFinancialYearTEUQuantity);
				AssertEquals("Committed current financial year to date TEU value", 48m, salesHeader.CommittedCurrentFinancialYearToDateTEUQuantity);
				AssertEquals("Committed trailing 12 months TEU value", 32m, salesHeader.CommittedTrailing12MonthsTEUQuantity);
				AssertEquals("Forecast current financial year TEU value", 24m, salesHeader.ForecastCurrentFinancialYearTEUQuantity);
			});
		}

		[TestDate(2018, 4, 1)]
		public void TestPipelineValue()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var opp1 = org.SalesOpportunities.AddNew();
			var opp2 = org.SalesOpportunities.AddNew();
			var product = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, "SHP");

			var sales1 = org.SalesCollection.AddNew();
			sales1.OW_MP_Product = product.PK;
			sales1.OW_IsTraded = false;
			sales1.OW_OH_Primary = org.PK;

			var tradeDetail1 = sales1.TradeDetails.AddNew();
			tradeDetail1.PA_Status = OpportunityTradeStatus.Codes.Active;
			tradeDetail1.ProspectDetail.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.Yearly;
			tradeDetail1.CurrentProspectPeriod.PAS_RX_NKCurrency = "AUD";
			tradeDetail1.CurrentProspectPeriod.PAS_EstimatedProfit = 1200m;

			var sales2 = org.SalesCollection.AddNew();
			sales2.OW_MP_Product = product.PK;
			sales2.OW_IsTraded = false;
			sales2.OW_OH_Primary = org.PK;

			var tradeDetail2a = sales2.TradeDetails.AddNew();
			tradeDetail2a.PA_Status = OpportunityTradeStatus.Codes.Successful;
			tradeDetail2a.ProspectDetail.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.Monthly;
			tradeDetail2a.ProspectPeriodStart = new ZDate(2017, 2, 1);
			tradeDetail2a.ProspectPeriodEnd = new ZDate(2017, 5, 1);
			tradeDetail2a.CurrentProspectPeriod.PAS_RX_NKCurrency = "USD";
			tradeDetail2a.CurrentProspectPeriod.PAS_EstimatedProfit = 100m;

			var tradeDetail2b = sales2.TradeDetails.AddNew();
			tradeDetail2b.PA_Status = OpportunityTradeStatus.Codes.Active;
			tradeDetail2b.ProspectDetail.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.Monthly;
			tradeDetail2b.CurrentProspectPeriod.PAS_RX_NKCurrency = "USD";
			tradeDetail2b.CurrentProspectPeriod.PAS_EstimatedProfit = 200m;

			opp1.AssociatedTradeLanesPivots.AddPivotFor(sales1);
			opp1.AssociatedTradeLanesPivots.AddPivotFor(tradeDetail1);

			opp2.AssociatedTradeLanesPivots.AddPivotFor(sales2);
			opp2.AssociatedTradeLanesPivots.AddPivotFor(tradeDetail2a);
			opp2.AssociatedTradeLanesPivots.AddPivotFor(tradeDetail2b);

			var usdExRate = Factory.New<RefExchangeRate>();
			usdExRate.RE_RX_NKExCurrency = "USD";
			usdExRate.RE_StartDate = new ZDateTime(2017, 7, 1);
			usdExRate.RE_ExpiryDate = new ZDateTime(2018, 7, 1);
			usdExRate.RE_SellRate = 2;
			usdExRate.RE_ExRateType = Enterprise.Core.Constants.ExchangeRateTypes.Code.SellRate;

			Factory.Save();

			var orgSalesHeader = new SalesHeader(org, product);
			AssertEquals(1200m + 200m * 12 / 2m, orgSalesHeader.PipelineValue);

			var opp1SalesHeader = new SalesHeader(org, opp1, product);
			AssertEquals(1200m, opp1SalesHeader.PipelineValue);

			var opp2SalesHeader = new SalesHeader(org, opp2, product);
			AssertEquals(200m * 12 / 2m, opp2SalesHeader.PipelineValue);
		}

		[TestDate(2018, 4, 1)]
		public void TestPipelineTEUQuantity()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var opp1 = org.SalesOpportunities.AddNew();
			var opp2 = org.SalesOpportunities.AddNew();
			var product = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, "SHP");

			var sales1 = org.SalesCollection.AddNew();
			sales1.OW_MP_Product = product.PK;
			sales1.OW_IsTraded = false;
			sales1.OW_OH_Primary = org.PK;

			var tradeDetail1 = sales1.TradeDetails.AddNew();
			tradeDetail1.PA_Status = OpportunityTradeStatus.Codes.Active;
			tradeDetail1.ProspectDetail.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.Yearly;
			tradeDetail1.CurrentProspectPeriod.PAS_TEUQuantity = 2m;
			tradeDetail1.CurrentProspectPeriod.PAS_RX_NKCurrency = "AUD";
			tradeDetail1.CurrentProspectPeriod.PAS_EstimatedProfit = 1200m;

			var sales2 = org.SalesCollection.AddNew();
			sales2.OW_MP_Product = product.PK;
			sales2.OW_IsTraded = false;
			sales2.OW_OH_Primary = org.PK;

			var tradeDetail2a = sales2.TradeDetails.AddNew();
			tradeDetail2a.PA_Status = OpportunityTradeStatus.Codes.Successful;
			tradeDetail2a.ProspectDetail.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.Monthly;
			tradeDetail2a.CurrentProspectPeriod.PAS_TEUQuantity = 2m;
			tradeDetail2a.ProspectPeriodStart = new ZDate(2017, 2, 1);
			tradeDetail2a.ProspectPeriodEnd = new ZDate(2017, 5, 1);
			tradeDetail2a.CurrentProspectPeriod.PAS_RX_NKCurrency = "USD";
			tradeDetail2a.CurrentProspectPeriod.PAS_EstimatedProfit = 100m;

			var tradeDetail2b = sales2.TradeDetails.AddNew();
			tradeDetail2b.PA_Status = OpportunityTradeStatus.Codes.Active;
			tradeDetail2b.ProspectDetail.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.Monthly;
			tradeDetail2b.CurrentProspectPeriod.PAS_TEUQuantity = 2m;
			tradeDetail2b.CurrentProspectPeriod.PAS_RX_NKCurrency = "USD";
			tradeDetail2b.CurrentProspectPeriod.PAS_EstimatedProfit = 200m;

			opp1.AssociatedTradeLanesPivots.AddPivotFor(sales1);
			opp1.AssociatedTradeLanesPivots.AddPivotFor(tradeDetail1);

			opp2.AssociatedTradeLanesPivots.AddPivotFor(sales2);
			opp2.AssociatedTradeLanesPivots.AddPivotFor(tradeDetail2a);
			opp2.AssociatedTradeLanesPivots.AddPivotFor(tradeDetail2b);

			var usdExRate = Factory.New<RefExchangeRate>();
			usdExRate.RE_RX_NKExCurrency = "USD";
			usdExRate.RE_StartDate = new ZDateTime(2017, 7, 1);
			usdExRate.RE_ExpiryDate = new ZDateTime(2018, 7, 1);
			usdExRate.RE_SellRate = 2;
			usdExRate.RE_ExRateType = Enterprise.Core.Constants.ExchangeRateTypes.Code.SellRate;

			Factory.Save();

			var orgSalesHeader = new SalesHeader(org, product);
			// tradeDetail1 : 1 (repeatsMth) * 2 (TEU quantity) * 1 (yearly) = 2
			// +
			// tradeDetail2b: 1 (repeatsMth)  * 2 (TEU quantity) * 12 (monthly) = 24
			AssertEquals((1m * 2m) + (1m * 2m * 12m), orgSalesHeader.PipelineTEUQuantity);

			var opp1SalesHeader = new SalesHeader(org, opp1, product);
			// tradeDetail1 : 1 (repeatsMth) * 2 (TEU quantity) * 1 (yearly) = 2
			AssertEquals(2m, opp1SalesHeader.PipelineTEUQuantity);

			var opp2SalesHeader = new SalesHeader(org, opp2, product);
			// tradeDetail2a: PA_Status != Active, so ignored. = 0
			// +
			// tradeDetail2b: 1 (repeatsMth)  * 2 (TEU quantity) * 12 (monthly) = 24
			AssertEquals(24m, opp2SalesHeader.PipelineTEUQuantity);
		}

		[TestDate(2018, 4, 1)]
		public void TestUnsuccessfulValue()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var opp1 = org.SalesOpportunities.AddNew();
			var opp2 = org.SalesOpportunities.AddNew();
			var product = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, "SHP");

			var sales1 = org.SalesCollection.AddNew();
			sales1.OW_MP_Product = product.PK;
			sales1.OW_IsTraded = false;
			sales1.OW_OH_Primary = org.PK;

			var tradeDetail1 = sales1.TradeDetails.AddNew();
			tradeDetail1.PA_Status = OpportunityTradeStatus.Codes.Unsuccessful;
			tradeDetail1.ProspectDetail.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.Yearly;
			tradeDetail1.CurrentProspectPeriod.PAS_RX_NKCurrency = "AUD";
			tradeDetail1.CurrentProspectPeriod.PAS_EstimatedProfit = 1200m;

			var sales2 = org.SalesCollection.AddNew();
			sales2.OW_MP_Product = product.PK;
			sales2.OW_IsTraded = false;
			sales2.OW_OH_Primary = org.PK;

			var tradeDetail2a = sales2.TradeDetails.AddNew();
			tradeDetail2a.PA_Status = OpportunityTradeStatus.Codes.Active;
			tradeDetail2a.ProspectDetail.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.Monthly;
			tradeDetail2a.CurrentProspectPeriod.PAS_RX_NKCurrency = "USD";
			tradeDetail2a.CurrentProspectPeriod.PAS_EstimatedProfit = 100m;

			var tradeDetail2b = sales2.TradeDetails.AddNew();
			tradeDetail2b.PA_Status = OpportunityTradeStatus.Codes.Unsuccessful;
			tradeDetail2b.ProspectDetail.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.Monthly;
			tradeDetail2b.CurrentProspectPeriod.PAS_RX_NKCurrency = "USD";
			tradeDetail2b.CurrentProspectPeriod.PAS_EstimatedProfit = 200m;

			opp1.AssociatedTradeLanesPivots.AddPivotFor(sales1);
			opp1.AssociatedTradeLanesPivots.AddPivotFor(tradeDetail1);

			opp2.AssociatedTradeLanesPivots.AddPivotFor(sales2);
			opp2.AssociatedTradeLanesPivots.AddPivotFor(tradeDetail2a);
			opp2.AssociatedTradeLanesPivots.AddPivotFor(tradeDetail2b);

			var usdExRate = Factory.New<RefExchangeRate>();
			usdExRate.RE_RX_NKExCurrency = "USD";
			usdExRate.RE_StartDate = new ZDateTime(2017, 7, 1);
			usdExRate.RE_ExpiryDate = new ZDateTime(2018, 7, 1);
			usdExRate.RE_SellRate = 2;
			usdExRate.RE_ExRateType = Enterprise.Core.Constants.ExchangeRateTypes.Code.SellRate;

			Factory.Save();

			var orgSalesHeader = new SalesHeader(org, product);
			AssertEquals(1200m + 200m * 12 / 2m, orgSalesHeader.UnsuccessfulValue);

			var opp1SalesHeader = new SalesHeader(org, opp1, product);
			AssertEquals(1200m, opp1SalesHeader.UnsuccessfulValue);

			var opp2SalesHeader = new SalesHeader(org, opp2, product);
			AssertEquals(200m * 12 / 2m, opp2SalesHeader.UnsuccessfulValue);
		}

		[TestDate(2018, 4, 1)]
		public void TestUnsuccessfulTEUQuantity()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var opp1 = org.SalesOpportunities.AddNew();
			var opp2 = org.SalesOpportunities.AddNew();
			var product = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, "SHP");

			var sales1 = org.SalesCollection.AddNew();
			sales1.OW_MP_Product = product.PK;
			sales1.OW_IsTraded = false;
			sales1.OW_OH_Primary = org.PK;

			var tradeDetail1 = sales1.TradeDetails.AddNew();
			tradeDetail1.PA_Status = OpportunityTradeStatus.Codes.Unsuccessful;
			tradeDetail1.ProspectDetail.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.Weekly;
			tradeDetail1.CurrentProspectPeriod.PAS_TEUQuantity = 3m;
			tradeDetail1.CurrentProspectPeriod.PAS_RX_NKCurrency = "AUD";
			tradeDetail1.CurrentProspectPeriod.PAS_EstimatedProfit = 1200m;

			var sales2 = org.SalesCollection.AddNew();
			sales2.OW_MP_Product = product.PK;
			sales2.OW_IsTraded = false;
			sales2.OW_OH_Primary = org.PK;

			var tradeDetail2a = sales2.TradeDetails.AddNew();
			tradeDetail2a.PA_Status = OpportunityTradeStatus.Codes.Active;
			tradeDetail2a.ProspectDetail.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.Monthly;
			tradeDetail2a.CurrentProspectPeriod.PAS_RX_NKCurrency = "USD";
			tradeDetail2a.CurrentProspectPeriod.PAS_EstimatedProfit = 100m;

			var tradeDetail2b = sales2.TradeDetails.AddNew();
			tradeDetail2b.PA_Status = OpportunityTradeStatus.Codes.Unsuccessful;
			tradeDetail2b.ProspectDetail.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.Monthly;
			tradeDetail2b.CurrentProspectPeriod.PAS_TEUQuantity = 5m;
			tradeDetail2b.CurrentProspectPeriod.PAS_RX_NKCurrency = "USD";
			tradeDetail2b.CurrentProspectPeriod.PAS_EstimatedProfit = 200m;

			opp1.AssociatedTradeLanesPivots.AddPivotFor(sales1);
			opp1.AssociatedTradeLanesPivots.AddPivotFor(tradeDetail1);

			opp2.AssociatedTradeLanesPivots.AddPivotFor(sales2);
			opp2.AssociatedTradeLanesPivots.AddPivotFor(tradeDetail2a);
			opp2.AssociatedTradeLanesPivots.AddPivotFor(tradeDetail2b);

			var usdExRate = Factory.New<RefExchangeRate>();
			usdExRate.RE_RX_NKExCurrency = "USD";
			usdExRate.RE_StartDate = new ZDateTime(2017, 7, 1);
			usdExRate.RE_ExpiryDate = new ZDateTime(2018, 7, 1);
			usdExRate.RE_SellRate = 2;
			usdExRate.RE_ExRateType = Enterprise.Core.Constants.ExchangeRateTypes.Code.SellRate;

			Factory.Save();

			var orgSalesHeader = new SalesHeader(org, product);
			// tradeDetail1 : 1 (repeatsMth)  * 3 (TEU quantity) * 52 (weekly) = 156
			// +
			// tradeDetail2b: 1 (repeatsMth) * 5 (TEU quantity) * 12 (monthly) = 60
			AssertEquals((1m * 3m * 52m) + (1m * 5m * 12m), orgSalesHeader.UnsuccessfulTEUQuantity);

			var opp1SalesHeader = new SalesHeader(org, opp1, product);
			// tradeDetail1 : 1 (repeatsMth) * 3 (TEU quantity) * 52 (weekly) = 156
			AssertEquals(156m, opp1SalesHeader.UnsuccessfulTEUQuantity);

			var opp2SalesHeader = new SalesHeader(org, opp2, product);
			// tradeDetail2a: PA_Status != Active, so ignored. = 0
			// +
			// tradeDetail2b: 1 (repeatsMth)  * 5 (TEU quantity) * 12 (monthly) = 60
			AssertEquals(60m, opp2SalesHeader.UnsuccessfulTEUQuantity);
		}

		public void TestCompanyFilter()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var opp1 = org.SalesOpportunities.AddNew();
			var opp2 = org.SalesOpportunities.AddNew();
			var product = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, "SHP");

			var sales1 = org.SalesCollection.AddNew();
			sales1.OW_MP_Product = product.PK;
			sales1.OW_IsTraded = false;
			sales1.OW_OH_Primary = org.PK;

			var tradeDetail1 = sales1.TradeDetails.AddNew();

			var sales2 = org.SalesCollection.AddNew();
			sales2.OW_MP_Product = product.PK;
			sales2.OW_IsTraded = false;
			sales2.OW_OH_Primary = org.PK;

			var tradeDetail2a = sales2.TradeDetails.AddNew();
			var tradeDetail2b = sales2.TradeDetails.AddNew();

			opp1.AssociatedTradeLanesPivots.AddPivotFor(tradeDetail1);
			opp2.AssociatedTradeLanesPivots.AddPivotFor(tradeDetail2a);
			opp2.AssociatedTradeLanesPivots.AddPivotFor(tradeDetail2b);

			Factory.Save();

			var anotherCompany = Factory.New<GlbCompany>();

			var orgSalesHeader = new SalesHeader(org, product);
			AssertEquals(Env.CurrentCompany.PK, orgSalesHeader.CompanyFilter);
			orgSalesHeader.CompanyFilter = anotherCompany.PK;
			AssertEquals(anotherCompany.PK, orgSalesHeader.CompanyFilter);

			var opp1SalesHeader = new SalesHeader(org, opp1, product);
			AssertEquals(ZGuid.Empty, opp1SalesHeader.CompanyFilter);
			opp1SalesHeader.CompanyFilter = anotherCompany.PK;
			AssertEquals(ZGuid.Empty, opp1SalesHeader.CompanyFilter);

			var opp2SalesHeader = new SalesHeader(org, opp2, product);
			AssertEquals(ZGuid.Empty, opp2SalesHeader.CompanyFilter);
			opp2SalesHeader.CompanyFilter = anotherCompany.PK;
			AssertEquals(ZGuid.Empty, opp2SalesHeader.CompanyFilter);
		}

		#endregion

		#region Delete

		public void TestDelete()
		{
			var productAAA = Factory.NewWithValidTestData<OrgSalesProduct>();
			var productBBB = Factory.NewWithValidTestData<OrgSalesProduct>();

			var org = Factory.New<OrgHeader>();
			var salesHeaderCollection = (SalesHeaderCollection)org.ActualAndProspectiveSalesHeaderCollection;

			var salesHeaderAAA = salesHeaderCollection.AddNew(productAAA);
			var salesAAA1 = salesHeaderAAA.EntitySalesCollectionProductView.AddNew();
			var salesAAA2 = salesHeaderAAA.EntitySalesCollectionProductView.AddNew();

			var salesHeaderBBB = salesHeaderCollection.AddNew(productBBB);
			var salesBBB1 = salesHeaderBBB.EntitySalesCollectionProductView.AddNew();
			var salesBBB2 = salesHeaderBBB.EntitySalesCollectionProductView.AddNew();

			salesHeaderAAA.Delete();

			AssertEquals(true, salesAAA1.IsDeleted);
			AssertEquals(true, salesAAA2.IsDeleted);
			AssertEquals(false, salesBBB1.IsDeleted);
			AssertEquals(false, salesBBB2.IsDeleted);
		}

		public void TestDelete_MultipleRelatedOpportunities()
		{
			var forwardingProduct = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var salesHeaderCollection = (SalesHeaderCollection)org.ActualAndProspectiveSalesHeaderCollection;
			var salesHeader = salesHeaderCollection.AddNew(forwardingProduct);
			var tradeLane1 = salesHeader.EntitySalesCollectionProductView.AddNew();
			var tradeLane2 = salesHeader.EntitySalesCollectionProductView.AddNew();

			var opportunity1 = org.SalesOpportunities.AddNew();
			var pivot11 = opportunity1.AssociatedTradeLanesPivots.AddPivotFor(tradeLane1);
			var pivot12 = opportunity1.AssociatedTradeLanesPivots.AddPivotFor(tradeLane2);

			var opportunity2 = org.SalesOpportunities.AddNew();
			var pivot21 = opportunity2.AssociatedTradeLanesPivots.AddPivotFor(tradeLane1);

			Factory.Save();

			var salesHeader1 = new SalesHeader(org, opportunity1, forwardingProduct);

			var salesHeader2 = new SalesHeader(org, opportunity2, forwardingProduct);

			salesHeader1.Delete();
			AssertEquals(false, tradeLane1.IsDeleted);
			AssertEquals(true, tradeLane2.IsDeleted);

			salesHeader1.Delete();
			AssertEquals(true, tradeLane2.IsDeleted);
		}

		#endregion

		#region

		public void TestDeleteWhenSalesNonEditableShouldDeleteTradeDetails()
		{
			var productAAA = Factory.NewWithValidTestData<OrgSalesProduct>();

			var org = Factory.New<OrgHeader>();
			var salesHeaderCollection = (SalesHeaderCollection)org.ActualAndProspectiveSalesHeaderCollection;

			var salesHeaderAAA = salesHeaderCollection.AddNew(productAAA);
			var salesAAA1 = Factory.New<EntitySalesWrapperForTest>();
			salesHeaderAAA.EntitySalesCollectionProductView.Add(salesAAA1);
			var salesAAA2 = salesHeaderAAA.EntitySalesCollectionProductView.AddNew();
			var detailAAA1 = salesAAA1.EntityTradeDetailsCollection.AddNew();
			var detailAAA2 = salesAAA2.EntityTradeDetailsCollection.AddNew();

			AssertEquals(false, salesAAA1.IsEditable);
			salesHeaderAAA.Delete();

			AssertEquals(false, salesAAA1.IsDeleted);
			AssertEquals(true, salesAAA2.IsDeleted);
			AssertEquals(true, detailAAA1.IsDeleted);
			AssertEquals(true, detailAAA2.IsDeleted);
		}

		public void TestDeleteWhenSalesNonEditableShouldDeleteGroupingAndDetails()
		{
			var productAAA = Factory.NewWithValidTestData<OrgSalesProduct>();

			var org = Factory.New<OrgHeader>();
			var salesHeaderCollection = (SalesHeaderCollection)org.ActualAndProspectiveSalesHeaderCollection;

			var salesHeaderAAA = salesHeaderCollection.AddNew(productAAA);
			var salesAAA1 = Factory.New<EntitySalesWrapperForTest>();
			salesHeaderAAA.EntitySalesCollectionProductView.Add(salesAAA1);
			var salesAAA2 = salesHeaderAAA.EntitySalesCollectionProductView.AddNew();
			var detailAAA1 = salesAAA1.EntityTradeDetailsCollection.AddNew();
			var detailAAA2 = salesAAA2.EntityTradeDetailsCollection.AddNew();
			var groupingAAA1 = salesAAA1.EntityDetailGroupings.AddNew();

			AssertEquals(false, salesAAA1.IsEditable);
			salesHeaderAAA.Delete();

			AssertEquals(false, salesAAA1.IsDeleted);
			AssertEquals(true, salesAAA2.IsDeleted);
			AssertEquals(true, groupingAAA1.IsDeleted);
			AssertEquals(true, detailAAA1.IsDeleted);
			AssertEquals(true, detailAAA2.IsDeleted);
		}

		public class EntitySalesWrapperForTest : EntitySalesWrapper
		{
			public EntitySalesWrapperForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public override bool IsEditable => false;
		}

		#endregion

		#region Fetch Strategy

		public void TestFetchStrategy()
		{
			var salesHeader = GetNewBusinessObject();
			AssertType(typeof(SalesHeaderFetchStrategy), salesHeader.FetchStrategy);
		}

		#endregion

		#region Related Business Objects

		public void TestProspectiveSalesCollection_ForOrgHeaderCollection_AddNew_ShouldAddOrgAssociation()
		{
			var product = Factory.New<OrgSalesProduct>();
			var org = Factory.New<OrgHeader>();
			var orgHeaderCollection = new SalesHeaderCollection(org);
			var salesHeader = orgHeaderCollection.AddNew(product);
			var entitySales = salesHeader.EntitySalesCollectionProductView.AddNew();
			AssertCollectionContains(org, entitySales.SalesAssociationPivotCollectionGlobal.Select(x => x.AssociatedEntity));
		}

		public void TestProspectiveSalesCollection_ForOrgHeaderCollection_ChangingProperty_ShouldAddOrgAssociation()
		{
			var product = Factory.NewWithValidTestData<OrgSalesProduct>();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var salesHeaderCollection = (SalesHeaderCollection)org.ActualAndProspectiveSalesHeaderCollection;
			var salesHeader = salesHeaderCollection.AddNew(product);
			var sales = salesHeader.EntitySalesCollectionProductView.AddNew();
			sales.SalesAssociationPivotCollectionGlobal.DeleteAll();
			Factory.Save();

			sales.OW_IsCustomRevenue = true;
			AssertContainsExactElementsInAnyOrder("Should add association on any change", new[] { org }, sales.SalesAssociationPivotCollectionGlobal.Select(x => x.AssociatedEntity));

			Factory.Save();

			sales.OW_MonthlyRevenue = 100;
			AssertContainsExactElementsInAnyOrder("Should not re-add association again", new[] { org }, sales.SalesAssociationPivotCollectionGlobal.Select(x => x.AssociatedEntity));

			Factory.Save();

			sales.SalesAssociationPivotCollectionGlobal.DeleteAll();
			var opp = Factory.NewWithValidTestData<OrgOpportunity>();
			sales.SalesAssociationPivotCollectionGlobal.AddNew(opp);

			Factory.Save();

			sales.OW_AnnualRevenue = 700;
			AssertContainsExactElementsInAnyOrder("Should not add association if already has another association", new[] { opp }, sales.SalesAssociationPivotCollectionGlobal.Select(x => x.AssociatedEntity));
		}

		public void TestProspectiveSalesCollection_ForOrgHeaderCollection_ChangingChildObject_ShouldAddOrgAssociation()
		{
			var product = Factory.NewWithValidTestData<OrgSalesProduct>();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var salesHeaderCollection = (SalesHeaderCollection)org.ActualAndProspectiveSalesHeaderCollection;
			var salesHeader = salesHeaderCollection.AddNew(product);
			var entitySales = salesHeader.FilterableEntitySalesCollection.AddNew();
			var entityTradeDetail = entitySales.EntityTradeDetailsCollection.AddNew();
			entityTradeDetail.FillWithValidTestData();
			entitySales.OW_MP_Product = product.PK;
			entitySales.SalesAssociationPivotCollectionGlobal.DeleteAll();
			Factory.Save();

			entityTradeDetail.CurrencyCode = "XXX";
			AssertContainsExactElementsInAnyOrder("Should add association on any change of child object", new[] { org }, entitySales.SalesAssociationPivotCollectionGlobal.Select(x => x.AssociatedEntity));
		}

		#endregion

		#region OnFactorySaving

		public void TestUpdateRelatedBizObjsEstimatedValue()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var forwardingProduct = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, "SHP");
			var salesHeaderCollection = (SalesHeaderCollection)org.ActualAndProspectiveSalesHeaderCollection;
			var orgSalesHeader = salesHeaderCollection.AddNew(forwardingProduct);

			var sales1 = orgSalesHeader.EntitySalesCollectionProductView.AddNew();
			var detail1 = sales1.TradeDetails.AddNew();
			detail1.ProspectDetail.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.Yearly;
			detail1.CurrentProspectPeriod.PAS_EstimatedProfit = 0m;

			var sales2 = orgSalesHeader.EntitySalesCollectionProductView.AddNew();
			var detail2 = sales2.TradeDetails.AddNew();
			detail2.ProspectDetail.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.Yearly;
			detail2.CurrentProspectPeriod.PAS_EstimatedProfit = 0m;

			var opp1 = org.SalesOpportunities.AddNew();
			var opp2 = org.SalesOpportunities.AddNew();

			var pivot11 = opp1.AssociatedTradeLanesPivots.AddPivotFor(sales1);
			var pivot12 = opp1.AssociatedTradeLanesPivots.AddPivotFor(sales2);
			opp1.AssociatedTradeLanesPivots.AddPivotFor(detail1);
			opp1.AssociatedTradeLanesPivots.AddPivotFor(detail2);

			var pivot22 = opp2.AssociatedTradeLanesPivots.AddPivotFor(sales2);
			opp2.AssociatedTradeLanesPivots.AddPivotFor(detail2);

			Factory.Save();

			AssertEquals(0m, opp1.P8_EstimatedValue);
			AssertEquals(0m, opp2.P8_EstimatedValue);

			opp1.ProspectiveSalesHeaderCollection.Refresh();
			var salesHeader1 = opp1.ProspectiveSalesHeaderCollection.Cast<SalesHeader>().Single();
			salesHeader1.EntitySalesCollectionProductView[0].TradeDetails[0].CurrentProspectPeriod.PAS_EstimatedProfit = 100m;
			salesHeader1.EntitySalesCollectionProductView[0].TradeDetails[0].CurrentProspectPeriod.PAS_RX_NKCurrency = "AUD";
			salesHeader1.EntitySalesCollectionProductView[1].TradeDetails[0].CurrentProspectPeriod.PAS_EstimatedProfit = 100m;
			salesHeader1.EntitySalesCollectionProductView[1].TradeDetails[0].CurrentProspectPeriod.PAS_RX_NKCurrency = "AUD";

			Factory.Save();
			AssertEquals(200m, opp1.P8_EstimatedValue);
			AssertEquals(0m, opp2.P8_EstimatedValue);

			opp2.ProspectiveSalesHeaderCollection.Refresh();
			var salesHeader2 = opp2.ProspectiveSalesHeaderCollection.Cast<SalesHeader>().Single();
			salesHeader2.EntitySalesCollectionProductView[0].TradeDetails[0].CurrentProspectPeriod.PAS_RX_NKCurrency = "AUD";
			salesHeader2.EntitySalesCollectionProductView[0].TradeDetails[0].CurrentProspectPeriod.PAS_EstimatedProfit = 150m;

			Factory.Save();
			AssertEquals(250m, opp1.P8_EstimatedValue);
			AssertEquals(150m, opp2.P8_EstimatedValue);

			orgSalesHeader.EntitySalesCollectionProductView[0].TradeDetails[0].CurrentProspectPeriod.PAS_EstimatedProfit = 50m;
			orgSalesHeader.EntitySalesCollectionProductView[0].TradeDetails[0].CurrentProspectPeriod.PAS_RX_NKCurrency = "AUD";
			orgSalesHeader.EntitySalesCollectionProductView[1].TradeDetails[0].CurrentProspectPeriod.PAS_EstimatedProfit = 50m;
			orgSalesHeader.EntitySalesCollectionProductView[1].TradeDetails[0].CurrentProspectPeriod.PAS_RX_NKCurrency = "AUD";

			Factory.Save();
			AssertEquals(100m, opp1.P8_EstimatedValue);
			AssertEquals(50m, opp2.P8_EstimatedValue);

			orgSalesHeader.EntitySalesCollectionProductView.RemoveAndDelete(sales2);
			Factory.Save();
			AssertEquals(50m, opp1.P8_EstimatedValue);
			AssertEquals(0m, opp2.P8_EstimatedValue);

			orgSalesHeader.EntitySalesCollectionProductView.AddNew();
			opp1.Delete();
			Factory.Save();
			AssertEquals(0m, opp2.P8_EstimatedValue);
		}

		public void TestUpdateRelatedBizObjsEstimatedValue_FetchHints()
		{
			var anotherFactory = new BusinessObjectFactory();
			var orgInOtherFactory = anotherFactory.NewWithValidTestData<OrgHeader>();
			var productInOtherFactory = anotherFactory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, "SHP");

			var sales1InOtherFactory = orgInOtherFactory.SalesCollection.AddNew();
			sales1InOtherFactory.OW_MP_Product = productInOtherFactory.PK;
			var detail1InOtherFactory = sales1InOtherFactory.TradeDetails.AddNew();
			detail1InOtherFactory.ProspectDetail.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.Yearly;
			detail1InOtherFactory.CurrentProspectPeriod.PAS_EstimatedProfit = 0m;

			anotherFactory.Save();

			var sql = @"UPDATE dbo.OrgSales
SET OW_SystemCreateTimeUtc = DATEADD(hour, -1, getutcdate()),
   OW_SystemCreateUser = 'E',
   OW_SystemLastEditTimeUtc = DATEADD(hour, -1, getutcdate()),
   OW_SystemLastEditUser = 'E'
WHERE OW_PK = @PK";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@PK", SqlDbType.UniqueIdentifier, sales1InOtherFactory.PK.ToGuid());
				command.ExecuteNonQuery();
			}

			var sales2InOtherFactory = orgInOtherFactory.SalesCollection.AddNew();
			sales2InOtherFactory.OW_MP_Product = productInOtherFactory.PK;
			var detail2InOtherFactory = sales2InOtherFactory.TradeDetails.AddNew();
			detail2InOtherFactory.ProspectDetail.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.Yearly;
			detail2InOtherFactory.CurrentProspectPeriod.PAS_EstimatedProfit = 0m;

			var opp1InOtherFactory = orgInOtherFactory.SalesOpportunities.AddNew();
			opp1InOtherFactory.AssociatedTradeLanesPivots.AddPivotFor(sales1InOtherFactory);
			opp1InOtherFactory.AssociatedTradeLanesPivots.AddPivotFor(sales2InOtherFactory);
			opp1InOtherFactory.AssociatedTradeLanesPivots.AddPivotFor(detail1InOtherFactory);
			opp1InOtherFactory.AssociatedTradeLanesPivots.AddPivotFor(detail2InOtherFactory);

			var opp2InOtherFactory = orgInOtherFactory.SalesOpportunities.AddNew();
			opp2InOtherFactory.AssociatedTradeLanesPivots.AddPivotFor(sales2InOtherFactory);
			opp2InOtherFactory.AssociatedTradeLanesPivots.AddPivotFor(detail2InOtherFactory);

			anotherFactory.Save();

			var expectedHits = new Dictionary<string, int>
			{
				{ OrgSalesSchema.Constants.TableName, 2 },
				{ OrgOpportunitySchema.Constants.TableName, 1 },
				{ OrgOpportunityValueSchema.Constants.TableName, 1 },
				{ ProcessTasksSchema.Constants.TableName, 2 },
				{ ProcessTaskTemplateSchema.Constants.TableName, 1 },
				{ OrgTradeDetailSchema.Constants.TableName, 2 },
				{ GlbCompanySchema.Constants.TableName, 1 },
				{ OrgTradeProspectSchema.Constants.TableName, 1 }
			};

			using (AssertDbHitsWithUsefulQueryInformation(expectedHits, Factory))
			{
				var org = Factory.Load<OrgHeader>(orgInOtherFactory.PK);
				var product = Factory.Load<OrgSalesProduct>(productInOtherFactory.PK);

				var salesHeader1 = new SalesHeaderForTest(org, product);
				var view = salesHeader1.EntitySalesCollectionProductView.Cast<EntitySalesWrapper>();
				var salesWrapper1 = view.Single(salesWrapper => salesWrapper.PK == sales1InOtherFactory.PK);
				var salesWrapper2 = view.Single(salesWrapper => salesWrapper.PK == sales2InOtherFactory.PK);
				salesWrapper1.TradeDetails[0].CurrentProspectPeriod.PAS_EstimatedProfit = 100m;
				salesWrapper1.TradeDetails[0].CurrentProspectPeriod.PAS_RX_NKCurrency = "AUD";
				salesWrapper2.TradeDetails[0].CurrentProspectPeriod.PAS_EstimatedProfit = 500m;
				salesWrapper2.TradeDetails[0].CurrentProspectPeriod.PAS_RX_NKCurrency = "AUD";

				Factory.ResetDatabaseLoadCount();
				Factory.Save();
			}

			var opp1 = Factory.Load<OrgOpportunity>(opp1InOtherFactory.PK);
			var opp2 = Factory.Load<OrgOpportunity>(opp2InOtherFactory.PK);
			AssertEquals(600m, opp1.P8_EstimatedValue);
			AssertEquals(500m, opp2.P8_EstimatedValue);
		}

		#endregion

		#region Overrides

		protected override BusinessObject GetNewBusinessObject()
		{
			var salesProduct = Factory.New<OrgSalesProduct>();
			var org = Factory.New<OrgHeader>();
			return new SalesHeader(org, salesProduct);
		}

		#endregion
	}
}
