using System.IO;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Common.Infrastructure.Test;

class RemoteTestDbInitializerWithCollationCS : AbstractTestDbInitializer
{
	internal const string DbName = "Test_RefDbRepoRemoteCollationCS";

	protected override string GetDbName => DbName;
	protected override string DacPacFilePath => Path.Combine(FolderHelper.GetBinFolder(), "RemoteDbCollationCS.dacpac");
	protected override string[] GetWriterMemeberships => ["db_datareader", "db_datawriter"];

	protected override void Deploy(string collation)
	{
		DBHelper.CreateDatabase(Conn, DbName, string.Empty, "SQL_Latin1_General_CP1_CS_AS");
		DBHelper.Deploy(Conn, DacPacFilePath, DbName, TestConnectionString.GetAdmin(DbName));
	}
}
