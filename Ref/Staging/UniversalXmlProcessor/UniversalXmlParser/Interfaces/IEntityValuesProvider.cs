namespace CargoWise.RefDbRepo.UniversalXmlParser.Interfaces
{
	public interface IEntityValuesProvider
	{
		object GetDefaultValue(string entityName, string propertyName);

		bool IsNullable(string entityName, string propertyName);
	}
}
