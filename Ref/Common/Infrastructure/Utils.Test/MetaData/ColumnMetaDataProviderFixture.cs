using System.Linq;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Common.Utils.Test;

[TestFixture]
[TransactionedTestCase]
class ColumnMetaDataProviderFixture
{
	[Test]
	public void GetMetaData_Returns_Columns_For_Valid_Object()
	{
		var connectionString = TestConnectionString.GetAdmin(null);
		using var provider = new ColumnMetaDataProvider(connectionString, TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoSafe));
		var columns = provider.GetMetaData("RefDbVersionControl");

		Assert.That(columns, Is.Not.Null);
		Assert.That(columns.Count, Is.GreaterThan(0), "Should return at least one column");

		Assert.That(columns.Any(c => c.ColumnName == "RVC_ParentPK"), "Should contain 'RVC_ParentPK' column");
	}

	[Test]
	public void GetMetaData_Returns_Empty_For_Invalid_Object()
	{
		var connectionString = TestConnectionString.GetAdmin(null);
		using var provider = new ColumnMetaDataProvider(connectionString, TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoSafe));
		var columns = provider.GetMetaData("NonExistentView");

		Assert.That(columns, Is.Not.Null);
		Assert.That(columns.Count, Is.EqualTo(0));
	}

	[Test]
	public void GetMetaData_Returns_Empty_If_Connection_Fails()
	{
		const string badConnectionString = "Server=invalid;Database=invalid;User Id=invalid;Password=invalid;";
		using var provider = new ColumnMetaDataProvider(badConnectionString, TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoSafe));
		var columns = provider.GetMetaData("RefAccTaxRateUserView");

		Assert.That(columns, Is.Not.Null);
		Assert.That(columns.Count, Is.EqualTo(0));
	}
}
