using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;

namespace CargoWise.RefDbRepo.AUReferenceData.Services
{
	public static class TextToEntitiesConverter<T>
		where T : RefDataRepoModelEntityType, new()
	{
		public static IEnumerable<T> Convert(string textFilePath, IEnumerable<PropertyMapping<T>> mappings)
		{
			var lines = File.ReadAllLines(textFilePath);
			var entities = ParseEntities(lines, mappings);
			return entities;
		}

		public static IEnumerable<T> ConvertFromString(string content, IEnumerable<PropertyMapping<T>> mappings)
		{
			var lines = new List<string>();
			using (var reader = new StringReader(content))
			{
				string line;
				while ((line = reader.ReadLine()) != null)
				{
					lines.Add(line);
				}
			}
			var entities = ParseEntities(lines, mappings);
			return entities;
		}

		static IEnumerable<T> ParseEntities(IEnumerable<string> lines, IEnumerable<PropertyMapping<T>> mappings)
		{
			var converter = new LineToEntityConverter<T>(mappings, leadingPosition: 1);
			var entities = lines.Where(x => !string.IsNullOrEmpty(x.Trim())).Select(x => converter.Convert(x)).ToList();
			return entities;
		}
	}
}
