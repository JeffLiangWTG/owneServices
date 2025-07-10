using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Rating.Business.Testing;

namespace Enterprise.Freight.QuotedBookings.Business.Test
{
	public class RatingTestUtils : RatingTestCase
	{
		public static void AssertArraysAreEquivalent(object[] expected, object[] actual)
		{
			AssertEquals(expected.Length, actual.Length);

			for (var i = 0; i < expected.Length; i++)
			{
				foreach (var property in expected[i].GetType().GetProperties())
				{
					AssertEquals(expected[i].GetPropertyValue(property.Name), actual[i].GetPropertyValue(property.Name));
				}
			}
		}
	}
}
