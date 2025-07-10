using System.Collections.Generic;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("CargoWise.RefDbRepo.Deployment.Test")]
namespace CargoWise.RefDbRepo.Deployment.ParameterCreation
{
	class TestEnvParameterCreationStrategy : IParameterCreationStrategy
	{
		public List<(string paramName, object paramValue)> CreateParameters(TestRig testRig)
		{
			if (testRig != null)
			{
				return testRig.GetTestRigParameters();
			}
			var stagingDeployPassword = DATCrypto.Decrypt(Passwords.TEST_StagingDeployPassword, true);
			var deliveryServiceDeployPassword = DATCrypto.Decrypt(Passwords.TEST_DeliveryServiceDeployPassword, true);
			return new List<(string, object)>
			{
				("xmlProducersDestination", @"XMLProducersDeployTest\"),
				("stagingDestination", @"StagingDeployTest\"),
				("stagingServer", new[] { "au2sp-srfd-402.sand.wtg.zone" }),
				("dbWriterUsername", "refdbrepowriter"),
				("dbReaderUsername", "refdbreporeader"),
				("dbWriterPassword", DATCrypto.Decrypt(Passwords.TEST_WriterPassword)),
				("dbReaderPassword", DATCrypto.Decrypt(Passwords.TEST_ReaderPassword)),
				("dbWriterPasswordJson", DATCrypto.Decrypt(Passwords.TEST_WriterPassword)),
				("dbReaderPasswordJson", DATCrypto.Decrypt(Passwords.TEST_ReaderPassword)),
				("dbPassword", DATCrypto.Decrypt(Passwords.TEST_AdminPassword)),
				("quartzServiceName", "RefDbRepoTest Quartz Server"),
				("refDbRepoSafeDbName", "RefDbRepoSafeTest"),
				("refDbRepoStagingDbName", "RefDbRepoStagingTest"),
				("deliveryServiceUrl", "refdbrepo-test.wtg.zone"),
				("updateServiceUrl", "refdbrepoupdate-test.wtg.zone"),
				("stagingDeployUsername", @".\refdbrepo"),
				("updateServiceDeployUsername", @".\refdbrepo"),
				("stagingDeployPassword", stagingDeployPassword),
				("updateServiceDeployPassword", stagingDeployPassword),
				("deliveryServiceDeployPassword", deliveryServiceDeployPassword),
				("seleniumServiceUrl", "au2sp-swss-401.sand.wtg.zone")
			};
		}
	}
}
