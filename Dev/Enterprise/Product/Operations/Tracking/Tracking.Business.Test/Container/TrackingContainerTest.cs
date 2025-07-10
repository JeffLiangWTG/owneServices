using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;

namespace Enterprise.Tracking.Business.Testing
{
	public class TrackingContainerTest : CommonContainerTest2
	{
		protected override CommonContainer GetNewContainer()
		{
			return Factory.New<TrackingContainer>();
		}

		protected override CommonConsol GetNewConsol()
		{
			return Factory.New<TrackingConsol>();
		}
	}
}
