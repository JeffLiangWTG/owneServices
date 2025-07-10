namespace Enterprise.Rating.Business.Testing
{
	public class MeasureTypeRetrieverTest : RatingTestCase
	{
		public void TestGetMeasureTypes()
		{
			var measureTypes = MeasureTypeRetriever.GetMeasureTypes();
			AssertEquals(177, measureTypes.Count);
		}
	}
}
