using System.IO;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Common.Utils.Test
{
	[TestFixture]
	public class SqlScriptHelperFixture
	{
		[Test]
		public void GetSqlScriptFromZippedFile()
		{
			var scripts = SqlScriptHelper.GetSqlScriptFromZippedFile(zippedSqlFilesPath, "Table", "RefCusCodeType");
			Assert.AreEqual(@"CREATE TABLE RefCusCodeType
(
	ZZK_PK UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_RefCusCodeType_ZZK_PK DEFAULT (NEWID()),
	ZZK_CodeType VARCHAR(5) NOT NULL,
	ZZK_Description VARCHAR(500) NOT NULL,
	ZZK_IsReadonly BIT NOT NULL CONSTRAINT DF_RefCusMapType_ZZK_IsReadonly DEFAULT (1),
	ZZK_MaxLength TINYINT NOT NULL CONSTRAINT DF_RefCusCodeType_ZZK_MaxLength DEFAULT (0),
	ZZK_ZZZ_NKDataGrouping VARCHAR (3) NOT NULL,
	CONSTRAINT PK_RefCusCodeType PRIMARY KEY CLUSTERED (ZZK_PK ASC),
	CONSTRAINT CK_RefCusCodeType_ZZK_CodeType CHECK (ZZK_CodeType <> ''),
	CONSTRAINT CK_RefCusCodeType_ZZK_Description CHECK (ZZK_Description <> ''),
	CONSTRAINT CK_RefCusCodeType_ZZK_MaxLength CHECK (ZZK_MaxLength >= 0 AND ZZK_MaxLength <= 35)
)
GO
CREATE UNIQUE NONCLUSTERED INDEX IX_RefCusCodeType_ZZK_ZZZ_NKDataGrouping_ZZK_CodeType ON RefCusCodeType (ZZK_ZZZ_NKDataGrouping ASC, ZZK_CodeType ASC)
GO", scripts);

			scripts = SqlScriptHelper.GetSqlScriptFromZippedFile(zippedSqlFilesPath, "Trigger", "TG_RefCusCodeList_INS_UPD");
			Assert.True(!string.IsNullOrEmpty(zippedSqlFilesPath));
			Assert.AreEqual(@"CREATE TRIGGER [dbo].[TG_RefCusCodeList_INS_UPD] ON [dbo].[RefCusCodeList]
FOR INSERT, UPDATE
AS
BEGIN
	IF UPDATE(ZZD_ZZK_NKCodeType) OR UPDATE(ZZD_ZZZ_NKDataGrouping)
	BEGIN
		IF EXISTS
		(
			SELECT NULL FROM inserted I
			LEFT JOIN RefCusCodeType C ON C.ZZK_CodeType = I.ZZD_ZZK_NKCodeType AND C.ZZK_ZZZ_NKDataGrouping = I.ZZD_ZZZ_NKDataGrouping
			WHERE C.ZZK_PK IS NULL
		)
		THROW 58012, 'A RefCusCodeList was inserted/updated with invalid values in ZZD_ZZK_NKCodeType and/or ZZD_ZZZ_NKDataGrouping which are not found in RefCusCodeType.ZZK_CodeType and RefCusCodeType.ZZK_ZZZ_NKDataGrouping', 1;
	END
END
", scripts);

			Assert.Throws<FileNotFoundException>(() => SqlScriptHelper.GetSqlScriptFromZippedFile(zippedSqlFilesPath, "Trigger", "TestTrigger"));
		}

		string zippedSqlFilesPath = "RemoteDbSqlFilesTest.zip";
	}
}
