using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Rating.Business.Testing;
using Enterprise.Rating.Business.WiseRates;
using WiseRates.Api.Model;

namespace Enterprise.Rating.Business.Test.WiseRates
{
	public class CalculatorDeciderTest : TestCaseWithFactory
	{
		public void TestHighestRateCalculatorResolver_DifferentUnitTypes_KG_M3()
			=> TestHighestRateCalculatorResolver_DifferentUnitTypes(unit1: "KG", unit2: "M3");
		public void TestHighestRateCalculatorResolver_DifferentUnitTypes_LB_CF()
			=> TestHighestRateCalculatorResolver_DifferentUnitTypes(unit1: "LB", unit2: "CF");
		public void TestHighestRateCalculatorResolver_DifferentUnitTypes_KG_CF()
			=> TestHighestRateCalculatorResolver_DifferentUnitTypes(unit1: "KG", unit2: "CF");
		public void TestHighestRateCalculatorResolver_DifferentUnitTypes_LB_M3()
			=> TestHighestRateCalculatorResolver_DifferentUnitTypes(unit1: "LB", unit2: "M3");

		void TestHighestRateCalculatorResolver_DifferentUnitTypes(string unit1, string unit2)
		{
			AssertDecider
			(
				messageForExpectedCode: "2 charges having different unit type should be equivalent to a HRC calculator",
				messageForExpectedItems: "the calculator should have 2 items of different unit types, MIN item should have the higher value of the MinRates",
				decider: CreateCalculatorDeciderWithSeaLcl(new[]
				{
					new Charge { ChargeCode = "FRT", PerUnitRate = 15, Unit = unit1, MinRate = 30, UnitMultiplier = 1 },
					new Charge { ChargeCode = "FRT", PerUnitRate = 20, Unit = unit2, MinRate = 40, UnitMultiplier = 5 },
				}),
				expectedCode: HighestRateCalculator.Code,
				expectedRateLineItems: new[]
				{
					CreateItemForComparison(tm_type: "UNT", tm_relevantValue: 15, tm_breakWeightVolume: unit1, tm_unitMultiple: 1),
					CreateItemForComparison(tm_type: "UNT", tm_relevantValue: 20, tm_breakWeightVolume: unit2, tm_unitMultiple: 5),
					CreateItemForComparison(tm_type: "MIN", tm_relevantValue: 40),
					CreateItemForComparison(tm_type: "RPR"),
				}
			);
		}

		public void TestHighestRateCalculatorResolver_WhenThereAre3Charges_ShouldNotResolve()
		{
			var decider = CreateCalculatorDeciderWithSeaLcl(new[]
			{
				new Charge { ChargeCode = "FRT", PerUnitRate = 1000, Unit = "LB", UnitMultiplier = 1000 },
			});
			AssertNotEquals("The decider code should not match the highest rate calculator code for a single charge.", HighestRateCalculator.Code, decider.Code);

			decider = CreateCalculatorDeciderWithSeaLcl(new[]
			{
				new Charge { ChargeCode = "FRT", PerUnitRate = 1000, Unit = "LB", UnitMultiplier = 1000 },
				new Charge { ChargeCode = "FRT", PerUnitRate = 4000, Unit = "LB", UnitMultiplier = 2000 },
				new Charge { ChargeCode = "FRT", PerUnitRate = 5000, Unit = "LB", UnitMultiplier = 3000 },
			});
			AssertNotEquals("The decider code should not match the highest rate calculator code for multiple charges.", HighestRateCalculator.Code, decider.Code);
		}

		public void TestHighestRateCalculatorResolver_SameUnit_ShouldNotResolve()
		{
			var decider = CreateCalculatorDeciderWithSeaLcl(new[]
			{
				new Charge { ChargeCode = "FRT", PerUnitRate = 1500, Unit = "LB", UnitMultiplier = 1000 },
				new Charge { ChargeCode = "FRT", PerUnitRate = 1000, Unit = "LB", UnitMultiplier = 1000 },
			});

			AssertNotEquals("The Code should not match the HighestRateCalculator code.", HighestRateCalculator.Code, decider.Code);
		}

