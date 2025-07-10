using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Moq;

namespace Enterprise.Freight.Business.Testing
{
	sealed class ScheduleUpdateQueryProviderFactoryTest : TestCaseWithFactory
	{
		public void TestGetNull()
		{
			AssertType(typeof(ScheduleUpdateNullQueryProvider), ScheduleUpdateQueryProviderFactory.Get(Factory));

			ScheduleUpdateQueryProviderFactory.Set(Factory, null);
			AssertType(typeof(ScheduleUpdateNullQueryProvider), ScheduleUpdateQueryProviderFactory.Get(Factory));
		}

		public void TestGetSet()
		{
			var provider = new Mock<IScheduleUpdateQueryProvider>(MockBehavior.Strict);
			var providerDelegate = new Mock<GetValueDelegate<IScheduleUpdateQueryProvider>>(MockBehavior.Strict);

			ScheduleUpdateQueryProviderFactory.Set(Factory, providerDelegate.Object);

			providerDelegate
				.SetupSequence(m => m())
				.Returns(provider.Object)
				.Returns(provider.Object);

			AssertEquals(true, object.ReferenceEquals(provider.Object, ScheduleUpdateQueryProviderFactory.Get(Factory)));
			AssertEquals(true, object.ReferenceEquals(provider.Object, ScheduleUpdateQueryProviderFactory.Get(Factory)));

			IScheduleUpdateQueryProvider queryProviderReturnedWhenFactoryIsInTransaction = null;
			Factory.Saving += (factory) =>
			{
				AssertEquals("Precondition", true, Factory.IsInTransaction);
				queryProviderReturnedWhenFactoryIsInTransaction = ScheduleUpdateQueryProviderFactory.Get(Factory);
			};

			Factory.Save();
			AssertType("Null query provider returned when factory is in transaction", typeof(ScheduleUpdateNullQueryProvider), queryProviderReturnedWhenFactoryIsInTransaction);
		}
	}
}
