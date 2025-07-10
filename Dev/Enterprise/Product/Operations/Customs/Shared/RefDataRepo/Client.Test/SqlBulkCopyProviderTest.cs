using CargoWise.Data;
using NUnit.Framework;
using SqlBulkCopyOptions = CargoWise.Data.Providers.Common.SqlBulkCopyOptions;

namespace CargoWise.RefDataRepo.Ent.Client.Test
{
	class SqlBulkCopyProviderTest : TestCase
	{
		public void testGetSqlBulkCopy()
		{
			var bulkCopyProvider = new SqlBulkCopyProvider();
			using (var dbConnection = Db.NewExtraConnectionToMainDb())
			{
				var sqlConnection = ((IDbConnectionInternals)dbConnection).ADOConnection;
				var transaction = ((IDbConnectionInternals)dbConnection).ADOTransaction;
				var option = SqlBulkCopyOptions.Default;
				var bulkCopy = bulkCopyProvider.GetSqlBulkCopy(sqlConnection, option, transaction);
				AssertEquals(0, bulkCopy.ColumnMappings.Count);
				AssertEquals(transaction, bulkCopy.Transaction);
				AssertEquals(option, bulkCopy.BulkCopyOptions);
			}
		}
	}
}
