using System.IO;
using System.Reflection;

namespace CargoWise.RefDbRepo.BRReferenceData.Tests
{
	public static class Utils
	{
		public static Stream GetManifestResourceStream(string name) => Assembly.GetExecutingAssembly().GetManifestResourceStream(name);
	}
}
