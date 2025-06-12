using System.IO;
using System.Reflection;

namespace CargoWise.eHub.Products.GCT.Tests
{
	public class TestHelper
	{
		public static string GetEmbeddedResourceAsString(string resourceName)
		{
			using (var stream = GetEmbeddedResource(resourceName))
			{
				return new StreamReader(stream).ReadToEnd();
			}
		}

		public static Stream GetEmbeddedResource(string resourceName)
		{
			string fullResourceName = Assembly.GetExecutingAssembly().GetName().Name + '.' + resourceName;
			return Assembly.GetExecutingAssembly().GetManifestResourceStream(fullResourceName);
		}
	}
}