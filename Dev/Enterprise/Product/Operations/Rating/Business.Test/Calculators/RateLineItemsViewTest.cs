using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	[TestedType(typeof(RateLineItemsView))]
	public class RateLineItemsViewTest : BusinessObjectCollectionViewTestCase<RateLineItemsView>
	{
		public void TestConstructorLoadsAndSortsView_CombinedCalculator()
		{
			var rateLine = GetRateEntry().AddRateLine("ODOC", CombinedCalculator.Code, QuantityUnit.KG);
			var existingItemsCount = rateLine.RateLineItems.Count;

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
			AssertEquals(message, plus10, rateLine.RateLineItems[existingItemsCount]);
			AssertEquals(message, minus10, rateLine.RateLineItems[existingItemsCount + 1]);
			AssertEquals(message, plus200, rateLine.RateLineItems[existingItemsCount + 2]);
			AssertEquals(message, plus50, rateLine.RateLineItems[existingItemsCount + 3]);

			var view = new RateLineItemsView(rateLine.RateLineItems);
			message = "Expected items to be sorted by type and break for the CMB calculator.";
			AssertEquals("For the view we expect only the visible items for the user i.e the sliding items", 4, view.Count);
			AssertEquals(message, 1m, view[0].TM_Value);
			AssertEquals(message, 2m, view[1].TM_Value);
			AssertEquals(message, 3m, view[2].TM_Value);
			AssertEquals(message, 4m, view[3].TM_Value);

			message = "But the CollectionToFilter should remain unsorted to avoid terrible terrible performance problems.";
			var rateLineItemsCollection = view.CollectionToFilter.ToArray();
			AssertEquals(message, plus10, rateLineItemsCollection[existingItemsCount]);
			AssertEquals(message, minus10, rateLineItemsCollection[existingItemsCount + 1]);
			AssertEquals(message, plus200, rateLineItemsCollection[existingItemsCount + 2]);
			AssertEquals(message, plus50, rateLineItemsCollection[existingItemsCount + 3]);
		}

		public void TestConstructorLoadsAndSortsView_CartageZoneDistanceCalculator()
		{
			var transportZoneSet = Factory.NewWithValidTestData<RateTransportProvider>();
			transportZoneSet.TP_RN_NKCountry = Core.Constants.CountryCodes.Australia;

			var zone1 = transportZoneSet.Zones.AddNew();
			var zone2 = transportZoneSet.Zones.AddNew();
			zone1.TZ_ZoneName = "Zone1";
			zone2.TZ_ZoneName = "Zone2";

			Factory.Save();

			var rateLine = GetRateEntry().AddRateLine("OCART", CartageZoneDistanceCalculator.Code, QuantityUnit.KG);
			var calculator = rateLine.Calculator;
			var calculatorCreatedItems = rateLine.RateLineItems.Cast<RateLineItem>().Select(ItemDesc).ToArray();

			var zone1Item0 = calculator.AddRateLineItemWithZone(Calculator.Items.Operator.BAS, 0m, 100m, zone1.PK);
			var zone1Item2 = calculator.AddRateLineItemWithZone(Calculator.Items.Operator.UNT, 0m, 10m, zone1.PK);
			var zone1Item1 = calculator.AddRateLineItemWithZone(Calculator.Items.Operator.MIN, 0m, 50m, zone1.PK);

			var standardItem2 = calculator.AddRateLineItem(Calculator.Items.Operator.Plus, 33m, 300m);

			var zone2Item4 = calculator.AddRateLineItemWithZone(Calculator.Items.Operator.Plus, 100m, 4m, zone2.PK);
			var zone2Item2 = calculator.AddRateLineItemWithZone(Calculator.Items.Operator.Plus, 10m, 2m, zone2.PK);
			var zone2Item1 = calculator.AddRateLineItemWithZone(Calculator.Items.Operator.Minus, 10m, 1m, zone2.PK);
			var zone2Item3 = calculator.AddRateLineItemWithZone(Calculator.Items.Operator.Plus, 50m, 3m, zone2.PK);
			var zone2Item0 = calculator.AddRateLineItemWithZone(Calculator.Items.Operator.BAS, 0m, 80m, zone2.PK);

			var standardItem1 = calculator.AddRateLineItem(Calculator.Items.Operator.Plus, 11m, 200m);
			var standardItem0 = calculator.AddRateLineItem(Calculator.Items.Operator.Minus, 11m, 100m);

			Factory.Save();

			string ItemDesc(RateLineItem x)
			{
				return $"{x.TM_Type}-{x.TM_RelevantValue}";
			}

			#region Assert Zone1

			var cartageZones = calculator.CartageZones.Cast<CartageZone>().ToArray();

			var message = "Expected only the visible items from Zone1 and in the correct order";
			var expected = new[]
			{
				ItemDesc(zone1Item0),
				ItemDesc(zone1Item1),
				ItemDesc(zone1Item2),
			};

			var cartageZone1 = cartageZones.Single(x => x.ZonePK == zone1.PK);
			var actual = new RateLineItemsView(cartageZone1).Cast<RateLineItem>().Select(ItemDesc).ToArray();

			AssertArrayEqualsByElements(message, expected, actual);

			#endregion

			#region Assert Zone2

			message = "Expected only the visible items from Zone2 with the sliding calculators in order";
			expected = new[]
			{
				ItemDesc(zone2Item0),
				ItemDesc(zone2Item1),
				ItemDesc(zone2Item2),
				ItemDesc(zone2Item3),
				ItemDesc(zone2Item4),
			};

			var cartageZone2 = cartageZones.Single(x => x.ZonePK == zone2.PK);
			actual = new RateLineItemsView(cartageZone2).Cast<RateLineItem>().Select(ItemDesc).ToArray();

			AssertArrayEqualsByElements(message, expected, actual);

			#endregion

			#region Assert Standard Zone

			message = "Expected only the standard zone items to be visible";
			expected = new[]
			{
				ItemDesc(standardItem0),
				ItemDesc(standardItem1),
				ItemDesc(standardItem2),
			};

			var standardCartageZone = cartageZones.Single(x => x.ZonePK.IsEmpty);
			actual = new RateLineItemsView(standardCartageZone).Cast<RateLineItem>().Select(ItemDesc).ToArray();

			AssertArrayEqualsByElements(message, expected, actual);

			#endregion

			#region Assert RateLineItemCollection

			message = "RateLine's RateLineItemCollection should not be sorted by any of these constructors as it will raise performance issues";

			var rateLineItemsInOrder = new List<string>(calculatorCreatedItems)
			{
				ItemDesc(zone1Item0),
				ItemDesc(zone1Item2),
				ItemDesc(zone1Item1),
				ItemDesc(standardItem2),
				ItemDesc(zone2Item4),
				ItemDesc(zone2Item2),
				ItemDesc(zone2Item1),
				ItemDesc(zone2Item3),
				ItemDesc(zone2Item0),
				ItemDesc(standardItem1),
				ItemDesc(standardItem0)
			};
			expected = rateLineItemsInOrder.ToArray();
			actual = rateLine.RateLineItems.Cast<RateLineItem>().Select(ItemDesc).ToArray();

			AssertArrayEqualsByElements(message, expected, actual);

			#endregion
		}

		public void TestIsThisPartOfTheCollectionPEBCalculator()
		{
			var rateLine = GetRateEntry().RateLines.AddNew();
			rateLine.TL_RateCalculator = PercentageBreaksCalculator.Code;
			var view = new RateLineItemsViewForTest(rateLine.RateLineItems);

			var item = rateLine.RateLineItems.AddNew();
			item.TM_Type = "XXX";
			AssertEquals(true, view.IsThisPartOfTheCollection(item));

			item.TM_Type = CalculatorConstants.Type.ApplyTo;
			AssertEquals(false, view.IsThisPartOfTheCollection(item));

			item.TM_Type = CalculatorConstants.Text.IncludeGST;
			AssertEquals(false, view.IsThisPartOfTheCollection(item));

			item.TM_Type = PercentageBreaksCalculator.Items.GreaterCharge;
			AssertEquals(false, view.IsThisPartOfTheCollection(item));

			item.TM_Type = PercentageBreaksCalculator.Items.BreaksBasedOnValues;
			AssertEquals(false, view.IsThisPartOfTheCollection(item));

			item.TM_Type = BaseCombinedCalculator.Items.UseAccumulated;
			AssertEquals(false, view.IsThisPartOfTheCollection(item));

			item.TM_Type = BaseCombinedCalculator.Items.UseInclusiveBreaks;
			AssertEquals(false, view.IsThisPartOfTheCollection(item));

			item.TM_Type = BaseCombinedCalculator.Items.HigherChargeableLowerRate;
			AssertEquals(false, view.IsThisPartOfTheCollection(item));

			item.TM_Type = "ZZZ";
			AssertEquals(true, view.IsThisPartOfTheCollection(item));
		}

		public void TestDINCalculator()
		{
			var entry = GetRateEntry();
			var rateLine = entry.RateLines.AddNew();
			rateLine.TL_RateCalculator = DisbursementInterestCalculator.Code;
			var view = new RateLineItemsViewForTest(rateLine.RateLineItems);

			var item = rateLine.RateLineItems.AddNew();
			item.TM_Type = "XXX";
			AssertEquals(false, view.IsThisPartOfTheCollection(item));

			item.TM_Type = CalculatorConstants.Type.ApplyTo;
			AssertEquals(true, view.IsThisPartOfTheCollection(item));

			item.TM_Type = CalculatorConstants.Type.ApplyTo;
			item.TM_Text = Calculator.Items.Value.CalculationOrder;
			AssertEquals(true, view.IsThisPartOfTheCollection(item));
		}

		public void TestNTECalculator()
		{
			var entry = GetRateEntry();
			var rateLine = entry.RateLines.AddNew();
			rateLine.TL_RateCalculator = NoteCalculator.Code;
			var view = new RateLineItemsViewForTest(rateLine.RateLineItems);

			var item = rateLine.RateLineItems.AddNew();
			item.TM_Text = "note";
			AssertEquals(true, view.IsThisPartOfTheCollection(item));

			item.TM_Type = NoteCalculator.Items.ShowOnBillingWithoutPrefix;
			AssertEquals(false, view.IsThisPartOfTheCollection(item));
		}

		public void TestAllowNewIsAppliedForEqualizationCalculator()
		{
			var entry = GetRateEntry();
			var rateLine = entry.RateLines.AddNew();
			rateLine.TL_RateCalculator = EqualizationCalculator.Code;
			var view = new RateLineItemsViewForTest(rateLine.RateLineItems);

			var item1 = rateLine.RateLineItems.AddNew();
			item1.TM_Type = "+";
			AssertEquals(false, view.AllowNew);

			rateLine.RateLineItems.Remove(item1);
			AssertEquals(true, view.AllowNew);
		}

		[ExpectNoExceptions]
		public void TestAllowNewAndAllowRemoveDontAccessDeletedObject()
		{
			var entry = GetRateEntry();
			var rateLine = entry.RateLines.AddNew();
			rateLine.TL_RateCalculator = CombinedCalculator.Code;

			var view = new RateLineItemsView(rateLine.RateLineItems);

			entry.Delete();

			Assert("Should not allow new", !view.AllowNew);
			Assert("Should not allow new", !view.AllowRemove);
		}

		#region Implementation

		RateEntry GetRateEntry()
		{
			var helper = new TestHelper(Factory);
			var clientRate = helper.NewClientRate(helper.NewOrgHeader());

			return clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.LCL, "AU", "");
		}

		class RateLineItemsViewForTest : RateLineItemsView
		{
			public RateLineItemsViewForTest(RateLineItemsCollection rateLineItems)
				: base(rateLineItems, false)
			{
			}

			public new bool IsThisPartOfTheCollection(BusinessObject element)
			{
				return base.IsThisPartOfTheCollection(element);
			}
		}

		protected override RateLineItemsView GetCollectionToTest()
		{
			var clientRate = Factory.New<ClientRate>();
			var rateEntry = clientRate.AddRateEntry("AIR");
			var rateLine = rateEntry.RateLines.AddNew();

			return new RateLineItemsView(rateLine.RateLineItems);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Collection.AddNew();
		}

		public override void TestAdd()
		{
			var rateLineItemCollection = GetCollectionToTest();

			int initialCount = rateLineItemCollection.Count;

			var rateLineItem1 = rateLineItemCollection.AddNew();
			var rateLineItem2 = rateLineItemCollection.AddNew();

			AssertEquals("Precondition : Collection count", initialCount + 2, rateLineItemCollection.Count);

			rateLineItemCollection.RemoveAndDelete(rateLineItem1);

			AssertEquals("Collection count", initialCount + 1, rateLineItemCollection.Count);
			Assert("Contains element 2", rateLineItemCollection.Contains(rateLineItem2));
			Assert("Doesn't contain element 1", !rateLineItemCollection.Contains(rateLineItem1));
			Assert("Element 1 was removed, and deleted", rateLineItem1.IsDeleted);
		}

		public override void TestDelete()
		{
			var rateLineItemCollection = GetCollectionToTest();
			var rateLineItem1NotTobeDeleted = rateLineItemCollection.AddNew();
			var rateLineItem2ToBeDeleted = rateLineItemCollection.AddNew();

			AssertEquals("Precondition : Collection count", 2, rateLineItemCollection.Count);

			rateLineItemCollection.RemoveAndDelete(rateLineItem2ToBeDeleted);

			AssertEquals("Collection count", 1, rateLineItemCollection.Count);

			Assert("Doesn't contain element 1", !rateLineItemCollection.Contains(rateLineItem2ToBeDeleted));
			Assert("Element 1 was removed, and deleted", rateLineItem2ToBeDeleted.IsDeleted);
		}

		#endregion
	}
}
