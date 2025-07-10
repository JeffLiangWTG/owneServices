using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace Enterprise.MarketingManager.GUI.Testing
{
	public class SalesAnalysisChartViewModelTest : TestCaseWithFactory
	{
		[TestDate(2015, 7, 3)]
		public void TestPopulateChartData()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var opp = org.SalesOpportunities.AddNew();

			var forwardingProduct = Factory.LoadTop1<OrgSalesProduct>(new ZQuery(OrgSalesProductSchema.MP_Code, "SHP"));
			var transportProduct = Factory.LoadTop1<OrgSalesProduct>(new ZQuery(OrgSalesProductSchema.MP_Code, "TRN"));

			var prospectShp1 = org.SalesCollection.AddNew();
			prospectShp1.OW_MP_Product = forwardingProduct.PK;
			prospectShp1.OW_IsCustomRevenue = false;

			var prospectShp1DetailA = prospectShp1.TradeDetails.AddNew();
			prospectShp1DetailA.PA_TradeMode = "AIR";
			prospectShp1DetailA.PA_Status = OpportunityTradeStatus.Codes.Successful;
			prospectShp1DetailA.CurrentProspectPeriod.PAS_RepeatsMnth = 1;
			prospectShp1DetailA.CurrentProspectPeriod.PAS_EstimatedProfit = 50;
			prospectShp1DetailA.CurrentProspectPeriod.PAS_RX_NKCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			prospectShp1DetailA.ProspectPeriodStart = new ZDate(2014, 8, 1);
			prospectShp1DetailA.ProspectPeriodEnd = new ZDate(2014, 12, 1);

			var prospectShp1DetailB = prospectShp1.TradeDetails.AddNew();
			prospectShp1DetailB.PA_TradeMode = "SEA";
			prospectShp1DetailB.PA_Status = OpportunityTradeStatus.Codes.Successful;
			prospectShp1DetailB.CurrentProspectPeriod.PAS_RepeatsMnth = 1;
			prospectShp1DetailB.CurrentProspectPeriod.PAS_EstimatedProfit = 500;
			prospectShp1DetailB.CurrentProspectPeriod.PAS_RX_NKCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			prospectShp1DetailB.ProspectPeriodStart = new ZDate(2014, 10, 1);
			prospectShp1DetailB.ProspectPeriodEnd = new ZDate(2015, 6, 1);

			var prospectShp2 = org.SalesCollection.AddNew();
			prospectShp2.OW_MP_Product = forwardingProduct.PK;
			var prospectShp2Detail = prospectShp2.TradeDetails.AddNew();
			prospectShp2Detail.PA_TradeMode = "SEA";
			prospectShp2Detail.PA_Status = OpportunityTradeStatus.Codes.Successful;
			prospectShp2Detail.CurrentProspectPeriod.PAS_RepeatsMnth = 1;
			prospectShp2Detail.CurrentProspectPeriod.PAS_EstimatedProfit = 60;
			prospectShp2Detail.CurrentProspectPeriod.PAS_RX_NKCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			prospectShp2Detail.ProspectDetail.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.OneOff;
			prospectShp2Detail.ProspectPeriodStart = new ZDate(2014, 10, 1);
			prospectShp2Detail.ProspectPeriodEnd = new ZDate(2014, 10, 1);

			var prospectTrn1 = org.SalesCollection.AddNew();
			prospectTrn1.OW_MP_Product = transportProduct.PK;
			var prospectTrn1Detail = prospectTrn1.TradeDetails.AddNew();
			prospectTrn1Detail.PA_Status = OpportunityTradeStatus.Codes.Successful;
			prospectTrn1Detail.CurrentProspectPeriod.PAS_RepeatsMnth = 1;
			prospectTrn1Detail.CurrentProspectPeriod.PAS_EstimatedProfit = 60;
			prospectTrn1Detail.CurrentProspectPeriod.PAS_RX_NKCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			prospectTrn1Detail.ProspectPeriodStart = new ZDate(2015, 3, 1);
			prospectTrn1Detail.ProspectPeriodEnd = new ZDate(2016, 2, 1);

			opp.AssociatedTradeLanesPivots.AddPivotFor(prospectShp1DetailA);
			opp.AssociatedTradeLanesPivots.AddPivotFor(prospectShp1DetailB);
			opp.AssociatedTradeLanesPivots.AddPivotFor(prospectShp2Detail);
			opp.AssociatedTradeLanesPivots.AddPivotFor(prospectTrn1Detail);

			var sales1 = org.SalesCollection.AddNew();
			sales1.OW_IsTraded = true;
			sales1.OW_MP_Product = forwardingProduct.PK;
			sales1.OW_OriginID = ViewLocationHelper.GetLocationFromString(Factory, "AUSYD", "RL").PK;
			sales1.OW_DestinationID = ViewLocationHelper.GetLocationFromString(Factory, "JPTYO", "RL").PK;
			sales1.OW_OH_Buyer = org.PK;

			var tradeDetail11 = sales1.TradeDetails.AddNew();
			var tradePeriod11 = tradeDetail11.TradedPeriods.AddNew();
			tradePeriod11.PAS_Period = new ZDate(2014, 8, 1);
			tradePeriod11.PAS_OH_Client = org.PK;
			var tradeValue11 = tradePeriod11.TradeValues.AddNew();
			tradeValue11.PAV_GC = GlbCompany.CurrentCompany.PK;
			tradeValue11.PAV_Revenue = 100m;
			tradeValue11.PAV_RX_NKCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			var tradeDetail12 = sales1.TradeDetails.AddNew();
			var tradePeriod12 = tradeDetail12.TradedPeriods.AddNew();
			tradePeriod12.PAS_Period = new ZDate(2014, 8, 1);
			tradePeriod12.PAS_OH_Client = org.PK;
			var tradeValue12 = tradePeriod12.TradeValues.AddNew();
			tradeValue12.PAV_GC = GlbCompany.CurrentCompany.PK;
			tradeValue12.PAV_Revenue = 200m;
			tradeValue12.PAV_RX_NKCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			var sales2 = org.SalesCollection.AddNew();
			sales2.OW_IsTraded = true;
			sales2.OW_MP_Product = transportProduct.PK;
			sales2.OW_OriginID = ViewLocationHelper.GetLocationFromString(Factory, "AUSYD", "RL").PK;
			sales2.OW_DestinationID = ViewLocationHelper.GetLocationFromString(Factory, "AUMEL", "RL").PK;
			sales2.OW_OH_Buyer = org.PK;

			var tradeDetail21 = sales2.TradeDetails.AddNew();
			var tradePeriod21 = tradeDetail21.TradedPeriods.AddNew();
			tradePeriod21.PAS_Period = new ZDate(2014, 8, 1);
			tradePeriod21.PAS_OH_Client = org.PK;
			var tradeValue21 = tradePeriod21.TradeValues.AddNew();
			tradeValue21.PAV_GC = GlbCompany.CurrentCompany.PK;
			tradeValue21.PAV_Revenue = 300m;
			tradeValue21.PAV_RX_NKCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			var sales3 = org.SalesCollection.AddNew();
			sales3.OW_IsTraded = true;
			sales3.OW_MP_Product = forwardingProduct.PK;
			sales3.OW_OriginID = ViewLocationHelper.GetLocationFromString(Factory, "AUSYD", "RL").PK;
			sales3.OW_DestinationID = ViewLocationHelper.GetLocationFromString(Factory, "JPTYO", "RL").PK;
			sales3.OW_OH_Buyer = org.PK;

			var tradeDetail31 = sales3.TradeDetails.AddNew();
			var tradePeriod31 = tradeDetail31.TradedPeriods.AddNew();
			tradePeriod31.PAS_Period = new ZDate(2014, 10, 1);
			tradePeriod31.PAS_OH_Client = org.PK;
			var tradeValue31 = tradePeriod31.TradeValues.AddNew();
			tradeValue31.PAV_GC = GlbCompany.CurrentCompany.PK;
			tradeValue31.PAV_Revenue = 400m;
			tradeValue31.PAV_RX_NKCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			var sales4 = org.SalesCollection.AddNew();
			sales4.OW_IsTraded = true;
			sales4.OW_MP_Product = forwardingProduct.PK;
			sales4.OW_OH_Buyer = org.PK;
			sales4.OW_OriginID = ViewLocationHelper.GetLocationFromString(Factory, "AUSYD", "RL").PK;
			sales4.OW_DestinationID = ViewLocationHelper.GetLocationFromString(Factory, "JPTYO", "RL").PK;
			var tradeDetail4 = sales4.TradeDetails.AddNew();
			var tradePeriod4 = tradeDetail4.TradedPeriods.AddNew();
			tradePeriod4.PAS_Period = new ZDate(2015, 2, 1);
			tradePeriod4.PAS_OH_Client = org.PK;

			var sales5 = org.SalesCollection.AddNew();
			sales5.OW_IsTraded = true;
			sales5.OW_MP_Product = forwardingProduct.PK;
			sales5.OW_OH_Buyer = org.PK;
			sales5.OW_OriginID = ViewLocationHelper.GetLocationFromString(Factory, "AUSYD", "RL").PK;
			sales5.OW_DestinationID = ViewLocationHelper.GetLocationFromString(Factory, "JPTYO", "RL").PK;
			var tradeDetail5 = sales5.TradeDetails.AddNew();
			var tradePeriod5 = tradeDetail5.TradedPeriods.AddNew();
			tradePeriod5.PAS_Period = new ZDate(2015, 5, 1);
			tradePeriod5.PAS_OH_Client = org.PK;

			var sales6 = org.SalesCollection.AddNew();
			sales6.OW_IsTraded = true;
			sales6.OW_MP_Product = transportProduct.PK;
			sales6.OW_OriginID = ViewLocationHelper.GetLocationFromString(Factory, "AUSYD", "RL").PK;
			sales6.OW_DestinationID = ViewLocationHelper.GetLocationFromString(Factory, "AUMEL", "RL").PK;
			sales6.OW_OH_Buyer = org.PK;

			var tradeDetail61 = sales6.TradeDetails.AddNew();
			var tradePeriod61 = tradeDetail61.TradedPeriods.AddNew();
			tradePeriod61.PAS_Period = new ZDate(2015, 5, 1);
			tradePeriod61.PAS_OH_Client = org.PK;
			var tradeValue61 = tradePeriod61.TradeValues.AddNew();
			tradeValue61.PAV_GC = GlbCompany.CurrentCompany.PK;
			tradeValue61.PAV_Revenue = 1000m;
			tradeValue61.PAV_RX_NKCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			var tradePeriod62 = tradeDetail61.TradedPeriods.AddNew();
			tradePeriod62.PAS_Period = new ZDate(2015, 6, 1);
			tradePeriod62.PAS_OH_Client = org.PK;
			var tradeValue62 = tradePeriod62.TradeValues.AddNew();
			tradeValue62.PAV_GC = GlbCompany.CurrentCompany.PK;
			tradeValue62.PAV_Revenue = 199m;
			tradeValue62.PAV_RX_NKCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			var tradePeriod63 = tradeDetail61.TradedPeriods.AddNew();
			tradePeriod63.PAS_Period = new ZDate(2015, 6, 1);
			tradePeriod63.PAS_OH_Client = org.PK;
			tradePeriod63.PAS_IsJobValue = true;
			var tradeValue63 = tradePeriod63.TradeValues.AddNew();
			tradeValue63.PAV_GC = GlbCompany.CurrentCompany.PK;
			tradeValue63.PAV_Revenue = 101m;
			tradeValue63.PAV_RX_NKCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			Factory.Save();

			var anotherFactory = new BusinessObjectFactory();
			var orgInOtherFactory = anotherFactory.Load<OrgHeader>(org.PK);

			var salesBreakdown = new SalesBreakdown(orgInOtherFactory);
			var viewModel = new SalesAnalysisChartViewModel(salesBreakdown);
			AssertEquals(15, viewModel.SalesAnalysisChartDataList.Count);
			CombineAssertions("ActualOrgRevenue", () =>
			{
				AssertEquals("2014-07", 0m, viewModel.SalesAnalysisChartDataList[0].ActualOrgRevenue);
				AssertEquals("2014-08", 600m, viewModel.SalesAnalysisChartDataList[1].ActualOrgRevenue);
				AssertEquals("2014-09", 0m, viewModel.SalesAnalysisChartDataList[2].ActualOrgRevenue);
				AssertEquals("2014-10", 400m, viewModel.SalesAnalysisChartDataList[3].ActualOrgRevenue);
				AssertEquals("2014-11", 0m, viewModel.SalesAnalysisChartDataList[4].ActualOrgRevenue);
				AssertEquals("2014-12", 0m, viewModel.SalesAnalysisChartDataList[5].ActualOrgRevenue);
				AssertEquals("2015-01", 0m, viewModel.SalesAnalysisChartDataList[6].ActualOrgRevenue);
				AssertEquals("2015-02", 0m, viewModel.SalesAnalysisChartDataList[7].ActualOrgRevenue);
				AssertEquals("2015-03", 0m, viewModel.SalesAnalysisChartDataList[8].ActualOrgRevenue);
				AssertEquals("2015-04", 0m, viewModel.SalesAnalysisChartDataList[9].ActualOrgRevenue);
				AssertEquals("2015-05", 1000m, viewModel.SalesAnalysisChartDataList[10].ActualOrgRevenue);
				AssertEquals("2015-06", 199m, viewModel.SalesAnalysisChartDataList[11].ActualOrgRevenue);

				AssertEquals("2015-06", 101m, viewModel.SalesAnalysisChartDataList[11].ActualJobRevenue);
			});

			CombineAssertions("Committed Value", () =>
			{
				AssertEquals("2014-07", 0m, viewModel.SalesAnalysisChartDataList[0].CommittedRevenue);
				AssertEquals("2014-08", 50m, viewModel.SalesAnalysisChartDataList[1].CommittedRevenue);
				AssertEquals("2014-09", 50m, viewModel.SalesAnalysisChartDataList[2].CommittedRevenue);
				AssertEquals("2014-10", 50m + 500m + 60m, viewModel.SalesAnalysisChartDataList[3].CommittedRevenue);
				AssertEquals("2014-11", 50m + 500m, viewModel.SalesAnalysisChartDataList[4].CommittedRevenue);
				AssertEquals("2014-12", 50m + 500m, viewModel.SalesAnalysisChartDataList[5].CommittedRevenue);
				AssertEquals("2015-01", 500m, viewModel.SalesAnalysisChartDataList[6].CommittedRevenue);
				AssertEquals("2015-02", 500m, viewModel.SalesAnalysisChartDataList[7].CommittedRevenue);
				AssertEquals("2015-03", 500m + 60m, viewModel.SalesAnalysisChartDataList[8].CommittedRevenue);
				AssertEquals("2015-04", 500m + 60m, viewModel.SalesAnalysisChartDataList[9].CommittedRevenue);
				AssertEquals("2015-05", 500m + 60m, viewModel.SalesAnalysisChartDataList[10].CommittedRevenue);
				AssertEquals("2015-06", 500m + 60m, viewModel.SalesAnalysisChartDataList[11].CommittedRevenue);
			});

			CombineAssertions("Committed + Forecast Value", () =>
			{
				AssertEquals("2014-07", 0m, viewModel.SalesAnalysisChartDataList[0].TotalEstimatedRevenue);
				AssertEquals("2014-08", 50m, viewModel.SalesAnalysisChartDataList[1].TotalEstimatedRevenue);
				AssertEquals("2014-09", 50m, viewModel.SalesAnalysisChartDataList[2].TotalEstimatedRevenue);
				AssertEquals("2014-10", 50m + 500m + 60m, viewModel.SalesAnalysisChartDataList[3].TotalEstimatedRevenue);
				AssertEquals("2014-11", 50m + 500m, viewModel.SalesAnalysisChartDataList[4].TotalEstimatedRevenue);
				AssertEquals("2014-12", 50m + 500m, viewModel.SalesAnalysisChartDataList[5].TotalEstimatedRevenue);
				AssertEquals("2015-01", 50m + 500m, viewModel.SalesAnalysisChartDataList[6].TotalEstimatedRevenue);
				AssertEquals("2015-02", 50m + 500m, viewModel.SalesAnalysisChartDataList[7].TotalEstimatedRevenue);
				AssertEquals("2015-03", 50m + 500m + 60m, viewModel.SalesAnalysisChartDataList[8].TotalEstimatedRevenue);
				AssertEquals("2015-04", 50m + 500m + 60m, viewModel.SalesAnalysisChartDataList[9].TotalEstimatedRevenue);
				AssertEquals("2015-05", 50m + 500m + 60m, viewModel.SalesAnalysisChartDataList[10].TotalEstimatedRevenue);
				AssertEquals("2015-06", 50m + 500m + 60m, viewModel.SalesAnalysisChartDataList[11].TotalEstimatedRevenue);
			});

			salesBreakdown.SelectedProduct = forwardingProduct;
			viewModel.RefreshChartModel();
			AssertEquals(15, viewModel.SalesAnalysisChartDataList.Count);
			CombineAssertions("SHP ActualOrgRevenue", () =>
			{
				AssertEquals("2014-07", 0m, viewModel.SalesAnalysisChartDataList[0].ActualOrgRevenue);
				AssertEquals("2014-08", 300m, viewModel.SalesAnalysisChartDataList[1].ActualOrgRevenue);
				AssertEquals("2014-09", 0m, viewModel.SalesAnalysisChartDataList[2].ActualOrgRevenue);
				AssertEquals("2014-10", 400m, viewModel.SalesAnalysisChartDataList[3].ActualOrgRevenue);
				AssertEquals("2014-11", 0m, viewModel.SalesAnalysisChartDataList[4].ActualOrgRevenue);
				AssertEquals("2014-12", 0m, viewModel.SalesAnalysisChartDataList[5].ActualOrgRevenue);
				AssertEquals("2015-01", 0m, viewModel.SalesAnalysisChartDataList[6].ActualOrgRevenue);
				AssertEquals("2015-02", 0m, viewModel.SalesAnalysisChartDataList[7].ActualOrgRevenue);
				AssertEquals("2015-03", 0m, viewModel.SalesAnalysisChartDataList[8].ActualOrgRevenue);
				AssertEquals("2015-04", 0m, viewModel.SalesAnalysisChartDataList[9].ActualOrgRevenue);
				AssertEquals("2015-05", 0m, viewModel.SalesAnalysisChartDataList[10].ActualOrgRevenue);
				AssertEquals("2015-06", 0m, viewModel.SalesAnalysisChartDataList[11].ActualOrgRevenue);
			});

			CombineAssertions("SHP Committed Value", () =>
			{
				AssertEquals("2014-07", 0m, viewModel.SalesAnalysisChartDataList[0].CommittedRevenue);
				AssertEquals("2014-08", 50m, viewModel.SalesAnalysisChartDataList[1].CommittedRevenue);
				AssertEquals("2014-09", 50m, viewModel.SalesAnalysisChartDataList[2].CommittedRevenue);
				AssertEquals("2014-10", 50m + 500m + 60m, viewModel.SalesAnalysisChartDataList[3].CommittedRevenue);
				AssertEquals("2014-11", 50m + 500m, viewModel.SalesAnalysisChartDataList[4].CommittedRevenue);
				AssertEquals("2014-12", 50m + 500m, viewModel.SalesAnalysisChartDataList[5].CommittedRevenue);
				AssertEquals("2015-01", 500m, viewModel.SalesAnalysisChartDataList[6].CommittedRevenue);
				AssertEquals("2015-02", 500m, viewModel.SalesAnalysisChartDataList[7].CommittedRevenue);
				AssertEquals("2015-03", 500m, viewModel.SalesAnalysisChartDataList[8].CommittedRevenue);
				AssertEquals("2015-04", 500m, viewModel.SalesAnalysisChartDataList[9].CommittedRevenue);
				AssertEquals("2015-05", 500m, viewModel.SalesAnalysisChartDataList[10].CommittedRevenue);
				AssertEquals("2015-06", 500m, viewModel.SalesAnalysisChartDataList[11].CommittedRevenue);
			});

			CombineAssertions("SHP Committed + Forecast Value", () =>
			{
				AssertEquals("2014-07", 0m, viewModel.SalesAnalysisChartDataList[0].TotalEstimatedRevenue);
				AssertEquals("2014-08", 50m, viewModel.SalesAnalysisChartDataList[1].TotalEstimatedRevenue);
				AssertEquals("2014-09", 50m, viewModel.SalesAnalysisChartDataList[2].TotalEstimatedRevenue);
				AssertEquals("2014-10", 50m + 500m + 60m, viewModel.SalesAnalysisChartDataList[3].TotalEstimatedRevenue);
				AssertEquals("2014-11", 50m + 500m, viewModel.SalesAnalysisChartDataList[4].TotalEstimatedRevenue);
				AssertEquals("2014-12", 50m + 500m, viewModel.SalesAnalysisChartDataList[5].TotalEstimatedRevenue);
				AssertEquals("2015-01", 50m + 500m, viewModel.SalesAnalysisChartDataList[6].TotalEstimatedRevenue);
				AssertEquals("2015-02", 50m + 500m, viewModel.SalesAnalysisChartDataList[7].TotalEstimatedRevenue);
				AssertEquals("2015-03", 50m + 500m, viewModel.SalesAnalysisChartDataList[8].TotalEstimatedRevenue);
				AssertEquals("2015-04", 50m + 500m, viewModel.SalesAnalysisChartDataList[9].TotalEstimatedRevenue);
				AssertEquals("2015-05", 50m + 500m, viewModel.SalesAnalysisChartDataList[10].TotalEstimatedRevenue);
				AssertEquals("2015-06", 50m + 500m, viewModel.SalesAnalysisChartDataList[11].TotalEstimatedRevenue);
			});

			salesBreakdown.SelectedProduct = transportProduct;
			viewModel.RefreshChartModel();
			AssertEquals(15, viewModel.SalesAnalysisChartDataList.Count);
			CombineAssertions("TRN ActualOrgRevenue", () =>
			{
				AssertEquals("2014-07", 0m, viewModel.SalesAnalysisChartDataList[0].ActualOrgRevenue);
				AssertEquals("2014-08", 300m, viewModel.SalesAnalysisChartDataList[1].ActualOrgRevenue);
				AssertEquals("2014-09", 0m, viewModel.SalesAnalysisChartDataList[2].ActualOrgRevenue);
				AssertEquals("2014-10", 0m, viewModel.SalesAnalysisChartDataList[3].ActualOrgRevenue);
				AssertEquals("2014-11", 0m, viewModel.SalesAnalysisChartDataList[4].ActualOrgRevenue);
				AssertEquals("2014-12", 0m, viewModel.SalesAnalysisChartDataList[5].ActualOrgRevenue);
				AssertEquals("2015-01", 0m, viewModel.SalesAnalysisChartDataList[6].ActualOrgRevenue);
				AssertEquals("2015-02", 0m, viewModel.SalesAnalysisChartDataList[7].ActualOrgRevenue);
				AssertEquals("2015-03", 0m, viewModel.SalesAnalysisChartDataList[8].ActualOrgRevenue);
				AssertEquals("2015-04", 0m, viewModel.SalesAnalysisChartDataList[9].ActualOrgRevenue);
				AssertEquals("2015-05", 1000m, viewModel.SalesAnalysisChartDataList[10].ActualOrgRevenue);
				AssertEquals("2015-06", 199m, viewModel.SalesAnalysisChartDataList[11].ActualOrgRevenue);
			});

			CombineAssertions("TRN Committed Value", () =>
			{
				AssertEquals("2014-07", 0m, viewModel.SalesAnalysisChartDataList[0].CommittedRevenue);
				AssertEquals("2014-08", 0m, viewModel.SalesAnalysisChartDataList[1].CommittedRevenue);
				AssertEquals("2014-09", 0m, viewModel.SalesAnalysisChartDataList[2].CommittedRevenue);
				AssertEquals("2014-10", 0m, viewModel.SalesAnalysisChartDataList[3].CommittedRevenue);
				AssertEquals("2014-11", 0m, viewModel.SalesAnalysisChartDataList[4].CommittedRevenue);
				AssertEquals("2014-12", 0m, viewModel.SalesAnalysisChartDataList[5].CommittedRevenue);
				AssertEquals("2015-01", 0m, viewModel.SalesAnalysisChartDataList[6].CommittedRevenue);
				AssertEquals("2015-02", 0m, viewModel.SalesAnalysisChartDataList[7].CommittedRevenue);
				AssertEquals("2015-03", 60m, viewModel.SalesAnalysisChartDataList[8].CommittedRevenue);
				AssertEquals("2015-04", 60m, viewModel.SalesAnalysisChartDataList[9].CommittedRevenue);
				AssertEquals("2015-05", 60m, viewModel.SalesAnalysisChartDataList[10].CommittedRevenue);
				AssertEquals("2015-06", 60m, viewModel.SalesAnalysisChartDataList[11].CommittedRevenue);
			});

			CombineAssertions("TRN Committed + Forecast Value", () =>
			{
				AssertEquals("2014-07", 0m, viewModel.SalesAnalysisChartDataList[0].TotalEstimatedRevenue);
				AssertEquals("2014-08", 0m, viewModel.SalesAnalysisChartDataList[1].TotalEstimatedRevenue);
				AssertEquals("2014-09", 0m, viewModel.SalesAnalysisChartDataList[2].TotalEstimatedRevenue);
				AssertEquals("2014-10", 0m, viewModel.SalesAnalysisChartDataList[3].TotalEstimatedRevenue);
				AssertEquals("2014-11", 0m, viewModel.SalesAnalysisChartDataList[4].TotalEstimatedRevenue);
				AssertEquals("2014-12", 0m, viewModel.SalesAnalysisChartDataList[5].TotalEstimatedRevenue);
				AssertEquals("2015-01", 0m, viewModel.SalesAnalysisChartDataList[6].TotalEstimatedRevenue);
				AssertEquals("2015-02", 0m, viewModel.SalesAnalysisChartDataList[7].TotalEstimatedRevenue);
				AssertEquals("2015-03", 60m, viewModel.SalesAnalysisChartDataList[8].TotalEstimatedRevenue);
				AssertEquals("2015-04", 60m, viewModel.SalesAnalysisChartDataList[9].TotalEstimatedRevenue);
				AssertEquals("2015-05", 60m, viewModel.SalesAnalysisChartDataList[10].TotalEstimatedRevenue);
				AssertEquals("2015-06", 60m, viewModel.SalesAnalysisChartDataList[11].TotalEstimatedRevenue);
			});

			var expectedDbHits = new Dictionary<string, int>
			{
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ OrgTradePeriodSchema.Constants.TableName, 6 },
				{ ViewOrgPeriodTradedValueSchema.Constants.TableName, 3 },
				{ RefCurrencySchema.Constants.TableName, 1 }
			};
			AssertDbHits(expectedDbHits, anotherFactory);
		}

		[TestDate(2015, 7, 3)]
		public void TestPopulateChartData_CompanyFilter()
		{
			var forwardingProduct = Factory.LoadTop1<OrgSalesProduct>(new ZQuery(OrgSalesProductSchema.MP_Code, "SHP"));

			var newCompany = Factory.NewWithValidTestData<GlbCompany>();
			newCompany.GC_Code = "DDD";

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var opp1 = org.SalesOpportunities.AddNew();
			opp1.P8_GC = GlbCompany.CurrentCompany.PK;
			var opp2 = org.SalesOpportunities.AddNew();
			opp2.P8_GC = newCompany.PK;

			var prospectShp1 = org.SalesCollection.AddNew();
			prospectShp1.OW_MP_Product = forwardingProduct.PK;
			var prospectShp1Detail = prospectShp1.TradeDetails.AddNew();
			prospectShp1Detail.PA_TradeMode = "AIR";
			prospectShp1Detail.PA_Status = OpportunityTradeStatus.Codes.Successful;
			prospectShp1Detail.CurrentProspectPeriod.PAS_RepeatsMnth = 1;
			prospectShp1Detail.CurrentProspectPeriod.PAS_EstimatedProfit = 50;
			prospectShp1Detail.CurrentProspectPeriod.PAS_RX_NKCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			prospectShp1Detail.ProspectPeriodStart = new ZDate(2014, 8, 1);
			prospectShp1Detail.ProspectPeriodEnd = new ZDate(2014, 12, 1);

			var prospectShp2 = org.SalesCollection.AddNew();
			prospectShp2.OW_MP_Product = forwardingProduct.PK;
			var prospectShp2Detail = prospectShp2.TradeDetails.AddNew();
			prospectShp2Detail.PA_TradeMode = "SEA";
			prospectShp2Detail.PA_Status = OpportunityTradeStatus.Codes.Successful;
			prospectShp2Detail.CurrentProspectPeriod.PAS_RepeatsMnth = 1;
			prospectShp2Detail.CurrentProspectPeriod.PAS_EstimatedProfit = 60;
			prospectShp2Detail.CurrentProspectPeriod.PAS_RX_NKCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			prospectShp2Detail.ProspectDetail.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.OneOff;
			prospectShp2Detail.ProspectPeriodStart = new ZDate(2014, 10, 1);
			prospectShp2Detail.ProspectPeriodEnd = new ZDate(2014, 10, 1);

			opp1.AssociatedTradeLanesPivots.AddPivotFor(prospectShp1Detail);
			opp2.AssociatedTradeLanesPivots.AddPivotFor(prospectShp2Detail);

			var sales1 = org.SalesCollection.AddNew();
			sales1.OW_IsTraded = true;
			sales1.OW_MP_Product = forwardingProduct.PK;
			sales1.OW_OH_Buyer = org.PK;

			var tradeDetail11 = sales1.TradeDetails.AddNew();
			var tradePeriod11 = tradeDetail11.TradedPeriods.AddNew();
			tradePeriod11.PAS_Period = new ZDate(2014, 8, 1);
			tradePeriod11.PAS_OH_Client = org.PK;
			var tradeValue11 = tradePeriod11.TradeValues.AddNew();
			tradeValue11.PAV_GC = GlbCompany.CurrentCompany.PK;
			tradeValue11.PAV_Revenue = 100m;
			tradeValue11.PAV_RX_NKCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			var tradeDetail12 = sales1.TradeDetails.AddNew();
			var tradePeriod12 = tradeDetail12.TradedPeriods.AddNew();
			tradePeriod12.PAS_Period = new ZDate(2014, 8, 1);
			tradePeriod12.PAS_OH_Client = org.PK;
			var tradeValue12 = tradePeriod12.TradeValues.AddNew();
			tradeValue12.PAV_GC = newCompany.PK;
			tradeValue12.PAV_Revenue = 200m;
			tradeValue12.PAV_RX_NKCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			Factory.Save();

			var anotherFactory = new BusinessObjectFactory();
			var orgInOtherFactory = anotherFactory.Load<OrgHeader>(org.PK);

			var salesBreakdown = new SalesBreakdown(orgInOtherFactory);
			var viewModel = new SalesAnalysisChartViewModel(salesBreakdown);
			CombineAssertions("ActualOrgRevenue", () =>
			{
				AssertEquals("2014-08", 100m, viewModel.SalesAnalysisChartDataList[1].ActualOrgRevenue);
			});

			CombineAssertions("Committed Value", () =>
			{
				AssertEquals("2014-07", 0m, viewModel.SalesAnalysisChartDataList[0].CommittedRevenue);
				AssertEquals("2014-08", 50m, viewModel.SalesAnalysisChartDataList[1].CommittedRevenue);
				AssertEquals("2014-09", 50m, viewModel.SalesAnalysisChartDataList[2].CommittedRevenue);
				AssertEquals("2014-10", 50m, viewModel.SalesAnalysisChartDataList[3].CommittedRevenue);
				AssertEquals("2014-11", 50m, viewModel.SalesAnalysisChartDataList[4].CommittedRevenue);
				AssertEquals("2014-12", 50m, viewModel.SalesAnalysisChartDataList[5].CommittedRevenue);
				AssertEquals("2015-01", 0m, viewModel.SalesAnalysisChartDataList[6].CommittedRevenue);
				AssertEquals("2015-02", 0m, viewModel.SalesAnalysisChartDataList[7].CommittedRevenue);
				AssertEquals("2015-03", 0m, viewModel.SalesAnalysisChartDataList[8].CommittedRevenue);
				AssertEquals("2015-04", 0m, viewModel.SalesAnalysisChartDataList[9].CommittedRevenue);
				AssertEquals("2015-05", 0m, viewModel.SalesAnalysisChartDataList[10].CommittedRevenue);
				AssertEquals("2015-06", 0m, viewModel.SalesAnalysisChartDataList[11].CommittedRevenue);
			});

			salesBreakdown.CompanyFilter = newCompany.PK;
			viewModel.RefreshChartModel();

			CombineAssertions("Company filter should not apply to ActualOrgRevenue", () =>
			{
				AssertEquals("2014-08", 100m, viewModel.SalesAnalysisChartDataList[1].ActualOrgRevenue);
			});

			CombineAssertions("Company filter should apply to Committed Value", () =>
			{
				AssertEquals("2014-07", 0m, viewModel.SalesAnalysisChartDataList[0].CommittedRevenue);
				AssertEquals("2014-08", 0m, viewModel.SalesAnalysisChartDataList[1].CommittedRevenue);
				AssertEquals("2014-09", 0m, viewModel.SalesAnalysisChartDataList[2].CommittedRevenue);
				AssertEquals("2014-10", 60m, viewModel.SalesAnalysisChartDataList[3].CommittedRevenue);
				AssertEquals("2014-11", 0m, viewModel.SalesAnalysisChartDataList[4].CommittedRevenue);
				AssertEquals("2014-12", 0m, viewModel.SalesAnalysisChartDataList[5].CommittedRevenue);
				AssertEquals("2015-01", 0m, viewModel.SalesAnalysisChartDataList[6].CommittedRevenue);
				AssertEquals("2015-02", 0m, viewModel.SalesAnalysisChartDataList[7].CommittedRevenue);
				AssertEquals("2015-03", 0m, viewModel.SalesAnalysisChartDataList[8].CommittedRevenue);
				AssertEquals("2015-04", 0m, viewModel.SalesAnalysisChartDataList[9].CommittedRevenue);
				AssertEquals("2015-05", 0m, viewModel.SalesAnalysisChartDataList[10].CommittedRevenue);
				AssertEquals("2015-06", 0m, viewModel.SalesAnalysisChartDataList[11].CommittedRevenue);
			});
		}

		[TestDate(2015, 7, 3)]
		public void TestMultiLanguages()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var anotherFactory = new BusinessObjectFactory();
			var orgInOtherFactory = anotherFactory.Load<OrgHeader>(org.PK);
			var salesBreakdown = new SalesBreakdown(orgInOtherFactory);

			//default
			{
				var rX_Symbol = GlbCompany.CurrentCompany.LocalCurrency.RX_Symbol;

				var viewModel = new SalesAnalysisChartViewModel(salesBreakdown);
				var revenueAxis = viewModel.SalesTimeLineChartModel.Axes.First(x => x.Position == AxisPosition.Left);
				var revenueAxisLabel = revenueAxis.LabelFormatter(123456789.99);
				AssertEquals(rX_Symbol + "123,456,790", revenueAxisLabel);

				var periodAxis = viewModel.SalesTimeLineChartModel.Axes.First(x => x.Position == AxisPosition.Bottom);
				var periodAxisLabel = periodAxis.LabelFormatter(-1);
				AssertEquals("JUN", periodAxisLabel);

				var actualOrgRevenueSeries = viewModel.SalesTimeLineChartModel.Series.OfType<LineSeries>().First(x => x.Title == Res.GetString("13c07b05-96ab-46dd-a76f-b00056374784", "Revenue"));
				var actualOrgRevenueSeriesLabel = string.Format(viewModel.SalesTimeLineChartModel.Culture, actualOrgRevenueSeries.LabelFormatString,
					1122334455.66, 2233445566.77);
				AssertEquals(rX_Symbol + "2,233,445,567", actualOrgRevenueSeriesLabel);
			}

			//CHS
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("CN"))
			{
				using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
				{
					var rX_Symbol = GlbCompany.CurrentCompany.LocalCurrency.RX_Symbol;
					GlbStaff.CurrentUser.GS_WorkingLanguage = Core.SharedConstants.Languages.ChineseSimplified;

					var viewModel = new SalesAnalysisChartViewModel(salesBreakdown);
					var revenueAxis = viewModel.SalesTimeLineChartModel.Axes.First(x => x.Position == AxisPosition.Left);
					var revenueAxisLabel = revenueAxis.LabelFormatter(123456789.99);
					AssertEquals(rX_Symbol + "123,456,790", revenueAxisLabel);

					var periodAxis = viewModel.SalesTimeLineChartModel.Axes.First(x => x.Position == AxisPosition.Bottom);
					var periodAxisLabel = periodAxis.LabelFormatter(-1);
					AssertEquals("2015-06", periodAxisLabel);

					var actualOrgRevenueSeries = viewModel.SalesTimeLineChartModel.Series.First(x => x.Title == Res.GetString("13c07b05-96ab-46dd-a76f-b00056374784", "Revenue")) as LineSeries;
					var actualOrgRevenueSeriesLabel = string.Format(viewModel.SalesTimeLineChartModel.Culture, actualOrgRevenueSeries.LabelFormatString,
						1122334455.66, 2233445566.77);
					AssertEquals(rX_Symbol + "2,233,445,567", actualOrgRevenueSeriesLabel);
				}
			}

			//CZ
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("CZ"))
			{
				using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.Czech))
				{
					var rX_Symbol = GlbCompany.CurrentCompany.LocalCurrency.RX_Symbol;
					GlbStaff.CurrentUser.GS_WorkingLanguage = Core.SharedConstants.Languages.Czech;

					var viewModel = new SalesAnalysisChartViewModel(salesBreakdown);
					var revenueAxis = viewModel.SalesTimeLineChartModel.Axes.First(x => x.Position == AxisPosition.Left);
					var revenueAxisLabel = revenueAxis.LabelFormatter(123456789.99);
					AssertEquals("123 456 790 " + rX_Symbol, revenueAxisLabel);

					var periodAxis = viewModel.SalesTimeLineChartModel.Axes.First(x => x.Position == AxisPosition.Bottom);
					var periodAxisLabel = periodAxis.LabelFormatter(-1);
					AssertEquals("CERV.", periodAxisLabel);

					var actualOrgRevenueSeries = viewModel.SalesTimeLineChartModel.Series.OfType<LineSeries>().First(x => x.Title == Res.GetString("13c07b05-96ab-46dd-a76f-b00056374784", "Revenue"));
					var actualOrgRevenueSeriesLabel = string.Format(viewModel.SalesTimeLineChartModel.Culture, actualOrgRevenueSeries.LabelFormatString,
						1122334455.66, 2233445566.77);
					AssertEquals("2 233 445 567 " + rX_Symbol, actualOrgRevenueSeriesLabel);
				}
			}
		}
	}
}
