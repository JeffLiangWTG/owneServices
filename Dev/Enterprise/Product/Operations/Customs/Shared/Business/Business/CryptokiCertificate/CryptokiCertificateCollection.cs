using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public class CryptokiCertificateCollection : NonPersistentBusinessObjectCollection<CryptokiCertificate>
	{
		public static CryptokiCertificateCollection Wrap(IReadOnlyList<CryptokiCertificate> certificates)
		{
			var collection = new CryptokiCertificateCollection();
			collection.AddRange(certificates);
			return collection;
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new CryptokiCertificate();
		}
	}
}
