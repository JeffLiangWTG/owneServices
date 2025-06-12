using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace CargoWise.eHub.Shared.Mime.Tests
{
	[DebuggerStepThrough]
	class TestFileHelpers
	{
		internal static Stream GetResourceStream(string name)
		{
			return Assembly.GetExecutingAssembly().GetManifestResourceStream("CargoWise.eHub.Shared.Mime.Tests." + name);
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
