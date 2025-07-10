namespace CargoWise.RefDbRepo.Common.Web.Auth
{
	public class RefUserAuthorization
	{
		public RefUserAuthorization(string user, string dataSetName, string tableName, string columnName, string columnValue)
		{
			UA_User = user;
			UA_DataSetName = dataSetName;
			UA_TableName = tableName;
			UA_ColumnName = columnName;
			UA_ColumnValue = columnValue;
		}

		public string UA_User { get; private set; }
		public string UA_DataSetName { get; private set; }
		public string UA_TableName { get; private set; }
		public string UA_ColumnName { get; private set; }
		public string UA_ColumnValue { get; private set; }
	}
}
