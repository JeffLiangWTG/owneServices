using System.Collections.Generic;

namespace CargoWise.RefDbRepo.Deployment.ParameterCreation
{
	class UATEnvParameterCreationStrategy : IParameterCreationStrategy
	{
		public List<(string paramName, object paramValue)> CreateParameters(TestRig testRig)
		{
			var stagingDeployPassword = DATCrypto.Decrypt(Passwords.UAT_StagingDeployPassword, true);
			var deliveryServiceDeployPassword = DATCrypto.Decrypt(Passwords.UAT_DeliveryServiceDeployPassword, true);
			return new List<(string, object)>
			{
				("xmlProducersDestination", @"XMLProducers\"),
				("stagingDestination", @"Staging\"),
				("stagingServer", new[] { "au2sp-srfd-401.sand.wtg.zone" }),
				("dbWriterPassword", DATCrypto.Decrypt(Passwords.TEST_WriterPassword)),
				("dbReaderPassword", DATCrypto.Decrypt(Passwords.TEST_ReaderPassword)),
				("dbWriterPasswordJson", DATCrypto.Decrypt(Passwords.TEST_WriterPassword)),
				("dbReaderPasswordJson", DATCrypto.Decrypt(Passwords.TEST_ReaderPassword)),
				("dbPassword", DATCrypto.Decrypt(Passwords.TEST_AdminPassword)),
				("refDbRepoSafeDbName", "RefDbRepoSafe"),
				("refDbRepoStagingDbName", "RefDbRepoStaging"),
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
