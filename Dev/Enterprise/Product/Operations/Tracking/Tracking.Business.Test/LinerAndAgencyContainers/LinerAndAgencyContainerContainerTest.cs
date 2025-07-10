using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;

namespace Enterprise.Tracking.Business.Testing
{
	sealed class LinerAndAgencyContainerContainerTest : CommonContainerTest2
	{
		protected override CommonContainer GetNewContainer()
		{
			return Factory.New<LinerAndAgencyContainer>();
		}
	}
}
