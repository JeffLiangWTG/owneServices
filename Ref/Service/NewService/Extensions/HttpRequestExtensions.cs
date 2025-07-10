using System.IO;
using Microsoft.AspNetCore.Http;

namespace CargoWise.RefDbRepo.NewService.Extensions
{
	public static class HttpRequestExtensions
	{
		public static string GetBody(this HttpRequest request)
		{
#pragma warning disable CA2000 // Dispose objects before losing scope
			var reader = new StreamReader(request.Body);
#pragma warning restore CA2000 // Dispose objects before losing scope
			var result = reader.ReadToEnd();
			request.Body.Position = 0;
			return result;
		}
	}
}
