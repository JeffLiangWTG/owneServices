using System.IO;
using System.Reflection;

namespace CargoWise.RefDbRepo.ESReferenceData.Tests;

public class TestFileHelper(string testClassName)
{
	const string CommonOutputPath = "ES";

	readonly string testClassName = testClassName;

	public string OutputFolder { get; private set; }

	public bool CreateOutputFolder(Assembly assembly)
	{
		if (string.IsNullOrEmpty(OutputFolder))
		{
			var esPath = Path.Combine(Path.GetDirectoryName(assembly.Location), CommonOutputPath);
			testFolder = Path.Combine(esPath, testClassName);
			OutputFolder = Path.Combine(testFolder, "Output");
			return true;
		}

		return false;
	}

	string testFolder;

	public void DeleteOutputFolder()
	{
		if (Directory.Exists(testFolder))
		{
			Directory.Delete(testFolder, true);
		}
	}

	public string GetFileContent(string inputPath, string fileName) => File.ReadAllText(Path.Combine(inputPath, fileName));
}
