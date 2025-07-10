using CargoWise.RefDbRepo.Common.DbUpgrade;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.Test
{
	class OffsetFromUTCToOffsetMinutesFromUTCFixture : TransformationFixture
	{
		protected override void AssertTransformationResults()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.Transaction = Transaction;
				cmd.CommandText = @"SELECT R2_OffsetMinutesFromUTC FROM RefTimeZone where R2_PK = 'F61A09F1-F310-4B6F-9633-6B42D99ADB9D'";
				Assert.AreEqual(240, cmd.ExecuteScalar());
				cmd.CommandText = @"SELECT R2_OffsetMinutesFromUTC FROM RefTimeZoneHistory where R2_CivilianTimeZoneFullName = 'Testing'";
				Assert.AreEqual(60, cmd.ExecuteScalar());
				cmd.CommandText = @"SELECT RLO_OffsetMinutesFromUTC FROM RefUNLOCOUTCOffset where RLO_PK = 'FC59590F-7A67-4080-B179-8FC92D26B854'";
				Assert.AreEqual(-360, cmd.ExecuteScalar());
				cmd.CommandText = @"SELECT RLO_OffsetMinutesFromUTC FROM RefUNLOCOUTCOffsetHistory where RLO_RL_NKCode = 'XXXX1'";
				Assert.AreEqual(-180, cmd.ExecuteScalar());
			}
		}

		protected override IDataTransformationTask GetTask()
		{
			return new OffsetFromUTCToOffsetMinutesFromUTC(0);
		}

		protected override void PrepareTestData()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = @"
IF NOT EXISTS(SELECT * FROM sys.tables t JOIN sys.all_columns c on c.object_id = t.object_id where c.name = 'R2_OffsetFromUTC')
	AND NOT EXISTS(SELECT * FROM sys.tables t JOIN sys.all_columns c on c.object_id = t.object_id where c.name = 'RLO_OffsetFromUtc')
BEGIN
	ALTER TABLE RefTimeZone DROP CONSTRAINT DF_RefTimeZone_R2_OffsetMinutesFromUTC;
	ALTER TABLE RefTimeZone DROP COLUMN R2_OffsetMinutesFromUTC;

	ALTER TABLE RefUNLOCOUTCOffset DROP CONSTRAINT DF_RefUNLOCOUTCOffset_RLO_OffsetMinutesFromUTC;
	ALTER TABLE RefUNLOCOUTCOffset DROP COLUMN RLO_OffsetMinutesFromUTC;

	ALTER TABLE RefTimeZone ADD R2_OffsetFromUTC DECIMAL (5,2);
	ALTER TABLE RefUNLOCOUTCOffset ADD RLO_OffsetFromUtc DECIMAL (5,2);
END";
				cmd.Transaction = Transaction;
				cmd.ExecuteNonQuery();
			}

			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = @"
INSERT INTO RefTimeZoneSet(R3_PK, R3_TimeZoneSetName, R3_IsActive)
VALUES('88CDDA31-8DDE-42A6-9E3A-FA072DA9B40F', 'Testing', 1);

INSERT INTO RefTimeZone (R2_PK, R2_CivilianTimeZoneFullName,R2_CivilianTimeZoneCode,R2_MilitaryTimeZoneCode,R2_OffsetFromUTC, R2_R3_TimeZoneSet)
VALUES('F61A09F1-F310-4B6F-9633-6B42D99ADB9D', 'Testing', 'CST', '', '1', '88CDDA31-8DDE-42A6-9E3A-FA072DA9B40F');

INSERT INTO RefUNLOCO (RL_PK,RL_Code, RL_RN_NKCountryCode) VALUES (NEWID(), 'XXXX1', 'AU')

INSERT INTO RefUNLOCOUtcOffset (RLO_PK, RLO_RL_NKCode, RLO_StartTimeUtc, RLO_EndTimeUtc, RLO_OffsetFromUtc)
VALUES('FC59590F-7A67-4080-B179-8FC92D26B854', 'XXXX1', '2021-05-17', '2021-05-20', -3)";
				cmd.Transaction = Transaction;
				cmd.ExecuteNonQuery();
			}

			//Create Historical data
			using (var cmd = Connection.CreateCommand())
			{
				cmd.Transaction = Transaction;
				cmd.CommandText = @"
UPDATE RefUNLOCOUtcOffset SET RLO_OffsetFromUtc = -6 WHERE RLO_PK = 'FC59590F-7A67-4080-B179-8FC92D26B854'
UPDATE RefTimeZone SET R2_OffsetFromUTC = 4 WHERE R2_PK = 'F61A09F1-F310-4B6F-9633-6B42D99ADB9D'";
				cmd.ExecuteNonQuery();
			}
		}
	}
}
