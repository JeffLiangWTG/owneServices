using System.Threading.Tasks;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor
{
	public interface IExpirationProcessor
	{
		Task<bool> UpdateResultsAndAutoExpire();
	}
}
