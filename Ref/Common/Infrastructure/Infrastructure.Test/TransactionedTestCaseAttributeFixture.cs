using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Common.Infrastructure.Test;

[TestFixture]
class TransactionedTestCaseAttributeFixture
{
	[Test]
	public void Test_RemoteDbTestCases()
	{
		var expected = new[] { DbSchema.RemoteDb, DbSchema.RemoteDbCollationCS };
		var actual = TransactionedTestCaseAttribute.RemoteDbTestCases()
			.Select(tc => tc.Arguments[0])
			.ToArray();

		Assert.That(actual, Is.EquivalentTo(expected));
	}

	[Test]
	public void Test_BlankDbTestCases()
	{
		var expected = new[] { DbSchema.None, DbSchema.NoneCollationCS };
		var actual = TransactionedTestCaseAttribute.BlankDbTestCases()
			.Select(tc => tc.Arguments[0])
			.ToArray();

		Assert.That(actual, Is.EquivalentTo(expected));
	}

	[Test]
	public void Test_GetBlankDbName()
	{
		Assert.That(TransactionedTestCaseAttribute.GetDbName(DbSchema.None), Is.EqualTo(BlankTestDbInitializer.DbName));
		Assert.That(TransactionedTestCaseAttribute.GetDbName(DbSchema.NoneCollationCS), Is.EqualTo(BlankTestDbInitializerWithCollationCS.DbName));
	}

	[Test]
	public void Test_GetRemoteDbName()
	{
		Assert.That(TransactionedTestCaseAttribute.GetDbName(DbSchema.RemoteDb), Is.EqualTo(RemoteTestDbInitializer.DbName));
		Assert.That(TransactionedTestCaseAttribute.GetDbName(DbSchema.RemoteDbCollationCS), Is.EqualTo(RemoteTestDbInitializerWithCollationCS.DbName));
	}

	[Test]
	public void Test_SafeDbName()
	{
		Assert.That(TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoSafe), Is.EqualTo(SafeTestDbInitializer.DbName));
	}

	[Test]
	public void Test_GetStagingDbName()
	{
		Assert.That(TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoStaging), Is.EqualTo(StagingTestDbInitializer.DbName));
	}

	
}