		public void TestCombinedCalculatorResolver_WithMinimumBreak_FromCargoguide_MinimumPresent()
		{
			var typesOfInterest = new[] { Calculator.Items.Operator.MIN, Calculator.Items.Operator.Minus, Calculator.Items.Operator.Plus, Calculator.Items.UseInclusiveBreaks };
			var charges = new[]
			{
				new Charge() { ChargeCode = "FRT",  MinRate = 100 },
				new Charge() { ChargeCode = "FRT", Break = 200, Unit = Core.Constants.Weight.Kilograms, PerUnitRate = 3, BreakOperator = Calculator.Items.Operator.Plus },
				new Charge() { ChargeCode = "FRT", Break = 250, Unit = Core.Constants.Weight.Kilograms, PerUnitRate = 4, BreakOperator = Calculator.Items.Operator.Plus }
			};
			var decider = CreateCalculatorDeciderForCargoguide(charges);

			AssertEquals("The calculator code should match the combined calculator code.", CombinedCalculator.Code, decider.Code);

			var items = decider.RateLineItems
				.Where(x => typesOfInterest.Contains((string)x.TM_Type))
				.Select(x => $"{(string)x.TM_Type}|{(string)x.TM_Text}|{(decimal)x.TM_Break}|{(decimal)x.TM_RelevantValue}")
				.ToArray();

			var expectedItems = new[]
			{
				$"{Calculator.Items.UseInclusiveBreaks}|N|0|0",
				$"{Calculator.Items.Operator.MIN}||0|100",
				$"{Calculator.Items.Operator.Plus}||200|3",
				$"{Calculator.Items.Operator.Plus}||250|4",
			};

			AssertContainsExactElementsInAnyOrder(
				"The items should match the expected rate line items.",
				expectedItems,
				items
			);
		}

		public void TestCombinedCalculatorResolver_WithoutMinimumBreak_FirstBreakZero_FromCargoguide_MinimumAbsent()
		{
			/*
			 * When a CG rate has a break
			 *		+0: $10
			 *		+100: $20
			 * Then the equivalent is a CW1 break of -100: $10; +100; $20
			 *		-100: $10
			 *		+100: $20
			 *
			 * Because it does not make sense to introduce the Minimum concept since
			 * all quantities will be less than the initial break.
			 */
			var typesOfInterest = new[] { Calculator.Items.Operator.MIN, Calculator.Items.Operator.Minus, Calculator.Items.Operator.Plus, Calculator.Items.UseInclusiveBreaks };
			var charges = new[]
			{
				new Charge() { ChargeCode = "FRT", Break = 0, Unit = Core.Constants.Weight.Kilograms, PerUnitRate = 3, BreakOperator = Calculator.Items.Operator.Plus },
				new Charge() { ChargeCode = "FRT", Break = 250, Unit = Core.Constants.Weight.Kilograms, PerUnitRate = 4, BreakOperator = Calculator.Items.Operator.Plus }
			};
			var decider = CreateCalculatorDeciderForCargoguide(charges);

			AssertEquals("The calculator code should match the combined calculator code.", CombinedCalculator.Code, decider.Code);

			var items =
				decider.RateLineItems
					.Where(x => typesOfInterest.Contains((string)x.TM_Type))
					.Select(x => new
					{
						Type = (string)x.TM_Type,
						Text = (string)x.TM_Text,
						Break = (decimal)x.TM_Break,
						Value = (decimal)x.TM_RelevantValue
					})
					.Select(i => $"{i.Type}|{i.Text}|{i.Break}|{i.Value}")
					.ToArray();

			var expectedItems = new[] {
				$"{Calculator.Items.Operator.Minus}||250|3",
				$"{Calculator.Items.Operator.Plus}||250|4",
				$"{Calculator.Items.UseInclusiveBreaks}|N|0|0"
			};

			AssertContainsExactElementsInAnyOrder(
				"The calculated rate line items should match the expected collection.",
				expectedItems,
				items
			);
		}

