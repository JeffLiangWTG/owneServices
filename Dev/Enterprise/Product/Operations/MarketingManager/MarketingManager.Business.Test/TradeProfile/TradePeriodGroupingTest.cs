using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(TradePeriodGrouping))]
	sealed class TradePeriodGroupingTest : NonPersistentBusinessObjectTestCase
	{
		public void TestCurrency()
		{
			var audCompany = Factory.NewWithValidTestData<GlbCompany>();
			audCompany.GC_RX_NKLocalCurrency = "AUD";
			var idrCompany = Factory.NewWithValidTestData<GlbCompany>();
			idrCompany.GC_RX_NKLocalCurrency = "IDR";
			var audBranch = Factory.NewWithValidTestData<GlbBranch>();
			audBranch.GB_GC = audCompany.PK;
			var idrBranch = Factory.NewWithValidTestData<GlbBranch>();
			idrBranch.GB_GC = idrCompany.PK;
			var salesProduct = Factory.NewWithValidTestData<OrgSalesProduct>();
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, audBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var org = Factory.New<OrgHeader>();
				var header = new SalesHeader(org, salesProduct);
				var salesAnalysis = new TradedSalesAnalysis(header);
				var node = new TradePeriodGrouping(salesAnalysis, Enumerable.Empty<OrgTradePeriod>(), null);
				AssertEquals("AUD", node.CurrencyCode);
				AssertEquals(2, node.CurrencyDecimals);
			}

			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, idrBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var org = Factory.New<OrgHeader>();
				var header = new SalesHeader(org, salesProduct);
				var salesAnalysis = new TradedSalesAnalysis(header);
				var node = new TradePeriodGrouping(salesAnalysis, Enumerable.Empty<OrgTradePeriod>(), null);
				AssertEquals("IDR", node.CurrencyCode);
				AssertEquals(0, node.CurrencyDecimals);
			}
		}

		public void TestRevenueAndCost()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var audCompany = Factory.NewWithValidTestData<GlbCompany>();
			audCompany.GC_RX_NKLocalCurrency = "AUD";

			var nzdCompany = Factory.NewWithValidTestData<GlbCompany>();
			nzdCompany.GC_RX_NKLocalCurrency = "NZD";

			var audBranch = Factory.NewWithValidTestData<GlbBranch>();
			audBranch.GB_GC = audCompany.PK;

			var nzdBranch = Factory.NewWithValidTestData<GlbBranch>();
			nzdBranch.GB_GC = nzdCompany.PK;

			var salesProduct = Factory.NewWithValidTestData<OrgSalesProduct>();

			var tradeLane = Factory.NewWithValidTestData<OrgSales>();
			tradeLane.OW_MP_Product = salesProduct.PK;
			tradeLane.OW_OH_Primary = org.PK;

			var tradeDetail1 = tradeLane.TradeDetails.AddNew();

			var jobPeriod1 = tradeDetail1.TradedPeriods.AddNew();
			jobPeriod1.PAS_OH_Client = org.PK;
			jobPeriod1.PAS_IsJobValue = true;

			var jobValue1 = jobPeriod1.TradeValues.AddNew();
			jobValue1.PAV_GC = audCompany.PK;
			jobValue1.PAV_RX_NKCurrency = "AUD";
			jobValue1.PAV_Revenue = 100m;
			jobValue1.PAV_Cost = 50m;

			var tradePeriod1 = tradeDetail1.TradedPeriods.AddNew();
			tradePeriod1.PAS_OH_Client = org.PK;

			var tradeValue11 = tradePeriod1.TradeValues.AddNew();
			tradeValue11.PAV_GC = audCompany.PK;
			tradeValue11.PAV_RX_NKCurrency = "AUD";
			tradeValue11.PAV_Revenue = 60m;

			var tradeValue12 = tradePeriod1.TradeValues.AddNew();
			tradeValue12.PAV_GC = nzdCompany.PK;
			tradeValue12.PAV_RX_NKCurrency = "AUD";
			tradeValue12.PAV_Revenue = 40m;

			var tradeDetail2 = tradeLane.TradeDetails.AddNew();

			var jobPeriod2 = tradeDetail2.TradedPeriods.AddNew();
			jobPeriod2.PAS_OH_Client = org.PK;
			jobPeriod2.PAS_IsJobValue = true;

			var jobValue2 = jobPeriod2.TradeValues.AddNew();
			jobValue2.PAV_GC = nzdCompany.PK;
			jobValue2.PAV_RX_NKCurrency = "NZD";
			jobValue2.PAV_Revenue = 150m;
			jobValue2.PAV_Cost = 75m;

			var tradePeriod2 = tradeDetail2.TradedPeriods.AddNew();
			tradePeriod2.PAS_OH_Client = org.PK;

			var tradeValue21 = tradePeriod2.TradeValues.AddNew();
			tradeValue21.PAV_GC = audCompany.PK;
			tradeValue21.PAV_RX_NKCurrency = "NZD";
			tradeValue21.PAV_Revenue = 60m;

			var tradeValue22 = tradePeriod2.TradeValues.AddNew();
			tradeValue22.PAV_GC = nzdCompany.PK;
			tradeValue22.PAV_RX_NKCurrency = "NZD";
			tradeValue22.PAV_Revenue = 90m;

			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, audBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var nzd = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "NZD");
				var rate = nzd.ExchangeRates.AddNew();
				rate.RE_ExRateType = "SEL";
				rate.RE_SellRate = 1.2m;

				var header = new SalesHeader(org, salesProduct);
				header.CompanyFilter = ZGuid.Empty;
				var salesAnalysis = new TradedSalesAnalysis(header);
				salesAnalysis.SetViewpointOrg(org);
				var node = new TradePeriodGrouping(salesAnalysis, new OrgTradePeriod[] { tradePeriod1, tradePeriod2, jobPeriod1, jobPeriod2 }, null);
				AssertEquals("AUD 60 + NZD 60 => AUD", 110m, node.GrossRevenue);
				AssertEquals("AUD 100 => AUD", 100m, node.JobRevenue);
				AssertEquals("AUD 50 => AUD", 50m, node.JobCost);
				AssertEquals("AUD 100 - AUD 50 => AUD", 50m, node.JobProfit);
			}

			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, nzdBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var nzd = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "AUD");
				var rate = nzd.ExchangeRates.AddNew();
				rate.RE_ExRateType = "SEL";
				rate.RE_SellRate = 0.8m;

				var header = new SalesHeader(org, salesProduct);
				header.CompanyFilter = ZGuid.Empty;
				var salesAnalysis = new TradedSalesAnalysis(header);
				salesAnalysis.SetViewpointOrg(org);
				var node = new TradePeriodGrouping(salesAnalysis, new OrgTradePeriod[] { tradePeriod1, tradePeriod2, jobPeriod1, jobPeriod2 }, null);
				AssertEquals("AUD 40 + NZD 90 => NZD", 140m, node.GrossRevenue);
				AssertEquals("NZD 150 => NZD", 150m, node.JobRevenue);
				AssertEquals("NZD 75 => NZD", 75m, node.JobCost);
				AssertEquals("NZD 150 - NZD 75 => NZD", 75m, node.JobProfit);
			}
		}

		public void TestTEUQuantity()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RX_NKLocalCurrency = "AUD";

			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = company.PK;

			var salesProduct = Factory.NewWithValidTestData<OrgSalesProduct>();

			var tradeLane = Factory.NewWithValidTestData<OrgSales>();
			tradeLane.OW_MP_Product = salesProduct.PK;
			tradeLane.OW_OH_Primary = org.PK;

			var tradeDetail1 = tradeLane.TradeDetails.AddNew();
			var tradePeriod1 = tradeDetail1.TradedPeriods.AddNew();
			tradePeriod1.PAS_OH_Client = org.PK;
			tradePeriod1.PAS_TEUQuantity = 123.45m;
			var jobPeriod1 = tradeDetail1.TradedPeriods.AddNew();
			jobPeriod1.PAS_OH_Client = org.PK;
			jobPeriod1.PAS_IsJobValue = true;
			jobPeriod1.PAS_TEUQuantity = 123.45m;

			var tradeDetail2 = tradeLane.TradeDetails.AddNew();
			var tradePeriod2 = tradeDetail2.TradedPeriods.AddNew();
			tradePeriod2.PAS_OH_Client = org.PK;
			tradePeriod2.PAS_TEUQuantity = 456.78m;
			var jobPeriod2 = tradeDetail2.TradedPeriods.AddNew();
			jobPeriod2.PAS_OH_Client = org.PK;
			jobPeriod2.PAS_IsJobValue = true;
			jobPeriod2.PAS_TEUQuantity = 456.78m;

			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, branch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var header = new SalesHeader(org, salesProduct);
				var salesAnalysis = new TradedSalesAnalysis(header);
				salesAnalysis.SetViewpointOrg(org);
				var node = new TradePeriodGrouping(salesAnalysis, new OrgTradePeriod[] { tradePeriod1, tradePeriod2, jobPeriod1, jobPeriod2 }, null);
				AssertEquals("123.45 + 456.78 => 580.23", 580.23m, node.TEUQuantity);
			}
		}

		public void TestOriginDetinationCountry()
		{
			var salesProduct = Factory.New<OrgSalesProduct>();

			var orgAAA = Factory.New<OrgHeader>();
			orgAAA.OH_Code = "AAA";
			var orgBBB = Factory.New<OrgHeader>();
			orgBBB.OH_Code = "BBB";

			var au = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU");
			var nsw = Factory.LoadTop1<RefCountryStates>(new ZQuery(new ZQuery(RefCountryStatesSchema.RW_RN_NKCountryCode, au.RN_Code), new ZQuery(RefCountryStatesSchema.RW_Code, "NSW")));
			var us = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "US");
			var newYork = Factory.LoadTop1<RefCountryStates>(new ZQuery(new ZQuery(RefCountryStatesSchema.RW_RN_NKCountryCode, us.RN_Code), new ZQuery(RefCountryStatesSchema.RW_Code, "NY")));

			var org = Factory.New<OrgHeader>();
			var sales = org.SalesCollection.AddNew();
			sales.OW_OriginID = nsw.PK;
			sales.OW_DestinationID = newYork.PK;
			var orgTradeDetailA = sales.TradeDetails.AddNew();
			var periodA = orgTradeDetailA.TradedPeriods.AddNew();
			var orgTradeDetailB = sales.TradeDetails.AddNew();
			var periodB = orgTradeDetailA.TradedPeriods.AddNew();

			var header = new SalesHeader(org, salesProduct);
			var salesAnalysis = new TradedSalesAnalysis(header);
			var leafNode = new TradePeriodGrouping(salesAnalysis, new[] { periodA, periodB }, null);

			CombineAssertions(() =>
			{
				AssertEquals("DestinationCountry", "US", leafNode.DestinationCountry);
				AssertEquals("OriginCountry", "AU", leafNode.OriginCountry);
			});

			var midNode = new TradePeriodGrouping(salesAnalysis, "", new[] { leafNode }, null);
			CombineAssertions(() =>
			{
				AssertEquals("DestinationCountry", "US", midNode.DestinationCountry);
				AssertEquals("OriginCountry", "AU", midNode.OriginCountry);
			});
		}

		public void TestGetWarehouseOrdAndRecColumnsThatArePerJobRatherThanSupplierPart()
		{
			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					TradePeriodGrouping.Schema.PalletCount,
					TradePeriodGrouping.Schema.GrossWeight,
					TradePeriodGrouping.Schema.WeightUnit,
					TradePeriodGrouping.Schema.NetVolume,
					TradePeriodGrouping.Schema.VolumeUnit
				},
				TradePeriodGrouping.GetWarehouseOrdAndRecColumnsThatArePerJobRatherThanSupplierPart(Factory));
		}

		public void TestGetWarehouseOrdAndRecColumnsThatHaveDifferentJobAndSupplierPartTotals()
		{
			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					TradePeriodGrouping.Schema.Count,
					TradePeriodGrouping.Schema.GrossRevenue,
					TradePeriodGrouping.Schema.JobRevenue,
					TradePeriodGrouping.Schema.CurrencyCode
				},
				TradePeriodGrouping.GetWarehouseOrdAndRecColumnsThatHaveDifferentJobAndSupplierPartTotals(Factory));
		}

		public void TestGetWarehouseStgUnusedColumns()
		{
			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					TradePeriodGrouping.Schema.UnitCount,
					TradePeriodGrouping.Schema.PalletCount,
					TradePeriodGrouping.Schema.LineCount,
					TradePeriodGrouping.Schema.GrossWeight,
					TradePeriodGrouping.Schema.WeightUnit,
					TradePeriodGrouping.Schema.NetVolume,
					TradePeriodGrouping.Schema.VolumeUnit
				},
				TradePeriodGrouping.GetWarehouseStgUnusedColumns(Factory));
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var product = Factory.New<OrgSalesProduct>();
			var org = Factory.New<OrgHeader>();
			var sales = org.SalesCollection.AddNew();
			var tradeDetail = sales.TradeDetails.AddNew();
			var tradePeriod = tradeDetail.TradedPeriods.AddNew();
			var salesHeader = new SalesHeader(org, product);
			var salesAnalysis = new TradedSalesAnalysis(salesHeader);
			return new TradePeriodGrouping(salesAnalysis, new[] { tradePeriod }, null);
		}

		#endregion
	}
}
