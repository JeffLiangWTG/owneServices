using System.Linq;
using CargoWise.RefDbRepo.Common.Utils;
using Microsoft.AspNetCore.Http;

namespace CargoWise.RefDbRepo.Common.Web
{
	public static class HttpContextExtension
	{
		public static string GetUserId(this HttpContext context)
		{
			Argument.Argument.NotNull(context, nameof(context));
			var userName = context.User?.Identity?.Name;
			if (string.IsNullOrEmpty(userName))
			{
				userName = context.User?.Claims?.FirstOrDefault(x => x.Type == AuthClaimType.UniqueName)?.Value;
			}
			if (string.IsNullOrEmpty(userName))
			{
				userName = context.User?.Claims?.FirstOrDefault(x => x.Type == AuthClaimType.Azp)?.Value;
			}
			return userName;
		}
	}
}