		public void TestCombinedCalculatorResolver_WithoutMinimumBreak_FirstBreakNonZero_FromCargoguide_MinimumPresent()
		{
			/*
			 * When a CG rate has a break
			 *		+100: $20
			 *		+200: $30
			 * Then the equivalent is a CW1 break of -100: $10; +100; $20
			 *		Min 100: $0
			 *		Inclusive breaks
			 *		+100: $20
			 *		+200: $30
			 *
			 * Because the first + break in CG, being +100 implies that amounts
			 * of the unit lower than 100 have to be treated as 100
			 *
			 * Hence we need MIN 100: 0 and Inclusive Breaks.
			 */
			var typesOfInterest = new[] { Calculator.Items.Operator.MIN, Calculator.Items.Operator.Minus, Calculator.Items.Operator.Plus, Calculator.Items.UseInclusiveBreaks };
			var charges = new[]
			{
				new Charge() { ChargeCode = "FRT", Break = 200, Unit = Core.Constants.Weight.Kilograms, PerUnitRate = 3, BreakOperator = Calculator.Items.Operator.Plus },
				new Charge() { ChargeCode = "FRT", Break = 250, Unit = Core.Constants.Weight.Kilograms, PerUnitRate = 4, BreakOperator = Calculator.Items.Operator.Plus }
			};
			var decider = CreateCalculatorDeciderForCargoguide(charges);

			AssertEquals("The calculated code should match the expected code.", CombinedCalculator.Code, decider.Code);

			var items =
				decider.RateLineItems
					.Where(x => typesOfInterest.Contains((string)x.TM_Type))
					.Select(x => new
					{
						Type = (string)x.TM_Type,
						Text = (string)x.TM_Text,
						Break = (decimal)x.TM_Break,
						Value = (decimal)x.TM_RelevantValue
					})
					.Select(x => $"{x.Type}|{x.Text}|{x.Break}|{x.Value}")
					.ToArray();

			var expectedItems = new[]
			{
				$"{Calculator.Items.Operator.MIN}|{string.Empty}|200|0",
				$"{Calculator.Items.Operator.Plus}|{string.Empty}|200|3",
				$"{Calculator.Items.Operator.Plus}|{string.Empty}|250|4",
				$"{Calculator.Items.UseInclusiveBreaks}|Y|0|0",
			};

			AssertContainsExactElementsInAnyOrder(
				"The calculated rate line items should match the expected collection.",
				expectedItems,
				items
			);
		}

		public void TestCombinedCalculatorResolver_FlatRate_MinimumNotPresent()
		{
			var typesOfInterest = new[] { Calculator.Items.Operator.MIN, Calculator.Items.Operator.BAS };
			var charges = new[]
			{
				new Charge() { ChargeCode = "FRT", FlatRate = 200, BreakOperator = Calculator.Items.Operator.BAS }
			};
			var decider = CreateCalculatorDeciderForCargoguide(charges);

			AssertEquals("The calculator code should match the expected combined calculator code.", CombinedCalculator.Code, decider.Code);

			var items = decider.RateLineItems
				.Where(x => typesOfInterest.Contains((string)x.TM_Type))
				.Select(x => new
				{
					Type = (string)x.TM_Type,
					Text = (string)x.TM_Text,
					Break = (decimal)x.TM_Break,
					Value = (decimal)x.TM_RelevantValue
				})
				.Select(i => $"{i.Type}|{i.Text}|{i.Break}|{i.Value}")
				.ToArray();

			var expectedItems = new[]
			{
				$"{Calculator.Items.Operator.BAS}|{string.Empty}|0|200"
			};

			AssertContainsExactElementsInAnyOrder(
				"The filtered rate line items should match the expected collection.",
				expectedItems,
				items
			);
		}

