using CargoWise.EntityFramework.Testing;
using Enterprise.Rating.Integration;

namespace Enterprise.Rating.Business.Testing
{
	public class RatingCacheTest : TestCaseWithFactory
	{
		public void TestGetMeasureTypeWeight()
		{
			AssertEquals(MeasureType.Weight, RatingCache.GetMeasureTypeFromUnit("KG"));
		}

		public void TestGetMeasureTypeVolume()
		{
			AssertEquals(MeasureType.Volume, RatingCache.GetMeasureTypeFromUnit("M3"));
		}

		public void TestGetMeasureTypeContainerCount()
		{
			AssertEquals(MeasureType.ContainerCount, RatingCache.GetMeasureTypeFromUnit("CN"));
		}

		public void TestGetMeasureTypeUnit()
		{
			AssertEquals(MeasureType.Unit, RatingCache.GetMeasureTypeFromUnit("CRT"));
		}

		public void TestGetMeasureTypeUnidentified()
		{
			AssertEquals(MeasureType.Unidentified, RatingCache.GetMeasureTypeFromUnit("ERROR"));
		}
	}
}
