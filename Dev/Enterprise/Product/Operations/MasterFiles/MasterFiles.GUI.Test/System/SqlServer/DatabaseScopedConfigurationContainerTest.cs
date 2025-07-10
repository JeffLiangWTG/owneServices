using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Test
{
	[TestedType(typeof(DatabaseScopedConfigurationContainer))]
	sealed class DatabaseScopedConfigurationContainerTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new DatabaseScopedConfigurationContainer(connection, DbName);
		}

		AdminConnection connection;
		const string DbName = nameof(DatabaseScopedConfigurationContainerTest);
		protected override void SetUp()
		{
			base.SetUp();
			connection = Db.NewAdminConnection(Db.SqlMasterDb);
			AdoTestUtils.CreateDbDropExisting(connection, DbName);
		}

		protected override void TearDown()
		{
			AdoTestUtils.DropDbIfExists(connection, DbName);
			connection.Dispose();
			base.TearDown();
		}
	}
}
