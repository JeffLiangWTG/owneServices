using System.Linq;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.Utils;
using CargoWise.RefDbRepo.Common.Web.Auth;
using Microsoft.AspNetCore.Http;

namespace CargoWise.RefDbRepo.NewService
{

	public static class HttpContextExtensions
	{
		public static string GetAuthType(this HttpContext context)
		{
			Argument.NotNull(context, nameof(context));
			return context.User?.Identity?.AuthenticationType;
		}

		public static string GetUserId(this HttpContext context)
		{
			Argument.NotNull(context, nameof(context));

			var authType = context.GetAuthType();
			switch (authType)
			{
				case AuthType.BasicAuth:
					return context.User?.Identity?.Name;
				case AuthType.TokenAuth:
					return context.User?.Claims.FirstOrDefault(c => c.Type == AuthClaimType.Azp)?.Value;
				default:
					return null;
			}
		}
	}
}
