using CargoWise.RefDbRepo.Common.Argument;
using Microsoft.AspNetCore.Http;

namespace CargoWise.RefDbRepo.NewService
{
	public class UserService : IUserService
	{
		readonly IHttpContextAccessor _accessor;
		public UserService(IHttpContextAccessor accessor)
		{
			Argument.NotNull(accessor, nameof(accessor));
			_accessor = accessor;
		}

		public string GetUserId()
		{
			return _accessor.HttpContext.GetUserId();
		}
	}
}
