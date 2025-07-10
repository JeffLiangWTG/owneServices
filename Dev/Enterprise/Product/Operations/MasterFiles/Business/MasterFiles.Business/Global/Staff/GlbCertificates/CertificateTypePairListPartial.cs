namespace Enterprise.MasterFiles.Business
{
	using System.Collections;
	using CargoWise.Application;
	using Enterprise.MasterFiles.Integration;

	partial class CertificateTypePairList
	{
		public static CertificateTypePairList GetCertificateTypesIncludingProvided()
		{
			var result = new CertificateTypePairList();
			foreach (IPersonCertificateTypesProvider provider in ObjectFactory.Get<IEnumerable>("PersonCertificateTypesProviders"))
			{
				result.AddRangeOverwriteIfExists(provider.GetAdditionalCertificateTypes());
			}
			return result;
		}
	}
}
