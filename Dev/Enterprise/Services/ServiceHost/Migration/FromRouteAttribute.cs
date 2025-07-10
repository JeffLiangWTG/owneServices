#if NETFRAMEWORK
using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using System.Web.Http.ValueProviders;
using WTG.StaticAnalysis.Annotation;

namespace Microsoft.AspNetCore.Mvc;

[CodeAlive("ServiceHost upgrade to .NET 8. Delete the class once fully upgraded.")]
// Repeats implementation of FromUriAttribute in .NET Framework
public sealed class FromRouteAttribute : ModelBinderAttribute
{
	public override IEnumerable<ValueProviderFactory> GetValueProviderFactories(HttpConfiguration configuration)
	{
		if (configuration == null)
		{
			throw new ArgumentNullException(nameof(configuration));
		}

		foreach (var valueProviderFactory in base.GetValueProviderFactories(configuration))
		{
			if (valueProviderFactory is IUriValueProviderFactory)
			{
				yield return valueProviderFactory;
			}
		}
	}
}
#endif
