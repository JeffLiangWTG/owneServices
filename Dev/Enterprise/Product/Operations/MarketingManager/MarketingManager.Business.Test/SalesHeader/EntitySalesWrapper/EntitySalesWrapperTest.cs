using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MarketingManager.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(EntitySalesWrapper))]
	sealed class EntitySalesWrapperTest : EnterpriseBusinessObjectTestCase
	{
		public void TestTotals()
		{
			var org = Helper.NewOrgHeader();
			var opportunity = Factory.New<OrgOpportunity>();
			opportunity.P8_RX_NKEstimatedValueCurrency = "AUD";
			var sales = org.SalesCollection.AddNew();
			Helper.NewOrgTradeDetail(sales, OrgTradeProspectRecurrenceTypeList.Codes.Monthly, 10, 3m, "AUD");
			Helper.NewOrgTradeDetail(sales, OrgTradeProspectRecurrenceTypeList.Codes.Monthly, 100, 5m, "USD");
			Helper.NewOrgTradeDetail(sales, OrgTradeProspectRecurrenceTypeList.Codes.Monthly, 1000, 8m, "AUD");

			var saleItem = EntitySalesWrapper.Get(sales, opportunity);

			AssertEquals(8030m * 12, saleItem.TotalEstimatedAnnualValue);
			AssertEquals("AUD", saleItem.CalculatedRevenueCurrencyCode);

			Helper.NewExchangeRate("USD", Core.Constants.ExchangeRateTypes.Code.SellRate, 2m);
			AssertEquals((8030m + 250m) * 12, saleItem.TotalEstimatedAnnualValue);
			AssertEquals("AUD", saleItem.CalculatedRevenueCurrencyCode);
		}

		[TestDate(2018, 2, 9)]
		public void TestTotals_ProspectValues()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var forwardingProduct = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, "SHP");
			var transportProduct = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, "TRN");

			var sales1 = Factory.New<OrgSales>();
			sales1.OW_MP_Product = forwardingProduct.PK;
			sales1.OW_IsTraded = false;
			sales1.OW_OH_Primary = org.PK;

			var detail1a = sales1.TradeDetails.AddNew();
			detail1a.PA_Status = OpportunityTradeStatus.Codes.Successful;
			detail1a.ProspectDetail.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.Monthly;
			detail1a.CurrentProspectPeriod.PAS_TEUQuantity = 20m;
			detail1a.CurrentProspectPeriod.PAS_RX_NKCurrency = "AUD";
			detail1a.CurrentProspectPeriod.PAS_EstimatedProfit = 200m;
			detail1a.ProspectPeriodStart = new ZDate(2017, 6, 1);
			detail1a.ProspectPeriodEnd = new ZDate(2018, 5, 1);

			var detail1b = sales1.TradeDetails.AddNew();
			detail1b.PA_Status = OpportunityTradeStatus.Codes.Active;
			detail1b.ProspectDetail.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.Monthly;
			detail1b.CurrentProspectPeriod.PAS_TEUQuantity = 10m;
			detail1b.CurrentProspectPeriod.PAS_RX_NKCurrency = "AUD";
			detail1b.CurrentProspectPeriod.PAS_EstimatedProfit = 100m;

			var sales2 = Factory.New<OrgSales>();
			sales2.OW_MP_Product = forwardingProduct.PK;
			sales2.OW_IsTraded = false;
			sales2.OW_OH_Primary = org.PK;

			var detail2 = sales2.TradeDetails.AddNew();
			detail2.PA_Status = OpportunityTradeStatus.Codes.Unsuccessful;
			detail2.ProspectDetail.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.Monthly;
			detail2.CurrentProspectPeriod.PAS_TEUQuantity = 10m;
			detail2.CurrentProspectPeriod.PAS_RX_NKCurrency = "USD";
			detail2.CurrentProspectPeriod.PAS_EstimatedProfit = 100m;

			var usdExRate = Factory.New<RefExchangeRate>();
			usdExRate.RE_RX_NKExCurrency = "USD";
			usdExRate.RE_StartDate = new ZDateTime(2018, 1, 1);
			usdExRate.RE_ExpiryDate = new ZDateTime(2018, 3, 1);
			usdExRate.RE_SellRate = 2;
			usdExRate.RE_ExRateType = Enterprise.Core.Constants.ExchangeRateTypes.Code.SellRate;

			Factory.Save();

			CombineAssertions(() =>
			{
				var saleItem1 = EntitySalesWrapper.Get(sales1, org);
				AssertEquals(3600m, saleItem1.TotalEstimatedAnnualValue);
				AssertEquals(2400m, saleItem1.CommittedAnnualValue);
				AssertEquals(240m, saleItem1.CommittedAnnualTEUQuantity);
				AssertEquals(200m, saleItem1.CommittedMonthlyValue);
				AssertEquals(1200m, saleItem1.PipelineValue);
				AssertEquals(120m, saleItem1.PipelineTEUQuantity);
				AssertEquals(0m, saleItem1.UnsuccessfulValue);
				AssertEquals(0m, saleItem1.UnsuccessfulTEUQuantity);

				var saleItem2 = EntitySalesWrapper.Get(sales2, org);
				AssertEquals(0m, saleItem2.TotalEstimatedAnnualValue);
				AssertEquals(0m, saleItem2.CommittedAnnualValue);
				AssertEquals(0m, saleItem2.CommittedAnnualTEUQuantity);
				AssertEquals(0m, saleItem2.CommittedMonthlyValue);
				AssertEquals(0m, saleItem2.PipelineValue);
				AssertEquals(0m, saleItem2.PipelineTEUQuantity);
				AssertEquals(600m, saleItem2.UnsuccessfulValue);
				AssertEquals(120m, saleItem2.UnsuccessfulTEUQuantity);
			});
		}

		public void TestOW_Calc_TotalAnnualChargeable()
		{
			var forProduct = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment);
			var org = Helper.NewOrgHeader();
			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			var sales = org.SalesCollection.AddNew();
			sales.OW_MP_Product = forProduct.PK;
			var tradeDetail1 = sales.TradeDetails.AddNew();
			tradeDetail1.PA_TradeMode = Constants.TransportModes.Air;
			tradeDetail1.ProspectDetail.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.Monthly;
			tradeDetail1.CurrentProspectPeriod.PAS_WeightUQ = Constants.Weight.Kilograms;
			tradeDetail1.CurrentProspectPeriod.PAS_Chargeable = 10;
			tradeDetail1.SalesAssociationPivotCollectionGlobal.AddNew(opportunity);
			var tradeDetail2 = sales.TradeDetails.AddNew();
			tradeDetail2.PA_TradeMode = Constants.TransportModes.Air;
			tradeDetail2.ProspectDetail.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.Monthly;
			tradeDetail2.CurrentProspectPeriod.PAS_WeightUQ = Constants.Weight.Kilograms;
			tradeDetail2.CurrentProspectPeriod.PAS_Chargeable = 10;
			tradeDetail2.SalesAssociationPivotCollectionGlobal.AddNew(opportunity);
			var tradeDetail3 = sales.TradeDetails.AddNew();
			tradeDetail3.PA_TradeMode = Constants.TransportModes.Air;
			tradeDetail3.ProspectDetail.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.Monthly;
			tradeDetail3.CurrentProspectPeriod.PAS_WeightUQ = Constants.Weight.Kilograms;
			tradeDetail3.CurrentProspectPeriod.PAS_Chargeable = 10;
			tradeDetail3.SalesAssociationPivotCollectionGlobal.AddNew(opportunity);

			Factory.Save();

			var entitySales = EntitySalesWrapper.Get(sales, opportunity);
			AssertEquals(30m * 12, entitySales.OW_Calc_TotalAnnualChargeable);

			var tradeDetail4 = entitySales.EntityTradeDetailsCollection.AddNew();
			tradeDetail4.CurrentProspectPeriod.PAS_Chargeable = 10;
			AssertEquals("Should include newly created tradeDetail", 40m * 12, entitySales.OW_Calc_TotalAnnualChargeable);
		}

		public void TestOW_Calc_TotalAnnualVolume()
		{
			var whsProduct = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.Warehouse);
			var org = Helper.NewOrgHeader();
			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			var sales = org.SalesCollection.AddNew();
			sales.OW_MP_Product = whsProduct.PK;
			var tradeDetail1 = sales.TradeDetails.AddNew();
			tradeDetail1.ProspectDetail.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.Monthly;
			tradeDetail1.CurrentProspectPeriod.PAS_Volume = 10;
			tradeDetail1.CurrentProspectPeriod.PAS_VolumeUQ = Constants.Volume.CubicMetres;
			tradeDetail1.SalesAssociationPivotCollectionGlobal.AddNew(opportunity);
			var tradeDetail2 = sales.TradeDetails.AddNew();
			tradeDetail2.ProspectDetail.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.Monthly;
			tradeDetail2.CurrentProspectPeriod.PAS_Volume = 10;
			tradeDetail2.CurrentProspectPeriod.PAS_VolumeUQ = Constants.Volume.CubicMetres;
			tradeDetail2.SalesAssociationPivotCollectionGlobal.AddNew(opportunity);
			var tradeDetail3 = sales.TradeDetails.AddNew();
			tradeDetail3.ProspectDetail.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.Monthly;
			tradeDetail3.CurrentProspectPeriod.PAS_Volume = 10;
			tradeDetail3.CurrentProspectPeriod.PAS_VolumeUQ = Constants.Volume.CubicMetres;

			Factory.Save();

			var entitySales = EntitySalesWrapper.Get(sales, opportunity);
			AssertEquals("Should only include tradeDetails associated to opportunity", 20m * 12, entitySales.OW_Calc_TotalAnnualVolume);

			var tradeDetail4 = entitySales.EntityTradeDetailsCollection.AddNew();
			tradeDetail4.ProspectDetail.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.Monthly;
			tradeDetail4.CurrentProspectPeriod.PAS_Volume = 10;
			tradeDetail4.CurrentProspectPeriod.PAS_VolumeUQ = Constants.Volume.CubicMetres;
			AssertEquals("Should include newly created tradeDetail", 30m * 12, entitySales.OW_Calc_TotalAnnualVolume);
		}

		public void TestCanDelete()
		{
			var product = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment);
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var tradeLane1 = org.SalesCollection.AddNew();
			tradeLane1.OW_MP_Product = product.PK;
			var tradeLane2 = org.SalesCollection.AddNew();
			tradeLane2.OW_MP_Product = product.PK;

			var opportunity1 = org.SalesOpportunities.AddNew();
			var pivot11 = opportunity1.AssociatedTradeLanesPivots.AddPivotFor(tradeLane1);
			var pivot12 = opportunity1.AssociatedTradeLanesPivots.AddPivotFor(tradeLane2);

			var opportunity2 = org.SalesOpportunities.AddNew();
			var pivot21 = opportunity2.AssociatedTradeLanesPivots.AddPivotFor(tradeLane1);

			Factory.Save();

			AssertEquals("Should be able to delete if only one opp associated to trade lane", true, EntitySalesWrapper.Get(tradeLane2, opportunity1).CanDelete);

			AssertEquals("Should not be able to delete if multiple opps associated to trade lane", false, EntitySalesWrapper.Get(tradeLane1, opportunity1).CanDelete);
		}

		public void TestDeleteShouldRemoveAndDeleteAllElementsOfEntityTradeDetailsCollection()
		{
			var product = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment);
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			var opportunity = organisation.SalesOpportunities.AddNew();
			var tradeLane = organisation.SalesCollection.AddNew();
			tradeLane.OW_MP_Product = product.PK;

			var entitySales = EntitySalesWrapper.Get(tradeLane, opportunity);
			var entityTradeDetail = entitySales.EntityTradeDetailsCollection.AddNew();
			var detail = (OrgTradeDetail)entityTradeDetail;

			AssertEquals(1, entitySales.TradeDetails.Count);
			AssertEquals(detail.PK, entitySales.TradeDetails.FirstOrDefault()?.PK);
			AssertEquals(1, entitySales.EntityTradeDetailsCollection.Count);
			AssertEquals(1, entitySales.EntityTradeDetailsCompanyView.Count);

			entitySales.TradeDetails.RemoveAll();

			AssertEquals(0, entitySales.TradeDetails.Count);
			AssertEquals(1, entitySales.EntityTradeDetailsCollection.Count);
			AssertEquals(1, entitySales.EntityTradeDetailsCompanyView.Count);
			AssertEquals(false, entityTradeDetail.IsDeleted);

			entitySales.Delete();

			AssertEquals(0, entitySales.TradeDetails.Count);
			AssertEquals(0, entitySales.EntityTradeDetailsCollection.Count);
			AssertEquals(0, entitySales.EntityTradeDetailsCompanyView.Count);
			AssertEquals(true, entityTradeDetail.IsDeleted);
		}

		public void TestCompanyFilter()
		{
			var product = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment);

			var org = Helper.NewOrgHeader();
			var opp1 = org.SalesOpportunities.AddNew();
			var opp2 = org.SalesOpportunities.AddNew();

			var sales1 = org.SalesCollection.AddNew();
			var sales2 = org.SalesCollection.AddNew();
			var sales3 = org.SalesCollection.AddNew();

			sales1.OW_MP_Product = product.PK;
			sales2.OW_MP_Product = product.PK;
			sales3.OW_MP_Product = product.PK;

			opp1.AssociatedTradeLanesPivots.AddPivotFor(sales2);
			opp2.AssociatedTradeLanesPivots.AddPivotFor(sales3);

			var anotherCompany = Factory.New<GlbCompany>();

			var orgSalesWrapper = EntitySalesWrapper.Get(sales1, org);
			orgSalesWrapper.SetReadOnlyIncludingChildren(true);
			AssertEquals(ZGuid.Empty, orgSalesWrapper.CompanyFilter);
			orgSalesWrapper.CompanyFilter = anotherCompany.PK;
			AssertEquals(anotherCompany.PK, orgSalesWrapper.CompanyFilter);
			AssertEquals(true, orgSalesWrapper.EntityDetailGroupings.ReadOnly);

			var opp1SalesWrapper = EntitySalesWrapper.Get(sales2, opp1);
			AssertEquals(ZGuid.Empty, opp1SalesWrapper.CompanyFilter);
			opp1SalesWrapper.CompanyFilter = anotherCompany.PK;
			AssertEquals(ZGuid.Empty, opp1SalesWrapper.CompanyFilter);

			var opp2SalesWrapper = EntitySalesWrapper.Get(sales3, opp2);
			AssertEquals(ZGuid.Empty, opp2SalesWrapper.CompanyFilter);
			opp2SalesWrapper.CompanyFilter = anotherCompany.PK;
			AssertEquals(ZGuid.Empty, opp2SalesWrapper.CompanyFilter);
		}

		public void TestReadOnly()
		{
			var product = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment);
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var tradeLane1 = org.SalesCollection.AddNew();
			tradeLane1.OW_MP_Product = product.PK;
			var tradeLane2 = org.SalesCollection.AddNew();
			tradeLane2.OW_MP_Product = product.PK;

			var opportunity1 = org.SalesOpportunities.AddNew();
			var pivot11 = opportunity1.AssociatedTradeLanesPivots.AddPivotFor(tradeLane1);
			var pivot12 = opportunity1.AssociatedTradeLanesPivots.AddPivotFor(tradeLane2);

			var opportunity2 = org.SalesOpportunities.AddNew();
			var pivot21 = opportunity2.AssociatedTradeLanesPivots.AddPivotFor(tradeLane1);

			Factory.Save();

			var sales1 = EntitySalesWrapper.Get(tradeLane1, opportunity1);
			AssertEquals(true, sales1.ReadOnly);

			var sales2 = EntitySalesWrapper.Get(tradeLane2, opportunity1);
			AssertEquals(false, sales2.ReadOnly);

			sales2.ReadOnly = true;
			AssertEquals(true, sales2.ReadOnly);
		}

		#region Overrides

		protected override BusinessObject GetNewBusinessObject()
		{
			var result = Factory.New<EntitySalesWrapper>();
			result.OW_IsCustomRevenue = true;
			return result;
		}

		protected override void SetupDataForSettingValueCallsRefreshBindingTestIfNeeded(ZPropertyInfo info)
		{
			base.SetupDataForSettingValueCallsRefreshBindingTestIfNeeded(info);

			if (info.Name == EntitySalesWrapper.Schema.TotalRevenueCurrencyCode)
			{
				info.BizObj[EntitySalesWrapper.Schema.OW_IsCustomRevenue] = ZBool.True;
			}
		}

		#endregion

		OrgSalesTestHelper Helper
		{
			get
			{
				if (helper == null)
				{
					helper = new OrgSalesTestHelper(Factory);
				}

				return helper;
			}
		}
		OrgSalesTestHelper helper;
	}
}
