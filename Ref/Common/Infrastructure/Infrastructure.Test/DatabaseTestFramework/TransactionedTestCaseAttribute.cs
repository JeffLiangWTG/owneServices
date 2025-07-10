using System;
using System.Collections.Generic;
using System.Transactions;
using Microsoft.Data.SqlClient;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace CargoWise.RefDbRepo.Common.Infrastructure.Test;

[Property("DAT:CapabilityRequirements", "SQL2019+")]
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class TransactionedTestCaseAttribute : Attribute, ITestAction
{
	public void BeforeTest(ITest test)
	{
		_ = DatabaseSetup.Value;

		transactionScope = new TransactionScope(
			TransactionScopeOption.Required,
			new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted, Timeout = TimeSpan.FromMinutes(5) },
			TransactionScopeAsyncFlowOption.Enabled
		);
		TransactionManager.ImplicitDistributedTransactions = true;
	}

	public void AfterTest(ITest test)
	{
		transactionScope?.Dispose();
	}

	TransactionScope transactionScope;

	public ActionTargets Targets => ActionTargets.Test;

	static readonly Lazy<List<AbstractTestDbInitializer>>
		DatabaseSetup = new(() =>
		{
			using var connection = new SqlConnection(TestConnectionString.GetAdmin(null));

			List<AbstractTestDbInitializer> testDbInitializers = [
				new BlankTestDbInitializer(),
				new BlankTestDbInitializerWithCollationCS(),
				new StagingTestDbInitializer(),
				// SafeTestDbInitializer must after StagingTestDbInitializer, it will create synonym to StagingDb
				new SafeTestDbInitializer(),
				new RemoteTestDbInitializer(),
				new RemoteTestDbInitializerWithCollationCS()
			];

			foreach (var initializer in testDbInitializers)
			{
				initializer.SetUp(connection);
			}

			return testDbInitializers;
		});

	public static string GetDbName(DbSchema dbSchema)
	{
		return dbSchema switch
		{
			DbSchema.RefDbRepoSafe => SafeTestDbInitializer.DbName,
			DbSchema.RefDbRepoStaging => StagingTestDbInitializer.DbName,
			DbSchema.None => BlankTestDbInitializer.DbName,
			DbSchema.NoneCollationCS => BlankTestDbInitializerWithCollationCS.DbName,
			DbSchema.RemoteDb => RemoteTestDbInitializer.DbName,
			DbSchema.RemoteDbCollationCS => RemoteTestDbInitializerWithCollationCS.DbName,
			_ => throw new ArgumentOutOfRangeException(nameof(dbSchema), dbSchema, null)
		};
	}

	public static IEnumerable<TestCaseData> RemoteDbTestCases()
	{
		yield return new TestCaseData(DbSchema.RemoteDb);
		yield return new TestCaseData(DbSchema.RemoteDbCollationCS);
	}

	public static IEnumerable<TestCaseData> BlankDbTestCases()
	{
		yield return new TestCaseData(DbSchema.None);
		yield return new TestCaseData(DbSchema.NoneCollationCS);
	}
}
