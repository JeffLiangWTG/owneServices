using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	sealed class ProspectValueCalculatorTest : TestCaseWithFactory
	{
		[TestDate(2018, 2, 9)]
		public void TestCalculateProspectFinancialYearValue()
		{
			PrepareCommittedTestData();

			var forwardingProduct = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, "SHP");
			var transportProduct = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, "TRN");

			var org = Factory.Load<OrgHeader>(orgPk);
			var revenueCalculator = new OrgSalesRevenueCalculator(Factory, org);
			var valueCalculator = new SalesValueCalculator(Factory, org.PK, revenueCalculator);

			CombineAssertions(() =>
			{
				var committedValue = valueCalculator.GetProductValue(SalesValueCalculator.ValueType.Committed, SalesValueCalculator.PeriodRange.CurrentFinancialYear, companyPk: ZGuid.Empty, salesProduct: forwardingProduct).TotalRevenue;
				AssertEquals("Forwarding current financial year", 2600m, committedValue);

				committedValue = valueCalculator.GetProductValue(SalesValueCalculator.ValueType.Committed, SalesValueCalculator.PeriodRange.CurrentFinancialYear, companyPk: ZGuid.Empty, salesProduct: forwardingProduct).TEUQuantity;
				AssertEquals("Forwarding TEU current financial year", 26m, committedValue);

				committedValue = valueCalculator.GetProductValue(SalesValueCalculator.ValueType.Committed, SalesValueCalculator.PeriodRange.CurrentFinancialYearToDate, companyPk: ZGuid.Empty, salesProduct: forwardingProduct).TotalRevenue;
				AssertEquals("Forwarding current financial year to date", 2600m, committedValue);

				committedValue = valueCalculator.GetProductValue(SalesValueCalculator.ValueType.Committed, SalesValueCalculator.PeriodRange.CurrentFinancialYearToDate, companyPk: ZGuid.Empty, salesProduct: forwardingProduct).TEUQuantity;
				AssertEquals("Forwarding TEU current financial year to date", 26m, committedValue);

				committedValue = valueCalculator.GetProductValue(SalesValueCalculator.ValueType.Committed, SalesValueCalculator.PeriodRange.CurrentFinancialYear, companyPk: ZGuid.Empty, salesProduct: transportProduct).TotalRevenue;
				AssertEquals("Transport current financial year", 450m, committedValue);

				committedValue = valueCalculator.GetProductValue(SalesValueCalculator.ValueType.Committed, SalesValueCalculator.PeriodRange.CurrentFinancialYear, companyPk: ZGuid.Empty, salesProduct: transportProduct).TEUQuantity;
				AssertEquals("Transport TEU current financial year", 18m, committedValue);

				committedValue = valueCalculator.GetProductValue(SalesValueCalculator.ValueType.Committed, SalesValueCalculator.PeriodRange.CurrentFinancialYearToDate, companyPk: ZGuid.Empty, salesProduct: transportProduct).TotalRevenue;
				AssertEquals("Transport current financial year to date", 450m, committedValue);

				committedValue = valueCalculator.GetProductValue(SalesValueCalculator.ValueType.Committed, SalesValueCalculator.PeriodRange.CurrentFinancialYearToDate, companyPk: ZGuid.Empty, salesProduct: transportProduct).TEUQuantity;
				AssertEquals("Transport TEU current financial year to date", 18m, committedValue);

				committedValue = valueCalculator.GetProductValue(SalesValueCalculator.ValueType.Committed, SalesValueCalculator.PeriodRange.CurrentFinancialYear, companyPk: ZGuid.Empty, salesProduct: null).TotalRevenue;
				AssertEquals("All products current financial year", 3050m, committedValue);

				committedValue = valueCalculator.GetProductValue(SalesValueCalculator.ValueType.Committed, SalesValueCalculator.PeriodRange.CurrentFinancialYear, companyPk: ZGuid.Empty, salesProduct: null).TEUQuantity;
				AssertEquals("All products TEU current financial year", 44m, committedValue);

				committedValue = valueCalculator.GetProductValue(SalesValueCalculator.ValueType.Committed, SalesValueCalculator.PeriodRange.CurrentFinancialYearToDate, companyPk: ZGuid.Empty, salesProduct: null).TotalRevenue;
				AssertEquals("All products current financial year to date", 3050m, committedValue);

				committedValue = valueCalculator.GetProductValue(SalesValueCalculator.ValueType.Committed, SalesValueCalculator.PeriodRange.CurrentFinancialYearToDate, companyPk: ZGuid.Empty, salesProduct: null).TEUQuantity;
				AssertEquals("All products TEU current financial year to date", 44m, committedValue);

				var tradeLane1 = Factory.Load<OrgSales>(sales1Pk);
				committedValue = valueCalculator.GetTradeLaneValue(SalesValueCalculator.ValueType.Committed, SalesValueCalculator.PeriodRange.CurrentFinancialYear, companyPk: ZGuid.Empty, tradeLane: tradeLane1).TotalRevenue;
				AssertEquals("Trade lane 1 current financial year", 2600m, committedValue);

				committedValue = valueCalculator.GetTradeLaneValue(SalesValueCalculator.ValueType.Committed, SalesValueCalculator.PeriodRange.CurrentFinancialYear, companyPk: ZGuid.Empty, tradeLane: tradeLane1).TEUQuantity;
				AssertEquals("Trade lane 1 TEU current financial year", 26m, committedValue);

				committedValue = valueCalculator.GetProductValue(SalesValueCalculator.ValueType.Committed, SalesValueCalculator.PeriodRange.NextFinancialYear, companyPk: ZGuid.Empty, salesProduct: forwardingProduct).TotalRevenue;
				AssertEquals("Forwarding next financial year", 2600m, committedValue);

				committedValue = valueCalculator.GetProductValue(SalesValueCalculator.ValueType.Committed, SalesValueCalculator.PeriodRange.NextFinancialYear, companyPk: ZGuid.Empty, salesProduct: forwardingProduct).TEUQuantity;
				AssertEquals("Forwarding next TEU financial year", 26m, committedValue);

				committedValue = valueCalculator.GetProductValue(SalesValueCalculator.ValueType.Committed, SalesValueCalculator.PeriodRange.NextFinancialYear, companyPk: ZGuid.Empty, salesProduct: transportProduct).TotalRevenue;
				AssertEquals("Transport next financial year", 450m, committedValue);

				committedValue = valueCalculator.GetProductValue(SalesValueCalculator.ValueType.Committed, SalesValueCalculator.PeriodRange.NextFinancialYear, companyPk: ZGuid.Empty, salesProduct: transportProduct).TEUQuantity;
				AssertEquals("Transport TEU next financial year", 18m, committedValue);
			});
		}

		[TestDate(2018, 2, 9)]
		public void TestCalculateCommittedTrailing12MonthsValue()
		{
			PrepareCommittedTestData();

			var forwardingProduct = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, "SHP");
			var transportProduct = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, "TRN");

			var org = Factory.Load<OrgHeader>(orgPk);
			var revenueCalculator = new OrgSalesRevenueCalculator(Factory, org);
			var valueCalculator = new SalesValueCalculator(Factory, org.PK, revenueCalculator);

			var committedValue = valueCalculator.GetProductValue(SalesValueCalculator.ValueType.Committed, SalesValueCalculator.PeriodRange.Trailing12Months, companyPk: ZGuid.Empty, salesProduct: forwardingProduct).TotalRevenue;
			AssertEquals("Forwarding trailing 12 months", 1600m, committedValue);

			committedValue = valueCalculator.GetProductValue(SalesValueCalculator.ValueType.Committed, SalesValueCalculator.PeriodRange.Trailing12Months, companyPk: ZGuid.Empty, salesProduct: forwardingProduct).TEUQuantity;
			AssertEquals("Forwarding TEU trailing 12 months", 16m, committedValue);

			committedValue = valueCalculator.GetProductValue(SalesValueCalculator.ValueType.Committed, SalesValueCalculator.PeriodRange.Trailing12Months, companyPk: ZGuid.Empty, salesProduct: transportProduct).TotalRevenue;
			AssertEquals("Transport trailing 12 months", 50m, committedValue);

			committedValue = valueCalculator.GetProductValue(SalesValueCalculator.ValueType.Committed, SalesValueCalculator.PeriodRange.Trailing12Months, companyPk: ZGuid.Empty, salesProduct: transportProduct).TEUQuantity;
			AssertEquals("Transport TEU trailing 12 months", 2m, committedValue);

			committedValue = valueCalculator.GetProductValue(SalesValueCalculator.ValueType.Committed, SalesValueCalculator.PeriodRange.Trailing12Months, companyPk: ZGuid.Empty, salesProduct: null).TotalRevenue;
			AssertEquals("All products trailing 12 months", 1650m, committedValue);

			committedValue = valueCalculator.GetProductValue(SalesValueCalculator.ValueType.Committed, SalesValueCalculator.PeriodRange.Trailing12Months, companyPk: ZGuid.Empty, salesProduct: null).TEUQuantity;
			AssertEquals("All products TEU trailing 12 months", 18m, committedValue);
		}

		[TestDate(2018, 2, 9)]
		public void TestCalculatePipelineAndUnsuccessfulValue()
		{
			PreparePipelineUnsuccessfulTestData();

			var forwardingProduct = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, "SHP");

			var org = Factory.Load<OrgHeader>(orgPk);
			var revenueCalculator = new OrgSalesRevenueCalculator(Factory, org);
			var valueCalculator = new SalesValueCalculator(Factory, org.PK, revenueCalculator);

			var committedValue = valueCalculator.GetProductValue(SalesValueCalculator.ValueType.Pipeline, SalesValueCalculator.PeriodRange.CurrentFinancialYear, companyPk: ZGuid.Empty, salesProduct: forwardingProduct).TotalRevenue;
			AssertEquals("Forwarding pipeline current financial year", 1200m, committedValue);

			committedValue = valueCalculator.GetProductValue(SalesValueCalculator.ValueType.Pipeline, SalesValueCalculator.PeriodRange.CurrentFinancialYear, companyPk: ZGuid.Empty, salesProduct: forwardingProduct).TEUQuantity;
			AssertEquals("Forwarding pipeline TEU current financial year", 24m, committedValue);

			committedValue = valueCalculator.GetProductValue(SalesValueCalculator.ValueType.Unsuccessful, SalesValueCalculator.PeriodRange.CurrentFinancialYear, companyPk: ZGuid.Empty, salesProduct: forwardingProduct).TotalRevenue;
			AssertEquals("Forwarding unsuccessful current financial year", 2400m, committedValue);

			committedValue = valueCalculator.GetProductValue(SalesValueCalculator.ValueType.Unsuccessful, SalesValueCalculator.PeriodRange.CurrentFinancialYear, companyPk: ZGuid.Empty, salesProduct: forwardingProduct).TEUQuantity;
			AssertEquals("Forwarding unsuccessful TEU current financial year", 36m, committedValue);
		}

		readonly Guid orgPk = Guid.NewGuid();
		readonly Guid sales1Pk = Guid.NewGuid();
		readonly Guid sales2Pk = Guid.NewGuid();

		void PrepareCommittedTestData()
		{
			var org = Factory.NewWithPrimaryKey<OrgHeader>(orgPk);
			org.FillWithValidTestData();

			var forwardingProduct = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, "SHP");
			var transportProduct = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, "TRN");

			var sales1 = Factory.NewWithPrimaryKey<OrgSales>(sales1Pk);
			sales1.OW_MP_Product = forwardingProduct.PK;
			sales1.OW_IsTraded = false;
			sales1.OW_OH_Primary = org.PK;

			var detail1a = sales1.TradeDetails.AddNew();
			detail1a.PA_Status = OpportunityTradeStatus.Codes.Successful;
			detail1a.ProspectDetail.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.Monthly;
			detail1a.CurrentProspectPeriod.PAS_TEUQuantity = 2m;
			detail1a.CurrentProspectPeriod.PAS_RX_NKCurrency = "AUD";
			detail1a.CurrentProspectPeriod.PAS_EstimatedProfit = 200m;
			detail1a.ProspectDetail.PAP_ExpectedTradeStartDate = new ZDate(2017, 6, 28);
			detail1a.ProspectPeriodStart = new ZDate(2017, 6, 1);
			detail1a.ProspectPeriodEnd = new ZDate(2018, 5, 1);

			var detail1b = sales1.TradeDetails.AddNew();
			detail1b.PA_Status = OpportunityTradeStatus.Codes.Active;
			detail1b.ProspectDetail.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.Monthly;
			detail1b.CurrentProspectPeriod.PAS_TEUQuantity = 2m;
			detail1b.CurrentProspectPeriod.PAS_RX_NKCurrency = "AUD";
			detail1b.CurrentProspectPeriod.PAS_EstimatedProfit = 100m;

			var sales2 = Factory.NewWithPrimaryKey<OrgSales>(sales2Pk);
			sales2.OW_MP_Product = transportProduct.PK;
			sales2.OW_IsTraded = false;
			sales2.OW_OH_Primary = org.PK;

			var detail2 = sales2.TradeDetails.AddNew();
			detail2.PA_Status = OpportunityTradeStatus.Codes.Successful;
			detail2.ProspectDetail.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.Monthly;
			detail2.CurrentProspectPeriod.PAS_TEUQuantity = 2m;
			detail2.CurrentProspectPeriod.PAS_RX_NKCurrency = "USD";
			detail2.CurrentProspectPeriod.PAS_EstimatedProfit = 100m;
			detail2.ProspectDetail.PAP_ExpectedTradeStartDate = new ZDate(2018, 1, 3);
			detail2.ProspectPeriodStart = new ZDate(2018, 1, 1);
			detail2.ProspectPeriodEnd = new ZDate(2018, 8, 1);

			var usdExRate = Factory.New<RefExchangeRate>();
			usdExRate.RE_RX_NKExCurrency = "USD";
			usdExRate.RE_StartDate = new ZDateTime(2017, 1, 1);
			usdExRate.RE_ExpiryDate = new ZDateTime(2018, 2, 1);
			usdExRate.RE_SellRate = 2;
			usdExRate.RE_ExRateType = Enterprise.Core.Constants.ExchangeRateTypes.Code.SellRate;

			Factory.Save();
		}

		void PreparePipelineUnsuccessfulTestData()
		{
			var org = Factory.NewWithPrimaryKey<OrgHeader>(orgPk);
			org.FillWithValidTestData();

			var forwardingProduct = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, "SHP");

			var sales1 = Factory.NewWithPrimaryKey<OrgSales>(sales1Pk);
			sales1.OW_MP_Product = forwardingProduct.PK;
			sales1.OW_IsTraded = false;
			sales1.OW_OH_Primary = org.PK;

			var detail1 = sales1.TradeDetails.AddNew();
			detail1.PA_Status = OpportunityTradeStatus.Codes.Successful;
			detail1.ProspectDetail.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.Monthly;
			detail1.CurrentProspectPeriod.PAS_TEUQuantity = 2m;
			detail1.CurrentProspectPeriod.PAS_RX_NKCurrency = "AUD";
			detail1.CurrentProspectPeriod.PAS_EstimatedProfit = 200m;
			detail1.ProspectDetail.PAP_ExpectedTradeStartDate = new ZDate(2017, 6, 28);
			detail1.ProspectPeriodStart = new ZDate(2017, 6, 1);
			detail1.ProspectPeriodEnd = new ZDate(2018, 5, 1);

			var detail2 = sales1.TradeDetails.AddNew();
			detail2.PA_Status = OpportunityTradeStatus.Codes.Active;
			detail2.ProspectDetail.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.Monthly;
			detail2.CurrentProspectPeriod.PAS_TEUQuantity = 2m;
			detail2.CurrentProspectPeriod.PAS_RX_NKCurrency = "AUD";
			detail2.CurrentProspectPeriod.PAS_EstimatedProfit = 100m;

			var detail3 = sales1.TradeDetails.AddNew();
			detail3.PA_Status = OpportunityTradeStatus.Codes.Unsuccessful;
			detail3.ProspectDetail.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.Monthly;
			detail3.CurrentProspectPeriod.PAS_TEUQuantity = 3m;
			detail3.CurrentProspectPeriod.PAS_RX_NKCurrency = "AUD";
			detail3.CurrentProspectPeriod.PAS_EstimatedProfit = 200m;

			Factory.Save();
		}
	}
}
