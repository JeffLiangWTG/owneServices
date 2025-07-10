namespace CargoWise.RefDbRepo.Common.Infrastructure.Test;

class BlankTestDbInitializerWithCollationCS : AbstractTestDbInitializer
{
	internal const string DbName = "Test_RefDbRepoBlankCollationCS";

	protected override string GetDbName => DbName;
	protected override string DacPacFilePath => string.Empty;
	protected override string[] GetWriterMemeberships => ["db_datareader", "db_datawriter"];

	protected override void Deploy(string collation)
	{
		DBHelper.CreateDatabase(Conn, DbName, string.Empty,  "SQL_Latin1_General_CP1_CS_AS");
	}
}
