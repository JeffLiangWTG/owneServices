using System;
using System.IO;
using System.Threading.Tasks;

namespace CargoWise.RefDbRepo.Common.Utils;

public class FileReader(string filePath) : IFileReader
{
	public async Task<string> ReadAllTextAsync()
	{
		if (!File.Exists(filePath))
		{
			var csrFileName = Path.GetFileName(filePath);
			throw new InvalidOperationException($"{csrFileName} does not exist.");
		}
		return await File.ReadAllTextAsync(filePath);
	}

}
