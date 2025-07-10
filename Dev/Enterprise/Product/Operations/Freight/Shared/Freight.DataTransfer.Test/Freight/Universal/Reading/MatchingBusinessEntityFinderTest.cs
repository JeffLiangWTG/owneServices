using CargoWise.EntityFramework.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;

namespace Enterprise.Freight.DataTransfer.Universal.Testing
{
	sealed class MatchingBusinessEntityFinderTest : TestCaseWithFactory
	{
		public void TestGetBestMatch()
		{
			IMatchingBusinessEntityFinder<DummyBusinessObject> finder = new MatchingBusinessEntityFinder<DummyBusinessObject>((DummyBusinessObject)null);

			AssertEquals("match", null, finder.GetBestMatch());

			var dummyBizObj = Factory.New<DummyBusinessObject>();

			finder = new MatchingBusinessEntityFinder<DummyBusinessObject>(dummyBizObj);

			AssertEquals("match", dummyBizObj, finder.GetBestMatch());

			finder = new MatchingBusinessEntityFinder<DummyBusinessObject>(() => dummyBizObj);

			AssertEquals("match", dummyBizObj, finder.GetBestMatch());
		}
	}
}
