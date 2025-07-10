using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration.Rating;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	[TestedType(typeof(QuoteSelectionItemCollection))]
	public class QuoteSelectionItemCollectionTest : NonPersistentBusinessObjectCollectionTestCase<QuoteSelectionItemCollection>
	{
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

		#region Relationship

		public void TestOnlyOneItemSelectedAtATime()
		{
			var collection = new QuoteSelectionItemCollection();
			var item1 = collection.AddNew((IRelatableActivity)Factory.New<IQuote>());
			var item2 = collection.AddNew((IRelatableActivity)Factory.New<IQuote>());
			var item3 = collection.AddNew((IRelatableActivity)Factory.New<IQuote>());

			item1.Selected = true;
			AssertEquals(false, item2.Selected);
			AssertEquals(false, item3.Selected);

			item2.Selected = true;
			AssertEquals(false, item1.Selected);
			AssertEquals(false, item3.Selected);

			item3.Selected = true;
			AssertEquals(false, item1.Selected);
			AssertEquals(false, item2.Selected);
		}

		#endregion

		#region Overrides

		protected override QuoteSelectionItemCollection GetCollectionToTest()
		{
			return new QuoteSelectionItemCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var quote = (IRelatableActivity)Factory.New<IQuote>();
			return new QuoteSelectionItem(quote);
		}

		#endregion
	}
}
