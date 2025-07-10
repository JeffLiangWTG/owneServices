using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	[TestedType(typeof(TradeDetailSelectionItemCollection))]
	public class TradeDetailSelectionItemCollectionTest : NonPersistentBusinessObjectCollectionTestCase<TradeDetailSelectionItemCollection>
	{
		public void TestSelectAll()
		{
			var collection = new TradeDetailSelectionItemCollection();
			var item1 = collection.AddNew(Factory.New<OrgTradeDetail>());
			var item2 = collection.AddNew(Factory.New<OrgTradeDetail>());
			var item3 = collection.AddNew(Factory.New<OrgTradeDetail>());

			item1.Selected = false;
			item2.Selected = false;
			item3.Selected = true;

			collection.SelectAll();

			AssertEquals(true, item1.Selected);
			AssertEquals(true, item2.Selected);
			AssertEquals(true, item3.Selected);
		}

		public void TestDeselectAll()
		{
			var collection = new TradeDetailSelectionItemCollection();
			var item1 = collection.AddNew(Factory.New<OrgTradeDetail>());
			var item2 = collection.AddNew(Factory.New<OrgTradeDetail>());
			var item3 = collection.AddNew(Factory.New<OrgTradeDetail>());

			item1.Selected = true;
			item2.Selected = true;
			item3.Selected = false;

			collection.DeselectAll();

			AssertEquals(false, item1.Selected);
			AssertEquals(false, item2.Selected);
			AssertEquals(false, item3.Selected);
		}

		#region Allowed Actions

		public void TestAllowNew()
		{
			var collection = GetCollectionToTest();
			AssertEquals(false, collection.AllowNew);
		}

		public void TestAllowRemove()
		{
			var collection = GetCollectionToTest();
			AssertEquals(false, collection.AllowRemove);
		}

		#endregion

		#region Implementation

		protected override TradeDetailSelectionItemCollection GetCollectionToTest()
		{
			return new TradeDetailSelectionItemCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var tradeDetail = Factory.New<OrgTradeDetail>();
			return new TradeDetailSelectionItem(tradeDetail);
		}

		#endregion
	}
}
