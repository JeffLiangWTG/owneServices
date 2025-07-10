using System.Collections.Generic;

namespace CargoWise.RefDbRepo.Deployment.ParameterCreation
{
	class ProEnvParameterCreationStrategy : IParameterCreationStrategy
	{
		public List<(string paramName, object paramValue)> CreateParameters(TestRig testRig)
		{
			var stagingDeployPassword = DATCrypto.Decrypt(Passwords.PRO_StagingDeployPassword, true);
			var deliveryServiceDeployPassword = DATCrypto.Decrypt(Passwords.PRO_DeliveryServiceDeployPassword, true);
			return new List<(string, object)>
			{
				("xmlProducersDestination", @"RefDbRepoXMLProducers\"),
				("stagingDestination", @"RefDbRepo\"),
				("stagingServer", new[] { "au2wp-srfd-401.wisecloud.zone", "au2wp-srfd-402.wisecloud.zone" }),
				("dbPassword", DATCrypto.Decrypt(Passwords.PRO_DbPassword)),
				("dbWriterPassword", DATCrypto.Decrypt(Passwords.PRO_WriterPassword)),
				("dbReaderPassword", DATCrypto.Decrypt(Passwords.PRO_ReaderPassword)),
				("dbWriterPasswordJson", DATCrypto.Decrypt(Passwords.PRO_WriterPassword, true)),
				("dbReaderPasswordJson", DATCrypto.Decrypt(Passwords.PRO_ReaderPassword, true)),
				("refDbRepoSafeDbName", "RefDbRepoSafe"),
				("refDbRepoStagingDbName", "RefDbRepoStage"),
				("stagingDeployUsername", @"PROD\s_refdbrepo_update"),
				("updateServiceDeployUsername", @"PROD\s_refdbrepo_update"),
				("stagingDeployPassword", stagingDeployPassword),
				("updateServiceDeployPassword", stagingDeployPassword),
				("deliveryServiceDeployPassword", deliveryServiceDeployPassword),
				("seleniumServiceUrl", "au2wp-swss-401.wisecloud.zone"),
				("updateServiceUrl", "refdbrepoupdate.wisecloud.zone"),
			};
		}
	}
}
