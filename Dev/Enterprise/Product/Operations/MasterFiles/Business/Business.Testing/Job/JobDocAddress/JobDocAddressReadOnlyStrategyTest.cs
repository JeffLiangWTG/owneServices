using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	class JobDocAddressReadOnlyStrategyTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertNoExceptionThrown(() => new JobDocAddressReadOnlyStrategy());
			AssertNoExceptionThrown(() => new JobDocAddressReadOnlyStrategy(null));
			AssertNoExceptionThrown(() => new JobDocAddressReadOnlyStrategy(() => false));
			AssertNoExceptionThrown(() => new JobDocAddressReadOnlyStrategy(null, null));
			AssertNoExceptionThrown(() => new JobDocAddressReadOnlyStrategy(() => false, null));
			AssertNoExceptionThrown(() => new JobDocAddressReadOnlyStrategy(null, () => false));
			AssertNoExceptionThrown(() => new JobDocAddressReadOnlyStrategy(() => false, () => false));
		}

		public void TestReadOnly()
		{
			IJobDocAddressReadOnlyStrategy provider1 = new JobDocAddressReadOnlyStrategy();
			AssertEquals(false, provider1.ReadOnly);

			IJobDocAddressReadOnlyStrategy provider2 = new JobDocAddressReadOnlyStrategy(() => true);
			AssertEquals(true, provider2.ReadOnly);

			IJobDocAddressReadOnlyStrategy provider3 = new JobDocAddressReadOnlyStrategy(() => false);
			AssertEquals(false, provider3.ReadOnly);

			int i = 0;
			IJobDocAddressReadOnlyStrategy provider4 = new JobDocAddressReadOnlyStrategy(() => i++ % 2 == 0);
			AssertEquals(true, provider4.ReadOnly);
			AssertEquals(false, provider4.ReadOnly);
			AssertEquals(true, provider4.ReadOnly);
			AssertEquals(false, provider4.ReadOnly);
		}

		public void TestOrganisationPKReadOnly()
		{
			IJobDocAddressReadOnlyStrategy provider1 = new JobDocAddressReadOnlyStrategy();
			AssertEquals(false, provider1.OrganisationPKReadOnly);

			IJobDocAddressReadOnlyStrategy provider2 = new JobDocAddressReadOnlyStrategy(organisationPKReadOnly: () => true);
			AssertEquals(true, provider2.OrganisationPKReadOnly);

			IJobDocAddressReadOnlyStrategy provider3 = new JobDocAddressReadOnlyStrategy(organisationPKReadOnly: () => false);
			AssertEquals(false, provider3.OrganisationPKReadOnly);

			IJobDocAddressReadOnlyStrategy provider4 = new JobDocAddressReadOnlyStrategy(() => true, () => false);
			AssertEquals(false, provider4.OrganisationPKReadOnly);

			int i = 0;
			IJobDocAddressReadOnlyStrategy provider5 = new JobDocAddressReadOnlyStrategy(organisationPKReadOnly: () => i++ % 2 == 0);
			AssertEquals(true, provider5.OrganisationPKReadOnly);
			AssertEquals(false, provider5.OrganisationPKReadOnly);
			AssertEquals(true, provider5.OrganisationPKReadOnly);
			AssertEquals(false, provider5.OrganisationPKReadOnly);
		}
	}
}
