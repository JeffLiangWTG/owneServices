using System.Net;

namespace CargoWise.RefDbRepo.Common.Utils
{
	public static class HtmlSanitizer
	{
		public static string Sanitize(string content)
		{
			return WebUtility.HtmlEncode(content);
		}
	}
}
