using System.IO;

namespace Enterprise.Services.ServiceHost
{
	public static class ResourceHelpers
	{
		public static string GetStringResource(string path)
		{
			using (var rs = typeof(ResourceHelpers).Assembly.GetManifestResourceStream($"Enterprise.Services.ServiceHost.{path}"))
			using (var reader = new StreamReader(rs))
			{
				return reader.ReadToEnd();
			}
		}
	}
}
