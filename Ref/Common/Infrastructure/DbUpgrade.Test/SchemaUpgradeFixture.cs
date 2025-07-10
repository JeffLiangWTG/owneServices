using System.Linq;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Common.DbUpgrade.Test
{
	[TestFixture]
	public class SchemaUpgradeFixture
	{
		[Test]
		[TransactionedTestCase]
		[Property("DAT:CapabilityRequirements", "SQL2019+")]
		public void GetDiffSql()
		{
			using (var connection = new SqlConnection(TestConnectionString.GetAdmin("master")))
			{
				connection.Open();
				var dbCreator = new DbCreator(connection);
				var sql1 = @"CREATE TABLE TestXXX (
Column1 uniqueidentifier,
Column2 char(3)
)";

				var tableTerm = "SqlTable";
				var testDbName1 = TransactionedTestCaseAttribute.GetDbName(DbSchema.None);
				dbCreator.ExcuteDbScript(testDbName1, sql1, null);
				var sqlDiff = new SchemaUpgrade("DbTest.dacpac", TestConnectionString.DataSource, null, null).GetDiffSql(testDbName1);
				Assert.That(sqlDiff, Does.Contain(
$@"PRINT N'Altering {tableTerm} [dbo].[TestXXX]...';


GO
BEGIN TRANSACTION;

SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;

SET XACT_ABORT ON;

UPDATE [dbo].[TestXXX]
SET    [Column2] = ''
WHERE  [Column2] IS NULL;

ALTER TABLE [dbo].[TestXXX] ALTER COLUMN [Column2] CHAR (3) NOT NULL;

COMMIT TRANSACTION;

SET TRANSACTION ISOLATION LEVEL READ COMMITTED;


GO
ALTER TABLE [dbo].[TestXXX]
    ADD [Column3] VARCHAR (MAX) NULL;


GO
PRINT N'Update complete.';"));
			}
		}

		[Test]
		[TransactionedTestCase]
		[Property("DAT:CapabilityRequirements", "SQL2019+")]
		public void GetDiffViewFunctionProcedureSql()
		{
			using (var connection = new SqlConnection(TestConnectionString.GetAdmin("master")))
			{
				connection.Open();
				var dbCreator = new DbCreator(connection);
				var sqlCreateTable = @"CREATE TABLE TestXXX (
Column1 uniqueidentifier,
Column2 char(3)
)";
				var sqlCreateView = @"CREATE VIEW TestXXXView
AS
SELECT Column1, Column2 FROM TestXXX;";
				var sqlCreateProcedure = @"CREATE PROCEDURE dbo.myproc @myid int
AS
BEGIN
 SELECT * From TestXXX
END";
				var sqlCreateFunction = @"CREATE FUNCTION myfunc()
RETURNS int AS
BEGIN
	RETURN (SELECT COUNT(*) FROM TestXXX)
END";

				var viewTerm = "SqlView";

				var procedureTerm = "SqlProcedure";

				var functionTerm = "SqlScalarFunction";
				var testDbName1 = TransactionedTestCaseAttribute.GetDbName(DbSchema.None);
				dbCreator.ExcuteDbScript(testDbName1, sqlCreateTable, null);
				dbCreator.ExcuteDbScript(testDbName1, sqlCreateView, null);
				dbCreator.ExcuteDbScript(testDbName1, sqlCreateProcedure, null);
				dbCreator.ExcuteDbScript(testDbName1, sqlCreateFunction, null);
				var sqlDiff = new SchemaUpgrade("DbTest.dacpac", TestConnectionString.DataSource, null, null).GetDiffSql(testDbName1);
				Assert.That(sqlDiff, Does.Contain(
$@"GO
PRINT N'Dropping {viewTerm} [dbo].[TestXXXView]...';


GO
DROP VIEW [dbo].[TestXXXView];"));

				Assert.That(sqlDiff, Does.Contain($@"PRINT N'Dropping {procedureTerm} [dbo].[myproc]...';


GO
DROP PROCEDURE [dbo].[myproc];"));
				Assert.That(sqlDiff, Does.Contain($@"PRINT N'Dropping {functionTerm} [dbo].[myfunc]...';


GO
DROP FUNCTION [dbo].[myfunc];"));
			}
		}

		[Test]
		[CreateDatabase("C2284710C1674B6BB14822FE8D96A78D", DbSchema.None)]
		[Property("DAT:CapabilityRequirements", "SQL2019+")]
		public void CloneSchema()
		{
			using (var connection = new SqlConnection(TestConnectionString.GetAdmin("master")))
			{
				connection.Open();
				var dbCreator = new DbCreator(connection);
				var sql1 = @"CREATE TABLE TestXXX (
Column1 uniqueidentifier,
Column2 char(3)
)";
				var testDbName1 = CreateDatabaseAttribute.DbNamePrefix + "C2284710C1674B6BB14822FE8D96A78D";
				dbCreator.ExcuteDbScript(testDbName1, sql1, null);
				var testDbName2 = CreateDatabaseAttribute.DbNamePrefix + "087F8AA591404FA294F0AF090CBD0583";
				dbCreator.DropDatabase(testDbName2);
				var schemaUpgrade = new SchemaUpgrade("DbTest.dacpac", TestConnectionString.DataSource, null, null);
				schemaUpgrade.CloneSchema(testDbName1, testDbName2, dbCreator);
				var diff1 = schemaUpgrade.GetDiffSql(testDbName1).Replace(testDbName1, "");
				var diff2 = schemaUpgrade.GetDiffSql(testDbName2).Replace(testDbName2, "");
				Assert.That(diff1, Is.EqualTo(diff2));
			}
		}

		[Test]
		[CreateDatabase("C2284710C1674B6BB14822FE8D96A78D", DbSchema.None)]
		[Property("DAT:CapabilityRequirements", "SQL2019+")]
		public void ClonedSchemaHasDefaultUsers()
		{
			var testDbName1 = CreateDatabaseAttribute.DbNamePrefix + "C2284710C1674B6BB14822FE8D96A78D";
			var testDbName2 = CreateDatabaseAttribute.DbNamePrefix + "087F8AA591404FA294F0AF090CBD0583";
			var defaultUsers = new[] { "[refdbrepowriter]", "[refdbreporeader]" };
			using (var connection = new SqlConnection(TestConnectionString.GetAdmin("master")))
			{
				connection.Open();
				var dbCreator = new DbCreator(connection);
				dbCreator.DropDatabase(testDbName2);

				foreach (var user in defaultUsers)
				{
					dbCreator.ExcuteDbScript(testDbName1, $"CREATE USER {user} WITHOUT LOGIN");
				}
				dbCreator.ExcuteDbScript(testDbName1, "CREATE USER notvalid WITHOUT LOGIN");

				var schemaUpgrade = new SchemaUpgrade("DbTest.dacpac", TestConnectionString.DataSource, null, null);
				schemaUpgrade.CloneSchema(testDbName1, testDbName2, dbCreator);
			}

			using (var conn2 = new SqlConnection(TestConnectionString.GetAdmin(testDbName2)))
			{
				conn2.Open();
				using (var cmd = conn2.CreateCommand())
				{
#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
					cmd.CommandText = $"select COUNT(*) from sys.sysusers where name in ({string.Join(",", defaultUsers.Select(x => $"'{x.Replace("[", "").Replace("]", "")}'"))})";
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities
					Assert.AreEqual(2, cmd.ExecuteScalar());
				}
				using (var cmd = conn2.CreateCommand())
				{
					cmd.CommandText = $"select COUNT(*) from sys.sysusers where name = 'notvalid'";
					Assert.AreEqual(0, cmd.ExecuteScalar());
				}
			}
		}
	}
}
