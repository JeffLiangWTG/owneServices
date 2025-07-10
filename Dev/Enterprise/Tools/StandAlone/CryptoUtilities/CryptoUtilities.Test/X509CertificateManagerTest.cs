using System.Security.Cryptography.X509Certificates;
using CargoWise.ApplicationManager.Common;
using CargoWise.IO;
using Enterprise.CryptoUtilities;
using NUnit.Framework;

namespace Enterprise.CMREdifact.CryptoUtilities.Testing
{
	sealed class X509CertificateManagerTest : TestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestInvoke()
		{
			using (var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly))
			{
				string encryptionKeyPairFileName = resourceRetriever.SaveResourceToFile("SEDI_Test_EncryptionKeyPair.p12");
				var cert = new X509Certificate2(encryptionKeyPairFileName, "4Customs&SEDI");
				var manager = new X509CertificateManager();
				var result = manager.Invoke(false, "wrong state");
				AssertEquals(AppManagerResultStatus.Error, result.Status);
				AssertEquals("state must be an array", result.Message);

				result = manager.Invoke(false, new object[] { "" });
				AssertEquals(AppManagerResultStatus.Error, result.Status);
				AssertEquals("Length of a state array must be 2", result.Message);

				result = manager.Invoke(false, new object[] { "", "" });
				AssertEquals(AppManagerResultStatus.Error, result.Status);
				AssertEquals("The first element in a state array must be a X509Certificate2 to add or remove.",
					result.Message);

				result = manager.Invoke(false, new object[] { cert, true });
				AssertEquals(AppManagerResultStatus.Error, result.Status);
				AssertEquals("The second element in a state array must be a string.", result.Message);

				result = manager.Invoke(false, new object[] { cert, "wrong_action" });
				AssertEquals(AppManagerResultStatus.Error, result.Status);
				AssertEquals("Unknown action \"wrong_action\". It must be \"add\" or \"remove\".", result.Message);

				using (var appManagerClient = AppManagerClientFactory.GetNewAppManager())
				{
					result = appManagerClient.Invoke(
						typeof(X509CertificateManager).Assembly.Location,
						typeof(X509CertificateManager).FullName,
						new object[] { cert, "add" },
						MutexRequest.Create("AddX509Certificate2"));
					AssertEquals(result.Message, AppManagerResultStatus.Success, result.Status);

					result = appManagerClient.Invoke(
						typeof(X509CertificateManager).Assembly.Location,
						typeof(X509CertificateManager).FullName,
						new object[] { cert, "add" },
						MutexRequest.Create("RemoveX509Certificate2"));
					AssertEquals(result.Message, AppManagerResultStatus.Success, result.Status);
				}
			}
		}
	}
}
