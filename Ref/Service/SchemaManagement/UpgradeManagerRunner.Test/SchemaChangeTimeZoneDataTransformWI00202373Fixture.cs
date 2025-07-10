using CargoWise.RefDbRepo.Common.DbUpgrade;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.Test
{
	class SchemaChangeTimeZoneDataTransformWI00202373Fixture : TransformationFixture
	{
		protected override void AssertTransformationResults()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.Transaction = Transaction;
				cmd.CommandText = "SELECT COUNT(*) FROM RefTimeZone Where R2_Type in ('STD','DLS') and R2_R3_TimeZoneSet is not null";
				Assert.AreEqual(2, cmd.ExecuteScalar());
				cmd.CommandText = "SELECT COUNT(*) FROM RefTimeZoneSet JOIN RefTimeZone on R2_R3_TimeZoneSet = R3_PK";
				Assert.AreEqual(2, cmd.ExecuteScalar());
			}
		}

		protected override IDataTransformationTask GetTask()
		{
			return new SchemaChangeTimeZoneDataTransformWI00202373(45);
		}

		protected override void PrepareTestData()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.Transaction = Transaction;
				cmd.CommandText = @"
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE name = 'R3_R2_StandardZone' AND object_id = OBJECT_ID('dbo.RefTimezoneSet'))
BEGIN
	Alter Table RefTimezoneSet Add [R3_R2_StandardZone] [uniqueidentifier] NOT NULL;
END
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE name = 'R3_R2_DaylightSavingZone' AND object_id = OBJECT_ID('dbo.RefTimezoneSet'))
BEGIN
	Alter Table RefTimezoneSet Add [R3_R2_DaylightSavingZone] [uniqueidentifier] NULL;
END
ALTER TABLE RefTimezoneSet NOCHECK CONSTRAINT ALL;

IF EXISTS(SELECT 1 FROM sys.views WHERE Name = 'RefTimeZoneView')
BEGIN
		Drop view RefTimeZoneView;
END

IF EXISTS (SELECT 1 FROM sys.columns WHERE name = 'R2_R3_TimezoneSet' AND object_id = OBJECT_ID('dbo.RefTimezone'))
BEGIN
	Drop INDEX IX_RefTimeZone_R2_R3_TimeZoneSet_R2_Type ON RefTimezone;
	ALTER TABLE RefTimeZone drop FK_RefTimeZoneSet_R2_R3_TimeZoneSet
	ALTER TABLE RefTimezone drop DF_RefTimeZone_R2_Type
	ALTER TABLE RefTimezone Drop COLUMN R2_R3_TimezoneSet;
	ALTER TABLE RefTimezone Drop COLUMN R2_Type;
	DISABLE TRIGGER ALL ON RefTimeZone;
END
";
				cmd.ExecuteNonQuery();

				cmd.CommandText = @"
Declare
	@TimezoneSetPK uniqueidentifier
	,@DaylightSavingsPK uniqueidentifier
	,@StandardPK uniqueidentifier
Select
	@TimezoneSetPK = newid()
	,@DaylightSavingsPK = newid()
	,@StandardPK = newid()

If (select count(*) from RefTimezoneSet Where R3_PK = @TimezoneSetPK) = 0
BEGIN
	INSERT INTO [dbo].[RefTimeZone] ([R2_PK],[R2_CivilianTimeZoneFullName],[R2_CivilianTimeZoneCode],[R2_MilitaryTimeZoneCode],[R2_OffsetMinutesFromUTC])
	VALUES (@DaylightSavingsPK,'DLS','DLS','DS',0);

	INSERT INTO [dbo].[RefTimeZone] ([R2_PK],[R2_CivilianTimeZoneFullName],[R2_CivilianTimeZoneCode],[R2_MilitaryTimeZoneCode],[R2_OffsetMinutesFromUTC])
	VALUES (@StandardPK,'STD','STD','ST',0);

	INSERT INTO [dbo].[RefTimeZoneSet] ([R3_PK],[R3_TimeZoneSetName],[R3_R2_StandardZone],[R3_R2_DaylightSavingZone],[R3_IsActive])
	VALUES (@TimezoneSetPK,'SYDNEY',@StandardPK,@DaylightSavingsPK,1);
END
";
				cmd.ExecuteNonQuery();
			}
		}
	}
}
