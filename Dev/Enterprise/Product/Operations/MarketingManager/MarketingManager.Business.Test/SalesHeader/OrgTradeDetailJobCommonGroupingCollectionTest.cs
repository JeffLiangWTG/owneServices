using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MarketingManager.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(OrgTradeDetailJobCommonGroupingCollection))]
	sealed class OrgTradeDetailJobCommonGroupingCollectionTest : NonPersistentBusinessObjectCollectionTestCase<OrgTradeDetailJobCommonGroupingCollection>
	{
		public void TestDefaultElementIfNew()
		{
			var collection = new OrgTradeDetailJobCommonGroupingCollection(EntitySales.EntityTradeDetailsCompanyView, ForwardingShipmentsProduct);
			var grouping = collection.AddNew();
			AssertEquals(1, grouping.Elements.Count);
		}

		#region Overrides

		protected override OrgTradeDetailJobCommonGroupingCollection GetCollectionToTest()
		{
			return new OrgTradeDetailJobCommonGroupingCollection(EntitySales.EntityTradeDetailsCompanyView, ForwardingShipmentsProduct);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new OrgTradeDetailJobCommonGrouping(EntitySales.EntityTradeDetailsCompanyView, ForwardingShipmentsProduct);
		}

		#endregion

		#region Implementation

		OrgSalesProduct ForwardingShipmentsProduct
		{
			get
			{
				if (forwardingShipmentsProduct == null)
				{
					forwardingShipmentsProduct = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment);
					AssertNotNull("forwardingShipmentsProduct", forwardingShipmentsProduct);
				}

				return forwardingShipmentsProduct;
			}
		}
		OrgSalesProduct forwardingShipmentsProduct;

		EntitySalesWrapper EntitySales
		{
			get
			{
				if (entitySales == null)
				{
					var org = Factory.New<OrgHeader>();
					var sales = Factory.New<OrgSales>();
					entitySales = EntitySalesWrapper.Get(sales, org);
				}

				return entitySales;
			}
		}
		EntitySalesWrapper entitySales;

		#endregion
	}
}
