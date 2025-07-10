using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Moq;

namespace Enterprise.MasterFiles.Business.Accounting.JobConfigurationHelpers.Testing
{
	sealed class JobConfigurationHelperFactoryTest : TestCaseWithFactory
	{
		public void TestGetJobTypeHelperDoesNotReturnNull()
		{
			var factory = ObjectFactory.Get<IJobConfigurationHelperFactory>();
			AssertNotNull(factory.GetJobTypeHelper(new Mock<IJobTypeConfiguration>().Object));
		}

		public void TestGetTransportModeHelperDoesNotReturnNull()
		{
			var factory = ObjectFactory.Get<IJobConfigurationHelperFactory>();
			AssertNotNull(factory.GetTransportModeHelper(new Mock<ITransportModeConfiguration>().Object));
		}

		public void TestGetSupplyTypeHelperDoesNotReturnNull()
		{
			var factory = ObjectFactory.Get<IJobConfigurationHelperFactory>();
			AssertNotNull(factory.GetSupplyTypeHelper(new Mock<ISupplyTypeConfiguration>().Object));
		}

		public void TestGetDuplicateValidationHelperDoesNotReturnNull()
		{
			var factory = ObjectFactory.Get<IJobConfigurationHelperFactory>();
			AssertNotNull(factory.GetDuplicateValidationHelper());
		}
	}
}
