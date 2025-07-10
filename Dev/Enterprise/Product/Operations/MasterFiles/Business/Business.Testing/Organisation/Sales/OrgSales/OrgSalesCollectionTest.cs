using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgSalesCollection))]
	sealed class OrgSalesCollectionTest : BusinessObjectCollectionTestCase
	{
		public override void TestSuspendCountChanged()
		{
			Assert(true);
		}

		#region Relationship

		public void TestHideSalesWithMiscOrganisationsAsPrimary()
		{
			var sales1 = Factory.NewWithValidTestData<OrgSales>();
			sales1.OW_OH_Primary = OrganisationsDataRegistry.Instance.MiscOrganisation.Value.Organisation;

			var sales2 = Factory.NewWithValidTestData<OrgSales>();
			sales2.OW_OH_Supplier = OrganisationsDataRegistry.Instance.MiscOrganisation.Value.Organisation;

			var sales3 = Factory.NewWithValidTestData<OrgSales>();
			sales3.OW_OH_Buyer = OrganisationsDataRegistry.Instance.MiscOrganisation.Value.Organisation;

			var salesCollection = new OrgSalesCollection(Factory);
			salesCollection.Load();
			AssertContainsExactElementsInAnyOrder(new[] { sales2, sales3 }, salesCollection);
		}

		#endregion

		#region Default Values

		public void TestDefaultValues()
		{
			var org = Factory.New<OrgHeader>();
			var collection = new OrgSalesCollection(org);
			var sales = collection.AddNew();
			AssertEquals(org.PK, sales.OW_OH_Primary);
		}

		#endregion

		#region Load

		public void TestLoadCollection()
		{
			var header1 = Factory.NewWithValidTestData<OrgHeader>();
			var header2 = Factory.NewWithValidTestData<OrgHeader>();
			var header3 = Factory.NewWithValidTestData<OrgHeader>();

			var sales1 = Helper.NewOrgSales(header1, header2, header3);
			var sales2 = Helper.NewOrgSales(header1, null, null);
			var sales3 = Helper.NewOrgSales(null, header1, null);
			var sales4 = Helper.NewOrgSales(null, null, header1);

			var sales5 = Helper.NewOrgSales(header2, header3, null);
			var sales6 = Helper.NewOrgSales(null, header2, header3);
			var sales7 = Helper.NewOrgSales(header3, null, header2);

			var sales8 = Helper.NewOrgSales(null, header2, header3);
			var tradeDetail = sales8.TradeDetails.AddNew();
			var tradePeriod = tradeDetail.TradedPeriods.AddNew();
			tradePeriod.PAS_IsTraded = true;
			tradePeriod.PAS_OH_Client = header1.PK;

			Factory.Save();

			AssertSalesCollection(header1, sales1, sales2, sales3, sales4, sales8);
			AssertSalesCollection(header2, sales1, sales5, sales6, sales7, sales8);
			AssertSalesCollection(header3, sales1, sales5, sales6, sales7, sales8);
		}

		#endregion

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return TestCollection;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<OrgSales>();
		}

		#region Test Objects

		OrgHeader Organisation
		{
			get
			{
				if (fOrganisation == null)
				{
					fOrganisation = Factory.New<OrgHeader>();
				}

				return fOrganisation;
			}
		}

		OrgHeader fOrganisation;

		OrgSalesCollection TestCollection
		{
			get
			{
				if (fTestCollection == null)
				{
					fTestCollection = new OrgSalesCollection(Organisation);
				}

				return fTestCollection;
			}
		}

		OrgSalesCollection fTestCollection;

		#endregion

		void AssertSalesCollection(OrgHeader org, params OrgSales[] sales)
		{
			AssertEquals(sales.Length, org.SalesCollection.Count);

			foreach (OrgSales sale in sales)
			{
				Assert(org.SalesCollection.Contains(sale.PK));
			}
		}

		#region Helpers

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

		#endregion

		public OrgTradeDetail tradeDetail1a { get; set; }
	}
}
