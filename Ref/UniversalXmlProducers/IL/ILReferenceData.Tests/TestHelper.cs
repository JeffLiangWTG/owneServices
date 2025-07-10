using System.IO;
using System.Reflection;

namespace CargoWise.RefDbRepo.ILReferenceData.Tests
{
	public static class TestHelper
	{
		public static string GetManifestResourceStream(string resourceName)
		{
			using (var fs = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
			using (StreamReader reader = new StreamReader(fs))
			{
				string result = reader.ReadToEnd();
				return result;
			}
		}
	}
}
