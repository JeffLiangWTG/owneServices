using System.IO;
using System.Reflection;

namespace CargoWise.RefDbRepo.MXReferenceData.Tests
{
	public static class TestUtils
	{
		public static Stream GetManifestResourceStream(string name) => Assembly.GetExecutingAssembly().GetManifestResourceStream(name);
	}
}
