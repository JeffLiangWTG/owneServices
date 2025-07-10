using System.Collections.Generic;

namespace CargoWise.RefDbRepo.Deployment.ParameterCreation
{
	class GammaEnvParameterCreationStrategy : IParameterCreationStrategy
	{
		public List<(string paramName, object paramValue)> CreateParameters(TestRig testRig)
		{
			var stagingDeployPassword = DATCrypto.Decrypt(Passwords.Gamma_StagingDeployPassword, true);
			var deliveryServiceDeployPassword = DATCrypto.Decrypt(Passwords.Gamma_DeliveryServiceDeployPassword, true);
			return new List<(string, object)>
			{
				("xmlProducersDestination", @"XMLProducersDeployGamma\"),
				("stagingDestination", @"StagingDeployGamma\"),
				("stagingServer", new[] { "au2sp-srfd-402.sand.wtg.zone" }),
				("dbWriterUsername", "refdbrepowriter"),
				("dbReaderUsername", "refdbreporeader"),
				("dbWriterPassword", DATCrypto.Decrypt(Passwords.TEST_WriterPassword)),
				("dbReaderPassword", DATCrypto.Decrypt(Passwords.TEST_ReaderPassword)),
				("dbWriterPasswordJson", DATCrypto.Decrypt(Passwords.TEST_WriterPassword)),
				("dbReaderPasswordJson", DATCrypto.Decrypt(Passwords.TEST_ReaderPassword)),
				("dbPassword", DATCrypto.Decrypt(Passwords.TEST_AdminPassword)),
				("quartzServiceName", "RefDbRepoGamma Quartz Server"),
				("refDbRepoSafeDbName", "RefDbRepoSafeGamma"),
				("refDbRepoStagingDbName", "RefDbRepoStagingGamma"),
				("deliveryServiceUrl", "refdbrepoupdate-test.wtg.zone"),
				("updateServiceUrl", "refdbrepoupdate-test.wtg.zone"),
				("stagingDeployUsername", @".\refdbrepo"),
				("updateServiceDeployUsername", @".\refdbrepo"),
				("stagingDeployPassword", stagingDeployPassword),
				("updateServiceDeployPassword", stagingDeployPassword),
				("deliveryServiceDeployPassword", deliveryServiceDeployPassword),
				("deliveryServiceServerName", "AU2SP-SRFD-402.sand.wtg.zone"),
				("seleniumServiceUrl", "au2sp-swss-401.sand.wtg.zone")
			};
		}
	}
}
