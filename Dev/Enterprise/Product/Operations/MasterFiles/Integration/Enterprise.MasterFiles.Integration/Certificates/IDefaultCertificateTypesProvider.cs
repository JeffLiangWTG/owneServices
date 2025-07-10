namespace Enterprise.MasterFiles.Integration
{
	using CargoWise.EntityFramework;

	public interface IDefaultCertificateTypesProvider
	{
		void AddDefaultCertificateTypes(ICertificateTypeCollection defaultTypes);
		ZValidation GetDefaultTypesSpecificValidation(BusinessObject parent);
	}
}