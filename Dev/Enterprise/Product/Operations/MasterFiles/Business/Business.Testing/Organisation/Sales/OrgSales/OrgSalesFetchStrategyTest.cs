using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MarketingManager.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgSalesFetchStrategyTest : TestCaseWithFactory
	{
		#region FetchForView

		#region OW_OriginID / OW_DestinationID

		public void TestFetchForView_OW_OriginID()
		{
			AddSalesForOriginDestinationFetchStrategyTest();

			var viewFactory = new BusinessObjectFactory();
			var testSalesCollection = viewFactory.Load<OrgSales>(new ZQuery(OrgSalesSchema.PK, testSalesPks));

			foreach (var sales in testSalesCollection)
			{
				sales.FetchStrategy.FetchForView(new[] { new TableColumn("", OrgSales.Schema.OW_OriginID) });
			}

			foreach (var sales in testSalesCollection)
			{
				object hitProperty = sales.Origin.VLO_Code;
			}

			var expectedDbHits = new Dictionary<string, int>();
			expectedDbHits.Add(OrgSalesSchema.Constants.TableName, 1);
			expectedDbHits.Add(ViewLocationSchema.Constants.TableName, 1);
			AssertDbHits(expectedDbHits, viewFactory);
		}

		public void TestFetchForView_OW_DestinationID()
		{
			AddSalesForOriginDestinationFetchStrategyTest();

			var viewFactory = new BusinessObjectFactory();
			var testSalesCollection = viewFactory.Load<OrgSales>(new ZQuery(OrgSalesSchema.PK, testSalesPks));

			foreach (var sales in testSalesCollection)
			{
				sales.FetchStrategy.FetchForView(new[] { new TableColumn("", OrgSales.Schema.OW_DestinationID) });
			}

			foreach (var sales in testSalesCollection)
			{
				object hitProperty = sales.Destination.VLO_Code;
			}

			var expectedDbHits = new Dictionary<string, int>();
			expectedDbHits.Add(OrgSalesSchema.Constants.TableName, 1);
			expectedDbHits.Add(ViewLocationSchema.Constants.TableName, 1);
			AssertDbHits(expectedDbHits, viewFactory);
		}

		void AddSalesForOriginDestinationFetchStrategyTest()
		{
			var countries = Factory.Load<RefCountry>(new ZQuery() { MaximumRows = 5, OrderBy = RefCountrySchema.Constants.RN_Code });

			var product = (BusinessObject)Factory.New<IOrgSalesProduct>();
			product.FillWithValidTestData();

			testSalesPks = new List<ZGuid>();

			for (var i = 0; i < 5; i++)
			{
				var origin = countries[i];
				var destination = countries[i];

				var prospective = Org.SalesCollection.AddNew();
				prospective.OW_MP_Product = product.PK;
				prospective.OW_OriginID = origin.PK;
				prospective.OW_OriginTableCode = origin.TablePrefix;
				prospective.OW_DestinationID = destination.PK;
				prospective.OW_DestinationTableCode = destination.TablePrefix;

				testSalesPks.Add(prospective.PK);
			}

			Factory.Save();
		}

		#endregion

		#endregion

		#region Implementation

		OrgHeader Org
		{
			get
			{
				if (org == null)
				{
					org = Factory.NewWithValidTestData<OrgHeader>();
				}

				return org;
			}
		}
		OrgHeader org;

		List<ZGuid> testSalesPks;

		#endregion
	}
}
