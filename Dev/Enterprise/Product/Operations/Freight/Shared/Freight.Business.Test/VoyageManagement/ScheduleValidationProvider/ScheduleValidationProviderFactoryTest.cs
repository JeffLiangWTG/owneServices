using System.Collections;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Moq;

namespace Enterprise.Freight.Business.Testing
{
	sealed class ScheduleValidationProviderFactoryTest : TestCaseWithFactory
	{
		public const string ObjectName = "ScheduleValidationProviders";

		public void TestGetProvidersCached()
		{
			TestGetProviders(true);
		}

		public void TestGetProvidersNotCached()
		{
			TestGetProviders(false);
		}

		#region Implementation

		void TestGetProviders(bool isCached)
		{
			var provider1 = new Mock<IScheduleValidationProvider>(MockBehavior.Strict);
			var provider2 = new Mock<IScheduleValidationProvider>(MockBehavior.Strict);

			var list = new ArrayList { provider1.Object, provider2.Object };

			using (ObjectFactory.Substitute(ObjectName, list))
			{
				var providers = isCached ? ScheduleValidationProviderFactory.GetProviders(Factory) : ScheduleValidationProviderFactory.GetProviders();

				AssertEquals("providers.Length", 2, providers.Length);
				AssertSame("providers[0]", provider1.Object, providers[0]);
				AssertSame("providers[1]", provider2.Object, providers[1]);

				if (isCached)
				{
					AssertSame("result should be cached", providers, ScheduleValidationProviderFactory.GetProviders(Factory));
				}
				else
				{
					AssertEquals("don't cache", false, object.ReferenceEquals(providers, ScheduleValidationProviderFactory.GetProviders()));
				}
			}
		}

		#endregion
	}
}
