using System.Collections.Generic;

namespace CargoWise.RefDbRepo.Staging.Schema_New
{
	public interface ISchemaMapper
	{
		(string originalName, string transformedName) NameMapper { get; }
		(string originalTablePrefix, string transformedTablePrefix) TablePrefixMapper { get; }
		List<(string originalProperty, string transformedProperty)> PropertyMappers { get; }
	}
}