		public void TestCombinedCalculatorResolver_WithMinimumBreak_FromCargoSphere_MinimumPresent()
		{
			var typesOfInterest = new[] { Calculator.Items.Operator.MIN, Calculator.Items.Operator.Minus, Calculator.Items.Operator.Plus, Calculator.Items.UseInclusiveBreaks };
			var charges = new[]
			{
				new Charge() { ChargeCode = "FRT", MinRate = 100 },
				new Charge() { ChargeCode = "FRT", Break = 200, Unit = Core.Constants.Weight.Kilograms, PerUnitRate = 3, BreakOperator = Calculator.Items.Operator.Plus },
				new Charge() { ChargeCode = "FRT", Break = 250, Unit = Core.Constants.Weight.Kilograms, PerUnitRate = 4, BreakOperator = Calculator.Items.Operator.Plus }
			};
			var decider = CreateCalculatorDeciderForCargoguide(charges);

			AssertEquals("The decider code should match the expected code.", CombinedCalculator.Code, decider.Code);

			var items =
				decider.RateLineItems
					.Where(x => typesOfInterest.Contains((string)x.TM_Type))
					.Select(x => new
					{
						Type = (string)x.TM_Type,
						Text = (string)x.TM_Text,
						Break = (decimal)x.TM_Break,
						Value = (decimal)x.TM_RelevantValue
					})
					.Select(x => $"{x.Type}|{x.Text}|{x.Break}|{x.Value}")
					.ToArray();

			var expectedItems = new[]
			{
				$"{Calculator.Items.UseInclusiveBreaks}|N|0|0",
				$"{Calculator.Items.Operator.MIN}||0|100",
				$"{Calculator.Items.Operator.Plus}||200|3",
				$"{Calculator.Items.Operator.Plus}||250|4",
			};

			AssertContainsExactElementsInAnyOrder(
				"The calculated rate line items should match the expected items.",
				expectedItems,
				items
			);
		}

		public void TestCombinedCalculatorResolver_WithoutMinimumBreak_FromCargoSphere_MinimumNotPresent()
		{
			/*
			 * Technically, this feature is CG, but no point limiting our code to be CG only because
			 * According to Kat, CS will not exercise this input at this stage.
			 * So, it does the Min like TestCombinedCalculatorResolver_WithoutMinimumBreak_FirstBreakNonZero_FromCargoguide_MinimumPresent
			 * also.
			 */
			var typesOfInterest = new[] { Calculator.Items.Operator.MIN, Calculator.Items.Operator.Minus, Calculator.Items.Operator.Plus, Calculator.Items.UseInclusiveBreaks };
			var charges = new[]
			{
				new Charge() { ChargeCode = "FRT", Break = 200, Unit = Core.Constants.Weight.Kilograms, PerUnitRate = 3, BreakOperator = Calculator.Items.Operator.Plus },
				new Charge() { ChargeCode = "FRT", Break = 250, Unit = Core.Constants.Weight.Kilograms, PerUnitRate = 4, BreakOperator = Calculator.Items.Operator.Plus }
			};
			var decider = CreateCalculatorDeciderForCargoguide(charges);

			AssertEquals("The calculator code does not match the expected CombinedCalculator code.", CombinedCalculator.Code, decider.Code);

			var items = decider.RateLineItems
				.Where(x => typesOfInterest.Contains((string)x.TM_Type))
				.Select(x => new
				{
					Type = (string)x.TM_Type,
					Text = (string)x.TM_Text,
					Break = (decimal)x.TM_Break,
					Value = (decimal)x.TM_RelevantValue
				})
				.Select(x => $"{x.Type}|{x.Text}|{x.Break}|{x.Value}")
				.ToArray();

			var expectedItems = new[]
			{
				$"{Calculator.Items.UseInclusiveBreaks}|Y|0|0",
				$"{Calculator.Items.Operator.MIN}||200|0",
				$"{Calculator.Items.Operator.Plus}||200|3",
				$"{Calculator.Items.Operator.Plus}||250|4"
			};

			AssertContainsExactElementsInAnyOrder(
				"The generated rate line items do not match the expected items.",
				expectedItems,
				items
			);
		}

