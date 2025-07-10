using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.TypeProvider;

namespace CargoWise.RefDbRepo.Staging.DataPurgingProcessor
{
	public static class PurgeHelper
	{
		public static XElement[] GetEntityTypeElements(string xmlContent)
		{
			var xml = XElement.Parse(xmlContent);
			var isTransformed = xml.Element("OriginalSchema") != null;
			var entityTypeElements = xml.Element(isTransformed ? "OriginalSchema" : "Schema")?.Elements("EntityType")?.ToArray();
			return entityTypeElements;
		}

		public static string CreatePurgingSqlWithEntity(IEnumerable<Tuple<Type, Type>> entityList, string insertSql)
		{
			Argument.NotNull(entityList, nameof(entityList));

			var result = new StringBuilder();
			foreach (var entityType in entityList.Select(x => x.Item2).Distinct())
			{
				result.AppendLine(CultureInfo.InvariantCulture, $@"DROP TABLE IF EXISTS #{entityType.Name}Temp; CREATE TABLE #{entityType.Name}Temp (PK uniqueidentifier PRIMARY KEY)");
			}
			var insertionList = new List<Tuple<Type, Type>>(entityList);
			var rootType = insertionList.FirstOrDefault(x => x.Item1 == null);
			result.AppendLine(insertSql);
			insertionList.Remove(rootType);
			while (insertionList.Count > 0)
			{
				var insertType = insertionList.FirstOrDefault(x => x.Item1 == null || insertionList.All(y => y.Item2 != x.Item1));
				var parentType = insertType.Item1;

				var pk = insertType.Item2.GetPKPropertyName();
				var fk = insertType.Item2.GetFKPropertyInfo(parentType)?.Name;
				result.AppendLine(CultureInfo.InvariantCulture, $@"INSERT INTO #{insertType.Item2.Name}Temp (PK)
SELECT {pk}
FROM {insertType.Item2.Name}
JOIN #{parentType.Name}Temp ON {fk} = PK");
				result.AppendLine(CultureInfo.InvariantCulture, $@"WHERE {pk} NOT IN (SELECT PK FROM #{insertType.Item2.Name}Temp)");
				insertionList.Remove(insertType);
			}
			var purgingList = new List<Tuple<Type, Type>>(entityList);
			while (purgingList.Count > 0)
			{
				var purgingType = purgingList.FirstOrDefault(x => purgingList.All(y => y.Item1 != x.Item2));
				var pk = purgingType.Item2.GetPKPropertyName();
				result.AppendLine(CultureInfo.InvariantCulture, $@"
DELETE entity
FROM {purgingType.Item2.Name} entity
JOIN #{purgingType.Item2.Name}Temp ON  PK = entity.{pk}
");
				purgingList.Remove(purgingType);
			}
			return result.ToString();
		}
	}
}
