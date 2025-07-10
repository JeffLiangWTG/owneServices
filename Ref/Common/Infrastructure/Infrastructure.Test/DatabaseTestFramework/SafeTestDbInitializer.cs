using System.IO;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Common.Infrastructure.Test;

class SafeTestDbInitializer : AbstractTestDbInitializer
{
	internal const string DbName = "Test_RefDbRepoSafe";
	internal static string[] WriterMemeberships => ["db_datareader", "db_datawriter"];

	protected override string GetDbName => DbName;
	protected override string DacPacFilePath => Path.Combine(FolderHelper.GetBinFolder(), "SafeDb.dacpac");
	protected override string[] GetWriterMemeberships => WriterMemeberships;

	protected override void Deploy(string collation)
	{
		DBHelper.Deploy(Conn, DacPacFilePath, DbName, TestConnectionString.GetAdmin(DbName));
		DBHelper.CreateSynonym(Conn, DbName, StagingTestDbInitializer.DbName);
	}
}
