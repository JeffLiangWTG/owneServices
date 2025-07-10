using System;
using System.Collections.Generic;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace CargoWise.RefDbRepo.Common.Infrastructure.Test
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
	public sealed class CreateDatabaseAttribute : Attribute, ITestAction
	{
		public CreateDatabaseAttribute(string uniqueDbName, DbSchema dbSchema, ActionTargets targets = ActionTargets.Default)
		{
			Argument.Argument.NotNullOrEmpty(uniqueDbName, nameof(uniqueDbName));

			UniqueDbName = uniqueDbName;
			DbSchema = dbSchema;

			dbInformation = new DbInformation(uniqueDbName, dbSchema);
			dbInformationList = new List<DbInformation> { dbInformation };
			Targets = targets;
		}

		public string UniqueDbName { get; }
		public DbSchema DbSchema { get; }

		public CreateDatabaseAttribute(string uniqueSafeDbName, DbSchema safeDbSchema, string uniqueStagingDbName, DbSchema stagingDbSchema, ActionTargets targets = ActionTargets.Default)
		{
			Argument.Argument.NotNullOrEmpty(uniqueSafeDbName, nameof(uniqueSafeDbName));
			Argument.Argument.NotNullOrEmpty(uniqueStagingDbName, nameof(uniqueStagingDbName));

			UniqueSafeDbName = uniqueSafeDbName;
			SafeDbSchema = safeDbSchema;
			UniqueStagingDbName = uniqueStagingDbName;
			StagingDbSchema = stagingDbSchema;

			safeDbInformation = new DbInformation(uniqueSafeDbName, safeDbSchema);
			stagingDbInformation = new DbInformation(uniqueStagingDbName, stagingDbSchema);
			dbInformationList = new List<DbInformation> { safeDbInformation, stagingDbInformation };
			Targets = targets;
		}

		public string UniqueSafeDbName { get; }
		public DbSchema SafeDbSchema { get; }
		public string UniqueStagingDbName { get; }
		public DbSchema StagingDbSchema { get; }

		public ActionTargets Targets { get; }

		public void AfterTest(ITest test)
		{
			dbInformationList.ForEach(databaseInformation =>
			{
				databaseInformation?.TearDown();
			});
		}

		public void BeforeTest(ITest test)
		{
			dbInformationList.ForEach(databaseInformation =>
			{
				databaseInformation?.CreateDatabase();
			});
			safeDbInformation?.CreateSynonymForStagingDb(stagingDbInformation);
			dbInformation?.CreateFakeSynonym();
		}

		public static string GetDbName(string uniqueDbName)
		{
			return DbNamePrefix + uniqueDbName;
		}

		readonly DbInformation dbInformation;
		readonly DbInformation safeDbInformation;
		readonly DbInformation stagingDbInformation;

		readonly List<DbInformation> dbInformationList;

		public const string DbNamePrefix = AbstractDbInformation.DbNamePrefix;
	}
}
