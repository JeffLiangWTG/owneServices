using System.IO;
using WinzorFramework.JSInterop;

namespace WinzorTestFramework;
public class DummyFileVersionHash : IFileVersionHash
{
	public string Get(string path)
	{
		return string.Empty;
	}

	public string GetHash(Stream stream)
	{
		return string.Empty;
	}
}
