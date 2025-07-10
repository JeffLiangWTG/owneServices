using System.Collections.Generic;

namespace ZAReferenceData.Services
{
	public interface ICSVParser
	{
		List<T> Parse<T>(string filePath) where T : new();
	}
}
