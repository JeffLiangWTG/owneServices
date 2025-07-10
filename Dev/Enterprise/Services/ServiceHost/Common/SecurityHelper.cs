using System.Web;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Services.ServiceHost.Common
{
	public static class SecurityHelper
	{
		public static string[][] MandatoryHeaders { get; } = {
			new[] { "X-Frame-Options", "SAMEORIGIN" },
			new[] { "X-XSS-Protection", (NoResString)"1; mode=block" },
			new[] { "Referrer-Policy", "no-referrer" },
			new[] { "Content-Security-Policy", (NoResString)"default-src 'none'; upgrade-insecure-requests; style-src 'self'; script-src 'self'; img-src 'self'; frame-ancestors 'none'" },
		};

		public static string[] DangerousHeaders { get; } = new string[]
		{
			(NoResString)"Server",
			"X-AspNet-Version",
			"X-Powered-By"
		};

		public static void SecureResponse(HttpResponse response)
		{
			if (response?.Headers == null)
			{
				return;
			}

			foreach (var header in DangerousHeaders)
			{
				response.Headers.Remove(header);
			}
			foreach (var header in MandatoryHeaders)
			{
				response.Headers.Remove(header[0]);
				response.Headers.Add(header[0], header[1]);
			}
		}

		public static void SetCacheHeader(HttpResponse response)
		{
			response.Headers.Remove("Cache-Control");
			if (IsStaticResource(response.ContentType))
			{
				response.Headers.Add("Cache-Control", (NoResString)"no-cache=\"Set-Cookie\"");
			}
			else
			{
				response.Headers.Add("Cache-Control", (NoResString)"no-store, no-cache");
			}
		}

		static bool IsStaticResource(string contentType)
		{
			contentType = contentType.ToLower();
			if (contentType.StartsWith((NoResString)"font/") || contentType.StartsWith((NoResString)"image/")
				|| contentType.StartsWith("application/ecmascript") || contentType.StartsWith("application/javascript")
				|| contentType.StartsWith("text/css"))
			{
				return true;
			}
			return false;
		}
	}
}
