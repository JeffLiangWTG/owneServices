namespace Enterprise.MasterData.GUI
{
	public class ColumnDefinition
	{
		public ColumnDefinition(string header, string property, string style = null, GridColumnType columnType = GridColumnType.TextColumn)
		{
			Header = header;
			Property = property;
			Style = style;
			ColumnType = columnType;
		}

		public string Header { get; }
		public string Property { get; }
		public string Style { get; }
		public GridColumnType ColumnType { get; }

		public enum GridColumnType
		{
			CheckBoxColumn,
			RadioBoxColumn,
			TextColumn
		}
	}
}
