using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.DbUpgrade;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.Test
{
	class RefTimeZoneRuleUniqueIndexValidationFixture : TransformationFixture
	{
		protected override void AssertTransformationResults()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.Transaction = Transaction;
				cmd.CommandText = @"SELECT COUNT(*) FROM (select R4_R2, R4_StartOrEndRule, R4_FromYear, R4_DaylightSavingMonth from RefTimeZoneRule
GROUP BY R4_R2, R4_StartOrEndRule, R4_FromYear, R4_DaylightSavingMonth
HAVING COUNT(*) > 1) tbl";
				Assert.AreEqual(0, cmd.ExecuteScalar());
			}
		}

		protected override IDataTransformationTask GetTask()
		{
			return new RefTimeZoneRuleUniqueIndexValidation(0);
		}

		protected override void PrepareTestData()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = @"
IF (SELECT COUNT(*) FROM sys.indexes where name = 'FK_UX__R4_R2_R4_FromYear_R4_StartOrEndRule_R4_DaylightSavingMonth') > 0
BEGIN
	DROP INDEX RefTimeZoneRule.FK_UX__R4_R2_R4_FromYear_R4_StartOrEndRule_R4_DaylightSavingMonth
END

INSERT INTO RefTimeZoneSet(R3_PK, R3_TimeZoneSetName, R3_IsActive)
VALUES('88CDDA31-8DDE-42A6-9E3A-FA072DA9B40F', 'Testing', 1);

INSERT INTO RefTimeZone (R2_PK, R2_CivilianTimeZoneFullName,R2_CivilianTimeZoneCode,R2_MilitaryTimeZoneCode,R2_OffsetMinutesFromUTC, R2_R3_TimeZoneSet)
VALUES('F61A09F1-F310-4B6F-9633-6B42D99ADB9D', '', 'CST', '', '-12', '88CDDA31-8DDE-42A6-9E3A-FA072DA9B40F');

INSERT INTO RefTimeZoneRule (R4_PK, R4_FromYear,R4_ToYear,R4_StartOrEndRule,R4_DaylightSavingDayWeekDate,R4_DaylightSavingDate,R4_DaylightSavingDayCount,R4_DaylightSavingDayName,R4_DaylightSavingMonth,R4_TypeOfTime,R4_R2)
VALUES (newid(), '2019', '0', 'STA', 'MON', '2019-01-01 00:00:00', '1', 'SUN', 'SEP', 'LOC', 'F61A09F1-F310-4B6F-9633-6B42D99ADB9D'),
(newid(), '2019', '0', 'STA', 'MON', '2019-01-01 00:00:00', '1', 'SUN', 'SEP', 'LOC', 'F61A09F1-F310-4B6F-9633-6B42D99ADB9D'),
(newid(), '2019', '0', 'STA', 'MON', '2019-01-01 00:00:00', '1', 'SUN', 'SEP', 'LOC', 'F61A09F1-F310-4B6F-9633-6B42D99ADB9D');";
				cmd.Transaction = Transaction;
				cmd.ExecuteNonQuery();
			}
		}
	}
}
