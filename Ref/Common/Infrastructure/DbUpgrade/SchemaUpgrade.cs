using System;
using System.IO;
using System.Linq;
using Microsoft.Data.SqlClient;
using Microsoft.SqlServer.Dac.Compare;

namespace CargoWise.RefDbRepo.Common.DbUpgrade
{
	public class SchemaUpgrade : ISchemaUpgrade
	{
		public SchemaUpgrade(string dacpacFileName, string dataSource, string userId, string password)
		{
			Argument.Argument.NotNullOrEmpty(dataSource, nameof(dataSource));
			Argument.Argument.NotNullOrEmpty(dacpacFileName, nameof(dacpacFileName));

			this.dataSource = dataSource;
			this.userId = userId;
			this.password = password;
			this.dacpacFileName = dacpacFileName;
		}

		public void CloneSchema(string sourceDbName, string targetDbName, IDbCreator dbCreator)
		{
			Argument.Argument.NotNullOrEmpty(sourceDbName, nameof(sourceDbName));
			Argument.Argument.NotNullOrEmpty(targetDbName, nameof(targetDbName));
			Argument.Argument.NotNull(dbCreator, nameof(dbCreator));
			dbCreator.CreateDatabase(targetDbName, null);

			var sourceSchemaEndPoint = new SchemaCompareDatabaseEndpoint(GetConnectionString(sourceDbName));
			var targetSchemaEndPoint = new SchemaCompareDatabaseEndpoint(GetConnectionString(targetDbName));
			Console.WriteLine($"Creating [refdbreporeader] user on database {targetDbName}");
			dbCreator.ExcuteDbScript(targetDbName, "CREATE USER [refdbreporeader] WITHOUT LOGIN");
			Console.WriteLine($"Creating [refdbrepowriter] user on database {targetDbName}");
			dbCreator.ExcuteDbScript(targetDbName, "CREATE USER [refdbrepowriter] WITHOUT LOGIN");
			var diffScript = GetScriptComparison(sourceSchemaEndPoint, targetSchemaEndPoint);

			Console.WriteLine($"Cloning schema from database {sourceDbName} to database {targetDbName}");
			dbCreator.ExcuteDbScript(targetDbName, diffScript);
		}

		public string GetDiffSql(string targetDbName)
		{
			Argument.Argument.NotNullOrEmpty(targetDbName, nameof(targetDbName));
			var dacpacFilePath = Path.Combine(FolderHelper.GetBinFolder(), dacpacFileName);

			Console.WriteLine($"Dacpac file path: {dacpacFilePath}");
			var sourceDacpac = new SchemaCompareDacpacEndpoint(dacpacFilePath);
			var targetDatabase = new SchemaCompareDatabaseEndpoint(GetConnectionString(targetDbName));
			return GetScriptComparison(sourceDacpac, targetDatabase);
		}

		static string GetScriptComparison(SchemaCompareEndpoint source, SchemaCompareDatabaseEndpoint target)
		{
			Argument.Argument.NotNull(source, nameof(source));
			Argument.Argument.NotNull(target, nameof(target));

			var comparison = new SchemaComparison(source, target);
			comparison.Options.IgnoreRoleMembership = true;
			comparison.Options.CommentOutSetVarDeclarations = true;
			comparison.Options.IgnoreComments = true;
			comparison.Options.BlockOnPossibleDataLoss = false;
			comparison.Options.IgnoreUserSettingsObjects = true;
			comparison.Options.GenerateSmartDefaults = true;

			var comparisonResult = comparison.Compare();
			var allowObjecTypes = AllowedObjectTypes;

			foreach (var difference in comparisonResult.Differences)
			{
				if (!allowObjecTypes.Any(x => x.Equals(difference.Name, StringComparison.OrdinalIgnoreCase)))
				{
					comparisonResult.Exclude(difference);
				}
			}
			return comparisonResult.GenerateScript(target.DatabaseName)?.Script ?? string.Empty;
		}

		static string[] AllowedObjectTypes => new[] { "SqlTable", "Table", "SqlView", "View", "Procedure", "SqlProcedure", "Function", "SqlScalarFunction" };

		string GetConnectionString(string dbName)
		{
			var builder = new SqlConnectionStringBuilder
			{
				InitialCatalog = dbName,
				DataSource = dataSource,
				TrustServerCertificate = true
			};
			if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(password))
			{
				builder.IntegratedSecurity = true;
			}
			else
			{
				builder.UserID = userId;
				builder.Password = password;
				builder.PersistSecurityInfo = true;
			}
			return builder.ToString();
		}

		readonly string userId;
		readonly string password;
		readonly string dataSource;
		readonly string dacpacFileName;
	}
}
