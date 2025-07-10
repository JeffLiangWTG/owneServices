using System.Threading.Tasks;

namespace CargoWise.RefDbRepo.NewSafeDataUpdateService
{
	public interface IClientLicenseInformationProvider
	{
		Task<string[]> GetLicenceInformationInactive();
	}
}
