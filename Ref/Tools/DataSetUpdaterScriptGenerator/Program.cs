using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.RefDbRepo.Client.SqlServer;
using CargoWise.RefDbRepo.Common.DbUpgrade;
using Microsoft.Data.SqlClient;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	class Program
	{
		static Program() { }

		static void Main(string[] args)
		{
			var dbName = "CW_RefDatabaseForScriptGen";
			var connString = ApplicationConfig.ConnectionString;
			using (var conn = new SqlConnection(connString))
			{
				conn.Open();
				var dbCreator = new DbCreator(conn);
				try
				{
					dbCreator.DropDatabase(dbName);
					dbCreator.CreateDatabase(dbName, Path.GetTempPath());
					dbCreator.Deploy(Path.Combine(FolderHelper.GetBinFolder(), "RemoteDb.dacpac"), dbName, connString);
					conn.ChangeDatabase(dbName);
					var schemaInfo = new SchemaInfo(conn);
					var sqlBuilder = new SqlServerSQLBuilder();
					var updaterInfos = UpdaterInfoProvider.GetUpdaterInfos.ToArray();
					var scriptGenerator = new ScriptGenerator(sqlBuilder, schemaInfo, updaterInfos.Select(x => x.Item1));
					var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"..\..\..\Service\SchemaManagement\RemoteDbManager\UpdaterScript");
					var customScriptProvider = new CustomScriptProvider();
					var classGenerator = new ScriptClassGenerator(scriptGenerator, schemaInfo, sqlBuilder, customScriptProvider);
					foreach (var info in updaterInfos)
					{
						var content = classGenerator.GenerateScriptContent(info.Item1, info.Item2);
						ScriptClassGenerator.WriteTo(info.Item1, info.Item2, content, path);
					}
				}
				finally
				{
					dbCreator.DropDatabase(dbName);
				}
			}
		}
	}
}
