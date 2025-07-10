using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MarketingManager.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(EntitySalesWrapperCollectionFilterableView))]
	sealed class EntitySalesWrapperCollectionFilterableViewTest : BusinessObjectCollectionViewTestCase<EntitySalesWrapperCollectionFilterableView>
	{
		[TestDate(2022, 2, 2)]
		public void TestStatusFilter()
		{
			var product = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment);
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var countries = Factory.Load<RefCountry>(new ZQuery() { MaximumRows = 10, OrderBy = RefCountrySchema.Constants.RN_Code });
			for (var i = 0; i < 5; i++)
			{
				var prospectiveSales = org.SalesCollection.AddNew();
				prospectiveSales.OW_MP_Product = product.PK;
				prospectiveSales.OW_OriginID = countries[i].PK;
				prospectiveSales.OW_OriginTableCode = countries[i].TablePrefix;
				prospectiveSales.OW_DestinationID = countries[i].PK;
				prospectiveSales.OW_DestinationTableCode = countries[i].TablePrefix;

				var opp = org.SalesOpportunities.AddNew();
				opp.AssociatedTradeLanesPivots.AddPivotFor(prospectiveSales);
			}

			for (var i = 5; i < 10; i++)
			{
				var prospectiveSalesWithActual = org.SalesCollection.AddNew();
				prospectiveSalesWithActual.OW_MP_Product = product.PK;
				prospectiveSalesWithActual.OW_OriginID = countries[i].PK;
				prospectiveSalesWithActual.OW_OriginTableCode = countries[i].TablePrefix;
				prospectiveSalesWithActual.OW_DestinationID = countries[i].PK;
				prospectiveSalesWithActual.OW_DestinationTableCode = countries[i].TablePrefix;

				var opp = org.SalesOpportunities.AddNew();
				opp.AssociatedTradeLanesPivots.AddPivotFor(prospectiveSalesWithActual);

				for (var j = 1; j <= 3; j++)
				{
					var actualSales = org.SalesCollection.AddNew();
					actualSales.OW_IsTraded = true;
					actualSales.OW_MP_Product = product.PK;
					actualSales.OW_OriginID = countries[i].PK;
					actualSales.OW_OriginTableCode = countries[i].TablePrefix;
					actualSales.OW_DestinationID = countries[i].PK;
					actualSales.OW_DestinationTableCode = countries[i].TablePrefix;

					var detail = actualSales.TradeDetails.AddNew();
					var period = detail.TradedPeriods.AddNew();
					period.PAS_Period = new ZDate(2022, j, 1);
					period.PAS_LastTraded = new ZDate(2022, j, 1);
					period.PAS_OH_Client = org.PK;
				}
			}

			Factory.Save();

			var anotherFactory = new BusinessObjectFactory();
			using (GetFactoryIsolater(anotherFactory))
			{
				var productInOtherFactory = anotherFactory.Load<OrgSalesProduct>(product.PK);
				var orgInOtherFactory = anotherFactory.Load<OrgHeader>(org.PK);
				anotherFactory.ResetDatabaseLoadCount();

				var salesHeaderCollection = (SalesHeaderCollection)orgInOtherFactory.ActualAndProspectiveSalesHeaderCollection;
				var salesHeader = salesHeaderCollection.Cast<SalesHeader>().Single(x => x.SalesProduct.PK == product.PK);
				var collection = salesHeader.FilterableEntitySalesCollection;

				AssertEquals(10, collection.Count);

				collection.StatusFilter = OrgSalesActualsStatusList.Codes.Traded;
				AssertEquals(5, collection.Count);

				var expectedDbHits = new Dictionary<string, int>
				{
					{ OrgSalesSchema.Constants.TableName, 2 },
					{ OrgTradeDetailSchema.Constants.TableName, 2 },
					{ OrgTradePeriodSchema.Constants.TableName, 1 },
					{ ViewSalesProspectToActualPivotSchema.Constants.TableName, 0 }
				};
				AssertDbHits(expectedDbHits, anotherFactory);
			}
		}

		[TestDate(2022, 2, 2)]
		public void TestShouldMatchOnBuyerSupplier()
		{
			var product = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment);
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();

			var opp = org.SalesOpportunities.AddNew();

			var countries = Factory.Load<RefCountry>(new ZQuery() { MaximumRows = 10, OrderBy = RefCountrySchema.Constants.RN_Code });
			for (var i = 0; i < 10; i++)
			{
				var prospectiveSalesWithActual = org.SalesCollection.AddNew();
				prospectiveSalesWithActual.OW_MP_Product = product.PK;
				prospectiveSalesWithActual.OW_OH_Buyer = org1.PK;
				prospectiveSalesWithActual.OW_OriginID = countries[i].PK;
				prospectiveSalesWithActual.OW_OriginTableCode = countries[i].TablePrefix;
				prospectiveSalesWithActual.OW_DestinationID = countries[i].PK;
				prospectiveSalesWithActual.OW_DestinationTableCode = countries[i].TablePrefix;

				opp.AssociatedTradeLanesPivots.AddPivotFor(prospectiveSalesWithActual);

				var createActualsWithSameBuyer = i < 5;
				for (var j = 1; j <= 3; j++)
				{
					var actualSales = org.SalesCollection.AddNew();
					actualSales.OW_MP_Product = product.PK;
					actualSales.OW_IsTraded = true;
					actualSales.OW_OH_Buyer = createActualsWithSameBuyer ? org1.PK : org2.PK;
					actualSales.OW_OriginID = countries[i].PK;
					actualSales.OW_OriginTableCode = countries[i].TablePrefix;
					actualSales.OW_DestinationID = countries[i].PK;
					actualSales.OW_DestinationTableCode = countries[i].TablePrefix;

					var detail = actualSales.TradeDetails.AddNew();
					var period = detail.TradedPeriods.AddNew();
					period.PAS_Period = new ZDate(2022, j, 1);
					period.PAS_LastTraded = new ZDate(2022, j, 1);
					period.PAS_OH_Client = createActualsWithSameBuyer ? org1.PK : org2.PK;
				}
			}

			Factory.Save();

			var anotherFactory = new BusinessObjectFactory();
			using (GetFactoryIsolater(anotherFactory))
			{
				var productInOtherFactory = anotherFactory.Load<OrgSalesProduct>(product.PK);
				var orgInOtherFactory = anotherFactory.Load<OrgHeader>(org.PK);
				anotherFactory.ResetDatabaseLoadCount();

				var salesHeaderCollection = (SalesHeaderCollection)orgInOtherFactory.ActualAndProspectiveSalesHeaderCollection;
				var salesHeader = salesHeaderCollection.Cast<SalesHeader>().Single(x => x.SalesProduct.PK == product.PK);
				var collection = salesHeader.FilterableEntitySalesCollection;
				collection.StatusFilter = OrgSalesActualsStatusList.Codes.Traded;

				AssertEquals(10, collection.Count);

				collection.ShouldMatchOnBuyerSupplier = true;
				AssertEquals(5, collection.Count);

				var expectedDbHits = new Dictionary<string, int>
				{
					{ OrgSalesSchema.Constants.TableName, 2 },
					{ OrgTradeDetailSchema.Constants.TableName, 2 },
					{ OrgTradePeriodSchema.Constants.TableName, 1 },
					{ ViewSalesProspectToActualPivotSchema.Constants.TableName, 0 }
				};
				AssertDbHits(expectedDbHits, anotherFactory);
			}
		}

		public void TestCompanyFilter()
		{
			var product = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment);
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var sales = org.SalesCollection.AddNew();
			sales.OW_MP_Product = product.PK;
			var salesWrapper = EntitySalesWrapper.Get(sales, org);

			Factory.Save();

			var salesHeaderCollection = (SalesHeaderCollection)org.ActualAndProspectiveSalesHeaderCollection;
			var salesHeader = salesHeaderCollection.Cast<SalesHeader>().Single(x => x.SalesProduct.PK == product.PK);
			var collectionView = salesHeader.FilterableEntitySalesCollection;

			salesHeader.CompanyFilter = ZGuid.Empty;
			collectionView.Rebuild();
			AssertEquals(1, collectionView.Count);

			salesHeader.CompanyFilter = Env.CurrentCompanyPK;
			collectionView.Rebuild();
			AssertEquals(0, collectionView.Count);

			salesWrapper.ProspectCompanyPks = new List<ZGuid>() { Env.CurrentCompanyPK };
			collectionView.Rebuild();
			AssertEquals(1, collectionView.Count);

			salesWrapper.ProspectCompanyPks = new List<ZGuid>() { ZGuid.Empty };
			collectionView.Rebuild();
			AssertEquals(1, collectionView.Count);

			salesWrapper.ProspectCompanyPks = new List<ZGuid>() { ZGuid.Empty, ZGuid.NewZGuid() };
			collectionView.Rebuild();
			AssertEquals(1, collectionView.Count);
		}

		#region Implementation

		protected override EntitySalesWrapperCollectionFilterableView GetCollectionToTest()
		{
			var org = Factory.New<OrgHeader>();
			var salesHeaderCollection = (SalesHeaderCollection)org.ActualAndProspectiveSalesHeaderCollection;
			var salesHeader = salesHeaderCollection.AddNew(Product);
			salesHeader.CompanyFilter = ZGuid.Empty;
			return salesHeader.FilterableEntitySalesCollection;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var org = Factory.New<OrgHeader>();
			var sales = Factory.New<OrgSales>();
			sales.OW_MP_Product = Product.PK;
			var element = EntitySalesWrapper.Get(sales, org);
			return element;
		}

		OrgSalesProduct Product
		{
			get { return product ?? (product = Factory.New<OrgSalesProduct>()); }
		}
		OrgSalesProduct product;

		#endregion
	}
}
