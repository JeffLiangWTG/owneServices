using System.IO;
using System.Reflection;
using System.Xml.Serialization;

namespace CargoWise.RefDbRepo.NOReferenceData.Services
{
	public static class EmbeddedResourceHelper
	{
		public static string ReadManifestResourceContent(string resourceDetails)
		{
			var assembly = Assembly.GetCallingAssembly();
			using (var stream = ReadManifestResourceContentAsStream(resourceDetails, assembly))
			{
				return stream.ReadToEnd();
			}
		}

		public static StreamReader ReadManifestResourceContentAsStream(string resourceDetails, Assembly assembly)
		{
			var stream = assembly.GetManifestResourceStream(resourceDetails) ?? throw new FileNotFoundException($"Missing resource: {resourceDetails}");
			var reader = new StreamReader(stream);
			return reader;
		}
	}
}
