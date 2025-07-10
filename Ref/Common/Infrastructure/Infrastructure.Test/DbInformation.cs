using System.IO;
using CargoWise.RefDbRepo.Common.DbUpgrade;
using Microsoft.Data.SqlClient;

namespace CargoWise.RefDbRepo.Common.Infrastructure.Test
{
	class DbInformation : AbstractDbInformation
	{
		public DbInformation(string uniqueDbName, DbSchema dbSchema)
		{
			UniqueDbName = uniqueDbName;
			DbSchema = dbSchema;

			MemberShips = IsStagingDb()
				? StagingTestDbInitializer.WriterMemeberships
				: SafeTestDbInitializer.WriterMemeberships;
		}

		DbSchema DbSchema { get; }

		bool IsSafeDb()
		{
			return DbSchema == DbSchema.RefDbRepoSafe;
		}

		bool IsStagingDb()
		{
			return DbSchema == DbSchema.RefDbRepoStaging;
		}

		public void CreateSynonymForStagingDb(DbInformation dbInformation)
		{
			using var connection = new SqlConnection(TestConnectionString.GetAdmin(null));
			DBHelper.CreateSynonym(connection, DbName, dbInformation.DbName);
		}

		public void CreateFakeSynonym()
		{
			if (IsSafeDb())
			{
				using var connection = new SqlConnection(TestConnectionString.GetAdmin(null));
				DBHelper.CreateSynonym(connection, DbName);
			}
		}

		protected override void Deploy(SqlConnection connection, string collation)
		{
			var dbCreator = new DbCreator(connection);

			if (DbSchema == DbSchema.None)
			{
				Directory.CreateDirectory(TmpPath);
				dbCreator.CreateDatabase(DbName, TmpPath, collation);
				return;
			}

			var dacpacFile = string.Empty;
			switch (DbSchema)
			{
				case DbSchema.RefDbRepoSafe:
					dacpacFile = "SafeDb.dacpac";
					break;
				case DbSchema.RefDbRepoStaging:
					dacpacFile = "StagingDb.dacpac";
					break;
			}
			dbCreator.Deploy(Path.Combine(FolderHelper.GetBinFolder(), dacpacFile), DbName, TestConnectionString.GetAdmin(DbName));
		}
	}
}
