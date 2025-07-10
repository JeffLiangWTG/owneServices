using CargoWise.EntityFramework.Testing;
using Moq;

namespace Enterprise.Freight.Common.Business.Testing
{
	sealed class SailingManagerQueryProviderFactoryTest : TestCaseWithFactory
	{
		public void TestGetSet()
		{
			AssertType(typeof(DefaultSailingManagerQueryProvider), SailingManagerQueryProviderFactory.Get(Factory));

			var providerMock = new Mock<ISailingManagerQueryProvider>(MockBehavior.Strict);

			SailingManagerQueryProviderFactory.Set(Factory, providerMock.Object);
			AssertSame(providerMock.Object, SailingManagerQueryProviderFactory.Get(Factory));

			ISailingManagerQueryProvider queryProviderReturnedWhenFactoryIsInTransaction = null;
			Factory.Saving += (factory) =>
			{
				AssertEquals("Precondition", true, Factory.IsInTransaction);
				queryProviderReturnedWhenFactoryIsInTransaction = SailingManagerQueryProviderFactory.Get(Factory);
			};

			Factory.Save();
			AssertType("DefaultSailingManagerQueryProvider returned when factory is in transaction", typeof(DefaultSailingManagerQueryProvider), queryProviderReturnedWhenFactoryIsInTransaction);
		}
	}
}
