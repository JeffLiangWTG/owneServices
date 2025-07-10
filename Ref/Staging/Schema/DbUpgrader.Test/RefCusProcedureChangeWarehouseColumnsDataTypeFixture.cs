using System;
using System.Globalization;
using CargoWise.RefDbRepo.Common.DbUpgrade;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.DbUpgrader.Test;

class RefCusProcedureChangeWarehouseColumnsDataTypeFixture
{
	[Test]
	[TransactionedTestCase]
	[Property("DAT:CapabilityRequirements", "SQL2019+")]
	public void RunAndAssertResultTwice()
	{
		var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.None);
		using var connection = new SqlConnection(TestConnectionString.GetAdmin(dbName));
		connection.Open();
		var dbCreator = new DbCreator(connection);
		dbCreator.ExcuteDbScript(dbName, @"
CREATE TABLE RefDbVersionControl
(
	ParentPK uniqueidentifier NOT NULL,
	ParentCode char(3) NOT NULL,
	LastUpdatedUTC datetime2 NOT NULL,
	Deleted BIT NOT NULL DEFAULT 0
)
GO
CREATE TABLE RefCusProcedure (
	ZZ6_PK UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_RefCusProcedure_ZZ6_PK DEFAULT (NEWID()),
	ZZ6_IntoWarehouse BIT NOT NULL CONSTRAINT DF_RefCusProcedure_ZZ6_IntoWarehouse DEFAULT (0),
	ZZ6_OutOfWarehouse BIT NOT NULL CONSTRAINT DF_RefCusProcedure_ZZ6_OutOfWarehouse DEFAULT (0)
)
GO
CREATE TRIGGER RefCusProcedure_Version_Create
	ON RefCusProcedure
	FOR INSERT
	AS
	INSERT RefDbVersionControl (ParentPK, ParentCode, LastUpdatedUTC, Deleted)
	SELECT ZZ6_PK, 'ZZ6', sysutcdatetime(), 0
	FROM inserted
GO
CREATE TRIGGER RefCusProcedure_Version_Update
	ON RefCusProcedure
	FOR Update
	AS
	UPDATE version SET LastUpdatedUTC = sysutcdatetime()
	FROM RefDbVersionControl version
	JOIN inserted ON ZZ6_PK = ParentPK
");
		dbCreator.ExcuteDbScript(dbName, @"
	INSERT INTO RefCusProcedure (ZZ6_PK, ZZ6_IntoWarehouse, ZZ6_OutOfWarehouse)
	VALUES
		('BABFE1D1-3C82-4616-8216-32939D96D079', 0, 0),
		('CE8916F0-4161-42BC-86F1-6483CB3205BA', 0, 1),
		('02903145-882A-4FC8-8494-A54B576E1EB9', 1, 0),
		('734AFA97-A8E0-45E5-95B3-4103711BDB12', 1, 1)
");
		RunWarehouseColumnsTransform(connection);
		AssertDataAndColumnsTransformedCorrectly(connection);
		RunWarehouseColumnsTransform(connection);
		AssertDataAndColumnsTransformedCorrectly(connection);
	}

	void RunWarehouseColumnsTransform(SqlConnection connection)
	{
		using var transaction = connection.BeginTransaction();
		var task = new RefCusProcedureChangeWarehouseColumnsDataType(1);
		task.Run(transaction);
		transaction.Commit();
	}

	void AssertDataAndColumnsTransformedCorrectly(SqlConnection connection)
	{
		using var cmd = connection.CreateCommand();
		cmd.CommandText = "SELECT COUNT(*) FROM RefCusProcedure WHERE ZZ6_PK = 'BABFE1D1-3C82-4616-8216-32939D96D079' AND ZZ6_IntoWarehouse = 'N' AND ZZ6_OutOfWarehouse = 'N'";
		Assert.AreEqual(1, Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture));
		cmd.CommandText = "SELECT COUNT(*) FROM RefCusProcedure WHERE ZZ6_PK = 'CE8916F0-4161-42BC-86F1-6483CB3205BA' AND ZZ6_IntoWarehouse = 'N' AND ZZ6_OutOfWarehouse = 'Y'";
		Assert.AreEqual(1, Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture));
		cmd.CommandText = "SELECT COUNT(*) FROM RefCusProcedure WHERE ZZ6_PK = '02903145-882A-4FC8-8494-A54B576E1EB9' AND ZZ6_IntoWarehouse = 'Y' AND ZZ6_OutOfWarehouse = 'N'";
		Assert.AreEqual(1, Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture));
		cmd.CommandText = "SELECT COUNT(*) FROM RefCusProcedure WHERE ZZ6_PK = '734AFA97-A8E0-45E5-95B3-4103711BDB12' AND ZZ6_IntoWarehouse = 'Y' AND ZZ6_OutOfWarehouse = 'Y'";
		Assert.AreEqual(1, Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture));
	}
}
