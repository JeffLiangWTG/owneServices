using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using static Enterprise.MasterFiles.GUI.DatabaseScopedConfigurationViewModel;

namespace Enterprise.MasterFiles.GUI.Test
{
	[TestedType(typeof(DatabaseScopedConfigurationContainerCollection))]
	sealed class DatabaseScopedConfigurationContainerCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DatabaseScopedConfigurationContainerCollection>
	{
		protected override DatabaseScopedConfigurationContainerCollection GetCollectionToTest()
		{
			return new DatabaseScopedConfigurationContainerCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new DatabaseScopedConfigurationContainer(connection, DbName);
		}

		AdminConnection connection;
		const string DbName = nameof(DatabaseScopedConfigurationContainerCollectionTest);
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
