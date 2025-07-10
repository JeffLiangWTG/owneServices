using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Moq;

namespace Enterprise.Freight.Business.Testing
{
	sealed class ScheduleUpdateServicesTest : TestCaseWithFactory
	{
		public void TestQueryProvider()
		{
			var provider = new Mock<IScheduleUpdateQueryProvider>(MockBehavior.Strict);
			var providerDelegate = new Mock<GetValueDelegate<IScheduleUpdateQueryProvider>>(MockBehavior.Strict);

			ScheduleUpdateQueryProviderFactory.Set(Factory, providerDelegate.Object);

			var voyage = Factory.New<JobVoyage>();

			providerDelegate.Setup(m => m()).Returns(provider.Object);
			ScheduleUpdateServices services = new ScheduleUpdateServices(voyage);
			AssertEquals(true, object.ReferenceEquals(provider.Object, services.QueryProvider));
		}
	}
}
