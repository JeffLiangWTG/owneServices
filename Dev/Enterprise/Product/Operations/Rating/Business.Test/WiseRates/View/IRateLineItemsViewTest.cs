using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	[TestedType(typeof(IRateLineItemsView))]
	public class IRateLineItemsViewTest : BusinessObjectCollectionViewTestCase<IRateLineItemsView>
	{
		public void TestConstructionFromWiseLineItemViews()
		{
			var entry = GetRateEntry();
			var rateLine = entry.RateLines.AddNew();
			rateLine.TL_RateCalculator = CombinedCalculator.Code;

			var rateLineView = new WiseLineView(rateLine);
			var rateLineItemViews = new WiseLineItemViewsCollection(rateLine.ChildRateLineItems);

			var view = new IRateLineItemsViewForTest(rateLineItemViews, rateLineView);
			Assert(!view.AllowNew);
			Assert(!view.AllowRemove);
		}

		public void TestWiseLineItemViewsAreNotSorted()
		{
			var rateLine = GetRateEntry().AddRateLine("ODOC", CombinedCalculator.Code, QuantityUnit.KG);
			rateLine.RateLineItems.RemoveAndDeleteAll();

			var plus10 = rateLine.RateLineItems.AddNew();
			plus10.TM_Type = Calculator.Items.Operator.Plus;
			plus10.TM_Break = 10m;
			plus10.TM_Value = 2m;
			var minus10 = rateLine.RateLineItems.AddNew();
			minus10.TM_Type = Calculator.Items.Operator.Minus;
			minus10.TM_Break = 10m;
			minus10.TM_Value = 1m;
			var plus200 = rateLine.RateLineItems.AddNew();
			plus200.TM_Type = Calculator.Items.Operator.Plus;
			plus200.TM_Break = 200m;
			plus200.TM_Value = 4m;
			var plus50 = rateLine.RateLineItems.AddNew();
			plus50.TM_Type = Calculator.Items.Operator.Plus;
			plus50.TM_Break = 50m;
			plus50.TM_Value = 3m;

			Factory.Save();

			var message = "Once we skip the items added by default to the calculator, we expect to find the sliding rate line items in the order they were added.";
			AssertEquals(message, plus10, rateLine.RateLineItems[0]);
			AssertEquals(message, minus10, rateLine.RateLineItems[1]);
			AssertEquals(message, plus200, rateLine.RateLineItems[2]);
			AssertEquals(message, plus50, rateLine.RateLineItems[3]);

			var rateLineView = new WiseLineView(rateLine);
			var rateLineItemViews = new WiseLineItemViewsCollection(rateLine.ChildRateLineItems);

			var view = new IRateLineItemsViewForTest(rateLineItemViews, rateLineView);
			var viewCollection = view.CollectionToFilter;

			message = "For the view now we expect the sliding items to be ordered";
			AssertEquals(message, plus10.TM_Break, view[1].TM_Break);
			AssertEquals(message, plus50.TM_Break, view[2].TM_Break);
			AssertEquals(message, plus200.TM_Break, view[3].TM_Break);
		}

		#region Implementation

		RateEntry GetRateEntry()
		{
			var helper = new TestHelper(Factory);
			var clientRate = helper.NewClientRate(helper.NewOrgHeader());

			return clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.LCL, "AU", "");
		}

		protected override IRateLineItemsView GetCollectionToTest()
		{
			var clientRate = Factory.New<ClientRate>();
			var rateEntry = clientRate.AddRateEntry("AIR");
			var rateLine = rateEntry.RateLines.AddNew();

			return new IRateLineItemsView(new WiseLineItemViewsCollection(rateLine.RateLineItems.Cast<IRateLineItem>().ToArray()), rateLine);
		}

		public override void TestAdd()
		{
			Assert("Not going to add anything", true);
		}

		public override void TestDelete()
		{
			Assert("Not going to delete anything either", true);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Collection.AddNew();
		}

		public override void TestRemoveFromRelationship()
		{
			Assert("Will not be used", true);
		}

		public override void TestTypedget_Item()
		{
			Assert("Will not be used", true);
		}

		class IRateLineItemsViewForTest : IRateLineItemsView
		{
			public IRateLineItemsViewForTest(WiseLineItemViewsCollection wiseLineItems, IRateLine parent) : base(wiseLineItems, parent)
			{
			}
		}

		#endregion
	}
}
