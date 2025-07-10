namespace Enterprise.MasterFiles.Business
{
	using CargoWise.Integration;
	using CargoWise.Types;

	public interface ICertificatesProvider
	{
		GenRegCertAccredMaintListCollection Certificates { get; }
		ICodeDescriptionPairList GetCertificateTypeList();
		ICodeDescriptionPairList GetActiveCertificateTypeList();
		ZString GetDefaultDescription(ZString code);
	}

	public interface ICertificatesValidationProvider
	{
		GenRegCertAccredMaintListValidation GetValidation(GenRegCertAccredMaintList parent);
	}
}