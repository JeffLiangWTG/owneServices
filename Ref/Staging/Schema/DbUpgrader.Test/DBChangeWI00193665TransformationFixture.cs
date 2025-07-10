using System;
using System.Globalization;
using CargoWise.RefDbRepo.Common.DbUpgrade;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.DbUpgrader.Test
{
	[Property("DAT:CapabilityRequirements", "SQL2019+")]
	class DBChangeWI00193665TransformationFixture : TransformationFixture
	{
		protected override void AssertTransformationResults()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.Transaction = Transaction;
				cmd.CommandText = @"Select Count(1) from DataProcessingInformation Where DPI_ID = '8BA4F7BE-A1AD-480F-97A1-9DFE5F8BCC16'";
				Assert.AreEqual(Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture), 1);
			}
		}

		protected override IDataTransformationTask GetTask()
		{
			return new DBChangeWI00193665Transformation(1);
		}

		protected override void PrepareTestData()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = @"
CREATE TABLE [dbo].[RefTariffSource](
	[SRC_PK] [uniqueidentifier] NOT NULL CONSTRAINT DF_RefTariffSource_SRC_PK DEFAULT (newid()),
	[SRC_ZZ1_PK] [uniqueidentifier] NOT NULL,
	[SRC_Source] [nvarchar](255) NOT NULL,
	[SRC_SourceDate] [smalldatetime] NOT NULL CONSTRAINT DF_RefTariffSource_SRC_SourceDate DEFAULT (getdate()),
	[SRC_TransactionType] [nvarchar](20) NOT NULL,
CONSTRAINT [PK_RefTariffSource] PRIMARY KEY CLUSTERED ([SRC_PK] ASC), 
CONSTRAINT [FK_RefTariffSource_RefCusTariff] FOREIGN KEY([SRC_ZZ1_PK]) REFERENCES [dbo].[RefCusTariff] ([ZZ1_PK]) ON UPDATE CASCADE ON DELETE CASCADE
)
CREATE NONCLUSTERED INDEX [IX_RefTariffSource_SRC_ZZ1_PK] ON [RefTariffSource] ([SRC_ZZ1_PK])

INSERT INTO [dbo].[SourceData]([SDA_PK],[SDA_Source],[SDA_Filename],[SDA_Filetype],[SDA_Content],[SDA_ContentText],[SDA_Status],[SDA_CreatedTime],[SDA_ContentType],[SDA_SourceTime])
VALUES (newid(),'ZAA','test','TXT',null,'','PRS',getdate(),'PRO','2010-01-01')

INSERT INTO [dbo].[RefCusTariff] ([ZZ1_PK],[ZZ1_ZZI_NKTariffType],[ZZ1_TariffCode],[ZZ1_IAMUnique],[ZZ1_Description],[ZZ1_StartDate],[ZZ1_EndDate],[ZZ1_ZZF_NKTaxOrFeeCode],[ZZ1_ZZZ_NKDataGrouping],[ZZ1_CompositeKeyOnZZ5])
VALUES ('30A0F7E1-EB26-4004-A4C2-5F18E500C1B3','1P1','101010',0,'Description','2015-01-01 00:00:00','2016-01-01 00:00:00','VAT','ZA','');

INSERT INTO [dbo].[RefTariffSource] ([SRC_PK],[SRC_ZZ1_PK],[SRC_Source],[SRC_SourceDate],[SRC_TransactionType])
VALUES ('8BA4F7BE-A1AD-480F-97A1-9DFE5F8BCC16','30A0F7E1-EB26-4004-A4C2-5F18E500C1B3','WEB','2010-01-01','Original');
			";
				cmd.Transaction = Transaction;
				cmd.ExecuteNonQuery();
			}
		}
	}
}
