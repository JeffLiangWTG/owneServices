using System.Text.RegularExpressions;

namespace CargoWise.RefDbRepo.MetaDataGenerator;

partial class ExposedObjectSqlFileFinder(string directory) : IFileFinder
{
	public IEnumerable<string> GetFiles()
	{
		return Directory.GetFiles(directory, "*.sql", SearchOption.AllDirectories)
			.Where(filePath => DboViewNameRegex().IsMatch(Path.GetFileNameWithoutExtension(filePath)));
	}

	[GeneratedRegex(@"^[A-Za-z0-9]+View_V\d+$")]
	private static partial Regex DboViewNameRegex();
}
