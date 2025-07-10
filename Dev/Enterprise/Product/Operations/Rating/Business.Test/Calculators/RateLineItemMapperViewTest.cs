using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	[TestedType(typeof(RateLineItemMapperView<RateLineItem>))]
	public class RateLineItemMapperViewTest : BusinessObjectCollectionViewTestCase<RateLineItemMapperView<RateLineItem>>
	{
		#region IsRateLineItemShownOnGrid

		public void TestRateLineItemShownOnGrid_EquipmentHireCalculator()
		{
			var rateLine = GetRateLine(EquipmentHireCalculator.Code);
			var allowedAttributes = rateLine.Calculator.Attributes;

			AssertEquals("The EquipmentHire calculator has no attributes", 0, allowedAttributes.Length);
		}

		public void TestRateLineItemShownOnGrid_PercentageBreaksCalculator()
		{
			var rateLine = GetRateLine(PercentageBreaksCalculator.Code);
			var attributes = rateLine.Calculator.Attributes;

			var disallowedAttributes = attributes
				.Where(x => new string[] {
					CalculatorConstants.Type.ApplyTo,
					CalculatorConstants.Text.IncludeGST,
					PercentageBreaksCalculator.Items.GreaterCharge,
					PercentageBreaksCalculator.Items.BreaksBasedOnValues,
					BaseCombinedCalculator.Items.UseAccumulated,
					BaseCombinedCalculator.Items.UseInclusiveBreaks,
					BaseCombinedCalculator.Items.HigherChargeableLowerRate,
					BaseCombinedCalculator.Items.BreaksPer,
				}.Contains(x.ItemType))
				.ToArray();

			AssertRateLineItemIsShownOrNot(rateLine, disallowedAttributes, expectedShown: false);
		}

		void AssertRateLineItemIsShownOrNot(RateLine rateLine, CalculatorPropertyAttribute[] attributes, bool expectedShown)
		{
			var mapper = new RateLineItemsViewForTest(rateLine.RateLineItems);

			foreach (var attribute in attributes)
			{
				var rateLineItem = GetRateLineItem(rateLine, attribute.ItemType);
				var wasShown = mapper.IsRateLineItemShownOnGrid(rateLineItem);

				AssertEquals($"Expect {attribute.ItemType} to be {(expectedShown ? "shown" : "hidden")}", expectedShown, wasShown);
			}
			Assert("There must have been some attributes tested", attributes.Length > 0);
		}

		#endregion

		#region Implementation

		class RateLineItemsViewForTest : RateLineItemMapperView<RateLineItem>
		{
			public RateLineItemsViewForTest(RateLineItemsCollection rateLineItems)
				: base(rateLineItems, rateLineItems.Parent)
			{
			}

			public bool IsRateLineItemShownOnGrid(BusinessObject rateLineItems)
			{
				return IsThisPartOfTheCollection(rateLineItems);
			}
		}

		RateLine GetRateLine(string calculatorType)
		{
			var costing = Helper.NewCosting(Helper.NewOrgHeader());
			var rateEntry = costing.AddRateEntry("AIR", "LSE", "AUSYD", "", removeLines: true);
			var rateLine = rateEntry.AddRateLine("XXX", calculatorType ,lineUnit: "KG");

			return rateLine;
		}

		RateLineItem GetRateLineItem(RateLine rateLine, string tmType)
		{
			// Because this test is about the default RateLineItems created by a calculator,
			// then we should be able to just find the required tmType entry in the already existing
			// RateLineItems in the RateLine - that were created when the calculator was selected
			var item = rateLine
				.RateLineItems
				.Cast<RateLineItem>()
				.First(x => x.TM_Type == tmType);

			return item;
		}

		protected override RateLineItemMapperView<RateLineItem> GetCollectionToTest()
		{
			var clientRate = Factory.New<ClientRate>();
			var rateEntry = clientRate.AddRateEntry("AIR");
			var rateLine = rateEntry.RateLines.AddNew();

			return new RateLineItemsViewForTest(rateLine.RateLineItems);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Collection.AddNew();
		}

		public RateLineItemMapperViewTest()
		{
			Helper = new TestHelper(Factory);
		}

		TestHelper Helper { get; }

		#endregion
	}
}
