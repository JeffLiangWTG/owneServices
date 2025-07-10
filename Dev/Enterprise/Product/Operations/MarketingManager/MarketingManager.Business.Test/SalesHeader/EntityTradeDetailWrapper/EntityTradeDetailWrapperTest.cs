using System.Linq;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MarketingManager.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.MasterFiles.Business.OrgTradeDetailLookups;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(EntityTradeDetailWrapper))]
	sealed class EntityTradeDetailWrapperTest : EnterpriseBusinessObjectTestCase
	{
		public void TestEntityCurrencyExchangeRate()
		{
			AssertEquals("Precondition", "AUD", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);

			var exchangeRate = Factory.New<RefExchangeRate>();
			exchangeRate.RE_RX_NKExCurrency = "USD";
			exchangeRate.RE_StartDate = new ZDateTime(2002, 2, 2);
			exchangeRate.RE_ExpiryDate = new ZDateTime(2002, 5, 5);
			exchangeRate.RE_SellRate = 0.5;
			exchangeRate.RE_ExRateType = Enterprise.Core.Constants.ExchangeRateTypes.Code.SellRate;

			var opportunity = Factory.New<OrgOpportunity>();
			opportunity.P8_DateForExchangeRate = new ZDateTime(2002, 2, 10);
			opportunity.P8_RX_NKEstimatedValueCurrency = "AUD";

			var sales = Factory.New<OrgSales>();
			var tradeDetail = sales.TradeDetails.AddNew();
			tradeDetail.CurrentProspectPeriod.PAS_RX_NKCurrency = "USD";
			var wrapper = EntityTradeDetailWrapper.Get(tradeDetail, opportunity);
			AssertEquals(0.5m, wrapper.EntityCurrencyExchangeRate);

			opportunity.P8_RX_NKEstimatedValueCurrency = "USD";
			AssertEquals(1m, wrapper.EntityCurrencyExchangeRate);

			tradeDetail.CurrentProspectPeriod.PAS_RX_NKCurrency = "AUD";
			AssertEquals(2.0m, wrapper.EntityCurrencyExchangeRate);
		}

		public void TestCurrentProspectPeriod()
		{
			var opportunity = Factory.New<OrgOpportunity>();

			var sales = Factory.New<OrgSales>();
			var tradeDetail = sales.TradeDetails.AddNew();
			var tradeDetail2 = sales.TradeDetails.AddNew();

			var period = Factory.New<OrgTradePeriod>();
			period.PAS_PA = tradeDetail.PK;
			period.PAS_Period = new ZDate(2018, 1, 1);
			var period2 = Factory.New<OrgTradePeriod>();
			period2.PAS_PA = tradeDetail2.PK;

			var wrapper = EntityTradeDetailWrapper.Get(tradeDetail, opportunity);
			AssertNotEquals(period.PK, wrapper.CurrentProspectPeriod.PK);
			AssertEquals("Period should be null for current prospect period", ZDate.Empty, wrapper.CurrentProspectPeriod.PAS_Period);

			var wrapper2 = EntityTradeDetailWrapper.Get(tradeDetail2, opportunity);
			AssertEquals(period2.PK, wrapper2.CurrentProspectPeriod.PK);
			AssertEquals("Period should be null for current prospect period", ZDate.Empty, wrapper2.CurrentProspectPeriod.PAS_Period);
		}

		public void TestTradeDetailAssociationPivotCollection()
		{
			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			var sales = Factory.NewWithValidTestData<OrgSales>();
			var tradeDetail = sales.TradeDetails.AddNew();
			AssertEquals(sales.PK, tradeDetail.PA_OW);
			var wrapper = EntityTradeDetailWrapper.Get(tradeDetail, opportunity);

			var opportunity2 = Factory.NewWithValidTestData<OrgOpportunity>();
			var tradeDetail2 = sales.TradeDetails.AddNew();
			AssertEquals(sales.PK, tradeDetail2.PA_OW);
			var wrapper2 = EntityTradeDetailWrapper.Get(tradeDetail2, opportunity2);

			OrgSalesValueAssociationPivot.AddPivotIfNotExist(opportunity, tradeDetail);
			OrgSalesValueAssociationPivot.AddPivotIfNotExist(opportunity2, tradeDetail2);

			Factory.Save();

			AssertEquals("There should be 1 association per opportunity for the first detail", 2, wrapper.SalesAssociationPivotCollectionCompanyView.Count);
			AssertNotNull(wrapper.SalesAssociationPivotCollectionCompanyView.FirstOrDefault(x => x.SVP_ActivityId == opportunity.PK));
			AssertNotNull(wrapper.SalesAssociationPivotCollectionCompanyView.FirstOrDefault(x => x.SVP_ActivityId == opportunity2.PK));
			AssertEquals("There should be 1 association per opportunity for the first detail", 2, wrapper.SalesAssociationPivotCollectionGlobal.Count);
			AssertNotNull(wrapper.SalesAssociationPivotCollectionGlobal.FirstOrDefault(x => x.SVP_ActivityId == opportunity.PK));
			AssertNotNull(wrapper.SalesAssociationPivotCollectionGlobal.FirstOrDefault(x => x.SVP_ActivityId == opportunity2.PK));

			AssertEquals("There should be 1 association per opportunity for the second detail", 2, wrapper2.SalesAssociationPivotCollectionCompanyView.Count);
			AssertNotNull(wrapper2.SalesAssociationPivotCollectionCompanyView.FirstOrDefault(x => x.SVP_ActivityId == opportunity.PK));
			AssertNotNull(wrapper2.SalesAssociationPivotCollectionCompanyView.FirstOrDefault(x => x.SVP_ActivityId == opportunity2.PK));
			AssertEquals("There should be 1 association per opportunity for the second detail", 2, wrapper2.SalesAssociationPivotCollectionGlobal.Count);
			AssertNotNull(wrapper2.SalesAssociationPivotCollectionGlobal.FirstOrDefault(x => x.SVP_ActivityId == opportunity.PK));
			AssertNotNull(wrapper2.SalesAssociationPivotCollectionGlobal.FirstOrDefault(x => x.SVP_ActivityId == opportunity2.PK));
		}

		public void TestTradeDetailAssociationPivotCollection_WarehouseDifferentParts()
		{
			var warehouseProduct = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.Warehouse);
			var warehouse1 = (IWhsWarehouse)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IWhsWarehouse)));
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var opportunity = org.SalesOpportunities.AddNew();
			var sales = org.SalesCollection.AddNew();

			sales.OW_MP_Product = warehouseProduct.PK;
			sales.OW_Service = OrgSalesWarehouseServiceTypesList.Codes.Orders;
			sales.OW_WW = warehouse1.PK;
			sales.OW_OriginID = ViewLocationHelper.GetLocationFromString(Factory, "AUSYD", RefUNLOCOSchema.Constants.Prefix).PK;

			var part1 = Factory.NewWithValidTestData<OrgSupplierPart>();
			part1.OP_PartNum = "PART1";
			var part2 = Factory.NewWithValidTestData<OrgSupplierPart>();
			part2.OP_PartNum = "BART2";

			var tradeDetail = sales.TradeDetails.AddNew();
			tradeDetail.PA_OP = part1.PK;
			var wrapper = EntityTradeDetailWrapper.Get(tradeDetail, opportunity);

			var opportunity2 = Factory.NewWithValidTestData<OrgOpportunity>();
			var tradeDetail2 = sales.TradeDetails.AddNew();
			tradeDetail2.PA_OP = part1.PK;
			var wrapper2 = EntityTradeDetailWrapper.Get(tradeDetail2, opportunity2);

			var opportunity3 = Factory.NewWithValidTestData<OrgOpportunity>();
			var tradeDetail3 = sales.TradeDetails.AddNew();
			tradeDetail3.PA_OP = part2.PK;
			var wrapper3 = EntityTradeDetailWrapper.Get(tradeDetail3, opportunity3);

			OrgSalesValueAssociationPivot.AddPivotIfNotExist(opportunity, tradeDetail);
			OrgSalesValueAssociationPivot.AddPivotIfNotExist(opportunity2, tradeDetail2);
			OrgSalesValueAssociationPivot.AddPivotIfNotExist(opportunity3, tradeDetail3);

			Factory.Save();

			AssertEquals("The first and second details have the same part, so they should be in the first collection", 2, wrapper.SalesAssociationPivotCollectionCompanyView.Count);
			AssertNotNull(wrapper.SalesAssociationPivotCollectionCompanyView.FirstOrDefault(x => x.SVP_ActivityId == opportunity.PK));
			AssertNotNull(wrapper.SalesAssociationPivotCollectionCompanyView.FirstOrDefault(x => x.SVP_ActivityId == opportunity2.PK));
			AssertEquals("The first and second details have the same part, so they should be in the first collection", 2, wrapper.SalesAssociationPivotCollectionGlobal.Count);
			AssertNotNull(wrapper.SalesAssociationPivotCollectionGlobal.FirstOrDefault(x => x.SVP_ActivityId == opportunity.PK));
			AssertNotNull(wrapper.SalesAssociationPivotCollectionGlobal.FirstOrDefault(x => x.SVP_ActivityId == opportunity2.PK));

			AssertEquals("The first and second details have the same part, so they should be in the second collection", 2, wrapper2.SalesAssociationPivotCollectionCompanyView.Count);
			AssertNotNull(wrapper2.SalesAssociationPivotCollectionCompanyView.FirstOrDefault(x => x.SVP_ActivityId == opportunity.PK));
			AssertNotNull(wrapper2.SalesAssociationPivotCollectionCompanyView.FirstOrDefault(x => x.SVP_ActivityId == opportunity2.PK));
			AssertEquals("The first and second details have the same part, so they should be in the second collection", 2, wrapper2.SalesAssociationPivotCollectionGlobal.Count);
			AssertNotNull(wrapper2.SalesAssociationPivotCollectionGlobal.FirstOrDefault(x => x.SVP_ActivityId == opportunity.PK));
			AssertNotNull(wrapper2.SalesAssociationPivotCollectionGlobal.FirstOrDefault(x => x.SVP_ActivityId == opportunity2.PK));

			AssertEquals("The third detail has its own part", 1, wrapper3.SalesAssociationPivotCollectionCompanyView.Count);
			AssertNotNull(wrapper3.SalesAssociationPivotCollectionCompanyView.FirstOrDefault(x => x.SVP_ActivityId == opportunity3.PK));
			AssertEquals("The third detail has its own part", 1, wrapper3.SalesAssociationPivotCollectionGlobal.Count);
			AssertNotNull(wrapper3.SalesAssociationPivotCollectionGlobal.FirstOrDefault(x => x.SVP_ActivityId == opportunity3.PK));
		}

		public void TestTradeDetailAssociationPivotCollection_WarehouseNoParts()
		{
			var warehouseProduct = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.Warehouse);
			var warehouse1 = (IWhsWarehouse)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IWhsWarehouse)));
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var opportunity = org.SalesOpportunities.AddNew();
			var sales = org.SalesCollection.AddNew();

			sales.OW_MP_Product = warehouseProduct.PK;
			sales.OW_Service = OrgSalesWarehouseServiceTypesList.Codes.Orders;
			sales.OW_WW = warehouse1.PK;
			sales.OW_OriginID = ViewLocationHelper.GetLocationFromString(Factory, "AUSYD", RefUNLOCOSchema.Constants.Prefix).PK;

			var part1 = Factory.NewWithValidTestData<OrgSupplierPart>();
			part1.OP_PartNum = "PART1";

			var tradeDetail = sales.TradeDetails.AddNew();
			tradeDetail.PA_OP = part1.PK;
			var wrapper = EntityTradeDetailWrapper.Get(tradeDetail, opportunity);

			var opportunity2 = Factory.NewWithValidTestData<OrgOpportunity>();
			var tradeDetail2 = sales.TradeDetails.AddNew();
			var wrapper2 = EntityTradeDetailWrapper.Get(tradeDetail2, opportunity2);

			var opportunity3 = Factory.NewWithValidTestData<OrgOpportunity>();
			var tradeDetail3 = sales.TradeDetails.AddNew();
			var wrapper3 = EntityTradeDetailWrapper.Get(tradeDetail3, opportunity3);

			OrgSalesValueAssociationPivot.AddPivotIfNotExist(opportunity, tradeDetail);
			OrgSalesValueAssociationPivot.AddPivotIfNotExist(opportunity2, tradeDetail2);
			OrgSalesValueAssociationPivot.AddPivotIfNotExist(opportunity3, tradeDetail3);

			Factory.Save();

			AssertEquals("The first detail is the only one with a part so it should be the only inclusion in this collection", 1, wrapper.SalesAssociationPivotCollectionCompanyView.Count);
			AssertNotNull(wrapper.SalesAssociationPivotCollectionCompanyView.FirstOrDefault(x => x.SVP_ActivityId == opportunity.PK));
			AssertEquals("The first detail is the only one with a part so it should be the only inclusion in this collection", 1, wrapper.SalesAssociationPivotCollectionGlobal.Count);
			AssertNotNull(wrapper.SalesAssociationPivotCollectionGlobal.FirstOrDefault(x => x.SVP_ActivityId == opportunity.PK));

			AssertEquals("The second and third details have no part, so they should be in the second collection", 2, wrapper2.SalesAssociationPivotCollectionCompanyView.Count);
			AssertNotNull(wrapper2.SalesAssociationPivotCollectionCompanyView.FirstOrDefault(x => x.SVP_ActivityId == opportunity2.PK));
			AssertNotNull(wrapper2.SalesAssociationPivotCollectionCompanyView.FirstOrDefault(x => x.SVP_ActivityId == opportunity3.PK));
			AssertEquals("The second and third details have no part, so they should be in the second collection", 2, wrapper2.SalesAssociationPivotCollectionGlobal.Count);
			AssertNotNull(wrapper2.SalesAssociationPivotCollectionGlobal.FirstOrDefault(x => x.SVP_ActivityId == opportunity2.PK));
			AssertNotNull(wrapper2.SalesAssociationPivotCollectionGlobal.FirstOrDefault(x => x.SVP_ActivityId == opportunity3.PK));

			AssertEquals("The second and third details have no part, so they should be in the third collection", 2, wrapper3.SalesAssociationPivotCollectionCompanyView.Count);
			AssertNotNull(wrapper3.SalesAssociationPivotCollectionCompanyView.FirstOrDefault(x => x.SVP_ActivityId == opportunity2.PK));
			AssertNotNull(wrapper3.SalesAssociationPivotCollectionCompanyView.FirstOrDefault(x => x.SVP_ActivityId == opportunity3.PK));
			AssertEquals("The second and third details have no part, so they should be in the third collection", 2, wrapper3.SalesAssociationPivotCollectionGlobal.Count);
			AssertNotNull(wrapper3.SalesAssociationPivotCollectionGlobal.FirstOrDefault(x => x.SVP_ActivityId == opportunity2.PK));
			AssertNotNull(wrapper3.SalesAssociationPivotCollectionGlobal.FirstOrDefault(x => x.SVP_ActivityId == opportunity3.PK));
		}

		public void TestTradeDetailAssociationPivotCollection_WarehouseDifferentWarehouses()
		{
			var warehouseProduct = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.Warehouse);
			var warehouse1 = (IWhsWarehouse)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IWhsWarehouse)));
			var warehouse2 = (IWhsWarehouse)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IWhsWarehouse)));
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var opportunity = org.SalesOpportunities.AddNew();
			var sales = org.SalesCollection.AddNew();
			var sales2 = org.SalesCollection.AddNew();

			sales.OW_MP_Product = warehouseProduct.PK;
			sales.OW_Service = OrgSalesWarehouseServiceTypesList.Codes.Orders;
			sales.OW_WW = warehouse1.PK;
			sales.OW_OriginID = ViewLocationHelper.GetLocationFromString(Factory, "AUSYD", RefUNLOCOSchema.Constants.Prefix).PK;
			sales2.OW_MP_Product = warehouseProduct.PK;
			sales2.OW_Service = OrgSalesWarehouseServiceTypesList.Codes.Receipts;
			sales2.OW_WW = warehouse2.PK;
			sales2.OW_OriginID = ViewLocationHelper.GetLocationFromString(Factory, "AUMEL", RefUNLOCOSchema.Constants.Prefix).PK;

			var part1 = Factory.NewWithValidTestData<OrgSupplierPart>();
			part1.OP_PartNum = "PART1";

			var tradeDetail = sales.TradeDetails.AddNew();
			tradeDetail.PA_OP = part1.PK;
			var wrapper = EntityTradeDetailWrapper.Get(tradeDetail, opportunity);

			var opportunity2 = org.SalesOpportunities.AddNew();
			var tradeDetail2 = sales2.TradeDetails.AddNew();
			tradeDetail2.PA_OP = part1.PK;
			var wrapper2 = EntityTradeDetailWrapper.Get(tradeDetail2, opportunity2);

			OrgSalesValueAssociationPivot.AddPivotIfNotExist(opportunity, tradeDetail);
			OrgSalesValueAssociationPivot.AddPivotIfNotExist(opportunity2, tradeDetail2);

			Factory.Save();

			AssertEquals("The details are from different warehouses", 1, wrapper.SalesAssociationPivotCollectionCompanyView.Count);
			AssertNotNull(wrapper.SalesAssociationPivotCollectionCompanyView.FirstOrDefault(x => x.SVP_ActivityId == opportunity.PK));

			AssertEquals("The details are from different warehouses", 1, wrapper2.SalesAssociationPivotCollectionCompanyView.Count);
			AssertNotNull(wrapper2.SalesAssociationPivotCollectionCompanyView.FirstOrDefault(x => x.SVP_ActivityId == opportunity2.PK));
		}

		public void TestTradeDetailAssociationPivotCollection_LinerAndAgency()
		{
			var linerAgencyProduct = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.LinerAgency);
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var opportunity1 = org.SalesOpportunities.AddNew();
			var opportunity2 = org.SalesOpportunities.AddNew();
			var opportunity3 = org.SalesOpportunities.AddNew();
			var sales = org.SalesCollection.AddNew();

			sales.OW_MP_Product = linerAgencyProduct.PK;
			sales.OW_OriginID = ViewLocationHelper.GetLocationFromString(Factory, "AUSYD", RefUNLOCOSchema.Constants.Prefix).PK;

			var tradeDetail1 = sales.TradeDetails.AddNew();
			tradeDetail1.PA_TradeType = Constants.ContainerModes.FCL;
			tradeDetail1.PA_TradeMode = LinerAgencyTradeModes.BillOfLading;
			var wrapper1 = EntityTradeDetailWrapper.Get(tradeDetail1, opportunity1);

			var tradeDetail2 = sales.TradeDetails.AddNew();
			tradeDetail2.PA_TradeType = Constants.ContainerModes.FCL;
			tradeDetail2.PA_TradeMode = LinerAgencyTradeModes.BillOfLading;
			var wrapper2 = EntityTradeDetailWrapper.Get(tradeDetail2, opportunity2);

			var tradeDetail3 = sales.TradeDetails.AddNew();
			tradeDetail3.PA_TradeType = Constants.ContainerModes.RollOnRollOff;
			tradeDetail3.PA_TradeMode = LinerAgencyTradeModes.BillOfLading;
			var wrapper3 = EntityTradeDetailWrapper.Get(tradeDetail3, opportunity3);

			OrgSalesValueAssociationPivot.AddPivotIfNotExist(opportunity1, tradeDetail1);
			OrgSalesValueAssociationPivot.AddPivotIfNotExist(opportunity2, tradeDetail2);
			OrgSalesValueAssociationPivot.AddPivotIfNotExist(opportunity3, tradeDetail3);

			Factory.Save();

			AssertEquals("There should be two pivots as Trade Mode and Trade Type are the same", 2, wrapper1.SalesAssociationPivotCollectionGlobal.Count);
			AssertNotNull(wrapper1.SalesAssociationPivotCollectionGlobal.FirstOrDefault(x => x.SVP_ActivityId == opportunity1.PK));
			AssertNotNull(wrapper1.SalesAssociationPivotCollectionGlobal.FirstOrDefault(x => x.SVP_ActivityId == opportunity2.PK));

			AssertEquals("There should be one pivot", 1, wrapper3.SalesAssociationPivotCollectionCompanyView.Count);
			AssertNotNull(wrapper3.SalesAssociationPivotCollectionCompanyView.FirstOrDefault(x => x.SVP_ActivityId == opportunity3.PK));
		}
	}
}
