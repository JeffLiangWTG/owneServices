using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.TypeProvider;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor
{
	public static class OdataExpandHelper
	{
		public static string Expand(Type type, IMetadataProvider metadataProvider, string previousPath)
		{
			Argument.NotNull(type, nameof(type));
			Argument.NotNull(metadataProvider, nameof(metadataProvider));

			
			var relatedEntityTypeNames = (metadataProvider.OriginalMetadataProvider ?? metadataProvider).GetProperties(type.Name)?.Where(x => !x.StartsWith(type.GetTablePrefix(), StringComparison.OrdinalIgnoreCase)).ToArray();
			var expandOptions = new List<string>();
			foreach (var relatedEntityTypeName in relatedEntityTypeNames)
			{
				var relatedEntityType = type.GetTypeFromBaseType(relatedEntityTypeName);
				if (relatedEntityType != null)
				{
					var relatedEntityPropertyType = typeof(ICollection<>).MakeGenericType(relatedEntityType);
					var relatedEntityProperty = type.GetProperties().FirstOrDefault(x => relatedEntityPropertyType.IsAssignableFrom(x.PropertyType));
					if (relatedEntityProperty != null)
					{
						expandOptions.Add(Expand(relatedEntityType, metadataProvider, relatedEntityProperty.Name));
					}
				}
			}
			var result = string.Join(",", expandOptions);
			if (!string.IsNullOrEmpty(previousPath))
			{
				result = result.Length > 0 ? $"{previousPath}($expand={result})" : previousPath;
			}
			return result;
		}
	}
}
