using NUnit.Framework;

namespace Enterprise.Customs.Business.BatchProcessor.Testing
{
	class CertificateManagerTest : TestCase
	{
		[ExpectNoExceptions]
		public void TestLoadCustomsCertificateFromDatabaseDoesntThrowExceptionIfInvalidCert()
		{
			using (CertificateManager manager = new CertificateManagerWrapper())
			{
			}
		}

		[ExpectNoExceptions]
		public void TestLoadTrustPointCertificateFromDatabaseDoesntThrowExceptionIfInvalidCert()
		{
			using (CertificateManager manager = new CertificateManagerWrapper())
			{
			}
		}

		public void TestIsType3Certificate()
		{
			using (CertificateManager manager = new CertificateManagerWrapper())
			{
				AssertEquals(true, manager.IsType3Certificate("Gatekeeper TYPE 3 CA"));
				AssertEquals(true, manager.IsType3Certificate("Gatekeeper General Supplementary Device CA-G3"));
				AssertEquals(true, manager.IsType3Certificate("DigiCert Gatekeeper Device Issuing CA"));
				AssertEquals(false, manager.IsType3Certificate("Fake issuer"));
			}
		}

		class CertificateManagerWrapper : CertificateManager
		{
			public CertificateManagerWrapper() : base()
			{
			}

			protected override byte[] GetCustomsCertificateData() => new byte[3] { 1, 2, 3 };

			protected override byte[] GetTrustPointCertificateData() => new byte[3] { 3, 2, 1 };
		}
	}
}
