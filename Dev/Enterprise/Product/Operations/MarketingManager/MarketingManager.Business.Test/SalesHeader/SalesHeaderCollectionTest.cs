using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MarketingManager.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(SalesHeaderCollection))]
	sealed class SalesHeaderCollectionTest : NonPersistentBusinessObjectCollectionTestCase<SalesHeaderCollection>
	{
		#region Constructor

		public void TestConstructor()
		{
			var productAAA = Factory.NewWithValidTestData<OrgSalesProduct>();
			productAAA.MP_Code = "AAA";
			var productBBB = Factory.NewWithValidTestData<OrgSalesProduct>();
			productBBB.MP_Code = "BBB";
			var productCCC = Factory.NewWithValidTestData<OrgSalesProduct>();
			productCCC.MP_Code = "CCC";

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var salesAAA1 = org.SalesCollection.AddNew();
			salesAAA1.OW_MP_Product = productAAA.PK;
			var salesAAA2 = org.SalesCollection.AddNew();
			salesAAA2.OW_MP_Product = productAAA.PK;
			var salesBBB = org.SalesCollection.AddNew();
			salesBBB.OW_MP_Product = productBBB.PK;
			var salesNoProduct = org.SalesCollection.AddNew();
			salesNoProduct.OW_MP_Product = ZGuid.Empty;

			Factory.Save();

			var anotherFactory = new BusinessObjectFactory();
			var orgInOtherFactory = anotherFactory.Load<OrgHeader>(org.PK);
			var collection = new SalesHeaderCollection(orgInOtherFactory);
			AssertContainsExactElementsInAnyOrder("Should have one sales header for each distinct sales product",
				new ZString[]
				{
					"AAA",
					"BBB"
				},
				collection.Cast<SalesHeader>().Select(x => x.SalesProductCode));

			AssertEquals("Should not flag org as has changes", false, orgInOtherFactory.HasChanges);
			var expectedDbHits = new Dictionary<string, int>
			{
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ OrgSalesSchema.Constants.TableName, 2 },
				{ OrgSalesProductSchema.Constants.TableName, 1 }
			};
			AssertDbHits(expectedDbHits, anotherFactory);
		}

		public void TestConstructor_ExcludeTraded()
		{
			var productAAA = Factory.NewWithValidTestData<OrgSalesProduct>();
			productAAA.MP_Code = "AAA";
			var productBBB = Factory.NewWithValidTestData<OrgSalesProduct>();
			productBBB.MP_Code = "BBB";
			var productCCC = Factory.NewWithValidTestData<OrgSalesProduct>();
			productCCC.MP_Code = "CCC";

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var salesAAA1 = org.SalesCollection.AddNew();
			salesAAA1.OW_MP_Product = productAAA.PK;
			var salesAAA2 = org.SalesCollection.AddNew();
			salesAAA2.OW_MP_Product = productAAA.PK;
			var salesBBB = org.SalesCollection.AddNew();
			salesBBB.OW_MP_Product = productBBB.PK;
			salesBBB.OW_IsTraded = true;
			var salesNoProduct = org.SalesCollection.AddNew();
			salesNoProduct.OW_MP_Product = ZGuid.Empty;

			Factory.Save();

			var anotherFactory = new BusinessObjectFactory();
			var orgInOtherFactory = anotherFactory.Load<OrgHeader>(org.PK);
			var collection = new SalesHeaderCollection(orgInOtherFactory, false);
			AssertContainsExactElementsInAnyOrder("Should not include sales product BBB since it only contains traded",
				new ZString[]
				{
					"AAA"
				},
				collection.Cast<SalesHeader>().Select(x => x.SalesProductCode));
		}

		#endregion

		#region Allowed Actions

		public void TestAllowNew()
		{
			var org = Factory.New<OrgHeader>();
			var collection = new SalesHeaderCollection(org);
			AssertEquals(false, collection.AllowNew);
		}

		public void TestAllowRemove()
		{
			var org = Factory.New<OrgHeader>();
			var collection = new SalesHeaderCollection(org);
			AssertEquals(false, collection.AllowRemove);
		}

		#endregion

		#region Refresh

		public void TestShouldNotRemoveNewlyAddedSalesHeaderWhenApplyingSort()
		{
			var product = Factory.New<OrgSalesProduct>();
			var org = Factory.New<OrgHeader>();
			var collection = new SalesHeaderCollection(org);

			var newSalesHeader = collection.AddNew(product);

			IBindingListView bindingListView = collection.EntitySalesCollection;
			var properties = TypeDescriptor.GetProperties(typeof(SalesHeader));
			var propertyDesc = properties.Find("SalesProductCode", false);
			var sortDesc = new ListSortDescription(propertyDesc, ListSortDirection.Ascending);
			var sortCollection = new ListSortDescriptionCollection(new[] { sortDesc });
			bindingListView.ApplySort(sortCollection);

			AssertEquals(true, collection.Contains(newSalesHeader));
		}

		public void TestShouldNotAddSalesHeaderIfNoSalesAreAssociatedToEntity()
		{
			var brkProduct = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.CustomsBrokerage);
			var shpProduct = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment);
			var trnProduct = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.Transport);
			AssertEquals("Precondition", OrgSalesProductAssociationTarget.OrgSales | OrgSalesProductAssociationTarget.OrgTradeDetail, brkProduct.AllowedAssociationTargets);
			AssertEquals("Precondition", OrgSalesProductAssociationTarget.OrgSales | OrgSalesProductAssociationTarget.OrgTradeDetail, shpProduct.AllowedAssociationTargets);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var opp = org.SalesOpportunities.AddNew();

			var salesHeaderCollection = (SalesHeaderCollection)org.ActualAndProspectiveSalesHeaderCollection;
			var brkSalesHeader = salesHeaderCollection.AddNew(brkProduct);
			var brkProspectSales = brkSalesHeader.EntitySalesCollectionProductView.AddNew();
			var shpSalesHeader = salesHeaderCollection.AddNew(shpProduct);
			var shpProspectSales = shpSalesHeader.EntitySalesCollectionProductView.AddNew();
			shpProspectSales.SalesAssociationPivotCollectionGlobal.AddNew(opp);
			var trnSalesHeader = salesHeaderCollection.AddNew(trnProduct);
			var trnTradedSales = trnSalesHeader.TradedSalesCollectionProductView.AddNew();
			trnTradedSales.OW_IsTraded = true;
			Factory.Save();

			org.SalesCollection.Load();
			var collection = (SalesHeaderCollection)opp.ActualAndProspectiveSalesHeaderCollection;
			collection.Refresh();
			AssertContainsExactElementsInAnyOrder(@"Should
 - not include BRK product: because does not have a prospect sales associated to opp
 - include SHP product: because has a prospect sales that is associated to opp
 - include TRN product: because has a traded sales",
				new[]
				{
					SystemDefinedSalesProductList.Codes.ForwardingShipment,
					SystemDefinedSalesProductList.Codes.Transport
				},
				collection.Cast<SalesHeader>().Select(x => (string)x.SalesProductCode));
		}

		public void TestShouldRefreshWhenOrgInfoChanges()
		{
			var productA = Factory.New<OrgSalesProduct>();
			var productB = Factory.New<OrgSalesProduct>();

			var org1 = Factory.New<OrgHeader>();
			var opportunity = Factory.New<OrgOpportunity>();

			var actual1A = org1.SalesCollection.AddNew();
			actual1A.OW_IsTraded = true;
			actual1A.OW_MP_Product = productA.PK;
			var actual1B = org1.SalesCollection.AddNew();
			actual1B.OW_IsTraded = true;
			actual1B.OW_MP_Product = productB.PK;
			var prospect1A = org1.SalesCollection.AddNew();
			prospect1A.OW_MP_Product = productA.PK;
			prospect1A.SalesAssociationPivotCollectionGlobal.AddNew(opportunity);
			var prospect1B = org1.SalesCollection.AddNew();
			prospect1B.OW_MP_Product = productB.PK;
			prospect1B.SalesAssociationPivotCollectionGlobal.AddNew(opportunity);

			var org2 = Factory.New<OrgHeader>();
			var actual2A = org2.SalesCollection.AddNew();
			actual2A.OW_IsTraded = true;
			actual2A.OW_MP_Product = productA.PK;
			var prospect2A = org2.SalesCollection.AddNew();
			prospect2A.OW_MP_Product = productA.PK;
			prospect2A.SalesAssociationPivotCollectionGlobal.AddNew(opportunity);

			var salesHeaderCollection = (SalesHeaderCollection)opportunity.ActualAndProspectiveSalesHeaderCollection;
			AssertEquals(0, salesHeaderCollection.Count);

			opportunity.P8_OH = org1.PK;
			AssertEquals(2, salesHeaderCollection.Count);

			opportunity.P8_OH = org2.PK;
			AssertEquals(1, salesHeaderCollection.Count);

			opportunity.P8_OH = ZGuid.Invalid;
			AssertEquals(0, salesHeaderCollection.Count);
		}

		public void TestAllCurrenciesShouldBeValidatedOnRefresh()
		{
			var product = Factory.NewWithValidTestData<OrgSalesProduct>();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var opp = org.SalesOpportunities.AddNew();
			opp.P8_RX_NKEstimatedValueCurrency = "USD";
			var salesHeaderCollection = (SalesHeaderCollection)opp.ActualAndProspectiveSalesHeaderCollection;
			var salesHeader = salesHeaderCollection.AddNew(product);
			var sales = salesHeader.EntitySalesCollectionProductView.AddNew();
			var tradeDetail = sales.EntityTradeDetailsCollection.AddNew();
			tradeDetail.CurrencyCode = "GBP";

			Factory.Save();

			var anotherFactory = new BusinessObjectFactory();
			var oppInOtherFactory = anotherFactory.Load<OrgOpportunity>(opp.PK);
			var salesHeaderCollectionInOtherFactory = (SalesHeaderCollection)oppInOtherFactory.ActualAndProspectiveSalesHeaderCollection;
			var salesHeaderInOtherFactory = salesHeaderCollectionInOtherFactory.Cast<SalesHeader>().Single();
			var salesInOtherFactory = salesHeaderInOtherFactory.EntitySales.Single();
			var tradeDetailsInOtherFactory = salesInOtherFactory.EntityTradeDetails.Single();

			salesHeaderCollectionInOtherFactory.Refresh();

			AssertHasNotifications(tradeDetailsInOtherFactory.CurrencyCodeInfo);
		}

		#endregion

		#region AddNew

		public void TestAddNew_ChangingBizObjShouldSetHasChangedOnOrg()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();
			var collection = new SalesHeaderCollection(Org);
			AssertEquals("Precondition", false, org.HasChanges);

			var salesProduct = Factory.New<OrgSalesProduct>();
			var header = collection.AddNew(salesProduct);
			AssertEquals("Adding new header should NOT flag org as changed", false, org.HasChanges);

			header.EntitySalesCollectionProductView.AddNew();
			AssertEquals("Adding new sales to the header SHOULD flag org as changed", false, org.HasChanges);
		}

		#endregion

		#region Overrides

		protected override SalesHeaderCollection GetCollectionToTest()
		{
			var org = Factory.New<OrgHeader>();
			return new SalesHeaderCollection(org);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var product = Factory.New<OrgSalesProduct>();
			return new SalesHeader(Org, product);
		}

		#endregion

		#region Implementation

		OrgHeader Org
		{
			get { return org ?? (org = Factory.New<OrgHeader>()); }
		}
		OrgHeader org;

		#endregion
	}
}
