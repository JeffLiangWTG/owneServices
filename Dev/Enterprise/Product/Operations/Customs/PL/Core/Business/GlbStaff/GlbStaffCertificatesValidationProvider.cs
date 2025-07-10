using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.Business;

public sealed class GlbStaffCertificatesValidationProvider : ICertificatesValidationProvider
{
	public MasterFiles.Business.GenRegCertAccredMaintListValidation GetValidation(GenRegCertAccredMaintList parent) => new GenRegCertAccredMaintListValidation(parent);
}
