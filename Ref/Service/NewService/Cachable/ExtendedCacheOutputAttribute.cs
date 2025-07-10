using System.Linq;
using System.Threading;
using AspNetCore.CacheOutput;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CargoWise.RefDbRepo.NewService
{
	public sealed class ExtendedCacheOutputAttribute : CacheOutputAttribute
	{
		protected override bool IsCachingAllowed(FilterContext actionContext, bool anonymousOnly)
		{
			if (anonymousOnly)
			{
				if (Thread.CurrentPrincipal?.Identity.IsAuthenticated == true)
				{
					return false;
				}
			}

			if (actionContext.ActionDescriptor.FilterDescriptors.Any(x => x.GetType() == typeof(IgnoreCacheOutputAttribute)))
			{
				return false;
			}

			var method = actionContext.HttpContext.Request?.Method.ToUpperInvariant();

			return method == "GET" || method == "POST";
		}
	}
}
