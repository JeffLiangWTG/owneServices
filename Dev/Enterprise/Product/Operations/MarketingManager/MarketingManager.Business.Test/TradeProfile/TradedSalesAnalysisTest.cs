using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MarketingManager.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(TradedSalesAnalysis))]
	sealed class TradedSalesAnalysisTest : NonPersistentBusinessObjectTestCase
	{
		#region Default Values

		public void TestDefaultValues_Period()
		{
			var salesAnalysis = GetNewSalesAnalysis();
			AssertEquals(SalesAnalysisPeriodList.Codes.Trailing12Months, salesAnalysis.Period);
		}

		public void TestDefaultValues_MainGroupingType()
		{
			var salesAnalysis = GetNewSalesAnalysis();
			AssertEquals(SalesAnalysisMainGroupingTypeList.Codes.ModeAndType, salesAnalysis.MainGroupingType);
		}

		public void TestDefaultValues_LocationGroupingType()
		{
			AssertDefaultLocationGroupingType(SystemDefinedSalesProductList.Codes.CustomsBrokerage, SalesAnalysisLocationGroupingTypeList.Codes.CountryToCountry);
			AssertDefaultLocationGroupingType(SystemDefinedSalesProductList.Codes.ForwardingShipment, SalesAnalysisLocationGroupingTypeList.Codes.CountryToCountry);
			AssertDefaultLocationGroupingType(SystemDefinedSalesProductList.Codes.LinerAgency, SalesAnalysisLocationGroupingTypeList.Codes.CountryToCountry);
			AssertDefaultLocationGroupingType(SystemDefinedSalesProductList.Codes.Transport, SalesAnalysisLocationGroupingTypeList.Codes.StateToState);
		}

		void AssertDefaultLocationGroupingType(ZString productCode, ZString expectedLocationGroupingType)
		{
			var salesProduct = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, productCode);
			var analysis = GetNewSalesAnalysis(salesProduct);
			AssertEquals(expectedLocationGroupingType, analysis.LocationGroupingType);
		}

		public void TestDefaultValues_IncludeJobValue()
		{
			var salesAnalysis = GetNewSalesAnalysis();
			AssertEquals(true, salesAnalysis.IncludeJobValue);
		}

		#endregion

		#region Properties

		public void TestMainGroupingType_FiresMainGroupingChangedEvent()
		{
			var org = Factory.New<OrgHeader>();
			var salesHeader = new SalesHeader(org, Factory.New<OrgSalesProduct>());
			var salesAnalysis = new TradedSalesAnalysis(salesHeader);
			salesAnalysis.MainGroupingType = SalesAnalysisMainGroupingTypeList.Codes.Location;
			AssertNotNull(salesAnalysis.MainGroupings); // Load MainGroupings

			var displayChangedEventFired = false;
			salesAnalysis.MainGroupingChanged += (sender, e) =>
			{
				displayChangedEventFired = true;
			};

			salesAnalysis.MainGroupingType = SalesAnalysisMainGroupingTypeList.Codes.Mode;
			AssertEquals("Should have fired 'main grouping changed' event", true, displayChangedEventFired);
		}

		public void TestLocationGroupingType_FiresMainGroupingChangedEvent()
		{
			var org = Factory.New<OrgHeader>();
			var salesHeader = new SalesHeader(org, Factory.New<OrgSalesProduct>());
			var salesAnalysis = new TradedSalesAnalysis(salesHeader);
			salesAnalysis.MainGroupingType = SalesAnalysisMainGroupingTypeList.Codes.Mode;
			salesAnalysis.LocationGroupingType = SalesAnalysisLocationGroupingTypeList.Codes.CountryToCountry;
			AssertNotNull(salesAnalysis.MainGroupings); // Load MainGroupings

			var displayChangedEventFired = false;
			salesAnalysis.MainGroupingChanged += (sender, e) =>
			{
				displayChangedEventFired = true;
			};

			salesAnalysis.LocationGroupingType = SalesAnalysisLocationGroupingTypeList.Codes.DestinationCountry;
			AssertEquals("Should not fire 'main grouping changed' event as current grouping is not location related", false, displayChangedEventFired);

			salesAnalysis.MainGroupingType = SalesAnalysisMainGroupingTypeList.Codes.Location;
			salesAnalysis.LocationGroupingType = SalesAnalysisLocationGroupingTypeList.Codes.OriginCountry;
			AssertEquals("Should have fired 'main grouping changed' event as current grouping is location related", true, displayChangedEventFired);
		}

		#endregion

		#region MainGrouping

		public void TestMainGroupingRefreshOncePerSalesCollectionLoad()
		{
			var salesProduct = Factory.New<OrgSalesProduct>();
			var org = Factory.New<OrgHeader>();
			var sales1 = org.SalesCollection.AddNew();
			sales1.OW_MP_Product = salesProduct.PK;
			sales1.OW_IsTraded = true;

			var sales2 = org.SalesCollection.AddNew();
			sales2.OW_MP_Product = salesProduct.PK;
			sales2.OW_IsTraded = true;

			var sales3 = org.SalesCollection.AddNew();
			sales3.OW_MP_Product = salesProduct.PK;
			sales3.OW_IsTraded = true;

			var salesHeader = new SalesHeader(org, salesProduct);
			var tradedSalesAnalysis = new TradedSalesAnalysis(salesHeader);
			var mainGroupings = tradedSalesAnalysis.MainGroupings; // hit property to initialize it
			AssertNotNull(mainGroupings);

			var mainGroupingRefreshes = 0;
			tradedSalesAnalysis.MainGroupingChanged += (sender, e) =>
				{
					mainGroupingRefreshes++;
				};

			org.SalesCollection.Load();
			AssertEquals(1, mainGroupingRefreshes);
		}

		public void TestMainGrouping_DbHits()
		{
			var salesHeader = CreateSalesHeaderForDbHitTest(Factory);

			var anotherFactory = new BusinessObjectFactory();
			var orgInAnotherFactory = anotherFactory.Load<OrgHeader>(salesHeader.ViewingOrg.PK);
			var productInAnotherFactory = anotherFactory.Load<OrgSalesProduct>(salesHeader.SalesProduct.PK);
			var salesHeaderInAnotherFactory = new SalesHeader(orgInAnotherFactory, productInAnotherFactory);
			anotherFactory.ResetDatabaseLoadCount();

			var tradedSalesAnalysis = new TradedSalesAnalysis(salesHeaderInAnotherFactory)
			{
				MainGroupingType = SalesAnalysisMainGroupingTypeList.Codes.ModeAndType
			};
			AssertNotNull(tradedSalesAnalysis.MainGroupings);

			var expectedDbHits = new Dictionary<string, int>
			{
				{ OrgSalesSchema.Constants.TableName, 0 },
				{ OrgTradeDetailSchema.Constants.TableName, 1 },
				{ OrgTradePeriodSchema.Constants.TableName, 1 },
			};
			AssertDbHits(expectedDbHits, anotherFactory);

			tradedSalesAnalysis.MainGroupingType = SalesAnalysisMainGroupingTypeList.Codes.Location;
			expectedDbHits.Add(ViewLocationSchema.Constants.TableName, 1);
			AssertDbHits(expectedDbHits, anotherFactory);
		}

		static SalesHeader CreateSalesHeaderForDbHitTest(BusinessObjectFactory factory)
		{
			var countriesQuery = new ZQuery();
			countriesQuery.OrderBy = RefCountrySchema.Constants.RN_Code;
			countriesQuery.MaximumRows = 10;
			var countries = factory.Load<RefCountry>(countriesQuery);

			var org = factory.NewWithValidTestData<OrgHeader>();
			var product = factory.NewWithValidTestData<OrgSalesProduct>();
			var today = ZDate.Today;
			var tradedPeriod = new ZDate(today.Year, today.Month, 1).AddMonths(-2);
			for (var i = 0; i < 10; i++)
			{
				var sales = org.SalesCollection.AddNew();
				sales.OW_MP_Product = product.PK;
				sales.OW_IsTraded = (i < 5);
				sales.OW_OriginID = countries[i].PK;
				sales.OW_OriginTableCode = RefCountrySchema.Constants.Prefix;

				for (var j = 0; j < 10; j++)
				{
					var detail = sales.TradeDetails.AddNew();

					if (i < 5)
					{
						var period = detail.TradedPeriods.AddNew();
						period.PAS_Period = tradedPeriod;
						period.PAS_OH_Client = org.PK;
					}
				}
			}

			factory.Save();

			return new SalesHeader(org, product);
		}

		#endregion

		#region TreeGroupers

		public void TestTreeGroupers()
		{
			AssertTreeGroupers(SalesAnalysisMainGroupingTypeList.Codes.Location, SalesAnalysisLocationGroupingTypeList.Codes.CountryToCountry,
				typeof(TradePeriodModeGrouper), typeof(TradePeriodTypeGrouper), typeof(TradePeriodSupplierAndBuyerGrouper));

			AssertTreeGroupers(SalesAnalysisMainGroupingTypeList.Codes.Mode, SalesAnalysisLocationGroupingTypeList.Codes.CountryToCountry,
				typeof(TradePeriodCountryToCountryGrouper), typeof(TradePeriodOriginToDestinationDescriptionGrouper), typeof(TradePeriodSupplierAndBuyerGrouper));

			AssertTreeGroupers(SalesAnalysisMainGroupingTypeList.Codes.Mode, SalesAnalysisLocationGroupingTypeList.Codes.DestinationCountry,
				typeof(TradePeriodDestinationCountryGrouper), typeof(TradePeriodOriginToDestinationDescriptionGrouper), typeof(TradePeriodSupplierAndBuyerGrouper));

			AssertTreeGroupers(SalesAnalysisMainGroupingTypeList.Codes.Mode, SalesAnalysisLocationGroupingTypeList.Codes.DestinationState,
				typeof(TradePeriodDestinationStateGrouper), typeof(TradePeriodOriginToDestinationDescriptionGrouper), typeof(TradePeriodSupplierAndBuyerGrouper));

			AssertTreeGroupers(SalesAnalysisMainGroupingTypeList.Codes.Mode, SalesAnalysisLocationGroupingTypeList.Codes.LowestToLowest,
				typeof(TradePeriodOriginToDestinationDescriptionGrouper), typeof(TradePeriodSupplierAndBuyerGrouper));

			AssertTreeGroupers(SalesAnalysisMainGroupingTypeList.Codes.Mode, SalesAnalysisLocationGroupingTypeList.Codes.OriginCountry,
				typeof(TradePeriodOriginCountryGrouper), typeof(TradePeriodOriginToDestinationDescriptionGrouper), typeof(TradePeriodSupplierAndBuyerGrouper));

			AssertTreeGroupers(SalesAnalysisMainGroupingTypeList.Codes.Mode, SalesAnalysisLocationGroupingTypeList.Codes.OriginState,
				typeof(TradePeriodOriginStateGrouper), typeof(TradePeriodOriginToDestinationDescriptionGrouper), typeof(TradePeriodSupplierAndBuyerGrouper));

			AssertTreeGroupers(SalesAnalysisMainGroupingTypeList.Codes.Mode, SalesAnalysisLocationGroupingTypeList.Codes.StateToState,
				typeof(TradePeriodStateToStateGrouper), typeof(TradePeriodOriginToDestinationDescriptionGrouper), typeof(TradePeriodSupplierAndBuyerGrouper));

			AssertTreeGroupers(SalesAnalysisMainGroupingTypeList.Codes.Mode, SalesAnalysisLocationGroupingTypeList.Codes.UnlocoToUnloco,
				typeof(TradePeriodUnlocoToUnlocoGrouper), typeof(TradePeriodSupplierAndBuyerGrouper));
		}

		public void TestTreeGroupers_Warehouse()
		{
			AssertTreeGroupers(SystemDefinedSalesProductList.Codes.Warehouse,
				SalesAnalysisMainGroupingTypeList.Codes.Location, SalesAnalysisLocationGroupingTypeList.Codes.Warehouse,
				typeof(TradePeriodServiceGrouper), typeof(TradePeriodSupplierPartGrouper), typeof(TradePeriodSupplierAndBuyerGrouper));

			AssertTreeGroupers(SystemDefinedSalesProductList.Codes.Warehouse,
				SalesAnalysisMainGroupingTypeList.Codes.Service, SalesAnalysisLocationGroupingTypeList.Codes.Warehouse,
				typeof(TradePeriodWarehouseGrouper), typeof(TradePeriodSupplierPartGrouper), typeof(TradePeriodSupplierAndBuyerGrouper));

			AssertTreeGroupers(SystemDefinedSalesProductList.Codes.Warehouse,
				SalesAnalysisMainGroupingTypeList.Codes.Service, SalesAnalysisLocationGroupingTypeList.Codes.WarehouseCountry,
				typeof(TradePeriodWarehouseCountryGrouper), typeof(TradePeriodSupplierPartGrouper), typeof(TradePeriodSupplierAndBuyerGrouper));

			AssertTreeGroupers(SystemDefinedSalesProductList.Codes.Warehouse,
				SalesAnalysisMainGroupingTypeList.Codes.Product, SalesAnalysisLocationGroupingTypeList.Codes.Warehouse,
				typeof(TradePeriodWarehouseGrouper), typeof(TradePeriodServiceGrouper), typeof(TradePeriodSupplierAndBuyerGrouper));

			AssertTreeGroupers(SystemDefinedSalesProductList.Codes.Warehouse,
				SalesAnalysisMainGroupingTypeList.Codes.Product, SalesAnalysisLocationGroupingTypeList.Codes.WarehouseCountry,
				typeof(TradePeriodWarehouseCountryGrouper), typeof(TradePeriodServiceGrouper), typeof(TradePeriodSupplierAndBuyerGrouper));
		}

		public void TestTreeGroupers_CustomsBrokerage()
		{
			AssertTreeGroupers(SystemDefinedSalesProductList.Codes.CustomsBrokerage,
				SalesAnalysisMainGroupingTypeList.Codes.Location, SalesAnalysisLocationGroupingTypeList.Codes.CountryToCountry,
				typeof(TradePeriodModeGrouper), typeof(TradePeriodTypeGrouper), typeof(TradePeriodCompanyGrouper), typeof(TradePeriodSupplierAndBuyerGrouper));

			AssertTreeGroupers(SystemDefinedSalesProductList.Codes.CustomsBrokerage,
				SalesAnalysisMainGroupingTypeList.Codes.Mode, SalesAnalysisLocationGroupingTypeList.Codes.CountryToCountry,
				typeof(TradePeriodCountryToCountryGrouper), typeof(TradePeriodUnlocoToUnlocoGrouper), typeof(TradePeriodCompanyGrouper), typeof(TradePeriodSupplierAndBuyerGrouper));

			AssertTreeGroupers(SystemDefinedSalesProductList.Codes.CustomsBrokerage,
				SalesAnalysisMainGroupingTypeList.Codes.Mode, SalesAnalysisLocationGroupingTypeList.Codes.DestinationCountry,
				typeof(TradePeriodDestinationCountryGrouper), typeof(TradePeriodUnlocoToUnlocoGrouper), typeof(TradePeriodCompanyGrouper), typeof(TradePeriodSupplierAndBuyerGrouper));

			AssertTreeGroupers(SystemDefinedSalesProductList.Codes.CustomsBrokerage,
				SalesAnalysisMainGroupingTypeList.Codes.Mode, SalesAnalysisLocationGroupingTypeList.Codes.DestinationState,
				typeof(TradePeriodDestinationStateGrouper), typeof(TradePeriodUnlocoToUnlocoGrouper), typeof(TradePeriodCompanyGrouper), typeof(TradePeriodSupplierAndBuyerGrouper));

			AssertTreeGroupers(SystemDefinedSalesProductList.Codes.CustomsBrokerage,
				SalesAnalysisMainGroupingTypeList.Codes.Mode, SalesAnalysisLocationGroupingTypeList.Codes.LowestToLowest,
				typeof(TradePeriodOriginToDestinationDescriptionGrouper), typeof(TradePeriodCompanyGrouper), typeof(TradePeriodSupplierAndBuyerGrouper));

			AssertTreeGroupers(SystemDefinedSalesProductList.Codes.CustomsBrokerage,
				SalesAnalysisMainGroupingTypeList.Codes.Mode, SalesAnalysisLocationGroupingTypeList.Codes.OriginCountry,
				typeof(TradePeriodOriginCountryGrouper), typeof(TradePeriodUnlocoToUnlocoGrouper), typeof(TradePeriodCompanyGrouper), typeof(TradePeriodSupplierAndBuyerGrouper));

			AssertTreeGroupers(SystemDefinedSalesProductList.Codes.CustomsBrokerage,
				SalesAnalysisMainGroupingTypeList.Codes.Mode, SalesAnalysisLocationGroupingTypeList.Codes.OriginState,
				typeof(TradePeriodOriginStateGrouper), typeof(TradePeriodUnlocoToUnlocoGrouper), typeof(TradePeriodCompanyGrouper), typeof(TradePeriodSupplierAndBuyerGrouper));

			AssertTreeGroupers(SystemDefinedSalesProductList.Codes.CustomsBrokerage,
				SalesAnalysisMainGroupingTypeList.Codes.Mode, SalesAnalysisLocationGroupingTypeList.Codes.StateToState,
				typeof(TradePeriodStateToStateGrouper), typeof(TradePeriodUnlocoToUnlocoGrouper), typeof(TradePeriodCompanyGrouper), typeof(TradePeriodSupplierAndBuyerGrouper));

			AssertTreeGroupers(SystemDefinedSalesProductList.Codes.CustomsBrokerage,
				SalesAnalysisMainGroupingTypeList.Codes.Mode, SalesAnalysisLocationGroupingTypeList.Codes.UnlocoToUnloco,
				typeof(TradePeriodUnlocoToUnlocoGrouper), typeof(TradePeriodCompanyGrouper), typeof(TradePeriodSupplierAndBuyerGrouper));
		}

		void AssertTreeGroupers(string mainGroupingType, string locationGroupingType, params Type[] expectedDetailGrouperTypes)
		{
			AssertTreeGroupers(SystemDefinedSalesProductList.Codes.Transport, mainGroupingType, locationGroupingType, expectedDetailGrouperTypes);
		}

		void AssertTreeGroupers(string productCode, string mainGroupingType, string locationGroupingType, params Type[] expectedDetailGrouperTypes)
		{
			var salesProduct = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, productCode);
			var salesAnalysis = GetNewSalesAnalysis(salesProduct);
			AssertNotNull(salesAnalysis.MainGroupings);

			salesAnalysis.MainGroupingType = mainGroupingType;
			salesAnalysis.LocationGroupingType = locationGroupingType;
			AssertMultilineASCIIEquals(string.Format("TreeGrouper Types for Main Grouping:{0} Location Grouping:{1}", mainGroupingType, locationGroupingType),
				string.Join(System.Environment.NewLine, expectedDetailGrouperTypes.Select(x => x.Name)),
				string.Join(System.Environment.NewLine, salesAnalysis.TreeGroupers.Select(x => x.GetType().Name)));
		}

		#endregion

		#region Lookups

		public void TestMainGroupingTypeList()
		{
			AssertMainGroupingTypeList(SystemDefinedSalesProductList.Codes.ForwardingShipment,
				new[]
				{
					SalesAnalysisMainGroupingTypeList.Codes.Location,
					SalesAnalysisMainGroupingTypeList.Codes.Mode,
					SalesAnalysisMainGroupingTypeList.Codes.ModeAndType
				});

			AssertMainGroupingTypeList(SystemDefinedSalesProductList.Codes.Warehouse,
				new[]
				{
					SalesAnalysisMainGroupingTypeList.Codes.Location,
					SalesAnalysisMainGroupingTypeList.Codes.Product,
					SalesAnalysisMainGroupingTypeList.Codes.Service
				});
		}

		void AssertMainGroupingTypeList(string productCode, string[] expectedItems)
		{
			var salesProduct = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, productCode);
			var salesAnalysis = GetNewSalesAnalysis(salesProduct);
			AssertArrayEqualsByElements(expectedItems, salesAnalysis.MainGroupingTypeList.Cast<ICodeDescription>().Select(x => x.Code).ToArray());
		}

		public void TestLocationGroupingTypeList()
		{
			AssertLocationGroupingTypeList(SystemDefinedSalesProductList.Codes.Transport,
				new[]
				{
					SalesAnalysisLocationGroupingTypeList.Codes.CountryToCountry,
					SalesAnalysisLocationGroupingTypeList.Codes.DestinationCountry,
					SalesAnalysisLocationGroupingTypeList.Codes.DestinationState,
					SalesAnalysisLocationGroupingTypeList.Codes.LowestToLowest,
					SalesAnalysisLocationGroupingTypeList.Codes.OriginCountry,
					SalesAnalysisLocationGroupingTypeList.Codes.OriginState,
					SalesAnalysisLocationGroupingTypeList.Codes.StateToState,
					SalesAnalysisLocationGroupingTypeList.Codes.Warehouse,
					SalesAnalysisLocationGroupingTypeList.Codes.WarehouseCountry
				});

			AssertLocationGroupingTypeList(SystemDefinedSalesProductList.Codes.ForwardingShipment,
				new[]
				{
					SalesAnalysisLocationGroupingTypeList.Codes.CountryToCountry,
					SalesAnalysisLocationGroupingTypeList.Codes.DestinationCountry,
					SalesAnalysisLocationGroupingTypeList.Codes.DestinationState,
					SalesAnalysisLocationGroupingTypeList.Codes.OriginCountry,
					SalesAnalysisLocationGroupingTypeList.Codes.OriginState,
					SalesAnalysisLocationGroupingTypeList.Codes.StateToState,
					SalesAnalysisLocationGroupingTypeList.Codes.UnlocoToUnloco
				});

			AssertLocationGroupingTypeList(SystemDefinedSalesProductList.Codes.Warehouse,
				new[]
				{
					SalesAnalysisLocationGroupingTypeList.Codes.Warehouse,
					SalesAnalysisLocationGroupingTypeList.Codes.WarehouseCountry
				});
		}

		void AssertLocationGroupingTypeList(string productCode, string[] expectedItems)
		{
			var salesProduct = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, productCode);
			var salesAnalysis = GetNewSalesAnalysis(salesProduct);
			AssertArrayEqualsByElements(expectedItems, salesAnalysis.LocationGroupingTypeList.Cast<ICodeDescription>().Select(x => x.Code).ToArray());
		}

		#endregion

		#region Overrides

		protected override BusinessObject GetNewBusinessObject()
		{
			var org = Factory.New<OrgHeader>();
			var salesHeader = new SalesHeader(org, Factory.New<OrgSalesProduct>());
			return new TradedSalesAnalysis(salesHeader);
		}

		#endregion

		#region Implementation

		TradedSalesAnalysis GetNewSalesAnalysis(OrgSalesProduct product)
		{
			var org = Factory.New<OrgHeader>();
			var salesHeader = new SalesHeader(org, product);
			return new TradedSalesAnalysis(salesHeader);
		}

		TradedSalesAnalysis GetNewSalesAnalysis()
		{
			var product = Factory.New<OrgSalesProduct>();
			return GetNewSalesAnalysis(product);
		}

		#endregion
	}
}