		public void TestCombinedCalculatorResolver_WithZeroRatesOnBreaks()
		{
			var typesOfInterest = new[] { Calculator.Items.Operator.MIN, Calculator.Items.Operator.Minus, Calculator.Items.Operator.Plus };
			var charges = new[]
			{
				new Charge { ChargeCode = "FRT", BreakOperator = ">=", Break = 0, PerUnitRate = 0, Restricted = true },
				new Charge { ChargeCode = "FRT", BreakOperator = ">=", Break = 45, PerUnitRate = 0, Restricted = true },
				new Charge { ChargeCode = "FRT", BreakOperator = ">=", Break = 100, PerUnitRate = 0, Restricted = true },
				new Charge { ChargeCode = "FRT", BreakOperator = ">=", Break = 250, PerUnitRate = 0, Restricted = true },
				new Charge { ChargeCode = "FRT", BreakOperator = ">=", Break = 300, PerUnitRate = 0, Restricted = true },
			};
			var decider = CreateCalculatorDeciderForCargoguide(charges);

			AssertEquals("The calculator code should match the combined calculator code.", CombinedCalculator.Code, decider.Code);

			var items = decider.RateLineItems
				.Where(x => typesOfInterest.Contains((string)x.TM_Type))
				.Select(x => $"{(string)x.TM_Type}|{(decimal)x.TM_Break}|{(decimal)x.TM_RelevantValue}")
				.ToArray();

			var expectedItems = new[]
			{
				"-|45|0",
				"+|45|0",
				"+|100|0",
				"+|250|0",
				"+|300|0",
			};

			AssertContainsExactElementsInAnyOrder(
				"The items should match the expected rate line items.",
				expectedItems,
				items
			);
		}

		public void TestUrsRateMappingRulesForUldRates()
		{
			var typesOfInterest = new[] { Calculator.Items.Operator.MIN, Calculator.Items.Operator.Minus, Calculator.Items.Operator.Plus };
			var charges = new[]
			{
				new Charge() { ChargeCode = "FRT", Break = 200, Unit = Core.Constants.Weight.Kilograms, PerUnitRate = 3, BreakOperator = Calculator.Items.Operator.Plus },
				new Charge() { ChargeCode = "FRT", Break = 250, Unit = Core.Constants.Weight.Kilograms, PerUnitRate = 4, BreakOperator = Calculator.Items.Operator.Plus }
			};
			var decider = CreateCalculatorDeciderForUniversalRatesService(charges, Core.Constants.TransportModes.Air, Core.Constants.ContainerModes.ULD);
			AssertEquals(CombinedCalculator.Code, decider.Code);

			var actualItems =
				decider.RateLineItems
					.Where(x => typesOfInterest.Contains((string)x.TM_Type))
					.Select(x => new
					{
						Type = (string)x.TM_Type,
						Text = (string)x.TM_Text,
						Break = (decimal)x.TM_Break,
						Value = (decimal)x.TM_RelevantValue
					})
					.Select(x => $"{x.Type}|{x.Text}|{x.Break}|{x.Value}")
					.ToArray();

			var expectedItems = new[]
			{
				$"{Calculator.Items.Operator.Plus}||200|3",
				$"{Calculator.Items.Operator.Plus}||250|4",
				$"{Calculator.Items.Operator.MIN}||200|0"
			};

			AssertContainsExactElementsInAnyOrder(
				"Items should match expected rate line items for the given ULD rates",
				expectedItems,
				actualItems
			);
		}

