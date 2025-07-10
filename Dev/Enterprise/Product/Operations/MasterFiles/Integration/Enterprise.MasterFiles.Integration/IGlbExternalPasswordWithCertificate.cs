using CargoWise.Types;

namespace Enterprise.MasterFiles.Integration
{
	public interface IGlbExternalPasswordWithCertificate : IGlbExternalPassword
	{
		ZString CurrentDecryptedCertificatePassphrase { get; set; }
		bool IsCertificateValid { get; }
	}
}
