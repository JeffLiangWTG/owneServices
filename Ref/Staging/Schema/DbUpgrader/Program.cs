using System.Linq;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.DbUpgrade;
using Microsoft.Data.SqlClient;

namespace CargoWise.RefDbRepo.Staging.DbUpgrader
{
	static class Program
	{
		static void Main(string[] args)
		{
			var connectionString = ApplicationConfig.StagingConnectionString;
			var forceUpgrade = false;
			if (args.Length > 0)
			{
				if (args.Contains("-force"))
				{
					forceUpgrade = true;
				}
				if (args[0] != "-force")
				{
					connectionString = connectionString.Replace("RefDbRepoStaging", args[0]);
				}
			}
			var builder = new SqlConnectionStringBuilder(connectionString);

			UpdateDatabase(builder, StagingSchemaVersion.Version, forceUpgrade);
			if (!forceUpgrade)
			{
				TransformData(builder, TransformationVersion.Version);
			}
		}

		static void UpdateDatabase(SqlConnectionStringBuilder builder, int targetVersion, bool forceUpgrade)
		{
			Argument.NotNull(builder, nameof(builder));
			using (var connection = new SqlConnection(builder.ToString()))
			{
				connection.Open();
				var versionMgr = new SchemaVersionManager(connection, VersionType.DbSchemaVersion, builder.InitialCatalog);
				var dbCreator = new DbCreator(connection);
				var schemaUpgrade = new SchemaUpgrade(DacPacFileName, builder.DataSource, builder.UserID, builder.Password);
				new UpgradeManager(
					connection,
					builder.InitialCatalog,
					GetPreUpgradeTransformationTasks(forceUpgrade),
					GetPostUpgradeTransformationTasks(forceUpgrade),
					targetVersion,
					schemaUpgrade,
					versionMgr,
					dbCreator, new DbLockout(connection, new[] { "refdbrepowriter", "refdbreporeader" }),
					forceUpgrade).Run();
			}
		}

		static TransformationTasks GetPreUpgradeTransformationTasks(bool forceUpgrade)
		{
			if (forceUpgrade)
			{
				return new EmptyTransformationTasks();
			}
			return new PreUpgradeTransformationTasks();
		}

		static TransformationTasks GetPostUpgradeTransformationTasks(bool forceUpgrade)
		{
			if (forceUpgrade)
			{
				return new EmptyTransformationTasks();
			}
			return new PostUpgradeTransformationTasks();
		}

		static void TransformData(SqlConnectionStringBuilder builder, int targetVersion)
		{
			Argument.NotNull(builder, nameof(builder));
			using (var connection = new SqlConnection(builder.ToString()))
			{
				connection.Open();
				var versionMgr = new SchemaVersionManager(connection, VersionType.DataTransformationVersion, builder.InitialCatalog);
				var tasks = new DataTransformationTasks();
				new DataTransformationManager(tasks, versionMgr, connection, targetVersion).Execute();
			}
		}

		const string DacPacFileName = "StagingDb.dacpac";
	}
}
