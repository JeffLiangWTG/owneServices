using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CryptokiCertificateCollection))]
	class CryptokiCertificateCollectionTest : NonPersistentBusinessObjectCollectionTestCase<CryptokiCertificateCollection>
	{
		protected override CryptokiCertificateCollection GetCollectionToTest()
		{
			return new CryptokiCertificateCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new CryptokiCertificate();
		}
	}
}
