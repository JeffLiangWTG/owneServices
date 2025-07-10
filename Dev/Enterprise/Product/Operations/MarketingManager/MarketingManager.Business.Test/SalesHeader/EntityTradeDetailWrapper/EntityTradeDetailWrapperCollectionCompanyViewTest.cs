using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(EntityTradeDetailWrapperCollectionCompanyView))]
	sealed class EntityTradeDetailWrapperCollectionCompanyViewTest : BusinessObjectCollectionViewTestCase<EntityTradeDetailWrapperCollectionCompanyView>
	{
		public void TestIsThisPartOfTheCollection()
		{
			var org = Factory.New<OrgHeader>();
			var sales = org.SalesCollection.AddNew();
			var tradeDetail = sales.TradeDetails.AddNew();
			var salesWrapper = EntitySalesWrapper.Get(sales, org);
			var detailWrapper = salesWrapper.EntityTradeDetailsCollection[0];

			var collectionView = new EntityTradeDetailWrapperCollectionCompanyView(salesWrapper.EntityTradeDetailsCollection);

			salesWrapper.CompanyFilter = ZGuid.Empty;
			collectionView.Rebuild();
			AssertEquals(1, collectionView.Count);

			salesWrapper.CompanyFilter = Env.CurrentCompanyPK;
			collectionView.Rebuild();
			AssertEquals(0, collectionView.Count);

			detailWrapper.ProspectCompanyPks = new List<ZGuid>() { Env.CurrentCompanyPK };
			collectionView.Rebuild();
			AssertEquals(1, collectionView.Count);

			detailWrapper.ProspectCompanyPks = new List<ZGuid>() { ZGuid.Empty };
			collectionView.Rebuild();
			AssertEquals(0, collectionView.Count);

			detailWrapper.ProspectCompanyPks = new List<ZGuid>() { ZGuid.Empty, ZGuid.NewZGuid() };
			collectionView.Rebuild();
			AssertEquals(0, collectionView.Count);
		}

		#region Implementation

		protected override EntityTradeDetailWrapperCollectionCompanyView GetCollectionToTest()
		{
			var org = Factory.New<OrgHeader>();
			var sales = Factory.New<OrgSales>();
			sales.OW_MP_Product = Product.PK;
			var entitySales = EntitySalesWrapper.Get(sales, org);
			return entitySales.EntityTradeDetailsCompanyView;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var org = Factory.New<OrgHeader>();
			var sales = Factory.New<OrgSales>();
			sales.OW_MP_Product = Product.PK;
			var detail = sales.TradeDetails.AddNew();
			var element = EntityTradeDetailWrapper.Get(detail, org);
			element.ProspectCompanyPks = new List<ZGuid>() { Env.CurrentCompany.PK };
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
