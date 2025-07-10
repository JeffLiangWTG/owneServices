using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Integration;
using Enterprise.MarketingManager.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.Rating.Business.Testing
{
	class QuoteTradeDetailImporterTest : TestCaseWithFactory
	{
		public void TestImport_CategoryAndMode_Freight()
		{
			var shipmentProduct = Factory.LoadFromNaturalKey<IOrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment);
			var linerAgencyProduct = Factory.LoadFromNaturalKey<IOrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.LinerAgency);

			AssertFrieghtCategoryAndMode(shipmentProduct, FreightMode.BBK, Tuple.Create(RatingConstants.RateCategory.LCL, "BBK"));
			AssertFrieghtCategoryAndMode(shipmentProduct, FreightMode.BLK, Tuple.Create(RatingConstants.RateCategory.LCL, "BLK"));
			AssertFrieghtCategoryAndMode(shipmentProduct, FreightMode.ROR, Tuple.Create(RatingConstants.RateCategory.LCL, "ROR"));
			AssertFrieghtCategoryAndMode(shipmentProduct, FreightMode.ULD, Tuple.Create(RatingConstants.RateCategory.AIR, "ULD"));
			AssertFrieghtCategoryAndMode(shipmentProduct, FreightMode.LSE, Tuple.Create(RatingConstants.RateCategory.AIR, "LSE"));
			AssertFrieghtCategoryAndMode(shipmentProduct, FreightMode.FCL, Tuple.Create(RatingConstants.RateCategory.FCL, "AAA"));
			AssertFrieghtCategoryAndMode(shipmentProduct, FreightMode.LRO, Tuple.Create(RatingConstants.RateCategory.LCL, "LRO"));
			AssertFrieghtCategoryAndMode(shipmentProduct, FreightMode.UKN, null);

			AssertFrieghtCategoryAndMode(linerAgencyProduct, FreightMode.FCL, Tuple.Create(RatingConstants.RateCategory.SCO, Core.Constants.RateMode.SEA));
			AssertFrieghtCategoryAndMode(linerAgencyProduct, FreightMode.BBK, Tuple.Create(RatingConstants.RateCategory.SNC, Core.Constants.RateMode.LCL));
			AssertFrieghtCategoryAndMode(linerAgencyProduct, FreightMode.UKN, null);
		}

		void AssertFrieghtCategoryAndMode(IOrgSalesProduct salesProduct, FreightMode freightMode, Tuple<string, string> expected)
		{
			var sales = Factory.New<OrgSales>();
			sales.OW_MP_Product = salesProduct.Identifier;
			var tradeDetail = sales.TradeDetails.AddNew();
			tradeDetail.PA_TradeMode = "AAA";
			tradeDetail.PA_TradeType = "BBB";

			var mockFreightRatingHelper = new Mock<IFreightRatingHelper>();
			mockFreightRatingHelper
				.Setup(x => x.CalculateFreightMode(
						It.Is<ZString>(p => p == new ZString(tradeDetail.TransportMode)),
						It.Is<ZString>(p => p == tradeDetail.PA_TradeType),
						It.IsAny<Func<FreightMode>>()))
				.Returns(freightMode);

			using (ObjectFactory.Substitute(mockFreightRatingHelper.Object))
			{
				var quote = Factory.New<Quote>();
				quote.ImportTradeDetailData(tradeDetail);

				if (expected != null)
				{
					AssertEquals(1, quote.AllEntries.Count());
					var entry = quote.AllEntries.First();

					CombineAssertions(string.Format("Product:[{0}] FreightMode:[{1}]", salesProduct.MP_Code, freightMode.ToString()), () =>
					{
						AssertEquals("TI_RateCategory", expected.Item1, entry.TI_RateCategory.ToString());
						AssertEquals("TI_Mode", expected.Item2, entry.TI_Mode.ToString());
					});
				}
				else
				{
					AssertEquals(0, quote.AllEntries.Count());
				}
			}
		}

		public void TestImport_CategoryAndMode_CustomsBrokerage()
		{
			var customsProduct = Factory.LoadFromNaturalKey<IOrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.CustomsBrokerage);

			AssertCategoryAndMode(customsProduct, "SEA", OrgTradeDetailLookups.CustomsBrokerageTradeTypes.Import, Tuple.Create(RatingConstants.RateCategory.DST, "SEA"));
			AssertCategoryAndMode(customsProduct, "SEA", OrgTradeDetailLookups.CustomsBrokerageTradeTypes.Export, Tuple.Create(RatingConstants.RateCategory.ORG, "SEA"));
			AssertCategoryAndMode(customsProduct, "AIR", OrgTradeDetailLookups.CustomsBrokerageTradeTypes.Import, Tuple.Create(RatingConstants.RateCategory.DST, "AIR"));
			AssertCategoryAndMode(customsProduct, "AIR", OrgTradeDetailLookups.CustomsBrokerageTradeTypes.Export, Tuple.Create(RatingConstants.RateCategory.ORG, "AIR"));
		}

		void AssertCategoryAndMode(IOrgSalesProduct salesProduct, string mode, string type, Tuple<string, string> expected)
		{
			var sales = Factory.New<OrgSales>();
			sales.OW_MP_Product = salesProduct.Identifier;
			var tradeDetail = sales.TradeDetails.AddNew();
			tradeDetail.PA_TradeMode = mode;
			tradeDetail.PA_TradeType = type;

			var quote = Factory.New<Quote>();
			quote.ImportTradeDetailData(tradeDetail);

			if (expected != null)
			{
				AssertEquals(1, quote.AllEntries.Count());
				var entry = quote.AllEntries.First();

				CombineAssertions(string.Format("Product:[{0}] Mode:[{1}] Type:[{2}]", salesProduct.MP_Code, mode, type), () =>
				{
					AssertEquals("TI_RateCategory", expected.Item1, entry.TI_RateCategory.ToString());
					AssertEquals("TI_Mode", expected.Item2, entry.TI_Mode.ToString());
				});
			}
			else
			{
				AssertEquals(0, quote.AllEntries.Count());
			}
		}

		public void TestImport_ForwardingShipment()
		{
			var shipmentProduct = Factory.LoadFromNaturalKey<IOrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment);
			var ausyd = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");
			var uslax = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "USLAX");
			var inbom = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "INBOM");
			var aumel = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUMEL");

			var testOrg = Helper.NewOrgHeader();
			var testOrgSupplier = Helper.NewOrgHeader();
			var testOrgServiceProvider = Helper.NewOrgHeader();

			var testSale1 = testOrg.SalesCollection.AddNew();
			testSale1.OW_MP_Product = shipmentProduct.Identifier;
			testSale1.OW_OriginID = ausyd.PK;
			testSale1.OW_DestinationID = uslax.PK;
			testSale1.OW_OH_Buyer = testOrg.PK;
			testSale1.OW_OH_Supplier = testOrgSupplier.PK;
			var detail1a = testSale1.TradeDetails.AddNew();
			detail1a.PA_TradeMode = "AIR";
			detail1a.PA_TradeType = "FCL";
			detail1a.ProspectDetail.PAP_RS_NKServiceLevel = "D2D";
			detail1a.ProspectDetail.PAP_RH_NKCommodityCode = "REF";
			detail1a.ProspectDetail.PAP_OH_ServiceProvider = testOrgServiceProvider.PK;
			detail1a.CurrentProspectPeriod.PAS_Chargeable = 100;
			detail1a.CurrentProspectPeriod.PAS_RateOffered = 10;
			var detail1b = testSale1.TradeDetails.AddNew();
			detail1b.PA_TradeMode = "SEA";
			detail1b.PA_TradeType = "LCL";
			detail1b.CurrentProspectPeriod.PAS_Chargeable = 100;
			detail1b.CurrentProspectPeriod.PAS_RateOffered = 20;

			var testSale2 = testOrg.SalesCollection.AddNew();
			testSale2.OW_MP_Product = shipmentProduct.Identifier;
			testSale2.OW_OriginID = inbom.PK;
			testSale2.OW_DestinationID = aumel.PK;
			var detail2a = testSale2.TradeDetails.AddNew();
			detail2a.PA_TradeMode = "SEA";
			detail2a.PA_TradeType = "FCL";
			detail2a.CurrentProspectPeriod.PAS_Chargeable = 100;
			detail2a.CurrentProspectPeriod.PAS_RateOffered = 40;
			var detail2bInvalid = testSale2.TradeDetails.AddNew();
			detail2bInvalid.PA_TradeMode = "XXX";
			detail2bInvalid.PA_TradeType = "XXX";
			detail2bInvalid.CurrentProspectPeriod.PAS_Chargeable = 100;
			detail2bInvalid.CurrentProspectPeriod.PAS_RateOffered = 50;
			var detail2c = testSale2.TradeDetails.AddNew();
			detail2c.PA_TradeMode = "AIR";
			detail2c.PA_TradeType = "LCL";
			detail2c.ProspectDetail.PAP_OH_ServiceProvider = testOrgServiceProvider.PK;
			detail2c.CurrentProspectPeriod.PAS_Chargeable = 100;
			detail2c.CurrentProspectPeriod.PAS_RateOffered = 60;

			var testQuote = Factory.New<Quote>();

			foreach (var detail in new OrgTradeDetail[] { detail1a, detail1b, detail2a, detail2bInvalid, detail2c })
			{
				testQuote.ImportTradeDetailData(detail);
			}

			var airRateEntries = testQuote.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.AIR);
			var lclRateEntries = testQuote.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.LCL);
			var fclRateEntries = testQuote.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.FCL);

			AssertEquals(testOrg.PK, testQuote.TH_OH);
			AssertEquals("2 Air entries", 2, airRateEntries.Count);
			AssertEquals("1 LCL entry", 1, lclRateEntries.Count);
			AssertEquals("1 FCL entry", 1, fclRateEntries.Count);
			AssertEquals("No Origin entries", 0, testQuote.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.ORG).Count);
			AssertEquals("No Destination entries", 0, testQuote.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.DST).Count);
			{
				var airAusydEntry = airRateEntries.Cast<RateEntry>().Single(x => x.TI_OriginLRC == "AUSYD");
				AssertEquals(testOrg.PK, airAusydEntry.TI_OH_Consignee);
				AssertEquals(testOrgSupplier.PK, airAusydEntry.TI_OH_Consignor);
				AssertEquals("AUSYD", airAusydEntry.TI_OriginLRC);
				AssertEquals("USLAX", airAusydEntry.TI_DestinationLRC);
				AssertEquals("D2D", airAusydEntry.TI_RS_NKServiceLevel_NI);
				AssertEquals("REF", airAusydEntry.TI_RH_NKCommodityCode);
				AssertEquals(testOrgServiceProvider.PK, airAusydEntry.TI_OH_TransportProvider);

				var airAusydEntryLine = airAusydEntry.RateLines.Cast<RateLine>().Single();
				AssertEquals(RegistryConstants.Strings.DefaultFreightChargeCode, airAusydEntryLine.ChargeCode.AC_Code);
				AssertType(typeof(CombinedCalculator), airAusydEntryLine.Calculator);
			}

			{
				var airInbomEntry = airRateEntries.Cast<RateEntry>().Single(x => x.TI_OriginLRC == "INBOM");
				AssertEquals("INBOM", airInbomEntry.TI_OriginLRC);
				AssertEquals("AUMEL", airInbomEntry.TI_DestinationLRC);
				AssertEquals(testOrgServiceProvider.PK, airInbomEntry.TI_OH_TransportProvider);

				var airInbomEntryLine = airInbomEntry.RateLines.Cast<RateLine>().Single();
				AssertEquals(RegistryConstants.Strings.DefaultFreightChargeCode, airInbomEntryLine.ChargeCode.AC_Code);
				AssertType(typeof(CombinedCalculator), airInbomEntryLine.Calculator);
			}

			{
				var lclEntry = lclRateEntries[0];
				AssertEquals("AUSYD", lclEntry.TI_OriginLRC);
				AssertEquals("USLAX", lclEntry.TI_DestinationLRC);
				AssertEquals(ZGuid.Empty, lclEntry.TI_OH_TransportProvider);

				var lclEntryLine = lclEntry.RateLines.Cast<RateLine>().Single();
				AssertEquals(RegistryConstants.Strings.DefaultFreightChargeCode, lclEntryLine.ChargeCode.AC_Code);
				AssertType(typeof(MinimumOrPerUnitCalculator), lclEntryLine.Calculator);
				AssertEquals(20m, ((MinimumOrPerUnitCalculator)lclEntryLine.Calculator).PerUnit);
			}

			{
				var fclEntry = fclRateEntries[0];
				AssertEquals("INBOM", fclEntry.TI_OriginLRC);
				AssertEquals("AUMEL", fclEntry.TI_DestinationLRC);
				AssertEquals(ZGuid.Empty, fclEntry.TI_OH_TransportProvider);

				var fclEntryLine = fclEntry.RateLines.Cast<RateLine>().Single();
				AssertEquals(RegistryConstants.Strings.DefaultFreightChargeCode, fclEntryLine.ChargeCode.AC_Code);
				AssertType(typeof(UnitCalculator), fclEntryLine.Calculator);
				AssertEquals(40m, ((UnitCalculator)fclEntryLine.Calculator).PerUnit);
			}
		}

		public void TestImport_Warehouse()
		{
			var warehouseProduct = Factory.LoadFromNaturalKey<IOrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.Warehouse);
			var warehouseA = Factory.New<IWhsWarehouse>();
			var warehouseB = Factory.New<IWhsWarehouse>();
			var productA = Factory.New<OrgSupplierPart>();
			var productB = Factory.New<OrgSupplierPart>();
			var containerA = Factory.New<RefContainer>();
			containerA.RC_Code = "ZZ1";
			var containerB = Factory.New<RefContainer>();
			containerB.RC_Code = "ZZ2";

			var testOrg = Helper.NewOrgHeader();
			var testOrg2 = Helper.NewOrgHeader();
			var testOrgServiceProvider = Helper.NewOrgHeader();

			var testSale1 = testOrg.SalesCollection.AddNew();
			testSale1.OW_MP_Product = warehouseProduct.Identifier;
			testSale1.OW_Service = OrgSalesWarehouseServiceTypesList.Codes.Orders;
			testSale1.OW_WW = warehouseA.PK;
			testSale1.OW_OH_Buyer = testOrg2.PK;
			testSale1.OW_OH_Supplier = ZGuid.Empty;
			var detail1a = testSale1.TradeDetails.AddNew();
			detail1a.PA_OP = productA.PK;
			detail1a.ProspectDetail.PAP_RC_NKContainer = containerA.RC_Code;
			detail1a.ProspectDetail.PAP_RS_NKServiceLevel = "D2D";
			detail1a.ProspectDetail.PAP_RH_NKCommodityCode = "REF";
			detail1a.ProspectDetail.PAP_OH_ServiceProvider = testOrgServiceProvider.PK;
			var detail1b = testSale1.TradeDetails.AddNew();
			detail1b.PA_OP = ZGuid.Empty;

			var testSale2 = testOrg.SalesCollection.AddNew();
			testSale2.OW_MP_Product = warehouseProduct.Identifier;
			testSale2.OW_Service = OrgSalesWarehouseServiceTypesList.Codes.Receipts;
			testSale2.OW_WW = warehouseB.PK;
			testSale2.OW_OH_Buyer = ZGuid.Empty;
			testSale2.OW_OH_Supplier = testOrg2.PK;
			var detail2a = testSale2.TradeDetails.AddNew();
			detail2a.PA_OP = productB.PK;

			var testQuote = Factory.New<Quote>();

			foreach (var detail in new OrgTradeDetail[] { detail1a, detail1b, detail2a })
			{
				testQuote.ImportTradeDetailData(detail);
			}

			var whsRateEntries = testQuote.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.WHS);

			AssertEquals(testOrg.PK, testQuote.TH_OH);
			AssertEquals("3 Whs entries", 3, whsRateEntries.Count);

			var whsAEntry = whsRateEntries.Cast<RateEntry>().Single(x => x.TI_WW_Warehouse == warehouseA.PK && x.TI_RC.IsEmpty);
			CombineAssertions("whsAEntry properties", () =>
			{
				AssertEquals("TI_WW_Warehouse", warehouseA.PK, whsAEntry.TI_WW_Warehouse);
				AssertEquals("TI_RC", ZGuid.Empty, whsAEntry.TI_RC);
				AssertEquals("TI_Mode", Core.Constants.RateMode.ALL, whsAEntry.TI_Mode);
				AssertEquals("TI_OH_Consignee", testOrg2.PK, whsAEntry.TI_OH_Consignee);
				AssertEquals("TI_OH_Consignor", ZGuid.Empty, whsAEntry.TI_OH_Consignor);
				AssertEquals("TI_RS_NKServiceLevel_NI", ZString.Empty, whsAEntry.TI_RS_NKServiceLevel_NI);
				AssertEquals("TI_RH_NKCommodityCode", ZString.Empty, whsAEntry.TI_RH_NKCommodityCode);
				AssertEquals("TI_OH_TransportProvider", ZGuid.Empty, whsAEntry.TI_OH_TransportProvider);

				AssertContainsExactElementsInAnyOrder("TL_OP_ProductNumber",
					Array.Empty<ZGuid>(),
					whsAEntry.RateLines.Cast<RateLine>().Select(x => x.TL_OP_ProductNumber));
			});

			var whsAForContainerEntry = whsRateEntries.Cast<RateEntry>().Single(x => x.TI_WW_Warehouse == warehouseA.PK && x.TI_RC == containerA.PK);
			CombineAssertions("whsAForContainerEntry properties", () =>
			{
				AssertEquals("TI_WW_Warehouse", warehouseA.PK, whsAForContainerEntry.TI_WW_Warehouse);
				AssertEquals("TI_RC", containerA.PK, whsAForContainerEntry.TI_RC);
				AssertEquals("TI_Mode", Core.Constants.RateMode.ALL, whsAForContainerEntry.TI_Mode);
				AssertEquals("TI_OH_Consignee", testOrg2.PK, whsAForContainerEntry.TI_OH_Consignee);
				AssertEquals("TI_OH_Consignor", ZGuid.Empty, whsAForContainerEntry.TI_OH_Consignor);
				AssertEquals("TI_RS_NKServiceLevel_NI", "D2D", whsAForContainerEntry.TI_RS_NKServiceLevel_NI);
				AssertEquals("TI_RH_NKCommodityCode", "REF", whsAForContainerEntry.TI_RH_NKCommodityCode);
				AssertEquals("TI_OH_TransportProvider", testOrgServiceProvider.PK, whsAForContainerEntry.TI_OH_TransportProvider);

				AssertContainsExactElementsInAnyOrder("TL_OP_ProductNumber",
					new[] { productA.PK },
					whsAForContainerEntry.RateLines.Cast<RateLine>().Select(x => x.TL_OP_ProductNumber));
			});

			var whsBEntry = whsRateEntries.Cast<RateEntry>().Single(x => x.TI_WW_Warehouse == warehouseB.PK);
			CombineAssertions("whsAEntry properties", () =>
			{
				AssertEquals("TI_WW_Warehouse", warehouseB.PK, whsBEntry.TI_WW_Warehouse);
				AssertEquals("TI_RC", ZGuid.Empty, whsBEntry.TI_RC);
				AssertEquals("TI_Mode", Core.Constants.RateMode.ALL, whsBEntry.TI_Mode);
				AssertEquals("TI_OH_Consignee", ZGuid.Empty, whsBEntry.TI_OH_Consignee);
				AssertEquals("TI_OH_Consignor", testOrg2.PK, whsBEntry.TI_OH_Consignor);
				AssertEquals("TI_RS_NKServiceLevel_NI", ZString.Empty, whsBEntry.TI_RS_NKServiceLevel_NI);
				AssertEquals("TI_RH_NKCommodityCode", ZString.Empty, whsBEntry.TI_RH_NKCommodityCode);
				AssertEquals("TI_OH_TransportProvider", ZGuid.Empty, whsBEntry.TI_OH_TransportProvider);

				AssertContainsExactElementsInAnyOrder("TL_OP_ProductNumber",
					new[] { productB.PK },
					whsBEntry.RateLines.Cast<RateLine>().Select(x => x.TL_OP_ProductNumber));
			});
		}

		public void TestImport_ShouldExistInForBindingCollections_WhileQuoteHasADifferentSelectedFilterCategory()
		{
			var warehouseProduct = Factory.LoadFromNaturalKey<IOrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.Warehouse);
			var productA = Factory.New<OrgSupplierPart>();
			var productB = Factory.New<OrgSupplierPart>();

			var testOrg = Helper.NewOrgHeader();

			var sale = testOrg.SalesCollection.AddNew();
			sale.OW_MP_Product = warehouseProduct.Identifier;
			sale.OW_Service = OrgSalesWarehouseServiceTypesList.Codes.Orders;
			sale.OW_WW = ZGuid.Empty;
			sale.OW_OriginID = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD").PK;
			var detail = sale.TradeDetails.AddNew();
			detail.PA_OP = productA.PK;
			detail.ProspectDetail.PAP_RS_NKServiceLevel = "D2D";
			detail.ProspectDetail.PAP_RH_NKCommodityCode = "REF";

			var testQuote = Factory.New<Quote>();
			testQuote.SelectedFilterCategory = RatingConstants.RateCategory.AIR;

			testQuote.ImportTradeDetailData(detail);

			var whsRateEntries = testQuote.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.WHS);
			AssertEquals(1, whsRateEntries.Count);

			var whsEntry = whsRateEntries[0];
			AssertContainsExactElementsInAnyOrder(new[] { whsEntry }, testQuote.WHSRateEntriesForBinding);
		}

		public void TestImport_DoNotImportDuplicates()
		{
			var shipmentProduct = Factory.LoadFromNaturalKey<IOrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment);
			var sales = Factory.New<OrgSales>();
			sales.OW_MP_Product = shipmentProduct.Identifier;
			var tradeDetail1 = sales.TradeDetails.AddNew();
			tradeDetail1.PA_TradeMode = "AIR";
			tradeDetail1.PA_TradeType = "LSE";
			var tradeDetail2 = sales.TradeDetails.AddNew();
			tradeDetail2.PA_TradeMode = "AIR";
			tradeDetail2.PA_TradeType = "LSE";

			var quote = Factory.New<Quote>();
			quote.ImportTradeDetailData(tradeDetail1);

			AssertEquals("Precondition", 1, quote.AllEntries.Count());
			var entry = quote.AllEntries.Single();

			quote.ImportTradeDetailData(tradeDetail2);
			AssertContainsExactElementsInAnyOrder("Should not create a new entry as the data would be exactly the same", new[] { entry }, quote.AllEntries);
		}

		#region Implementation

		protected TestHelper Helper
		{
			get { return helper ?? (helper = new TestHelper(Factory)); }
		}
		TestHelper helper;

		#endregion
	}
}