		public void TestUrsRateMappingRulesForLooseRates()
		{
			var typesOfInterest = new[] { Calculator.Items.Operator.MIN, Calculator.Items.Operator.Minus, Calculator.Items.Operator.Plus };
			var charges = new[]
			{
				new Charge() { ChargeCode = "FRT", Break = 200, Unit = Core.Constants.Weight.Kilograms, PerUnitRate = 3, BreakOperator = Calculator.Items.Operator.Plus, Applicability = "TACT – TACT Reference rate" },
				new Charge() { ChargeCode = "FRT", Break = 250, Unit = Core.Constants.Weight.Kilograms, PerUnitRate = 4, BreakOperator = Calculator.Items.Operator.Plus, Applicability = "TACT – TACT Reference rate" }
			};
			var decider = CreateCalculatorDeciderForUniversalRatesService(charges, Core.Constants.TransportModes.Air, Core.Constants.ContainerModes.Loose);
			AssertEquals("The calculator code should match the expected code.", CombinedCalculator.Code, decider.Code);

			var items = decider.RateLineItems
				.Where(x => typesOfInterest.Contains((string)x.TM_Type))
				.Select(x => new
				{
					Type = (string)x.TM_Type,
					Text = (string)x.TM_Text,
					Break = (decimal)x.TM_Break,
					Value = (decimal)x.TM_RelevantValue,
				})
				.Select(i => $"{i.Type}|{i.Text}|{i.Break}|{i.Value}")
				.ToArray();

			var expectedItems = new[]
			{
				$"{Calculator.Items.Operator.Plus}|TACT – TACT Reference rate|200|3",
				$"{Calculator.Items.Operator.Plus}|TACT – TACT Reference rate|250|4",
				$"{Calculator.Items.Operator.MIN}||200|0",
			};

			AssertContainsExactElementsInAnyOrder("The mapped items should match the expected items.", expectedItems, items);
		}

		#region Helpers

		CalculatorDecider CreateCalculatorDeciderWithSeaLcl(
			Charge[] charges) => CreateCalculatorDecider(
				charges,
				Core.Constants.TransportModes.Sea,
				Core.Constants.ContainerModes.LCL);

		CalculatorDecider CreateCalculatorDeciderForCargoguide(
			Charge[] charges) => CreateCalculatorDecider(
				charges,
				Core.Constants.TransportModes.Air,
				Core.Constants.ContainerModes.ULD);

		CalculatorDecider CreateCalculatorDeciderForUniversalRatesService(
			Charge[] charges, string transportMode, string contrainerMode) => CreateCalculatorDecider(
				charges,
				transportMode,
				contrainerMode);

		CalculatorDecider CreateCalculatorDecider(
			Charge[] charges,
			string transportMode,
			string containerMode,
			string origin = "AUSYD",
			string destination = "USLAX")
		{
			var wiseEntry = new WiseEntry(new Rate
			{
				Origin = origin,
				Destination = destination,
				TransportMode = transportMode,
				ContainerMode = containerMode,
			}, Factory);
			var wiseLine = new WiseLine(Factory, charges[0]) { ParentRateEntry = wiseEntry };
			var chargeCodeMapping = new Dictionary<string, ZGuid>();
			var decider = new CalculatorDecider(charges, wiseLine, Logger, containerMode, transportMode, chargeCodeMapping);
			return decider;
		}

		static void AssertDecider(string messageForExpectedCode, string messageForExpectedItems, CalculatorDecider decider, string expectedCode, IEnumerable<object> expectedRateLineItems)
		{
			AssertEquals(messageForExpectedCode, expectedCode, decider.Code);

			var actualRateLineItems = decider.RateLineItems.Select(CreateItemForComparison).ToArray();
			var expectedRateLineItemsAsStrings = expectedRateLineItems.Select(item => item.ToString()).ToArray();
			var actualRateLineItemsAsStrings = actualRateLineItems.Select(item => item.ToString()).ToArray();

			AssertContainsExactElementsInAnyOrder(
				messageForExpectedItems,
				expectedRateLineItemsAsStrings,
				actualRateLineItemsAsStrings
			);
		}

		static object CreateItemForComparison(IRateLineItem rateLineItem)
		{
			return CreateItemForComparison(rateLineItem.TM_Type, rateLineItem.TM_RelevantValue, rateLineItem.TM_BreakWeightVolume, rateLineItem.TM_UnitMultiple);
		}

		static object CreateItemForComparison(ZString tm_type, ZDecimal tm_relevantValue = default, ZString tm_breakWeightVolume = default, ZInt tm_unitMultiple = default)
		{
			return new
			{
				TM_Type = tm_type,
				TM_RelevantValue = tm_relevantValue,
				TM_BreakWeightVolume = tm_breakWeightVolume,
				TM_UnitMultiple = tm_unitMultiple,
			};
		}

		TestLogger Logger => new TestLogger();

		#endregion
	}
}
