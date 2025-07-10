namespace CargoWise.RefDbRepo.Tools.Common
{
	public class FKRelationship
	{
		public FKRelationship(string table, string referencedTable, string column, string referencedColumn, string dataType)
		{
			Table = table;
			ReferencedTable = referencedTable;
			Column = column;
			ReferencedColumn = referencedColumn;
			DataType = dataType;
		}

		public string Table { get; set; }
		public string ReferencedTable { get; set; }
		public string Column { get; set; }
		public string ReferencedColumn { get; set; }
		public string DataType { get; set; }
	}
}
