using System.IO;
using System.Reflection;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.Test
{
	static class EmbeddedResourceHelper
	{
		public static string ReadManifestResourceContent(string resourceDetails)
		{
			using (var stream = GetManifestResourceStreamSafe(Assembly.GetCallingAssembly(), resourceDetails))
			using (var streamReader = new StreamReader(stream))
			{
				return streamReader.ReadToEnd();
			}
		}

		static Stream GetManifestResourceStreamSafe(Assembly assembly, string resourceDetails)
		{
			var manifestResourceStream = assembly.GetManifestResourceStream(resourceDetails);
			return manifestResourceStream ?? throw new IOException("Missing resource: " + resourceDetails);
		}
	}
}
