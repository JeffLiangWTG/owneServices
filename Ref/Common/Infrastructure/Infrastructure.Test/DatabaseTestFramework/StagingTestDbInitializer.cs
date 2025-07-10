using System.IO;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Common.Infrastructure.Test;

class StagingTestDbInitializer : AbstractTestDbInitializer
{
	internal const string DbName = "Test_RefDbRepoStaging";
	internal static string[] WriterMemeberships => ["db_datareader", "db_datawriter", "db_ddladmin"];

	protected override string GetDbName => DbName;
	protected override string DacPacFilePath => Path.Combine(FolderHelper.GetBinFolder(), "StagingDb.dacpac");
	protected override string[] GetWriterMemeberships => WriterMemeberships;

	protected override void Deploy(string collation)
	{
		DBHelper.Deploy(Conn, DacPacFilePath, DbName, TestConnectionString.GetAdmin(DbName));
	}
}
