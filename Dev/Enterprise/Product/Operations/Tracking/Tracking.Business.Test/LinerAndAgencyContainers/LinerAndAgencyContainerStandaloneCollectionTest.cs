using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(LinerAndAgencyContainerStandaloneCollection))]
	sealed class LinerAndAgencyContainerStandaloneCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new LinerAndAgencyContainerStandaloneCollection(Factory);
		}

		public void TestTestingCorrectCollection()
		{
			AssertEquals("Test the correct collection", typeof(LinerAndAgencyContainerStandaloneCollection), GetCollectionToTest().GetType());
		}
	}
}
