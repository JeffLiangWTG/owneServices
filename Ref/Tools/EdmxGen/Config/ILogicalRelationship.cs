namespace CargoWise.RefDbRepo.EdmxGen
{
	public interface ILogicalRelationship
	{
		string Table { get; set; }
		string ReferencedTable { get; set; }
		string Column { get; set; }
		string ReferencedColumn { get; set; }
		bool IsNullable { get; set; }

		string GetAssociationName();
	}
}
