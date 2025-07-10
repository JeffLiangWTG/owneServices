using System;
using System.Web;
using CargoWise.Common;

namespace Enterprise.Services.ServiceHost.Test;

static class HttpContextHelper
{
	public static IDisposable SetUp(HttpContext newContext)
	{
		var previousContext = HttpContext.Current;
		return new DisposableAction(() => HttpContext.Current = newContext, () => HttpContext.Current = previousContext);
	}
}
