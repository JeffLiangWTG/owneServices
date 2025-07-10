using CargoWise.Types;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks
{
	public interface IACELicenceAndPermit
	{
		ZString LicenseCertificatePermitTypeCode { get; }
		ZString LicenseNumberCertificateNumberPermitNumber { get; }
	}
}
