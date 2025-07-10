using System;
using Enterprise.Rating.Integration;
using NUnit.Framework;

namespace Enterprise.Rating.Rateable.Test
{
	public class RateablePartTest : TestCase
	{
		public void TestGetDeclarationLineCount_Throws()
		{
			var part = new RateablePart();
			AssertExceptionThrown<InvalidOperationException>(() => { part.GetDeclarationLineCount(MeasureType.AMS); });
		}

		public void TestWeight()
		{
			var part = new RateablePart();
			AssertEquals(0m, part.Weight);
			AssertNull(part.WeightMeasure);

			part.Weight = 1m;
			AssertEquals(1m, part.Weight);
			AssertNotNull(part.WeightMeasure);
		}

		public void TestVolume()
		{
			var part = new RateablePart();
			AssertEquals(0m, part.Volume);
			AssertNull(part.VolumeMeasure);

			part.Volume = 1m;
			AssertEquals(1m, part.Volume);
			AssertNotNull(part.VolumeMeasure);
		}
	}
}
