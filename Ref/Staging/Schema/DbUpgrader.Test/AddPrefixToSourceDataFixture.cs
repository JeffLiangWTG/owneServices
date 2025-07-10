using System.Data;
using System.Globalization;
using CargoWise.RefDbRepo.Common.DbUpgrade;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.DbUpgrader.Test
{
	[Property("DAT:CapabilityRequirements", "SQL2019+")]
	class AddPrefixToSourceDataFixture : TransformationFixture
	{
		protected override void AssertTransformationResults()
		{
			var expectedResult = new string[] { "SDA_PK", "SDA_Source", "SDA_Filename", "SDA_Filetype", "SDA_Content", "SDA_ContentText", "SDA_Status", "SDA_CreatedTime", "SDA_ContentType", "SDA_SourceTime", "SDA_SubSource", "SDA_NotProcessedUntil" };
			var expectedCount = 1;
			var expectedPK = "617757BB-F548-4DD7-89D4-13C4A9779679";

			using (var cmd = Connection.CreateCommand())
			{
				cmd.Transaction = Transaction;
				cmd.CommandText = @"SELECT * FROM SourceData";

				using (var datatable = new DataTable())
				{
					var dataReader = cmd.ExecuteReader();
					datatable.Load(dataReader);
					Assert.AreEqual(datatable.Rows.Count, expectedCount);
					Assert.AreEqual(datatable.Rows[0][0].ToString().ToUpper(CultureInfo.InvariantCulture), expectedPK);
					Assert.AreEqual(expectedResult.Length, datatable.Columns.Count);

					for (var x = 0; x < datatable.Columns.Count; x++)
					{
						Assert.AreEqual(datatable.Columns[x].ColumnName, expectedResult[x]);
					}
				}
			}
		}

		protected override IDataTransformationTask GetTask()
		{
			return new AddPrefixToSourceDataColumns(30);
		}

		protected override void PrepareTestData()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = @"
DROP TABLE [dbo].[RefErrorReport]
DROP TABLE [dbo].[SourceData]";
				cmd.Transaction = Transaction;
				cmd.ExecuteNonQuery();

				cmd.CommandText = @"
CREATE TABLE [dbo].[SourceData](
	[ID] [uniqueidentifier] NOT NULL,
	[Source] [char](3) NOT NULL,
	[Filename] [varchar](max) NULL,
	[Filetype] [varchar](3) NULL,
	[Content] [varbinary](max) NULL,
	[ContentText] [varchar](max) NOT NULL,
	[Status] [char](3) NULL,
	[CreatedTime] [datetime2](7) NOT NULL,
	[ContentType] [char](3) NULL,
	[SourceTime] [datetime2](7) NULL,
	[SubSource] [varchar](75) NOT NULL,
	[NotProcessedUntil] datetime2 NULL,
 CONSTRAINT [PK_SourceData] PRIMARY KEY NONCLUSTERED ([ID] ASC));

ALTER TABLE [dbo].[SourceData] ADD  CONSTRAINT [DF_SourceData_ID]  DEFAULT (newid()) FOR [ID];

ALTER TABLE [dbo].[SourceData] ADD  CONSTRAINT [DF_SourceData_ContentText]  DEFAULT ('') FOR [ContentText];

ALTER TABLE [dbo].[SourceData] ADD  CONSTRAINT [DF_SourceData_Status]  DEFAULT ('QUE') FOR [Status];

ALTER TABLE [dbo].[SourceData] ADD  CONSTRAINT [DF_SourceData_CreatedTime]  DEFAULT (sysutcdatetime()) FOR [CreatedTime];

ALTER TABLE [dbo].[SourceData] ADD  CONSTRAINT [DF_SourceData_SubSource]  DEFAULT ('') FOR [SubSource];

INSERT INTO SourceData (ID, Source, Filename, Filetype, Content, ContentText, Status, CreatedTime, ContentType, SourceTime, SubSource)
VALUES('617757BB-F548-4DD7-89D4-13C4A9779679', 'INT', 'C:\RefD\RefDataRepo\Bin\UxmlFiles\RefCusCodeListZZ_US.xml', 'XML',NULL,'<UniversalReferenceData><DataSource>US Customs Export Port Codes</DataSource><PublicationTime>2017-10-05T00:00:00</PublicationTime></UniversalReferenceData>', 'QUE', '2017-12-06 04:43:12.7889706', 'URD', '2017-10-05 00:00:00.0000000', 'US Customs Export Port Codes');

";
				cmd.Transaction = Transaction;
				cmd.ExecuteNonQuery();
			}
		}
	}
}
