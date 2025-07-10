namespace CargoWise.RefDbRepo.UniversalXmlParser.Interfaces
{
	public interface IEntityMissingValuesProvider
	{
		void SetPrimaryKey(object instance);
		void FillOutMissingValues(object instance);
	}
}
