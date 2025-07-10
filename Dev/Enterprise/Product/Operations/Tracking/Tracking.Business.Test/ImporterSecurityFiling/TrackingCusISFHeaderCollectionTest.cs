using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.ImporterSecurityFiling
{
	[TestedType(typeof(TrackingCusISFHeaderCollection))]
	sealed class TrackingCusISFHeaderCollectionTest : ActiveBusinessObjectCollectionTestCase<TrackingCusISFHeaderCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New(typeof(TrackingCusISFHeader));
		}

		protected override TrackingCusISFHeaderCollection GetCollectionToTest()
		{
			return new TrackingCusISFHeaderCollection(Factory);
		}
	}
}
