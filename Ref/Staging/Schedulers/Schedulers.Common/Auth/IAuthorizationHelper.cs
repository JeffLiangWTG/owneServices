using Microsoft.AspNetCore.Http;

namespace CargoWise.RefDbRepo.Staging.Schedulers.Common
{
	public interface IAuthorizationHelper
	{
		bool IsAuthorized(string user, IFormCollectionService formCollectionService);
	}
}
