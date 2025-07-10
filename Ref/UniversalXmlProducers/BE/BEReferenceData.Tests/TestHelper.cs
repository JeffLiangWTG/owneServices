using System.IO;
using System.Reflection;

namespace CargoWise.RefDbRepo.BEReferenceData.Business.Testing
{
	internal static class TestHelper
	{
		internal static string ReadManifestResourceContent(string resourceDetails)
		{
			var result = string.Empty;
			using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceDetails))
			using (var reader = new StreamReader(stream))
			{
				result = reader.ReadToEnd();
			}
			return result;
		}

		internal static void SimulateDownload(string fileName, string resourceDetails)
		{
			var assembly = Assembly.GetExecutingAssembly();
			using (var stream = assembly.GetManifestResourceStream(resourceDetails))
			using (var writer = new FileStream(fileName, FileMode.Create))
			{
				stream.Seek(0, SeekOrigin.Begin);
				stream.CopyTo(writer);

				writer.Flush();
			}
		}
	}
}

