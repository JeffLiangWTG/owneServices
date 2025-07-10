using CargoWise.RefDbRepo.Common.DbUpgrade;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.Test
{
	[TestFixture]
	public class RemoveDuplicateRefAccTaxRateTransformationFixture
	{
		[Test]
		[TransactionedTestCase]
		public void Run()
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoSafe);
			using (var connection = new SqlConnection(TestConnectionString.GetAdmin(dbName)))
			{
				connection.Open();
				var dbCreator = new DbCreator(connection);
				dbCreator.ExcuteDbScript(dbName, @"ALTER INDEX IX_RefAccTaxRate_ZAT_RN_NKCountry_ReferenceRateType_StartDate ON RefAccTaxRate DISABLE;");
				PrepareData(dbCreator, dbName);
				using (var transaction = connection.BeginTransaction())
				{
					var task = new RemoveDuplicateRefAccTaxRateTransformation(1);
					task.Run(transaction);
					transaction.Commit();
				}
				using (var command = connection.CreateCommand())
				{
					command.CommandText = "SELECT COUNT(*) FROM RefAccTaxRate WHERE ZAT_PK IN ('82E0FBB1-4292-40C5-AEF0-9EFB0E22B61B')";
					Assert.AreEqual(1, (int)command.ExecuteScalar());
					command.CommandText = "SELECT COUNT(*) FROM RefAccTaxRate WHERE ZAT_PK IN ('9AE96C18-3662-40F1-8DAB-8693F2595B5F', '079479E1-0E58-42B6-9C83-1680877123A6')";
					Assert.AreEqual(0, (int)command.ExecuteScalar());
				}

				Assert.DoesNotThrow(() => dbCreator.ExcuteDbScript(dbName, @"ALTER INDEX IX_RefAccTaxRate_ZAT_RN_NKCountry_ReferenceRateType_StartDate ON RefAccTaxRate REBUILD;"));
			}
		}

		void PrepareData(DbCreator dbCreator, string dbName)
		{
			dbCreator.ExcuteDbScript(dbName, @"INSERT INTO RefAccTaxRate (ZAT_PK, ZAT_RN_NKCountry, ZAT_ReferenceRateType, ZAT_StartDate, ZAT_EndDate)
VALUES('9AE96C18-3662-40F1-8DAB-8693F2595B5F', 'AU', 'STD', '2019-04-22', '2019-04-23'),
('079479E1-0E58-42B6-9C83-1680877123A6', 'AU', 'STD', '2019-04-22', '2019-04-24'),
('82E0FBB1-4292-40C5-AEF0-9EFB0E22B61B', 'AU', 'STD', '2019-04-22', '2019-04-25')");
		}
	}
}
