using System.Collections.Generic;
using CsvHelper.Configuration;

namespace CargoWise.RefDbRepo.USReferenceData.Services
{
	public interface ICsvParser
	{
		List<T> Parse<T>(string filePath, Configuration config = null) where T : new();
	}
}
