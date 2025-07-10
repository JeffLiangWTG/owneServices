namespace CargoWise.RefDbRepo.Common.DbUpgrade
{
	public interface ISchemaUpgrade
	{
		string GetDiffSql(string targetDbName);
		void CloneSchema(string sourceDbName, string targetDbName, IDbCreator dbCreator);
	}
}
