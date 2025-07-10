using System.IO;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Common.Infrastructure.Test;

class RemoteTestDbInitializer : AbstractTestDbInitializer
{
	internal const string DbName = "Test_RefDbRepoRemote";

	protected override string GetDbName => DbName;
	protected override string DacPacFilePath => Path.Combine(FolderHelper.GetBinFolder(), "RemoteDb.dacpac");
	protected override string[] GetWriterMemeberships => ["db_datareader", "db_datawriter"];

	protected override void Deploy(string collation)
	{
		DBHelper.Deploy(Conn, DacPacFilePath, DbName, TestConnectionString.GetAdmin(DbName));
	}
}
