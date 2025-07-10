using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MarketingManager.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(SalesMatching))]
	sealed class SalesMatchingTest : NonPersistentBusinessObjectTestCase
	{
		public void TestFindMatch_Sales()
		{
			var ausyd = ViewLocationHelper.GetLocationFromString(Factory, "AUSYD", RefUNLOCOSchema.Constants.Prefix);
			var aumel = ViewLocationHelper.GetLocationFromString(Factory, "AUMEL", RefUNLOCOSchema.Constants.Prefix);
			var nzakl = ViewLocationHelper.GetLocationFromString(Factory, "NZAKL", RefUNLOCOSchema.Constants.Prefix);
			var au = ViewLocationHelper.GetLocationFromString(Factory, "AU", RefCountrySchema.Constants.Prefix);
			var nz = ViewLocationHelper.GetLocationFromString(Factory, "NZ", RefCountrySchema.Constants.Prefix);

			var org = Factory.NewWithValidTestData<OrgHeader>();

			var forwardingProduct = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment);
			var customsProduct = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.CustomsBrokerage);

			var sales1 = org.SalesCollection.AddNew();
			sales1.OW_MP_Product = forwardingProduct.PK;
			sales1.OW_OriginID = ausyd.PK;
			sales1.OW_DestinationID = nzakl.PK;

			var details11 = sales1.TradeDetails.AddNew();
			details11.PA_TradeMode = Constants.TransportModes.Sea;
			details11.PA_TradeType = Constants.ContainerModes.FCL;
			details11.ProspectDetail.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.Monthly;
			details11.CurrentProspectPeriod.PAS_EstimatedProfit = 100m;
			details11.CurrentProspectPeriod.PAS_RX_NKCurrency = "AUD";

			var details12 = sales1.TradeDetails.AddNew();
			details12.PA_TradeMode = Constants.TransportModes.Air;
			details12.PA_TradeType = Constants.ContainerModes.ULD;
			details12.ProspectDetail.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.Yearly;
			details12.CurrentProspectPeriod.PAS_EstimatedProfit = 250m;
			details12.CurrentProspectPeriod.PAS_RX_NKCurrency = "USD";

			var sales2 = org.SalesCollection.AddNew();
			sales2.OW_MP_Product = forwardingProduct.PK;
			sales2.OW_OriginID = aumel.PK;
			sales2.OW_DestinationID = nzakl.PK;

			var details21 = sales2.TradeDetails.AddNew();
			details21.PA_TradeMode = Constants.TransportModes.Sea;
			details21.PA_TradeType = Constants.ContainerModes.FCL;
			details21.ProspectDetail.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.Yearly;
			details21.CurrentProspectPeriod.PAS_EstimatedProfit = 200m;
			details21.CurrentProspectPeriod.PAS_RX_NKCurrency = "AUD";

			var sales3 = org.SalesCollection.AddNew();
			sales3.OW_MP_Product = forwardingProduct.PK;
			sales3.OW_OriginID = au.PK;
			sales3.OW_DestinationID = nz.PK;
			sales3.OW_IsCustomRevenue = true;
			sales3.OW_AnnualRevenue = 400m;
			sales3.OW_RX_NKRevenueCurrency = "AUD";

			var sales4 = org.SalesCollection.AddNew();
			sales4.OW_MP_Product = forwardingProduct.PK;
			sales4.OW_OriginID = ausyd.PK;
			sales4.OW_DestinationID = aumel.PK;

			var sales5 = org.SalesCollection.AddNew();
			sales5.OW_MP_Product = customsProduct.PK;
			sales5.OW_OriginID = ausyd.PK;
			sales5.OW_DestinationID = nzakl.PK;

			var sales6 = org.SalesCollection.AddNew();
			sales6.OW_MP_Product = customsProduct.PK;
			sales6.OW_OriginID = au.PK;
			sales6.OW_DestinationID = ZGuid.Empty;

			var sales7 = org.SalesCollection.AddNew();
			sales7.OW_MP_Product = customsProduct.PK;
			sales7.OW_OriginID = ZGuid.Empty;
			sales7.OW_DestinationID = nz.PK;

			var sales8 = org.SalesCollection.AddNew();
			sales8.OW_MP_Product = forwardingProduct.PK;
			sales8.OW_OriginID = au.PK;
			sales8.OW_DestinationID = nz.PK;
			sales8.OW_IsTraded = true;
			sales8.OW_RX_NKRevenueCurrency = "AUD";

			var salesHeader = new SalesHeader(org, forwardingProduct);
			var newSales = salesHeader.EntitySalesCollectionProductView.AddNew();
			newSales.OW_MP_Product = forwardingProduct.PK;
			newSales.OW_OriginID = au.PK;
			newSales.OW_DestinationID = nz.PK;

			var matching = new SalesMatching(org, newSales, null, forwardingProduct.SalesMatchingOptions);
			AssertEquals(4, matching.MatchedSalesCollection.Count);

			AssertMatchData(matching.MatchedSalesCollection[0], "AU", "NZ", "", "", null, null, "AUD", 400m);
			AssertMatchData(matching.MatchedSalesCollection[1], "AUMEL", "NZAKL", "", "", "SEA", "FCL", "AUD", 200m);
			AssertMatchData(matching.MatchedSalesCollection[2], "AUSYD", "NZAKL", "", "", "AIR", "ULD", "USD", 250m);
			AssertMatchData(matching.MatchedSalesCollection[3], "AUSYD", "NZAKL", "", "", "SEA", "FCL", "AUD", 100m * 12);
		}

		public void TestFindMatch_TradeDetail()
		{
			var ausyd = ViewLocationHelper.GetLocationFromString(Factory, "AUSYD", RefUNLOCOSchema.Constants.Prefix);
			var aumel = ViewLocationHelper.GetLocationFromString(Factory, "AUMEL", RefUNLOCOSchema.Constants.Prefix);

			var org = Factory.NewWithValidTestData<OrgHeader>();

			var warehouseProduct = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.Warehouse);
			var warehouse1 = (BusinessObject)Factory.New<IWhsWarehouse>();
			warehouse1.FillWithValidTestData();
			var warehouse2 = (BusinessObject)Factory.New<IWhsWarehouse>();
			warehouse2.FillWithValidTestData();
			var supplierPartA = Factory.NewWithValidTestData<OrgSupplierPart>();
			var supplierPartB = Factory.NewWithValidTestData<OrgSupplierPart>();

			var salesHeaderCollection = (SalesHeaderCollection)org.ActualAndProspectiveSalesHeaderCollection;
			var salesHeader = salesHeaderCollection.AddNew(warehouseProduct);
			var sales1 = salesHeader.EntitySalesCollectionProductView.AddNew();
			sales1.OW_WW = warehouse1.PK;
			var tradeDetail1A = sales1.EntityTradeDetailsCollection.AddNew();
			tradeDetail1A.PA_OP = supplierPartA.PK;
			var tradeDetail1B = sales1.EntityTradeDetailsCollection.AddNew();
			tradeDetail1B.PA_OP = supplierPartB.PK;

			var sales2 = salesHeader.EntitySalesCollectionProductView.AddNew();
			sales2.OW_WW = warehouse2.PK;
			var tradeDetail2A = sales2.EntityTradeDetailsCollection.AddNew();
			tradeDetail2A.PA_OP = supplierPartA.PK;

			var newSales1 = salesHeader.EntitySalesCollectionProductView.AddNew();
			newSales1.OW_WW = warehouse1.PK;
			var newTradeDetail1A = newSales1.EntityTradeDetailsCollection.AddNew();
			newTradeDetail1A.PA_OP = supplierPartA.PK;

			Factory.Save();
			org.SalesCollection.Load();

			var matching = new SalesMatching(org, null, newTradeDetail1A, warehouseProduct.SalesMatchingOptions);
			AssertEquals(1, matching.MatchedSalesCollection.Count);
			AssertEquals(sales1.PK, matching.MatchedSalesCollection[0].Sales.PK);
			AssertEquals(tradeDetail1A.PK, matching.MatchedSalesCollection[0].TradeDetail.PK);
		}

		public void TestReplaceSalesWithClonedExisting_OrgSales()
		{
			var product = Factory.New<OrgSalesProduct>();
			var org = Factory.New<OrgHeader>();

			var consignee = Factory.New<OrgHeader>();

			var opportunity1 = org.SalesOpportunities.AddNew();
			var headerCollection1 = (SalesHeaderCollection)opportunity1.ProspectiveSalesHeaderCollection;
			var header1 = headerCollection1.AddNew(product);
			var opportunitySales1 = header1.EntitySalesCollectionProductView.AddNew();
			opportunitySales1.OW_OH_Buyer = consignee.PK;
			var opportunityDetail1a = opportunitySales1.EntityTradeDetailsCollection.AddNew();
			opportunityDetail1a.PA_TradeMode = "SEA";
			opportunityDetail1a.CurrentProspectPeriod.PAS_RepeatsMnth = 15m;
			var opportunityDetail1b = opportunitySales1.EntityTradeDetailsCollection.AddNew();
			opportunityDetail1b.PA_TradeMode = "AIR";
			opportunityDetail1b.ProspectDetail.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.Yearly;

			AssertContainsExactElementsInAnyOrder(new[] { opportunity1 }, opportunitySales1.SalesAssociationPivotCollectionCompanyView.Select(x => x.AssociatedEntity));

			var opportunity2 = org.SalesOpportunities.AddNew();
			var headerCollection2 = (SalesHeaderCollection)opportunity2.ProspectiveSalesHeaderCollection;
			var header2 = headerCollection2.AddNew(product);
			var opportunitySales2 = header2.EntitySalesCollectionProductView.AddNew();

			var matching = new SalesMatching(org, opportunitySales2, null, product.SalesMatchingOptions);
			matching.ReplaceWithClonedExisting(new SalesMatchingData(opportunitySales1, null), false);

			AssertContainsExactElementsInAnyOrder("opportunitySales1 should be associated to opportunity1", new[] { opportunity1, opportunity2 }, opportunitySales1.SalesAssociationPivotCollectionCompanyView.Select(x => x.AssociatedEntity));
			AssertContainsExactElementsInAnyOrder("opportunitySales2 should be associated to opportunity2", new[] { opportunity2 }, opportunitySales2.SalesAssociationPivotCollectionCompanyView.Select(x => x.AssociatedEntity));

			AssertEquals(consignee.PK, opportunitySales2.OW_OH_Buyer);
			AssertEquals(2, opportunitySales2.EntityTradeDetailsCollection.Count);
			var opportunityDetail2a = opportunitySales2.EntityTradeDetailsCollection.Cast<EntityTradeDetailWrapper>().Single(x => x.PA_TradeMode == "SEA");
			AssertEquals(15m, opportunityDetail2a.CurrentProspectPeriod.PAS_RepeatsMnth);
			var opportunityDetail2b = opportunitySales2.EntityTradeDetailsCollection.Cast<EntityTradeDetailWrapper>().Single(x => x.PA_TradeMode == "AIR");
			AssertEquals(OrgTradeProspectRecurrenceTypeList.Codes.Yearly, opportunityDetail2b.ProspectDetail.PAP_RecurrenceType);
		}

		public void TestReplaceSalesWithClonedExisting_OrgSalesForMatching_ButTradeDetailSelected()
		{
			var product = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.Warehouse);
			var org = Factory.New<OrgHeader>();

			var opportunity1 = org.SalesOpportunities.AddNew();
			var headerCollection1 = (SalesHeaderCollection)opportunity1.ProspectiveSalesHeaderCollection;
			var header1 = headerCollection1.AddNew(product);
			var sales1 = header1.EntitySalesCollectionProductView.AddNew();
			var detail1 = sales1.EntityTradeDetailsCollection.AddNew();
			detail1.PA_TradeMode = "SEA";
			detail1.CurrentProspectPeriod.PAS_RepeatsMnth = 15m;
			detail1.ProspectDetail.PAP_IncoTradeTerm = "FOB";

			var opportunity2 = org.SalesOpportunities.AddNew();
			var headerCollection2 = (SalesHeaderCollection)opportunity2.ProspectiveSalesHeaderCollection;
			var header2 = headerCollection2.AddNew(product);
			var sales2 = header2.EntitySalesCollectionProductView.AddNew();

			var matching = new SalesMatching(org, sales2, null, product.SalesMatchingOptions);
			matching.ReplaceWithClonedExisting(new SalesMatchingData(sales1, detail1), false);

			var detail2 = sales2.EntityTradeDetailsCollection[0];
			AssertEquals(15m, detail2.CurrentProspectPeriod.PAS_RepeatsMnth);
			AssertEquals("FOB", detail2.ProspectDetail.PAP_IncoTradeTerm);
		}

		public void TestReplaceSalesWithClonedExisting_TradeDetail()
		{
			var product = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.Warehouse);
			var org = Factory.New<OrgHeader>();

			var opportunity1 = org.SalesOpportunities.AddNew();
			var headerCollection1 = (SalesHeaderCollection)opportunity1.ProspectiveSalesHeaderCollection;
			var header1 = headerCollection1.AddNew(product);
			var opportunitySales1 = header1.EntitySalesCollectionProductView.AddNew();
			var opportunityDetail1 = opportunitySales1.EntityTradeDetailsCollection.AddNew();
			opportunityDetail1.PA_TradeMode = "SEA";
			opportunityDetail1.CurrentProspectPeriod.PAS_RepeatsMnth = 15m;
			opportunityDetail1.ProspectDetail.PAP_IncoTradeTerm = "FOB";

			AssertContainsExactElementsInAnyOrder(new[] { opportunity1 }, opportunityDetail1.SalesAssociationPivotCollectionCompanyView.Select(x => x.AssociatedEntity));

			var opportunity2 = org.SalesOpportunities.AddNew();
			var headerCollection2 = (SalesHeaderCollection)opportunity2.ProspectiveSalesHeaderCollection;
			var header2 = headerCollection2.AddNew(product);
			var opportunitySales2 = header2.EntitySalesCollectionProductView.AddNew();
			var opportunityDetail2a = opportunitySales2.EntityTradeDetailsCollection.AddNew();
			var opportunityDetail2b = opportunitySales2.EntityTradeDetailsCollection.AddNew();
			opportunityDetail2a.PA_TradeMode = "SEA";
			opportunityDetail2b.PA_TradeMode = "SEA";

			var matching = new SalesMatching(org, null, opportunityDetail2a, product.SalesMatchingOptions);
			matching.ReplaceWithClonedExisting(new SalesMatchingData(opportunitySales1, opportunityDetail1), false);

			AssertContainsExactElementsInAnyOrder("Should be associated to sales1 and sales2", new[] { opportunitySales1, opportunitySales2 }, header2.EntitySalesCollectionProductView);
			AssertEquals(15m, opportunityDetail2a.CurrentProspectPeriod.PAS_RepeatsMnth);
			AssertEquals("FOB", opportunityDetail2a.ProspectDetail.PAP_IncoTradeTerm);
			AssertEquals(0m, opportunityDetail2b.CurrentProspectPeriod.PAS_RepeatsMnth);
			AssertEquals(ZString.Empty, opportunityDetail2b.ProspectDetail.PAP_IncoTradeTerm);

			matching = new SalesMatching(org, null, opportunityDetail2b, product.SalesMatchingOptions);
			matching.ReplaceWithClonedExisting(new SalesMatchingData(opportunitySales1, opportunityDetail1), false);

			AssertContainsExactElementsInAnyOrder("Should be associated to sales1", new[] { opportunitySales1 }, header2.EntitySalesCollectionProductView);
			AssertEquals(15m, opportunityDetail2b.CurrentProspectPeriod.PAS_RepeatsMnth);
			AssertEquals("FOB", opportunityDetail2b.ProspectDetail.PAP_IncoTradeTerm);
		}

		public void TestReplaceSalesWithClonedExisting_RemoveEmptyDefaultTradeDetail()
		{
			var product = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment);
			var org = Factory.New<OrgHeader>();

			var opportunity1 = org.SalesOpportunities.AddNew();
			var headerCollection1 = (SalesHeaderCollection)opportunity1.ProspectiveSalesHeaderCollection;
			var header1 = headerCollection1.AddNew(product);
			var opportunitySales1 = header1.EntitySalesCollectionProductView.AddNew();
			var opportunityDetail1 = opportunitySales1.EntityTradeDetailsCollection.AddNew();
			opportunityDetail1.PA_TradeMode = "SEA";
			var emptyDefaultTradeDetail1 = opportunitySales1.EntityTradeDetailsCollection.AddNew();

			AssertContainsExactElementsInAnyOrder(new[] { opportunity1 }, opportunityDetail1.SalesAssociationPivotCollectionCompanyView.Select(x => x.AssociatedEntity));

			var opportunity2 = org.SalesOpportunities.AddNew();
			var headerCollection2 = (SalesHeaderCollection)opportunity2.ProspectiveSalesHeaderCollection;
			var header2 = headerCollection2.AddNew(product);
			var opportunitySales2 = header2.EntitySalesCollectionProductView.AddNew();
			var opportunityDetail2a = opportunitySales2.EntityTradeDetailsCollection.AddNew();
			var opportunityDetail2b = opportunitySales2.EntityTradeDetailsCollection.AddNew();
			opportunityDetail2a.PA_TradeMode = "SEA";
			opportunityDetail2b.PA_TradeMode = "SEA";
			var emptyDefaultTradeDetail2 = opportunitySales2.EntityTradeDetailsCollection.AddNew();

			var matching = new SalesMatching(org, null, opportunityDetail2a, product.SalesMatchingOptions);
			matching.ReplaceWithClonedExisting(new SalesMatchingData(opportunitySales1, opportunityDetail1), false);

			matching = new SalesMatching(org, null, opportunityDetail2b, product.SalesMatchingOptions);
			matching.ReplaceWithClonedExisting(new SalesMatchingData(opportunitySales1, opportunityDetail1), false);

			AssertEquals(true, emptyDefaultTradeDetail1.IsDeleted);
			AssertEquals(false, emptyDefaultTradeDetail2.IsDeleted);
		}

		public void TestReplaceSalesWithClonedExisting_DoesNotRemoveWarehouseTradeDetail()
		{
			var product = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.Warehouse);
			var org = Factory.New<OrgHeader>();

			var opportunity1 = org.SalesOpportunities.AddNew();
			var headerCollection1 = (SalesHeaderCollection)opportunity1.ProspectiveSalesHeaderCollection;
			var header1 = headerCollection1.AddNew(product);
			var opportunitySales1 = header1.EntitySalesCollectionProductView.AddNew();
			var opportunityDetail1 = opportunitySales1.EntityTradeDetailsCollection.AddNew();

			AssertContainsExactElementsInAnyOrder(new[] { opportunity1 }, opportunityDetail1.SalesAssociationPivotCollectionCompanyView.Select(x => x.AssociatedEntity));

			var opportunity2 = org.SalesOpportunities.AddNew();
			var headerCollection2 = (SalesHeaderCollection)opportunity2.ProspectiveSalesHeaderCollection;
			var header2 = headerCollection2.AddNew(product);
			var opportunitySales2 = header2.EntitySalesCollectionProductView.AddNew();
			var opportunityDetail2A = opportunitySales2.EntityTradeDetailsCollection.AddNew();

			var matching = new SalesMatching(org, null, opportunityDetail2A, product.SalesMatchingOptions);
			matching.ReplaceWithClonedExisting(new SalesMatchingData(opportunitySales1, opportunityDetail1), false);

			AssertEquals(false, opportunityDetail1.IsDeleted);
		}

		public void TestUseExisting()
		{
			var au = ViewLocationHelper.GetLocationFromString(Factory, "AU", RefCountrySchema.Constants.Prefix);
			var nz = ViewLocationHelper.GetLocationFromString(Factory, "NZ", RefCountrySchema.Constants.Prefix);

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();

			var forwardingProduct = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment);

			var org1Sales = (SalesHeaderCollection)org1.ProspectiveSalesHeaderCollection;
			var org1header = org1Sales.AddNew(forwardingProduct);
			var org1Sale1 = org1header.EntitySalesCollectionProductView.AddNew();
			org1Sale1.OW_OriginID = au.PK;
			org1Sale1.OW_DestinationID = nz.PK;
			org1Sale1.OW_IsCustomRevenue = true;
			org1Sale1.OW_AnnualRevenue = 150000m;

			var org1Detail1 = org1Sale1.EntityTradeDetailsCollection.AddNew();
			org1Detail1.PA_TradeMode = Constants.TransportModes.Sea;
			org1Detail1.PA_TradeType = Constants.ContainerModes.FCL;
			org1Detail1.EstimatedProfit = 47;
			org1Detail1.CurrencyCode = "AUD";
			org1Detail1.ProspectDetail.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.Monthly;

			var opp1 = org2.SalesOpportunities.AddNew();
			var opp1Sales = (SalesHeaderCollection)opp1.ProspectiveSalesHeaderCollection;
			var opp1Header = opp1Sales.AddNew(forwardingProduct);
			var opp1Sale1 = opp1Header.EntitySalesCollectionProductView.AddNew();
			opp1Sale1.OW_OriginID = au.PK;
			opp1Sale1.OW_DestinationID = nz.PK;
			opp1Sale1.OW_IsCustomRevenue = true;
			opp1Sale1.OW_AnnualRevenue = 6000m;

			var opp1Detail1 = opp1Sale1.EntityTradeDetailsCollection.AddNew();
			opp1Detail1.PA_TradeMode = Constants.TransportModes.Sea;
			opp1Detail1.PA_TradeType = Constants.ContainerModes.FCL;
			opp1Detail1.EstimatedProfit = 99m;
			opp1Detail1.CurrencyCode = "AUD";
			opp1Detail1.ProspectDetail.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.Monthly;

			Factory.Save();

			opp1.P8_OH = org1.PK;
			Factory.Save();

			var headers = (SalesHeaderCollection)opp1.ProspectiveSalesHeaderCollection;
			headers.Rebuild();

			var matching = new SalesMatching(opp1.Header, null, opp1Sale1.EntityTradeDetails.FirstOrDefault(), forwardingProduct.SalesMatchingOptions);
			matching.UseExisting(matching.MatchedSalesCollection.Cast<SalesMatchingData>().First());

			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var opp1reload = factory2.Load<OrgOpportunity>(opp1.PK);
			AssertEquals(1, opp1reload.ProspectiveSalesHeaderCollection.Count);
			var headerReload = ((SalesHeaderCollection)opp1reload.ProspectiveSalesHeaderCollection)[0];
			AssertEquals(1, headerReload.EntitySalesCollectionProductView.Count);
			var updatedSale = headerReload.EntitySalesCollectionProductView.Cast<EntitySalesWrapper>()
				.Single(x => x.PK == org1Sale1.PK);

			AssertEquals("custom annual value from org", 150000m, updatedSale.OW_AnnualRevenue);

			// Only original organization detail is kept
			AssertEquals(1, updatedSale.EntityTradeDetailsCollection.Count);
			AssertEquals(org1Detail1.PK, updatedSale.EntityTradeDetailsCollection[0].PK);
		}

		public void TestMigrateToExisting()
		{
			var product = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.Warehouse);

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();

			var warehouse = (BusinessObject)Factory.New<IWhsWarehouse>();
			warehouse.FillWithValidTestData();

			var part1 = Factory.NewWithValidTestData<OrgSupplierPart>();
			var part2 = Factory.NewWithValidTestData<OrgSupplierPart>();

			part1.RelatedOrganisations.AddOrganisationIfNotExist(org1.PK, OrgPartRelation.RelationshipTypes.Both);
			part1.RelatedOrganisations.AddOrganisationIfNotExist(org2.PK, OrgPartRelation.RelationshipTypes.Both);
			part2.RelatedOrganisations.AddOrganisationIfNotExist(org1.PK, OrgPartRelation.RelationshipTypes.Both);
			part2.RelatedOrganisations.AddOrganisationIfNotExist(org2.PK, OrgPartRelation.RelationshipTypes.Both);

			var org1Sales = (SalesHeaderCollection)org1.ProspectiveSalesHeaderCollection;
			var org1header = org1Sales.AddNew(product);
			var org1Sale1 = org1header.EntitySalesCollectionProductView.AddNew();
			org1Sale1.OW_Service = OrgSalesWarehouseServiceTypesList.Codes.Storage;
			org1Sale1.OW_WW = warehouse.PK;
			org1Sale1.OW_OriginID = ViewLocationHelper.GetLocationFromString(Factory, "AUSYD", RefUNLOCOSchema.Constants.Prefix).PK;

			var org1Detail1 = org1Sale1.EntityTradeDetailsCollection.AddNew();
			org1Detail1.PA_OP = part1.PK;
			org1Detail1.EstimatedProfit = 47;
			org1Detail1.ProspectDetail.PAP_ConversionCertainty = 2;

			var org1Detail2 = org1Sale1.EntityTradeDetailsCollection.AddNew();
			org1Detail2.PA_OP = part2.PK;
			org1Detail2.EstimatedProfit = 71;
			org1Detail2.ProspectDetail.PAP_ConversionCertainty = 4;

			var opp1 = org2.SalesOpportunities.AddNew();
			var opp1Sales = (SalesHeaderCollection)opp1.ProspectiveSalesHeaderCollection;
			var opp1Header = opp1Sales.AddNew(product);
			var opp1Sale1 = opp1Header.EntitySalesCollectionProductView.AddNew();
			opp1Sale1.OW_Service = OrgSalesWarehouseServiceTypesList.Codes.Storage;
			opp1Sale1.OW_WW = warehouse.PK;
			opp1Sale1.OW_OriginID = ViewLocationHelper.GetLocationFromString(Factory, "AUSYD", RefUNLOCOSchema.Constants.Prefix).PK;

			var opp1Detail1 = opp1Sale1.EntityTradeDetailsCollection.AddNew();
			opp1Detail1.PA_OP = part1.PK;
			opp1Detail1.EstimatedProfit = 100;
			opp1Detail1.ProspectDetail.PAP_ConversionCertainty = 5;

			var opp1Detail2 = opp1Sale1.EntityTradeDetailsCollection.AddNew();
			opp1Detail2.PA_OP = part2.PK;
			opp1Detail2.EstimatedProfit = 99;
			opp1Detail2.ProspectDetail.PAP_ConversionCertainty = 3;

			Factory.Save();

			opp1.P8_OH = org1.PK;
			Factory.Save();
			var headers = (SalesHeaderCollection)opp1.ProspectiveSalesHeaderCollection;
			headers.Rebuild();

			opp1Sale1.TradeDetails.Load();
			AssertEquals(2, opp1Sale1.TradeDetails.Count);

			// MigrateToExisting detail1
			var tradeDetailWrapper = EntityTradeDetailWrapper.Get(opp1Detail1, opp1);
			var matching = new SalesMatching(opp1.Header, null, tradeDetailWrapper, product.SalesMatchingOptions);
			matching.MigrateToExisting(matching.MatchedSalesCollection.Cast<SalesMatchingData>().Single(x => x.TradeDetail.PK == org1Detail1.PK));

			// Append detail2
			tradeDetailWrapper = EntityTradeDetailWrapper.Get(opp1Detail2, opp1);
			matching = new SalesMatching(opp1.Header, null, tradeDetailWrapper, product.SalesMatchingOptions);
			matching.Append();
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var opp1reload = factory2.Load<OrgOpportunity>(opp1.PK);
			AssertEquals(1, opp1reload.ProspectiveSalesHeaderCollection.Count);
			var headerReload = ((SalesHeaderCollection)opp1reload.ProspectiveSalesHeaderCollection)[0];
			AssertEquals(2, headerReload.EntitySalesCollectionProductView.Count);
			var updatedSale = headerReload.EntitySalesCollectionProductView.Cast<EntitySalesWrapper>()
				.Single(x => x.PK == org1Sale1.PK);
			var appendedSale = headerReload.EntitySalesCollectionProductView.Cast<EntitySalesWrapper>()
				.Single(x => x.PK == opp1Sale1.PK);

			AssertEquals(1, updatedSale.EntityTradeDetailsCollection.Count);
			AssertEquals(1, appendedSale.EntityTradeDetailsCollection.Count);

			var updatedDetail1 = updatedSale.EntityTradeDetailsCollection.Cast<EntityTradeDetailWrapper>().Single(x => x.PK == org1Detail1.PK);
			var appendedDetail2 = appendedSale.EntityTradeDetailsCollection.Cast<EntityTradeDetailWrapper>().Single(x => x.PK == opp1Detail2.PK);

			AssertEquals("opp detail1 values copied to org", 100m, updatedDetail1.EstimatedProfit);
			AssertEquals("opp detail1 values copied to org", (byte)5, updatedDetail1.ProspectDetail.PAP_ConversionCertainty);
			AssertEquals("opp detail1 deleted", true, opp1Detail1.IsDeleted);

			AssertEquals("opp detail2 values unchanged", 99m, appendedDetail2.EstimatedProfit);
		}

		public void TestMigrateToExisting_IsCustomRevenue()
		{
			var au = ViewLocationHelper.GetLocationFromString(Factory, "AU", RefCountrySchema.Constants.Prefix);
			var nz = ViewLocationHelper.GetLocationFromString(Factory, "NZ", RefCountrySchema.Constants.Prefix);

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();

			var product = Factory.New<OrgSalesProduct>();
			product.MP_Code = "Foo";
			product.MP_Name = "Foo";

			var org1Sales = (SalesHeaderCollection)org1.ProspectiveSalesHeaderCollection;
			var org1header = org1Sales.AddNew(product);
			var org1Sale1 = org1header.EntitySalesCollectionProductView.AddNew();
			org1Sale1.OW_OriginID = au.PK;
			org1Sale1.OW_DestinationID = nz.PK;
			org1Sale1.OW_IsCustomRevenue = true;
			org1Sale1.OW_AnnualRevenue = 150000m;

			var org1Detail1 = org1Sale1.EntityTradeDetailsCollection.AddNew();
			org1Detail1.PA_TradeMode = Constants.TransportModes.Sea;
			org1Detail1.PA_TradeType = Constants.ContainerModes.FCL;
			org1Detail1.EstimatedProfit = 47;
			org1Detail1.CurrencyCode = "AUD";
			org1Detail1.ProspectDetail.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.Monthly;

			var opp1 = org2.SalesOpportunities.AddNew();
			var opp1Sales = (SalesHeaderCollection)opp1.ProspectiveSalesHeaderCollection;
			var opp1Header = opp1Sales.AddNew(product);
			var opp1Sale1 = opp1Header.EntitySalesCollectionProductView.AddNew();
			opp1Sale1.OW_OriginID = au.PK;
			opp1Sale1.OW_DestinationID = nz.PK;
			opp1Sale1.OW_IsCustomRevenue = true;
			opp1Sale1.OW_AnnualRevenue = 6000m;
			opp1Sale1.OW_MonthlyRevenue = 500m;

			var opp1Detail1 = opp1Sale1.EntityTradeDetailsCollection.AddNew();
			opp1Detail1.PA_TradeMode = Constants.TransportModes.Sea;
			opp1Detail1.PA_TradeType = Constants.ContainerModes.FCL;
			opp1Detail1.EstimatedProfit = 99m;
			opp1Detail1.CurrencyCode = "AUD";
			opp1Detail1.ProspectDetail.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.Monthly;

			Factory.Save();

			opp1.P8_OH = org1.PK;
			Factory.Save();

			var headers = (SalesHeaderCollection)opp1.ProspectiveSalesHeaderCollection;
			headers.Rebuild();
			var matching = new SalesMatching(opp1.Header, opp1Sale1, null, product.SalesMatchingOptions);
			matching.MigrateToExisting(matching.MatchedSalesCollection.Cast<SalesMatchingData>().First());
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var opp1reload = factory2.Load<OrgOpportunity>(opp1.PK);
			AssertEquals(1, opp1reload.ProspectiveSalesHeaderCollection.Count);
			var headerReload = ((SalesHeaderCollection)opp1reload.ProspectiveSalesHeaderCollection)[0];
			AssertEquals(1, headerReload.EntitySalesCollectionProductView.Count);
			var updatedSale = headerReload.EntitySalesCollectionProductView.Cast<EntitySalesWrapper>()
				.Single(x => x.PK == org1Sale1.PK);

			AssertEquals(2, updatedSale.EntityTradeDetailsCollection.Count);

			AssertEquals("custom annual value copied to org", 6000m, updatedSale.OW_AnnualRevenue);
			AssertEquals("custom monthly value copied to org", 500m, updatedSale.OW_MonthlyRevenue);
		}

		public void TestReplaceWithClonedExistingWithSkipCloningDetail()
		{
			var product = Factory.New<OrgSalesProduct>();
			var org = Factory.New<OrgHeader>();
			var consignee = Factory.New<OrgHeader>();

			var opportunity1 = org.SalesOpportunities.AddNew();
			var headerCollection1 = (SalesHeaderCollection)opportunity1.ProspectiveSalesHeaderCollection;
			var header1 = headerCollection1.AddNew(product);
			var opportunitySales1 = header1.EntitySalesCollectionProductView.AddNew();
			opportunitySales1.OW_OH_Buyer = consignee.PK;

			var opportunityDetail = opportunitySales1.EntityTradeDetailsCollection.AddNew();
			opportunityDetail.PA_TradeMode = "SEA";
			opportunityDetail.CurrentProspectPeriod.PAS_RepeatsMnth = 15m;

			var opportunity2 = org.SalesOpportunities.AddNew();
			var headerCollection2 = (SalesHeaderCollection)opportunity2.ProspectiveSalesHeaderCollection;
			var header2 = headerCollection2.AddNew(product);
			var opportunitySales2 = header2.EntitySalesCollectionProductView.AddNew();

			var matching = new SalesMatching(org, opportunitySales2, null, product.SalesMatchingOptions);
			matching.ReplaceWithClonedExisting(new SalesMatchingData(opportunitySales1, null), true);

			AssertEquals(0, opportunitySales2.EntityTradeDetailsCollection.Count);
		}

		void AssertMatchData(SalesMatchingData data, string originCode, string destinationCode, string buyerCode, string supplierCode, string mode, string type, string currency, decimal revenue)
		{
			CombineAssertions(() =>
			{
				AssertEquals("Origin", originCode, data.Sales.OriginCode);
				AssertEquals("Destination", destinationCode, data.Sales.DestinationCode);
				AssertEquals("Buyer", buyerCode, data.BuyerCode);
				AssertEquals("Supplier", supplierCode, data.SupplierCode);
				AssertEquals("Mode", mode, data.TradeDetail?.PA_TradeMode);
				AssertEquals("Type", type, data.TradeDetail?.PA_TradeType);
				AssertEquals("Currency", currency, data.EstimatedValueCurrency);
				AssertEquals("Revenue", revenue, data.EstimatedValue);
			});
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var org = Factory.New<OrgHeader>();
			var forwardingProduct = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment);
			var salesHeader = new SalesHeader(org, forwardingProduct);
			var newSales = salesHeader.EntitySalesCollectionProductView.AddNew();
			return new SalesMatching(org, newSales, null, forwardingProduct.SalesMatchingOptions);
		}
	}
}
