using System.Data;
using System.Globalization;
using CargoWise.RefDbRepo.Common.DbUpgrade;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.DbUpgrader.Test
{
	[Property("DAT:CapabilityRequirements", "SQL2019+")]
	class AddPrefixToProcessorStatusFixture : TransformationFixture
	{
		protected override void AssertTransformationResults()
		{
			var expectedResult = new string[] { "PRC_PK", "PRC_Processor", "PRC_Country", "PRC_LastRunTime", "PRC_LastSuccessRunTime", "PRC_LastSuccessRecordUpdatedCount", "PRC_LastDataSetUpdatedTime" };
			var expectedCount = 1;
			var expectedPK = "617757BB-F548-4DD7-89D4-13C4A9779679";

			using (var cmd = Connection.CreateCommand())
			{
				cmd.Transaction = Transaction;
				cmd.CommandText = @"SELECT * FROM ProcessorStatus";
				using (var datatable = new DataTable())
				{
					var dataReader = cmd.ExecuteReader();
					datatable.Load(dataReader);
					Assert.AreEqual(datatable.Rows.Count, expectedCount);
					Assert.AreEqual(datatable.Rows[0][0].ToString()?.ToUpper(CultureInfo.InvariantCulture), expectedPK);
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
			return new AddPrefixToProcessorStatusColumns(30);
		}

		protected override void PrepareTestData()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = "DROP TABLE [dbo].[ProcessorStatus]";
				cmd.Transaction = Transaction;
				cmd.ExecuteNonQuery();

				cmd.CommandText = @"
CREATE TABLE [dbo].[ProcessorStatus](
	[Id] [uniqueidentifier] NOT NULL,
	[Processor] [varchar](200) NOT NULL,
	[Country] [char](2) NOT NULL,
	[LastRunTime] [datetime2](7) NOT NULL,
	[LastSuccessRunTime] [datetime2](7) NULL,
	[LastSuccessRecordUpdatedCount] [int] NULL,
	[LastDataSetUpdatedTime] [datetime2](7) NULL,
 CONSTRAINT [PK_ProcessorStatus] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
));

ALTER TABLE [dbo].[ProcessorStatus] ADD  CONSTRAINT [DF_ProcessorStatus]  DEFAULT (newid()) FOR [Id];
ALTER TABLE [dbo].[ProcessorStatus]  WITH CHECK ADD  CONSTRAINT [CK_ProcessorStatus_Country] CHECK  (([Country]<>''));

ALTER TABLE [dbo].[ProcessorStatus] CHECK CONSTRAINT [CK_ProcessorStatus_Country];

ALTER TABLE [dbo].[ProcessorStatus]  WITH CHECK ADD  CONSTRAINT [CK_ProcessorStatus_Processor] CHECK  (([Processor]<>''));

ALTER TABLE [dbo].[ProcessorStatus] CHECK CONSTRAINT [CK_ProcessorStatus_Processor];

INSERT INTO ProcessorStatus(Id, Processor, Country, LastRunTime, LastSuccessRunTime, LastSuccessRecordUpdatedCount, LastDataSetUpdatedTime)
VALUES('617757BB-F548-4DD7-89D4-13C4A9779679', 'Staging test processor', 'ZA', SYSUTCDATETIME(), SYSUTCDATETIME(), 1000, SYSUTCDATETIME())
";
				cmd.ExecuteNonQuery();
			}
		}
	}
}
