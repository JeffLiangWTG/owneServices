using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using NPOI.SS.UserModel;

namespace CargoWise.RefDbRepo.KRReferenceData.Business
{
	public class KeyLookupDetails<T> where T : RefDataRepoModelEntityType
	{
		public KeyLookupDetails(EntityConfiguration configuration, IRow row)
		{
			KeyFieldNames = GetEntityKeyNamesExcludingConstantValues(configuration);
			KeyValues = GetEntityKeyValues(GetMappingEntityType(configuration), row);
		}

		public IEnumerable<string> KeyFieldNames { get; private set; }
		public Dictionary<string, string> KeyValues { get; private set; }

		public string GetCombinedKeyValues()
		{
			var result = new StringBuilder();

			foreach (var key in KeyFieldNames)
			{
				string value;
				if (KeyValues.TryGetValue(key, out value))
				{
					result.Append(value);
				}
			}
			return result.ToString();
		}

		static EntityType GetEntityType(EntityConfiguration configuration)
		{
			return configuration.EntityTypes.Where(x => x.Name == typeof(T).Name).First();
		}

		static MappingEntityType GetMappingEntityType(EntityConfiguration configuration)
		{
			return configuration.EntityTypeExcelColumnMapping.EntityTypes.Where(x => x.Name == typeof(T).Name).First();
		}

		static IEnumerable<string> GetEntityKeyNamesExcludingConstantValues(EntityConfiguration configuration)
		{
			var entity = GetEntityType(configuration);
			var mappingEntityType = GetMappingEntityType(configuration);
			foreach (var key in entity.Key)
			{
				if (string.IsNullOrEmpty(entity.Properties.Single(x => x.Name == key.Name)?.ConstantValue))
				{
					yield return key.Name;
				}
			}
			if (configuration.EntityTypeExcelColumnMapping.IsCompositeKeyNeeded)
			{
				foreach (var name in mappingEntityType.Properties.Where(x => x.IsCompositeKey).Select(x => x.Name))
				{
					yield return name;
				}
			}
		}

		Dictionary<string, string> GetEntityKeyValues(MappingEntityType mappingEntityType, IRow row)
		{
			var result = new Dictionary<string, string>();

			foreach (var key in KeyFieldNames)
			{
				var cellValue = string.Empty;
				var cellIndex = mappingEntityType.Properties.SingleOrDefault(x => x.Name == key)?.ExcelColumn ?? -1;
				if (cellIndex != -1)
				{
					var cell = row.GetCell(cellIndex);
					cell?.SetCellType(CellType.String);
					result.Add(key, cell?.StringCellValue ?? string.Empty);
				}
			}
			return result;
		}
	}
}
