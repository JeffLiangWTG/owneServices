namespace Enterprise.MasterFiles.Integration
{
	using CargoWise.Integration;

	public interface IPersonCertificateTypesProvider
	{
		ICodeDescriptionPairList GetAdditionalCertificateTypes();
	}
}