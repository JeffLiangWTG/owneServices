using System.Collections.Generic;
using System.Data;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Common.DbUpgrade.Test;

[TestFixture]
class RenameColumnsDataTransformationFixture
{
	[Test]
	[TransactionedTestCase]
	[Property("DAT:CapabilityRequirements", "SQL2019+")]
	public void Run()
	{
		var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.None);
		using var conn = new SqlConnection(TestConnectionString.GetAdmin(dbName));
		conn.Open();
		var dbCreator = new DbCreator(conn);

		dbCreator.ExcuteDbScript(dbName, GetTableScript());

		PrepareData(dbCreator, dbName);

		using (var cmd = conn.CreateCommand())
		{
			using (var dataReader = new SqlDataAdapter("SELECT * FROM DummyRenameObject", cmd.Connection))
			using (var dt = new DataTable())
			{
				var columnNamesExpected = new List<string>();
				dataReader.Fill(dt);
				foreach (var col in dt.Columns)
				{
					columnNamesExpected.Add(col.ToString());
				}
				Assert.AreEqual(columnNamesExpected[0], "DMY_PK");
				Assert.AreEqual(columnNamesExpected[1], "DMY_Old1");
				Assert.AreEqual(columnNamesExpected[2], "DMY_Old2");
				Assert.AreEqual(columnNamesExpected[3], "DMY_Old3");
			}
		}
		using (var trans = conn.BeginTransaction())
		{
			var task = new DummyRenameColumnsTransformation(1);
			task.Run(trans);
			trans.Commit();
		}
		using (var cmd = conn.CreateCommand())
		{
			using (var dataReader = new SqlDataAdapter(@"SELECT * FROM DummyRenameObject", cmd.Connection))
			using (var dt = new DataTable())
			{
				var columnNamesExpected = new List<string>();
				dataReader.Fill(dt);
				foreach (var col in dt.Columns)
				{
					columnNamesExpected.Add(col.ToString());
				}
				Assert.AreEqual(columnNamesExpected[0], "DMY_PK");
				Assert.AreEqual(columnNamesExpected[1], "DMY_New1");
				Assert.AreEqual(columnNamesExpected[2], "DMY_New2");
				Assert.AreEqual(columnNamesExpected[3], "DMY_New3");
			}
		}
		using (var trans = conn.BeginTransaction())
		{
			var task = new DummyRenameColumnsTransformation(1);
			Assert.DoesNotThrow(() => task.Run(trans), "The rename function was not able to run twice");
			trans.Commit();
		}
	}

	static void PrepareData(DbCreator dbCreator, string dbName)
	{
		dbCreator.ExcuteDbScript(dbName, @"INSERT INTO [dbo].[DummyRenameObject]([DMY_PK],[DMY_Old1],[DMY_Old2],[DMY_Old3])
VALUES ('B4EA765B-DD33-425D-BCC3-0008D00CDCDE','a1','a2','a3')");
	}

	static string GetTableScript()
	{
		return @"
CREATE TABLE [dbo].[JustToTrigger](
	[ParentPK] [varchar](10) NOT NULL,
	[ParentCode] [varchar](10) NOT NULL)
GO

CREATE TABLE [dbo].[DummyRenameObject](
	[DMY_PK] [uniqueidentifier] NOT NULL,
	[DMY_Old1] [varchar](10) NOT NULL,
	[DMY_Old2] [varchar](500) NOT NULL,
	[DMY_Old3] [varchar](3) NOT NULL,
 CONSTRAINT [PK_DummyRenameObject] PRIMARY KEY CLUSTERED 
(
	[DMY_PK] ASC
))
GO
ALTER TABLE [dbo].[DummyRenameObject]  WITH CHECK ADD  CONSTRAINT [CK_DummyRenameObject_DMY_Old1] CHECK  (([DMY_Old1]<>''));
GO
";
	}
}
