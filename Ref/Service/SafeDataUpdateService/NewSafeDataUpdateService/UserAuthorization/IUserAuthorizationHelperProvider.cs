using CargoWise.RefDbRepo.Common.Web.Auth;

namespace CargoWise.RefDbRepo.NewSafeDataUpdateService
{
	public interface IUserAuthorizationHelperProvider
	{
		IAuthorizationHelper UserAuthorizationHelper { get; }
	}
}
