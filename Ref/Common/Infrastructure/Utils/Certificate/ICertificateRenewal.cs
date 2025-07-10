using System.Threading.Tasks;

namespace CargoWise.RefDbRepo.Common.Utils;

public interface ICertificateRenewal
{
	Task<byte[]> GetNewCertificateContentAsync();
}
