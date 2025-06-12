using System.IO;
using System.Reflection;

namespace CargoWise.eHub.Portal.Tests.TestFiles
{
	class TestFileHelpers
	{
		internal static Stream GetResourceStream(string name)
		{
			return Assembly.GetExecutingAssembly().GetManifestResourceStream("CargoWise.eHub.Portal.Tests.TestFiles." + name);
		}

		internal static string GetResourceText(string name)
		{
			using (var resStream = GetResourceStream(name))
			using (var streamRdr = new StreamReader(resStream))
				return streamRdr.ReadToEnd();
		}

		internal static byte[] GetResourceData(string name)
		{
			using (var resStream = GetResourceStream(name))
			using (var binRdr = new BinaryReader(resStream))
				return binRdr.ReadBytes((int)resStream.Length);
		}
	}
}
