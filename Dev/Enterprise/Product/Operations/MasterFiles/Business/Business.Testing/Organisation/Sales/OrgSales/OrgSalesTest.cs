using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MarketingManager.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgSales))]
	sealed class OrgSalesTest : EnterpriseBusinessObjectTestCase
	{
		public void TestSupportNotes()
		{
			var sales = Factory.New<OrgSales>();
			AssertEquals(false, sales.SupportsNotes);
		}

		public void TestParentOrganisation()
		{
			OrgHeader supplier = Helper.NewOrgHeader();
			OrgHeader buyer = Helper.NewOrgHeader();

			OrgSales sales = Helper.NewOrgSales(null, supplier, buyer);
			Factory.Save();

			AssertNull(sales.ParentOrganisation);

			AssertEquals(1, supplier.SalesCollection.Count);
			AssertEquals(supplier.PK, sales.ParentOrganisation.PK);

			AssertEquals(1, buyer.SalesCollection.Count);
			AssertNull(sales.ParentOrganisation);

			buyer.IsBoundToOrganisationForm = true;
			AssertEquals(buyer.PK, sales.ParentOrganisation.PK);

			OrgHeader anotherOrg = Helper.NewOrgHeader();
			sales.ParentOrganisation = anotherOrg;
			AssertEquals(anotherOrg.PK, sales.ParentOrganisation.PK);

			sales.ParentOrganisation = null;
			AssertEquals(buyer.PK, sales.ParentOrganisation.PK);
		}

		public void TestParentOrganisation_UsingSubsetCollection()
		{
			var org = Factory.New<OrgHeader>();
			var collection = new OrgSalesCollection(org);
			var subsetCollection = new OrgSalesSubsetBusinessObjectCollectionForTest(collection);

			var sales = subsetCollection.AddNew();
			AssertEquals(org, sales.ParentOrganisation);
		}

		public void TestParentOrganisation_UsingMultipleLevelsOfSubsetCollection()
		{
			var org = Factory.New<OrgHeader>();
			var collection = new OrgSalesCollection(org);
			var subsetCollection = new OrgSalesSubsetBusinessObjectCollectionForTest(collection);
			var subsubsetCollection = new OrgSalesSubsetBusinessObjectCollectionForTest(subsetCollection);

			var sales = subsubsetCollection.AddNew();
			AssertEquals(org, sales.ParentOrganisation);
		}

		public void TestOW_OriginID()
		{
			var org = Factory.New<OrgHeader>();
			var sales = org.SalesCollection.AddNew();

			AssertEquals(ZGuid.Empty, sales.OW_OriginID);
			AssertEquals(ZString.Empty, sales.OW_OriginTableCode);

			var unloco = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "AUSYD"));
			sales.OW_OriginID = unloco.PK;
			AssertEquals(unloco.PK, sales.OW_OriginID);
			AssertEquals(RefUNLOCOSchema.Constants.Prefix, sales.OW_OriginTableCode);

			var city = Factory.LoadTop1<RefCityTown>(new ZQuery(RefCityTownSchema.R9_InternationalName, "Sydney"));
			sales.OW_OriginID = city.PK;
			AssertEquals(city.PK, sales.OW_OriginID);
			AssertEquals(RefCityTownSchema.Constants.Prefix, sales.OW_OriginTableCode);

			sales.OW_OriginID = ZGuid.Empty;
			AssertEquals(ZGuid.Empty, sales.OW_OriginID);
			AssertEquals(ZString.Empty, sales.OW_OriginTableCode);
		}

		public void TestOW_DestinationID()
		{
			var org = Factory.New<OrgHeader>();
			var sales = org.SalesCollection.AddNew();

			AssertEquals(ZGuid.Empty, sales.OW_DestinationID);
			AssertEquals(ZString.Empty, sales.OW_DestinationTableCode);

			var unloco = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "AUSYD"));
			sales.OW_DestinationID = unloco.PK;
			AssertEquals(unloco.PK, sales.OW_DestinationID);
			AssertEquals(RefUNLOCOSchema.Constants.Prefix, sales.OW_DestinationTableCode);

			var city = Factory.LoadTop1<RefCityTown>(new ZQuery(RefCityTownSchema.R9_InternationalName, "Sydney"));
			sales.OW_DestinationID = city.PK;
			AssertEquals(city.PK, sales.OW_DestinationID);
			AssertEquals(RefCityTownSchema.Constants.Prefix, sales.OW_DestinationTableCode);

			var newGuid = ZGuid.Empty;
			sales.OW_DestinationID = newGuid;
			AssertEquals(ZGuid.Empty, sales.OW_DestinationID);
			AssertEquals(ZString.Empty, sales.OW_DestinationTableCode);
		}

		public void TestOW_Destination_PopulatesOW_OH_BuyerIfSameAsOrg()
		{
			var unlocoXXX = Factory.NewWithValidTestData<RefUNLOCO>();
			unlocoXXX.RL_Code = "XXX";
			var unlocoZZZ = Factory.NewWithValidTestData<RefUNLOCO>();
			unlocoZZZ.RL_Code = "ZZZ";
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_RL_NKClosestPort = "XXX";

			Factory.Save();

			var collection = new OrgSalesCollection(org);
			var sales = collection.AddNew();

			sales.OW_DestinationID = unlocoZZZ.PK;
			AssertEquals("Should not populate to current org if destination not in current org", ZGuid.Empty, sales.OW_OH_Buyer);

			sales.OW_DestinationID = unlocoXXX.PK;
			AssertEquals("Should auto-populate to current org when setting destination to org location", org.PK, sales.OW_OH_Buyer);

			var customBuyer = Factory.New<OrgHeader>();
			sales.OW_OH_Buyer = customBuyer.PK;
			sales.OW_DestinationID = ZGuid.Empty;

			sales.OW_DestinationID = unlocoXXX.PK;
			AssertEquals("Should not overwrite existing buyer value", customBuyer.PK, sales.OW_OH_Buyer);

			sales.OW_DestinationID = ZGuid.Empty;
			sales.OW_OH_Buyer = ZGuid.Empty;
			sales.OW_OH_Supplier = org.PK;

			sales.OW_DestinationID = unlocoXXX.PK;
			AssertEquals("Should not auto-populate buyer if current org is the supplier", ZGuid.Empty, sales.OW_OH_Buyer);

			sales.OW_OriginID = ZGuid.Empty;
			sales.OW_DestinationID = ZGuid.Empty;
			sales.OW_OH_Buyer = ZGuid.Empty;
			sales.OW_OH_Supplier = ZGuid.Empty;

			var product = Factory.LoadFromNaturalKey<IOrgSalesProduct>(OrgSalesProductSchema.MP_Code, "SHP") as BusinessObject;
			product[OrgSalesProductSchema.MP_IsSystemDefined] = false;
			sales.OW_MP_Product = product.PK;

			sales.OW_OriginID = unlocoXXX.PK;
			AssertEquals("Should auto-populate to current org for generic product", org.PK, sales.OW_OH_Buyer);
		}

		public void TestOW_Origin_PopulatesOW_OH_SupplierIfSameAsOrg()
		{
			var unlocoXXX = Factory.NewWithValidTestData<RefUNLOCO>();
			unlocoXXX.RL_Code = "XXX";
			var unlocoZZZ = Factory.NewWithValidTestData<RefUNLOCO>();
			unlocoZZZ.RL_Code = "ZZZ";
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_RL_NKClosestPort = "XXX";

			Factory.Save();

			var collection = new OrgSalesCollection(org);
			var sales = collection.AddNew();

			sales.OW_OriginID = unlocoZZZ.PK;
			AssertEquals("Should not populate to current org if origin not in current org", ZGuid.Empty, sales.OW_OH_Supplier);

			sales.OW_OriginID = unlocoXXX.PK;
			AssertEquals("Should auto-populate to current org when setting origin to same as org", org.PK, sales.OW_OH_Supplier);

			var customSupplier = Factory.New<OrgHeader>();
			sales.OW_OH_Supplier = customSupplier.PK;
			sales.OW_OriginID = ZGuid.Empty;

			sales.OW_OriginID = unlocoXXX.PK;
			AssertEquals("Should not overwrite existing supplier value", customSupplier.PK, sales.OW_OH_Supplier);

			sales.OW_OriginID = ZGuid.Empty;
			sales.OW_OH_Supplier = ZGuid.Empty;
			sales.OW_OH_Buyer = org.PK;

			sales.OW_OriginID = unlocoXXX.PK;
			AssertEquals("Should not auto-populate supplier if current org is the buyer", ZGuid.Empty, sales.OW_OH_Supplier);
		}

		public void TestOW_WW_ShouldDefaultOriginID()
		{
			var ausyd = ViewLocationHelper.GetLocationFromString(Factory, "AUSYD", RefUNLOCOSchema.Constants.Prefix);
			var sydBranch = Factory.New<GlbBranch>();
			sydBranch.GB_RL_NKHomePort = "AUSYD";
			var sydWarehouse = Factory.New<IWhsWarehouse>();
			sydWarehouse.WW_GB_RelatedCompanyBranch = sydBranch.PK;

			var aumel = ViewLocationHelper.GetLocationFromString(Factory, "AUMEL", RefUNLOCOSchema.Constants.Prefix);
			var melBranch = Factory.New<GlbBranch>();
			melBranch.GB_RL_NKHomePort = "AUMEL";
			var melWarehouse = Factory.New<IWhsWarehouse>();
			melWarehouse.WW_GB_RelatedCompanyBranch = melBranch.PK;

			var sales = Factory.New<OrgSales>();
			sales.OW_WW = sydWarehouse.PK;
			AssertEquals("Should have defaulted warehouse's branch home port", "AUSYD", sales.OriginCode);

			sales.OW_WW = melWarehouse.PK;
			AssertEquals("Should have updated OriginID to new warehouse branch home port", "AUMEL", sales.OriginCode);

			sales.OW_OriginID = ViewLocationHelper.GetLocationFromString(Factory, "AUBNE", RefUNLOCOSchema.Constants.Prefix).PK;
			sales.OW_WW = sydWarehouse.PK;
			AssertEquals("Should not have overriden originID since the origin was manually changed to a different value", "AUBNE", sales.OriginCode);
		}

		public void TestOW_Service_OnChangeShouldClearBuyerSupplierIfReadOnlyForWarehouse()
		{
			var org = Factory.New<OrgHeader>();
			var sales = Factory.New<OrgSales>();
			sales.OW_MP_Product = Factory.LoadFromNaturalKey<IOrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.Warehouse).Identifier;

			sales.OW_OH_Buyer = org.PK;
			sales.OW_OH_Supplier = org.PK;
			sales.OW_Service = OrgSalesWarehouseServiceTypesList.Codes.Orders;
			AssertEquals(org.PK, sales.OW_OH_Buyer);
			AssertEquals(ZGuid.Empty, sales.OW_OH_Supplier);

			sales.OW_OH_Buyer = org.PK;
			sales.OW_OH_Supplier = org.PK;
			sales.OW_Service = OrgSalesWarehouseServiceTypesList.Codes.Receipts;
			AssertEquals(ZGuid.Empty, sales.OW_OH_Buyer);
			AssertEquals(org.PK, sales.OW_OH_Supplier);

			sales.OW_OH_Buyer = org.PK;
			sales.OW_OH_Supplier = org.PK;
			sales.OW_Service = OrgSalesWarehouseServiceTypesList.Codes.Storage;
			AssertEquals(ZGuid.Empty, sales.OW_OH_Buyer);
			AssertEquals(ZGuid.Empty, sales.OW_OH_Supplier);
		}

		public void TestOW_ServiceDescription_MaxLength()
		{
			var sales = Factory.New<OrgSales>();
			sales.OW_MP_Product = Factory.LoadFromNaturalKey<IOrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.Warehouse).Identifier;

			var list = new OrgSalesWarehouseServiceTypesList();
			var expectedMaxLength = list.Cast<ICodeDescription>().Max(x => x.Description.Length);
			AssertEquals(expectedMaxLength, sales.OW_ServiceDescriptionInfo.MaxLength);
		}

		public void TestOW_OH_Buyer_ReadOnly_Warehouse()
		{
			var warehouseProduct = Factory.LoadFromNaturalKey<IOrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.Warehouse);
			var sales = Factory.New<OrgSales>();
			sales.OW_MP_Product = warehouseProduct.Identifier;
			sales.OW_Service = OrgSalesWarehouseServiceTypesList.Codes.Receipts;
			AssertEquals(true, sales.OW_OH_Buyer_ReadOnly);

			sales.OW_Service = OrgSalesWarehouseServiceTypesList.Codes.Orders;
			AssertEquals(false, sales.OW_OH_Buyer_ReadOnly);

			sales.OW_Service = OrgSalesWarehouseServiceTypesList.Codes.Storage;
			AssertEquals(true, sales.OW_OH_Buyer_ReadOnly);

			sales.OW_Service = "";
			AssertEquals(true, sales.OW_OH_Buyer_ReadOnly);
		}

		public void TestOW_OH_Supplier_ReadOnly_Warehouse()
		{
			var warehouseProduct = Factory.LoadFromNaturalKey<IOrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.Warehouse);
			var sales = Factory.New<OrgSales>();
			sales.OW_MP_Product = warehouseProduct.Identifier;
			sales.OW_Service = OrgSalesWarehouseServiceTypesList.Codes.Receipts;
			AssertEquals(false, sales.OW_OH_Supplier_ReadOnly);

			sales.OW_Service = OrgSalesWarehouseServiceTypesList.Codes.Orders;
			AssertEquals(true, sales.OW_OH_Supplier_ReadOnly);

			sales.OW_Service = OrgSalesWarehouseServiceTypesList.Codes.Storage;
			AssertEquals(true, sales.OW_OH_Supplier_ReadOnly);

			sales.OW_Service = "";
			AssertEquals(true, sales.OW_OH_Supplier_ReadOnly);
		}

		public void TestImpExpMode()
		{
			var org = Helper.NewOrgHeader();
			var sales = org.SalesCollection.AddNew();

			sales.OW_OH_Buyer = org.PK;
			sales.OW_OH_Supplier = ZGuid.Empty;
			AssertEquals(Constants.Sales.Mode.Import, sales.ImpExpMode);

			sales.OW_OH_Buyer = ZGuid.Empty;
			sales.OW_OH_Supplier = org.PK;
			AssertEquals(Constants.Sales.Mode.Export, sales.ImpExpMode);

			sales.OW_OH_Buyer = ZGuid.Empty;
			sales.OW_OH_Supplier = ZGuid.Empty;
			AssertEquals(ZString.Empty, sales.ImpExpMode);

			sales.OW_OH_Buyer = org.PK;
			sales.OW_OH_Supplier = org.PK;
			AssertEquals(Constants.Sales.Mode.Import, sales.ImpExpMode);
		}

		public void TestLocationType()
		{
			var transportZoneSet = Factory.New(BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(RateTransportProviderSchema.Constants.Prefix));
			transportZoneSet.FillWithValidTestData();
			var transportZone = Factory.New(BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(RateTransportZonesSchema.Constants.Prefix));
			transportZone[RateTransportZonesSchema.TZ_TP] = transportZoneSet.PK;
			Factory.Save();

			var org = Helper.NewOrgHeader();
			var sales = org.SalesCollection.AddNew();
			AssertEquals(ZString.Empty, sales.OriginLocationType);
			AssertEquals(ZString.Empty, sales.DestinationLocationType);

			sales.OW_OriginID = Factory.LoadTop1<RefUNLOCO>(new ZQuery()).PK;
			AssertEquals(ViewLocationTypeList.Codes.UNLOCO, sales.OriginLocationType);
			sales.OW_OriginID = Factory.LoadTop1<RefCountry>(new ZQuery()).PK;
			AssertEquals(ViewLocationTypeList.Codes.Country, sales.OriginLocationType);
			sales.OW_OriginID = Factory.LoadTop1<RefCountryStates>(new ZQuery()).PK;
			AssertEquals(ViewLocationTypeList.Codes.State, sales.OriginLocationType);
			sales.OW_OriginID = Factory.LoadTop1<RefCityTown>(new ZQuery()).PK;
			AssertEquals(ViewLocationTypeList.Codes.City, sales.OriginLocationType);
			sales.OW_OriginID = Factory.LoadTop1<RefZoneHeader>(new ZQuery()).PK;
			AssertEquals(ViewLocationTypeList.Codes.InternationalZone, sales.OriginLocationType);
			sales.OW_OriginID = transportZone.PK;
			AssertEquals(ViewLocationTypeList.Codes.TransportZone, sales.OriginLocationType);

			sales.OW_DestinationID = Factory.LoadTop1<RefUNLOCO>(new ZQuery()).PK;
			AssertEquals(ViewLocationTypeList.Codes.UNLOCO, sales.DestinationLocationType);
			sales.OW_DestinationID = Factory.LoadTop1<RefCountry>(new ZQuery()).PK;
			AssertEquals(ViewLocationTypeList.Codes.Country, sales.DestinationLocationType);
			sales.OW_DestinationID = Factory.LoadTop1<RefCountryStates>(new ZQuery()).PK;
			AssertEquals(ViewLocationTypeList.Codes.State, sales.DestinationLocationType);
			sales.OW_DestinationID = Factory.LoadTop1<RefCityTown>(new ZQuery()).PK;
			AssertEquals(ViewLocationTypeList.Codes.City, sales.DestinationLocationType);
			sales.OW_DestinationID = Factory.LoadTop1<RefZoneHeader>(new ZQuery()).PK;
			AssertEquals(ViewLocationTypeList.Codes.InternationalZone, sales.DestinationLocationType);
			sales.OW_DestinationID = transportZone.PK;
			AssertEquals(ViewLocationTypeList.Codes.TransportZone, sales.DestinationLocationType);
		}

		public void TestIsActual()
		{
			var sales = Factory.New<OrgSales>();
			AssertEquals(false, sales.IsActual);

			sales.OW_IsTraded = true;
			AssertEquals(true, sales.IsActual);
		}

		public void TestLogMessage()
		{
			var ausyd = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");
			var uslax = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "USLAX");

			OrgHeader org = Helper.NewOrgHeader();
			OrgSales sales1 = org.SalesCollection.AddNew();
			sales1.OW_OH_Buyer = org.PK;
			sales1.OW_OriginID = ausyd.PK;
			sales1.OW_DestinationID = uslax.PK;

			OrgSales sales2 = org.SalesCollection.AddNew();
			sales2.OW_OH_Supplier = org.PK;
			sales2.OW_OriginID = uslax.PK;
			sales2.OW_DestinationID = ausyd.PK;

			OrgSales sales3 = org.SalesCollection.AddNew();
			sales3.OW_OH_Supplier = org.PK;
			sales3.OW_OriginID = uslax.PK;
			sales3.OW_DestinationID = ausyd.PK;
			sales3.OW_IsTraded = true;

			AssertEquals("Precondition", Constants.Sales.Mode.Import, sales1.ImpExpMode);
			AssertEquals("Precondition", Constants.Sales.Mode.Export, sales2.ImpExpMode);

			Factory.Save();

			AssertEquals("IMP Trade Lane for USLAX <- AUSYD", sales1.Logs.AutoCreatedLog.SL_Reference);
			AssertEquals("EXP Trade Lane for USLAX -> AUSYD", sales2.Logs.AutoCreatedLog.SL_Reference);
			AssertNull(sales3.Logs.AutoCreatedLog);
		}

		[ExpectNoExceptions]
		public void TestDeleteTradeLane()
		{
			OrgHeader testOrg1 = Helper.NewOrgHeader();
			OrgHeader testOrg2 = Helper.NewOrgHeader();

			OrgSales saleItem = Helper.NewOrgSales(testOrg1, testOrg1, testOrg2);
			Helper.NewOrgTradeDetail(saleItem, OrgTradeProspectRecurrenceTypeList.Codes.Monthly, 3, 5m, "AUD");

			Factory.Save();
			Assert("One Trade Lane in Client", testOrg1.SalesCollection.Count == 1);

			testOrg1.SalesCollection.Remove(saleItem);
			Assert("Trade Lane Removed", testOrg1.SalesCollection.Count == 0);
		}

		public void TestDelete_ShouldRemoveAllAssociationPivots()
		{
			var sales = Factory.New<OrgSales>();
			var opportunity = Factory.New<OrgOpportunity>();
			opportunity.AssociatedTradeLanesPivots.AddPivotFor(sales);

			AssertEquals("Precondition", 1, opportunity.AssociatedTradeLanesPivots.Count);

			sales.Delete();

			AssertEquals(0, opportunity.AssociatedTradeLanesPivots.Count);
		}

		public void TestCanDelete_ForProductWithOrgSalesAllowedAssociation()
		{
			var product = Factory.LoadFromNaturalKey<IOrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment);
			AssertEquals("Precondition", true, product.AllowedAssociationTargets.HasFlag(OrgSalesProductAssociationTarget.OrgSales));

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var tradeLane1 = org.SalesCollection.AddNew();
			tradeLane1.OW_MP_Product = product.Identifier;
			var tradeLane2 = org.SalesCollection.AddNew();
			tradeLane2.OW_MP_Product = product.Identifier;

			var opportunity1 = org.SalesOpportunities.AddNew();
			opportunity1.AssociatedTradeLanesPivots.AddPivotFor(tradeLane1);
			opportunity1.AssociatedTradeLanesPivots.AddPivotFor(tradeLane2);

			var opportunity2 = org.SalesOpportunities.AddNew();
			opportunity2.AssociatedTradeLanesPivots.AddPivotFor(tradeLane1);

			Factory.Save();

			AssertEquals("Should not be able to delete because associations are done on sales level, and sales has multiple associations", false, tradeLane1.CanDelete);
			AssertEquals("Should be able to delete because associations are done on sales level, and sales has only 1 association", true, tradeLane2.CanDelete);
		}

		public void TestCanDelete_ForProductWithTradeDetailAllowedAssociation()
		{
			var product = Factory.LoadFromNaturalKey<IOrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.Warehouse);
			AssertEquals("Precondition", true, product.AllowedAssociationTargets.HasFlag(OrgSalesProductAssociationTarget.OrgTradeDetail));

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var tradeLane1 = org.SalesCollection.AddNew();
			tradeLane1.OW_MP_Product = product.Identifier;
			var tradeDetail1 = tradeLane1.TradeDetails.AddNew();
			var tradeLane2 = org.SalesCollection.AddNew();
			tradeLane2.OW_MP_Product = product.Identifier;
			var tradeDetail2 = tradeLane2.TradeDetails.AddNew();

			Factory.Save();

			var opportunity1 = org.SalesOpportunities.AddNew();
			opportunity1.AssociatedTradeLanesPivots.AddPivotFor(tradeDetail1);

			AssertEquals("Should still be able to delete because no child trade details have multiple associations", true, tradeLane1.CanDelete);
			AssertEquals("Should still be able to delete because no child trade details have multiple associations", true, tradeLane2.CanDelete);

			var opportunity2 = org.SalesOpportunities.AddNew();
			opportunity2.AssociatedTradeLanesPivots.AddPivotFor(tradeDetail1);
			AssertEquals("Should not longer be able to delete because a child trade detail with multiple associations now exists", false, tradeLane1.CanDelete);
			AssertEquals("Should still be able to delete because no child trade details have multiple associations", true, tradeLane2.CanDelete);
		}

		public void TestTradeLaneDetailedDescription_OrginAndDestinationProducts()
		{
			var ausyd = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");
			var nzakl = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "NZAKL");

			var product = Factory.LoadFromNaturalKey<IOrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment);
			AssertEquals("Precondition", OrgSalesProductLocationArrangement.OriginDestination, product.LocationArrangement);

			OrgHeader org = Helper.NewOrgHeader();
			OrgHeader supplier = Helper.NewOrgHeader();
			supplier.OH_Code = "TSTSUPSYD";
			OrgHeader buyer = Helper.NewOrgHeader();
			buyer.OH_Code = "TSTBUYSYD";
			OrgSales sales = org.SalesCollection.AddNew();
			sales.OW_OriginID = ausyd.PK;
			sales.OW_DestinationID = nzakl.PK;
			sales.OW_MP_Product = product.Identifier;

			sales.OW_OH_Buyer = org.PK;
			sales.OW_OH_Supplier = ZGuid.Empty;
			AssertEquals("Forwarding: AUSYD -> NZAKL", sales.TradeLaneDetailedDescription);

			sales.OW_OH_Buyer = ZGuid.Empty;
			sales.OW_OH_Supplier = org.PK;
			AssertEquals("Forwarding: AUSYD -> NZAKL", sales.TradeLaneDetailedDescription);

			sales.OW_OH_Buyer = org.PK;
			sales.OW_OH_Supplier = supplier.PK;
			AssertEquals("Forwarding: AUSYD -> NZAKL (Consignor: TSTSUPSYD)", sales.TradeLaneDetailedDescription);

			sales.OW_OH_Buyer = ZGuid.Empty;
			sales.OW_OH_Supplier = supplier.PK;
			AssertEquals("Forwarding: AUSYD -> NZAKL (Consignor: TSTSUPSYD)", sales.TradeLaneDetailedDescription);

			sales.OW_OH_Buyer = buyer.PK;
			sales.OW_OH_Supplier = org.PK;
			AssertEquals("Forwarding: AUSYD -> NZAKL (Consignee: TSTBUYSYD)", sales.TradeLaneDetailedDescription);

			sales.OW_OH_Buyer = buyer.PK;
			sales.OW_OH_Supplier = ZGuid.Empty;
			AssertEquals("Forwarding: AUSYD -> NZAKL (Consignee: TSTBUYSYD)", sales.TradeLaneDetailedDescription);

			sales.OW_OH_Buyer = ZGuid.Empty;
			sales.OW_OH_Supplier = ZGuid.Empty;
			AssertEquals("Forwarding: AUSYD -> NZAKL", sales.TradeLaneDetailedDescription);

			sales.OW_OH_Buyer = buyer.PK;
			sales.OW_OH_Supplier = supplier.PK;
			AssertEquals("Forwarding: AUSYD -> NZAKL (Consignor: TSTSUPSYD, Consignee: TSTBUYSYD)", sales.TradeLaneDetailedDescription);

			sales.OW_OH_Buyer = org.PK;
			sales.OW_OH_Supplier = org.PK;
			AssertEquals("Forwarding: AUSYD -> NZAKL", sales.TradeLaneDetailedDescription);
		}

		public void TestTradeLaneDetailedDescription_CustomsBrokerage()
		{
			var ausyd = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");

			var product = Factory.LoadFromNaturalKey<IOrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.CustomsBrokerage);
			AssertEquals("Precondition", OrgSalesProductLocationArrangement.SingleLocation, product.LocationArrangement);

			OrgHeader org = Helper.NewOrgHeader();
			OrgHeader supplier = Helper.NewOrgHeader();
			supplier.OH_Code = "TSTSUPSYD";
			OrgHeader buyer = Helper.NewOrgHeader();
			buyer.OH_Code = "TSTBUYSYD";
			OrgSales sales = org.SalesCollection.AddNew();
			sales.OW_OriginID = ausyd.PK;
			sales.OW_MP_Product = product.Identifier;

			sales.OW_OH_Buyer = org.PK;
			sales.OW_OH_Supplier = ZGuid.Empty;
			AssertEquals("Customs Brokerage: AUSYD", sales.TradeLaneDetailedDescription);

			sales.OW_OH_Buyer = org.PK;
			sales.OW_OH_Supplier = supplier.PK;
			AssertEquals("Customs Brokerage: AUSYD (Consignor: TSTSUPSYD)", sales.TradeLaneDetailedDescription);

			sales.OW_OH_Buyer = buyer.PK;
			sales.OW_OH_Supplier = org.PK;
			AssertEquals("Customs Brokerage: AUSYD (Consignee: TSTBUYSYD)", sales.TradeLaneDetailedDescription);
		}

		public void TestTradeLaneDetailedDescription_Warehouse()
		{
			var ausyd = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");
			var warehouse = Factory.New<IWhsWarehouse>();
			warehouse.WW_WarehouseName = "Mega Warehouse";

			var product = Factory.LoadFromNaturalKey<IOrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.Warehouse);
			AssertEquals("Precondition", OrgSalesProductLocationArrangement.SingleLocation, product.LocationArrangement);

			OrgHeader org = Helper.NewOrgHeader();
			OrgHeader supplier = Helper.NewOrgHeader();
			supplier.OH_Code = "TSTSUPSYD";
			OrgHeader buyer = Helper.NewOrgHeader();
			buyer.OH_Code = "TSTBUYSYD";
			OrgSales sales = org.SalesCollection.AddNew();
			sales.OW_MP_Product = product.Identifier;

			sales.OW_Service = OrgSalesWarehouseServiceTypesList.Codes.Orders;
			sales.OW_OriginID = ausyd.PK;
			sales.OW_OH_Buyer = buyer.PK;
			sales.OW_OH_Supplier = ZGuid.Empty;
			AssertEquals("Warehouse (Orders): AUSYD (Consignee: TSTBUYSYD)", sales.TradeLaneDetailedDescription);

			sales.OW_Service = OrgSalesWarehouseServiceTypesList.Codes.Receipts;
			sales.OW_WW = warehouse.PK;
			sales.OW_OH_Buyer = ZGuid.Empty;
			sales.OW_OH_Supplier = supplier.PK;
			AssertEquals("Warehouse (Receipts): Mega Warehouse (Consignor: TSTSUPSYD)", sales.TradeLaneDetailedDescription);

			sales.OW_Service = OrgSalesWarehouseServiceTypesList.Codes.Storage;
			sales.OW_WW = warehouse.PK;
			sales.OW_OriginID = ZGuid.Empty;
			sales.OW_OH_Buyer = ZGuid.Empty;
			sales.OW_OH_Supplier = ZGuid.Empty;
			AssertEquals("Warehouse (Storage): Mega Warehouse", sales.TradeLaneDetailedDescription);
		}

		public void TestCreateDate()
		{
			OrgSales sales = Factory.New<OrgSales>();
			Factory.Save();
			AssertNotEquals("Create date should not be default value", default(ZDateTime), sales.OW_SystemCreateTimeUtc);

			ZDateTime yesterday = ZDateTime.Today.AddDays(-1);
			sales.OW_SystemCreateTimeUtc = yesterday;
			AssertEquals("Create date should respect stored value", yesterday, sales.OW_SystemCreateTimeUtc);
		}

		public void TestUpdateAllTradePeriods()
		{
			var product = Factory.LoadFromNaturalKey<IOrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.Warehouse);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var tradeLane1 = org.SalesCollection.AddNew();
			tradeLane1.OW_MP_Product = product.Identifier;
			var tradeDetail1 = tradeLane1.TradeDetails.AddNew();

			var p1 = Factory.New<OrgTradePeriod>();
			p1.PAS_PA = tradeDetail1.PK;
			p1.PAS_OH_Client = org.PK;
			p1.PAS_IsTraded = true;

			var p2 = Factory.New<OrgTradePeriod>();
			p2.PAS_PA = tradeDetail1.PK;
			p2.PAS_OH_Client = org.PK;
			p2.PAS_IsTraded = false;

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			tradeLane1.UpdateAllTradePeriods(org2.PK);

			AssertEquals(org2.PK, p1.PAS_OH_Client);
			AssertEquals(org2.PK, p2.PAS_OH_Client);
		}

		#region ReadOnly Security

		public void TestReadOnlySecurity()
		{
			bool oldTradeProfileValue = Env.Security.ClientIntelligenceModifyTradeProfile.IsAllowed;

			try
			{
				OrgSales testOrgSales = Factory.NewWithValidTestData<OrgSales>();
				Assert("Access Allowed - Not ReadOnly", !testOrgSales.OW_DestinationIDInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !testOrgSales.OW_OH_BuyerInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !testOrgSales.OW_OH_SupplierInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !testOrgSales.OW_OriginIDInfo.ReadOnly);

				OrgSales testTradeLane = OrgInDB.SalesCollection.AddNew();
				AssertNotNull(testTradeLane.ParentOrganisation);

				Env.Security.ClientIntelligenceModifyTradeProfile.IsAllowed = true;
				Assert("Access Allowed - Not ReadOnly", !testTradeLane.OW_DestinationIDInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !testTradeLane.OW_OH_BuyerInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !testTradeLane.OW_OH_SupplierInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !testTradeLane.OW_OriginIDInfo.ReadOnly);

				Env.Security.ClientIntelligenceModifyTradeProfile.IsAllowed = false;
				Assert("Access NOT Allowed - ReadOnly", testTradeLane.OW_DestinationIDInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", testTradeLane.OW_OH_BuyerInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", testTradeLane.OW_OH_SupplierInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", testTradeLane.OW_OriginIDInfo.ReadOnly);
			}
			finally
			{
				Env.Security.ClientIntelligenceModifyTradeProfile.IsAllowed = oldTradeProfileValue;
			}
		}

		OrgHeader OrgInDB
		{
			get
			{
				if (fOrgInDB == null)
				{
					ResetOrgInDB();
				}

				return fOrgInDB;
			}
		}
		OrgHeader fOrgInDB;

		void ResetOrgInDB()
		{
			fOrgInDB = new BusinessObjectFactory().LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "DEMORG");
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var bizObj = base.GetNewBusinessObject();
			((OrgSales)bizObj).OW_IsCustomRevenue = true;   // some properties can only be set if this flag is true
			return bizObj;
		}

		protected override void SetupDataForSettingValueCallsRefreshBindingTestIfNeeded(ZPropertyInfo info)
		{
			base.SetupDataForSettingValueCallsRefreshBindingTestIfNeeded(info);

			if (info.Name == OrgSales.Schema.OW_ServiceDescription)
			{
				info.BizObj[OrgSalesSchema.Constants.OW_MP_Product] = Factory.LoadFromNaturalKey<IOrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.Warehouse).Identifier;
			}
		}

		protected override Dictionary<string, IZType> CachedValueForSettingValueCallsRefreshBindingTestCore
		{
			get
			{
				var result = base.CachedValueForSettingValueCallsRefreshBindingTestCore;
				result[OrgSales.Schema.OW_ServiceDescription] = (ZString)OrgSalesWarehouseServiceTypesList.Descriptions.Orders;
				return result;
			}
		}

		OrgSalesTestHelper Helper
		{
			get
			{
				if (fHelper == null)
				{
					fHelper = new OrgSalesTestHelper(Factory);
				}

				return fHelper;
			}
		}

		OrgSalesTestHelper fHelper;

		#endregion

		class OrgSalesSubsetBusinessObjectCollectionForTest : SubsetBusinessObjectCollection<OrgSales>
		{
			public OrgSalesSubsetBusinessObjectCollectionForTest(BusinessObjectCollection collectionToFilter)
				: base(collectionToFilter)
			{
			}

			protected override bool IsThisPartOfTheCollection(BusinessObject element)
			{
				return true;
			}
		}
	}
}
