using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using CargoWise.RefDbRepo.STLBillingCollector.Business;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;

namespace CargoWise.RefDbRepo.STLBillingCollector.Tests
{
	class TestHelper
	{
		public static string ReadManifestResourceContent(string resourceDetails)
		{
			var result = string.Empty;
			using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceDetails))
			using (var reader = new StreamReader(stream))
			{
				result = reader.ReadToEnd();
			}
			return result;
		}
		public static Stream ReadManifestResourceAsStream(string resourceDetails)
		{
			using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceDetails))
			{
				if (stream == null)
				{
					throw new FileNotFoundException("Resource not found", resourceDetails);
				}

				using (var reader = new StreamReader(stream))
				{
					string base64Content = reader.ReadToEnd();
					byte[] byteArray = Convert.FromBase64String(base64Content);
					return new MemoryStream(byteArray);
				}
			}
		}

		public static DirectoryInfo GenerateTempDirectory()
		{
			var uniqueDir = Path.Combine(Path.GetTempPath(), "STLCollectorTestOutput_" + Guid.NewGuid().ToString("N"));
			return Directory.CreateDirectory(uniqueDir);
		}
	}
	public class STLBillingCollectorForTest : STLBillingCollectorHelper
	{
		public STLBillingCollectorForTest(IHttpClientHelper httpClientHelper, StringBuilder errorBuilder) : base(httpClientHelper, errorBuilder) {}
		protected override DateTime PublicationDateTime => new DateTime(2024, 02, 26, 22, 51, 56);
	}
}
