using System;
using System.Collections.Generic;
using Enterprise.Integration.Accounting;

namespace Enterprise.Rating.Business.Testing
{
	internal abstract class BaseAutoRateCalculatorTest : RatingTestCase
	{
		public void TestUnitCalculator()
		{
			var expected = TestUnitCalculatorCore(out var category, out var mode, out var origin, out var destination, out var chargeCode, out var unit, out var perUnit, out var setRateLine);

			if (expected != null)
			{
				var rateEntry = RatingHeader.AddRateEntryWithUnitRateLine(category, mode, origin, destination, chargeCode, perUnit, unit);
				setRateLine(rateEntry.RateLines[0]);

				Save();

				AssertAutorate(CostSell, expected);
			}
			else
			{
				Assert(true);
			}
		}

		protected abstract SimpleArInfo[] TestUnitCalculatorCore(out string category, out string mode, out string origin, out string destination, out string chargeCode, out string unit, out decimal perUnit, out Action<RateLine> setRateLine);

		public void TestFlatCalculator()
		{
			var expected = TestFlatCalculatorCore(out var category, out var mode, out var origin, out var destination, out var chargeCode, out var flatCalculatorBaseRate, out var setRateLine);

			if (expected != null)
			{
				var rateEntry = RatingHeader.AddRateEntryWithFlatRateLine(category, mode, origin, destination, chargeCode, flatCalculatorBaseRate);
				setRateLine(rateEntry.RateLines[0]);

				Save();

				AssertAutorate(CostSell, expected);
			}
			else
			{
				Assert(true);
			}
		}

		protected abstract SimpleArInfo[] TestFlatCalculatorCore(out string category, out string mode, out string origin, out string destination, out string chargeCode, out decimal flatCalculatorBaseRate, out Action<RateLine> setRateLine);

		protected abstract void AssertAutorate(CostSell costSell, IEnumerable<SimpleArInfo> expected, string assertionMessage = default);

		protected abstract RatingHeader RatingHeader { get; }

		protected abstract CostSell CostSell { get; }

		protected abstract void Save();
	}
}
