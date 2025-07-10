using System.IO;
using System.Reflection;
using CargoWise.Types;

namespace Enterprise.Customs.PL.Business.Testing;

public static class ResourceTestHelper
{
	public static StreamReader GetTestFileReader(this Assembly assembly, ZString documentPath)
	{
		var stream = assembly.GetManifestResourceStream(documentPath);
		return stream is not null
			? new StreamReader(stream)
			: throw new FileNotFoundException($"The specified embedded \"{documentPath}\" is not found.", documentPath);
	}

	public static string GetTestFile(this Assembly assembly, ZString documentPath)
	{
		using var reader = assembly.GetTestFileReader(documentPath);
		return new ZString(reader.ReadToEnd());
	}

	public static bool TryGetTestFile(this Assembly assembly, ZString documentPath, out ZString result)
	{
		using var stream = assembly.GetManifestResourceStream(documentPath);

		if (stream == null)
		{
			result = default;
			return false;
		}

		using var reader = new StreamReader(stream);
		result = new ZString(reader.ReadToEnd());

		return true;
	}
}
