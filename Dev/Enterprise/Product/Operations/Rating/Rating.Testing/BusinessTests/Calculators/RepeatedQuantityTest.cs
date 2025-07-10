using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	public class RepeatedQuantityTest : TestCase
	{
		public void TestSum()
		{
			var q1 = new RepeatedQuantity(new Quantity(11m, QuantityUnit.CN, "", "ref1"), 1, 0);
			var q2 = new RepeatedQuantity(new Quantity(12m, QuantityUnit.CN, "", "ref2"), 2, 0);
			var q3 = new RepeatedQuantity(new Quantity(13m, QuantityUnit.CN, "", "ref3"), 3, 0);

			AssertQuantity(new Quantity(0, QuantityUnit.CN), RepeatedQuantity.Sum(System.Array.Empty<RepeatedQuantity>(), QuantityUnit.CN));
			AssertQuantity(q1.Quantity, RepeatedQuantity.Sum(new[] { q1 }, QuantityUnit.CN));
			AssertQuantity(new Quantity(13m * 3, QuantityUnit.CN, "", "ref3"), RepeatedQuantity.Sum(new[] { q3 }, QuantityUnit.CN));
			AssertQuantity(new Quantity(11m + 12m * 2, QuantityUnit.CN, "", "ref1, ref2"), RepeatedQuantity.Sum(new[] { q1, q2 }, QuantityUnit.CN));
			AssertQuantity(new Quantity(11m + 12m * 2 + 13m * 3, QuantityUnit.CN, "", "ref1, ref2, ref3"), RepeatedQuantity.Sum(new[] { q1, q2, q3 }, QuantityUnit.CN));
		}

		void AssertQuantity(Quantity expected, Quantity actual)
		{
			CombineAssertions(() =>
			{
				AssertEquals("Amount", expected.Amount, actual.Amount);
				AssertEquals("Unit", expected.Unit, actual.Unit);
				AssertEquals("Source", expected.Source, actual.Source);
				AssertEquals("Reference", expected.Reference, actual.Reference);
			});
		}
	}
}
