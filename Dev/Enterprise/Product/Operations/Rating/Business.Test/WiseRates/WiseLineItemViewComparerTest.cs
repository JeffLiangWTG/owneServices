using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Rating.Business.Testing
{
	public class WiseLineItemViewComparer : TestCaseWithFactory
	{
		public void TestWiseLineItemViewSorting_ByBreaks()
		{
			var input = new List<TestRateLineItem>() {
				new TestRateLineItem() { TM_Break = 10 },
				new TestRateLineItem() { TM_Break = 2 },
				new TestRateLineItem() { TM_Break = -5 },
				new TestRateLineItem() { TM_Break = -10 },
				new TestRateLineItem() { TM_Break = 0 },
			};

			var expected = new[] { -10, -5, 0, 2, 10 };
			input.Sort(new IRateLineItemComparer());

			var actual = input.Select(x => (int)x.TM_Break).ToArray();

			AssertContainsExactElementsInAnyOrder("The sorted TM_Break values should match the expected order.", expected, actual);
		}

		public void TestWiseLineItemViewSorting_ByMinPlus()
		{
			var input = new List<TestRateLineItem>() {
				new TestRateLineItem() { TM_Type = "+" },
				new TestRateLineItem() { TM_Type = "-" },
				new TestRateLineItem() { TM_Type = "+" },
				new TestRateLineItem() { TM_Type = "-" },
			};

			var expected = new[] { "-", "-", "+", "+" };
			input.Sort(new IRateLineItemComparer());

			var actual = input
				.Select(x => (string)x.TM_Type)
				.ToArray();

			AssertContainsExactElementsInAnyOrder(
				"The sorted collection should match the expected order",
				expected,
				actual
			);
		}

		public void TestWiseLineItemViewSorting_ByTypeLength()
		{
			var input = new List<TestRateLineItem>() {
				new TestRateLineItem() { TM_Type = "AAA" },
				new TestRateLineItem() { TM_Type = "A" },
				new TestRateLineItem() { TM_Type = "AAAA" },
				new TestRateLineItem() { TM_Type = string.Empty },
			};

			var expected = new[] { "AAAA", "AAA", "A", string.Empty };
			input.Sort(new IRateLineItemComparer());

			var actual = input.Select(x => (string)x.TM_Type).ToArray();
			AssertContainsExactElementsInAnyOrder("The sorted list should match the expected ordering.", expected, actual);
		}

		class TestRateLineItem : IRateLineItem
		{
			public object this[string propertyName] { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

			public IRateLine ParentRateLine => throw new System.NotImplementedException();

			public ZString TM_Text => throw new System.NotImplementedException();

			public ZGuid TM_AC => throw new System.NotImplementedException();

			public ZString TM_Type { get; set; }

			public ZDecimal TM_Break { get; set; }

			public ZDecimal TM_BreakHourRate => throw new System.NotImplementedException();

			public ZDecimal TM_BreakHour => throw new System.NotImplementedException();

			public ZDecimal TM_RelevantValue => throw new System.NotImplementedException();

			public ZDecimal TM_FlatAmount => throw new System.NotImplementedException();

			public ZDecimal TM_BreakMinimum => throw new System.NotImplementedException();

			public ZString TM_BreakWeightVolume => throw new System.NotImplementedException();

			public ZString TM_F1Zone => throw new System.NotImplementedException();

			public ZGuid TM_TZ_DomesticZone => throw new System.NotImplementedException();

			public ZBool TM_CallForPricing => throw new System.NotImplementedException();

			public ZDecimal TM_Value => throw new System.NotImplementedException();

			public ZDecimal TM_AgentDeclaredRate => throw new System.NotImplementedException();

			public ZInt TM_UnitMultiple => throw new System.NotImplementedException();

			public ZString ApplyToDescription => throw new System.NotImplementedException();

			public ZString CalculationOrderOrPercentOf => throw new System.NotImplementedException();

			public BusinessObjectFactory Factory => throw new System.NotImplementedException();

			public IEnumerable<IRateLineItem> RateLineItemsFromSameGroup => throw new System.NotImplementedException();

			public string InvalidReason => throw new System.NotImplementedException();

			public void OverrideBreak(ZDecimal breakValue)
			{
				throw new System.NotImplementedException();
			}

			public void ResetBreakToOriginalValue()
			{
				throw new System.NotImplementedException();
			}
		}
	}
}
