using System;
using System.Linq;
using CargoWise.RefDbRepo.Common.Argument;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor
{
	public static class MetadataProviderExtensions
	{
		public static bool IsKeyProperty(this IMetadataProvider metadataProvider, string entityName, string propertyName)
		{
			Argument.NotNull(metadataProvider, nameof(metadataProvider));
			Argument.NotNullOrEmpty(entityName, nameof(entityName));
			Argument.NotNullOrEmpty(propertyName, nameof(propertyName));

			return metadataProvider.GetKeys(entityName).Any(x => x.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase));
		}
	}
}
