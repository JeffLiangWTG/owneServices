using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.Business.Test
{
	[TestedType(typeof(MNRSurveyCollection))]
	class MNRSurveyCollectionTest : ActiveBusinessObjectCollectionTestCase<MNRSurveyCollection>
	{
		protected override MNRSurveyCollection GetCollectionToTest()
		{
			return new MNRSurveyCollection(Factory);
		}

		public void TestInitCollection()
		{
			var collection = new MNRSurveyCollection(Factory);
			AssertEquals("Precondition: collection count", 0, collection.Count);
		}
	}
}
