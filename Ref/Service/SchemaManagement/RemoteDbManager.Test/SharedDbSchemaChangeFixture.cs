using NUnit.Framework;

namespace CargoWise.RefDbRepo.RemoteDbManager.Test
{
	[TestFixture]
	class SharedDbSchemaChangeFixture
	{
		[Test]
		public void TestGetAddColumnIfNotExistsScript()
		{
			var result = SharedDbSchemaChange.GetAddColumnIfNotExistsScript("TT", "CC", "INT NOT NULL");
			Assert.AreEqual(@"
IF NOT EXISTS(
	SELECT * FROM sys.tables tab 
	INNER JOIN sys.columns col ON tab.object_id = col.object_id
	WHERE tab.name = 'TT' AND col.name = 'CC'
)
BEGIN
	ALTER TABLE TT ADD CC INT NOT NULL
END", result);
		}

		[Test]
		public void TestGetAddCheckConstraintIfNotExistsScript()
		{
			var result = SharedDbSchemaChange.GetAddCheckConstraintIfNotExistsScript("TT", "CC", "([CC]<>'')");
			Assert.AreEqual(@"
IF NOT EXISTS(
	SELECT NULL FROM sys.tables tab 
	INNER JOIN sys.check_constraints ckc ON tab.object_id = ckc.parent_object_id
	WHERE tab.name = 'TT' AND ckc.name = 'CC'
)
BEGIN
	ALTER TABLE TT ADD CONSTRAINT CC CHECK (([CC]<>''))
END", result);
		}

		[Test]
		public void TestGetAddPrimaryKeyIfNotExistsScript()
		{
			var result = SharedDbSchemaChange.GetAddPrimaryKeyIfNotExistsScript("TT", "PK_TT", "TT_PK");
			Assert.AreEqual(@"
IF NOT EXISTS(
	SELECT NULL FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS con WHERE con.CONSTRAINT_TYPE = 'PRIMARY KEY'
	AND con.TABLE_NAME = 'TT'
)
BEGIN
	ALTER TABLE TT ADD CONSTRAINT PK_TT PRIMARY KEY CLUSTERED (TT_PK);
END", result);
		}

		[Test]
		public void TestGetAddForeignKeyIfNotExistsScript()
		{
			var result = SharedDbSchemaChange.GetAddForeignKeyIfNotExistsScript("TT", "FK_TT_FF", "F1_T1", "TT(TT_PK)");
			Assert.AreEqual(@"
IF NOT EXISTS(
	SELECT NULL FROM sys.tables tab 
	INNER JOIN sys.foreign_keys fk ON tab.object_id = fk.parent_object_id
	WHERE tab.name = 'TT' AND fk.name = 'FK_TT_FF'
)
BEGIN
	ALTER TABLE TT WITH CHECK ADD CONSTRAINT FK_TT_FF FOREIGN KEY (F1_T1) REFERENCES TT(TT_PK)
END", result);
		}

		[Test]
		public void TestGetAddDefaultConstranintIfNotExistsScript()
		{
			var result = SharedDbSchemaChange.GetAddDefaultConstranintIfNotExistsScript("TT", "DF_TT_T1", "'Default'", "T1");
			Assert.AreEqual(@"
IF NOT EXISTS(
	SELECT NULL FROM sys.tables tab 
	INNER JOIN sys.default_constraints ckc ON tab.object_id = ckc.parent_object_id
	WHERE tab.name = 'TT' AND ckc.name = 'DF_TT_T1'
)
BEGIN
	ALTER TABLE TT ADD CONSTRAINT DF_TT_T1 DEFAULT ('Default') FOR T1
END", result);
		}

		[Test]
		public void TestGetCreateIndexIfNotExistsScript()
		{
			var result = SharedDbSchemaChange.GetCreateIndexIfNotExistsScript("TT", "IX_TT_T1", "CREATE UNIQUE NONCLUSTERED INDEX IX_TT_T1 ON TT(T1 ASC)");
			Assert.AreEqual(@"
IF NOT EXISTS(
	SELECT null
	FROM sys.indexes ind
	WHERE ind.name = 'IX_TT_T1'
	AND (
		EXISTS
		(
			SELECT NULL
			FROM sys.tables tab
			WHERE tab.object_id = ind.object_id AND tab.name = 'TT'
		) OR
		EXISTS
		(
			SELECT NULL
			FROM sys.views vie
			WHERE vie.object_id = ind.object_id AND vie.name = 'TT'
		)
	)
)
BEGIN
	CREATE UNIQUE NONCLUSTERED INDEX IX_TT_T1 ON TT(T1 ASC)
END", result);
		}

		[Test]
		public void TestGetDropIndexIfExistsScript()
		{
			var result = SharedDbSchemaChange.GetDropIndexIfExistsScript("TT", "IX_TT_T1");
			Assert.AreEqual(@"
IF EXISTS(
	SELECT null
	FROM sys.indexes ind
	WHERE ind.name = 'IX_TT_T1'
	AND (
		EXISTS
		(
			SELECT NULL
			FROM sys.tables tab
			WHERE tab.object_id = ind.object_id AND tab.name = 'TT'
		) OR
		EXISTS
		(
			SELECT NULL
			FROM sys.views vie
			WHERE vie.object_id = ind.object_id AND vie.name = 'TT'
		)
	)
)
BEGIN
	DROP INDEX TT.IX_TT_T1
END", result);
		}

		[Test]
		public void TestGetDropTriggerIfExistsScript()
		{
			var result = SharedDbSchemaChange.GetDropTriggerIfExistsScript("TT", "TG_TT_INS_UPD");
			Assert.AreEqual(@"
IF EXISTS(SELECT null FROM sys.tables tab 
				INNER JOIN sys.triggers tg ON tab.object_id = tg.parent_id
				WHERE tab.name = 'TT' AND tg.name = 'TG_TT_INS_UPD')
BEGIN
	 DROP TRIGGER TG_TT_INS_UPD
END", result);
		}

		[Test]
		public void TestGetDropStoredProcedureIfExistsScript()
		{
			var result = SharedDbSchemaChange.GetDropStoredProcedureIfExistsScript("TT_TestStoreProcedure");
			Assert.AreEqual(@"
if (OBJECT_ID('TT_TestStoreProcedure', 'P') is NOT NULL)
begin
	DROP PROCEDURE TT_TestStoreProcedure
end", result);
		}

		[Test]
		public void TestGetCreateTableIfNotExistsScript()
		{
			var createTableScript = @"CREATE TABLE [TT](
[TT_PK] [uniqueidentifier] NOT NULL DEFAULT (NEWID()),
[T1] [nvarchar](50) NOT NULL,
CONSTRAINT PK_TT PRIMARY KEY CLUSTERED( TT_PK ASC )
)";
			var result = SharedDbSchemaChange.GetCreateTableIfNotExistsScript("TT", createTableScript);
			Assert.AreEqual($@"
IF NOT EXISTS(SELECT null FROM sys.tables tab WHERE tab.name = 'TT')
BEGIN
	{createTableScript}
END", result);
		}

		[Test]
		public void TestGetDropConstraintIfExistsScript()
		{
			var result = SharedDbSchemaChange.GetDropConstraintIfExistsScript("check_constraints", "TT", "CK_TT_T1");
			Assert.AreEqual(@"
IF EXISTS(
	SELECT NULL FROM sys.tables tab 
	INNER JOIN sys.check_constraints ckc ON tab.object_id = ckc.parent_object_id
	WHERE tab.name = 'TT' AND ckc.name = 'CK_TT_T1'
)
BEGIN
	ALTER TABLE TT DROP CONSTRAINT CK_TT_T1;
END;", result);
		}

		[Test]
		public void TestGetDropColumnIfExistsScript()
		{
			var result = SharedDbSchemaChange.GetDropColumnIfExistsScript("TT", "T1");
			Assert.AreEqual(@"
IF EXISTS(
	SELECT * FROM sys.tables tab 
	INNER JOIN sys.columns col ON tab.object_id = col.object_id
	WHERE tab.name = 'TT' AND col.name = 'T1'
)
BEGIN
	ALTER TABLE TT DROP COLUMN T1;
END;", result);
		}

		[Test]
		public void TestGetDropConstraintIfExistsFromColumnNameScript()
		{
			var result = SharedDbSchemaChange.GetDropConstraintIfExistsFromColumnNameScript("TT", "T1");
			Assert.AreEqual(@"
DECLARE @dfname_T1 NVARCHAR(MAX),
        @dropsql_T1 NVARCHAR(MAX)
SELECT @dfname_T1=name FROM sys.objects WHERE object_id = (SELECT sys.columns.default_object_id FROM sys.objects INNER JOIN sys.columns ON objects.object_id = sys.columns.object_id 
    WHERE sys.columns.name = 'T1' AND sys.objects.name = 'TT')
    IF LEN(@dfname_T1)>0
    SET @dropsql_T1='ALTER TABLE TT DROP CONSTRAINT '+ CAST(@dfname_T1 AS NVARCHAR(50))
    IF LEN(@dfname_T1)>0
    EXEC sp_executesql @dropsql_T1", result);
		}

		[Test]
		public void TestGetRenameColumnIfExistsScript()
		{
			var result = SharedDbSchemaChange.GetRenameColumnIfExistsScript("TT", "T1Old", "T1New");
			Assert.AreEqual(@"
IF EXISTS(
	SELECT * FROM sys.tables tab 
	INNER JOIN sys.columns col ON tab.object_id = col.object_id
	WHERE tab.name = 'TT' AND col.name = 'T1Old'
)
BEGIN
	EXEC sp_rename 'TT.T1Old', 'T1New', 'COLUMN'
END;", result);
		}

		[Test]
		public void TestGetDropSqlObjectIfExistsScript()
		{
			var result = SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "TestView", "VIEW");
			Assert.AreEqual(@"
IF EXISTS(
	SELECT * FROM sys.objects o
	WHERE o.type = 'V' AND o.name = 'TestView'
)
BEGIN
DROP VIEW TestView
END", result);
		}

		[Test]
		public void TestGetCreateSqlObjectIfNotExistsScript()
		{
			var query = @"CREATE VIEW RefCarrierVesselPivot_TableView_V1 AS
SELECT [ZZQ_PK],
[ZZQ_ZZ4],
[ZZQ_ZZO]
FROM RefCarrierVesselPivot";
			var result = SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(query, "V", "RefCarrierVesselPivot_TableView_V1");
			Assert.AreEqual($@"
IF NOT EXISTS(
	SELECT * FROM sys.objects o
	WHERE o.type = 'V' AND o.name = 'RefCarrierVesselPivot_TableView_V1'
)
EXEC dbo.sp_executesql @statement = N'{query}'", result);
		}

		[Test]
		public void TestGetDropCheckConstranintIfExistsScript()
		{
			var result = SharedDbSchemaChange.GetDropCheckConstranintIfExistsScript("TT", "CK_TT_TT_T1");
			Assert.AreEqual(@"
IF EXISTS(
	SELECT NULL FROM sys.tables tab 
	INNER JOIN sys.check_constraints ckc ON tab.object_id = ckc.parent_object_id
	WHERE tab.name = 'TT' AND ckc.name = 'CK_TT_TT_T1'
)
BEGIN
	ALTER TABLE TT DROP CONSTRAINT CK_TT_TT_T1
END", result);
		}

		[Test]
		public void GetSqlScriptFromZippedFileUsingSubfolderAndObjectname()
		{
			var result = SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname("TableView", "RefAccessorialTableView_V1");
			Assert.AreEqual(@"CREATE VIEW RefAccessorialTableView_V1 AS
SELECT ASI_PK,
ASI_Code,
ASI_Description
FROM RefAccessorial
", result);
		}

		[Test]
		public void GetTableViewSqlScript()
		{
			var result = SharedDbSchemaChange.GetTableViewSqlScript("RefDataGrouping", 1);
			Assert.AreEqual(@"CREATE VIEW RefDataGroupingTableView_V1 AS
SELECT ZZZ_PK,
ZZZ_DataGrouping,
ZZZ_Description,
ZZZ_ZZZ_Grouping
FROM RefDataGrouping", result);
		}
	}
}
