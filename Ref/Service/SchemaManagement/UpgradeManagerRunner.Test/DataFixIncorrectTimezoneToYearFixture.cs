using System;
using System.Globalization;
using CargoWise.RefDbRepo.Common.DbUpgrade;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.Test
{
	class DataFixIncorrectTimezoneToYearFixture : TransformationFixture
	{
		protected override void AssertTransformationResults()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.Transaction = Transaction;
				cmd.CommandText = @"select count(*) from RefTimeZoneSet tzs join RefTimeZone tz on tz.R2_R3_TimeZoneSet = tzs.R3_PK join RefTimeZoneRule tzr on tzr.R4_R2 = tz.R2_PK
where tzs.R3_TimeZoneSetName = 'america/sao_paulo' and tzr.R4_StartOrEndRule = 'STA' and tzr.R4_ToYear = 2017 and tzr.R4_FromYear = 2008";
				Assert.AreEqual(1, Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture));
				cmd.CommandText = @"select count(*) from RefTimeZoneSet tzs join RefTimeZone tz on tz.R2_R3_TimeZoneSet = tzs.R3_PK join RefTimeZoneRule tzr on tzr.R4_R2 = tz.R2_PK
where tzs.R3_TimeZoneSetName = 'america/sao_paulo' and tzr.R4_StartOrEndRule = 'STA' and tzr.R4_ToYear = 2019 and tzr.R4_FromYear = 2018";
				Assert.AreEqual(1, Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture));
				cmd.CommandText = @"select count(*) from RefTimeZoneSet tzs join RefTimeZone tz on tz.R2_R3_TimeZoneSet = tzs.R3_PK join RefTimeZoneRule tzr on tzr.R4_R2 = tz.R2_PK
where tzs.R3_TimeZoneSetName = 'america/sao_paulo' and tzr.R4_StartOrEndRule = 'END' and tzr.R4_ToYear = 2019 and tzr.R4_FromYear = 2016";
				Assert.AreEqual(1, Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture));
				cmd.CommandText = @"select count(*) from RefTimeZoneSet tzs join RefTimeZone tz on tz.R2_R3_TimeZoneSet = tzs.R3_PK join RefTimeZoneRule tzr on tzr.R4_R2 = tz.R2_PK
where tzs.R3_TimeZoneSetName = 'Africa/Cairo' and tzr.R4_StartOrEndRule = 'STA' and tzr.R4_ToYear = 2008 and tzr.R4_FromYear = 2000";
				Assert.AreEqual(1, Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture));
				cmd.CommandText = @"select count(*) from RefTimeZoneSet tzs join RefTimeZone tz on tz.R2_R3_TimeZoneSet = tzs.R3_PK join RefTimeZoneRule tzr on tzr.R4_R2 = tz.R2_PK
where tzs.R3_TimeZoneSetName = 'Africa/Cairo' and tzr.R4_StartOrEndRule = 'STA' and tzr.R4_ToYear = 2014 and tzr.R4_FromYear = 2014";
				Assert.AreEqual(1, Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture));
				cmd.CommandText = @"select count(*) from RefTimeZoneSet tzs join RefTimeZone tz on tz.R2_R3_TimeZoneSet = tzs.R3_PK join RefTimeZoneRule tzr on tzr.R4_R2 = tz.R2_PK
where tzs.R3_TimeZoneSetName = 'Africa/Cairo' and tzr.R4_StartOrEndRule = 'END' and tzr.R4_ToYear = 2009 and tzr.R4_FromYear = 2008";
				Assert.AreEqual(1, Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture));
				cmd.CommandText = @"select count(*) from RefTimeZoneSet tzs join RefTimeZone tz on tz.R2_R3_TimeZoneSet = tzs.R3_PK join RefTimeZoneRule tzr on tzr.R4_R2 = tz.R2_PK
where tzs.R3_TimeZoneSetName = 'Africa/Cairo' and tzr.R4_StartOrEndRule = 'END' and tzr.R4_ToYear = 2010 and tzr.R4_FromYear = 2010";
				Assert.AreEqual(1, Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture));
				cmd.CommandText = @"select count(*) from RefTimeZoneSet tzs join RefTimeZone tz on tz.R2_R3_TimeZoneSet = tzs.R3_PK join RefTimeZoneRule tzr on tzr.R4_R2 = tz.R2_PK
where tzs.R3_TimeZoneSetName = 'Africa/Cairo' and tzr.R4_StartOrEndRule = 'END' and tzr.R4_ToYear = 2014 and tzr.R4_FromYear = 2014";
				Assert.AreEqual(1, Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture));
				cmd.CommandText = @"select count(*) from RefTimeZoneSet tzs join RefTimeZone tz on tz.R2_R3_TimeZoneSet = tzs.R3_PK join RefTimeZoneRule tzr on tzr.R4_R2 = tz.R2_PK
where tzs.R3_TimeZoneSetName = 'America/Recife' and tzr.R4_StartOrEndRule = 'END' and tzr.R4_ToYear = 1999 and tzr.R4_FromYear = 0";
				Assert.AreEqual(0, Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture));
				cmd.CommandText = @"select count(*) from RefTimeZoneSet tzs join RefTimeZone tz on tz.R2_R3_TimeZoneSet = tzs.R3_PK join RefTimeZoneRule tzr on tzr.R4_R2 = tz.R2_PK
where tzs.R3_TimeZoneSetName = 'America/Recife'";
				Assert.AreEqual(5, Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture));
				cmd.CommandText = @"select count(*) from RefTimeZoneSet tzs join RefTimeZone tz on tz.R2_R3_TimeZoneSet = tzs.R3_PK join RefTimeZoneRule tzr on tzr.R4_R2 = tz.R2_PK
where tzs.R3_TimeZoneSetName = 'Asia/Dhaka' and R4_ToYear = 0";
				Assert.AreEqual(0, Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture));
			}
		}

		protected override IDataTransformationTask GetTask()
		{
			return new DataFixIncorrectTimezoneToYear(61);
		}

		protected override void PrepareTestData()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = @"
INSERT INTO RefTimeZoneSet(R3_PK,R3_TimeZoneSetName,R3_IsActive)
select newid(),'america/sao_paulo',1
union all select newid(),'Africa/Cairo',1
union all select newid(),'america/santiago',1
union all select newid(),'America/Recife',1
union all select newid(),'Pacific/Fiji',1
union all select newid(),'Asia/Bishkek',1
union all select newid(),'America/Argentina/La_Rioja',1
union all select newid(),'Asia/Oral',1
union all select newid(),'Pacific/Apia',1
union all select newid(),'America/Argentina/Ushuaia',1
union all select newid(),'Asia/Aqtobe',1
union all select newid(),'Africa/Tripoli',1
union all select newid(),'America/Bahia',1
union all select newid(),'Indian/Mauritius',1
union all select newid(),'America/Tegucigalpa',1
union all select newid(),'America/Maceio',1
union all select newid(),'America/Argentina/Catamarca',1
union all select newid(),'Asia/Tbilisi',1
union all select newid(),'Africa/El_Aaiun',1
union all select newid(),'Europe/Istanbul',1
union all select newid(),'America/Argentina/Cordoba',1
union all select newid(),'America/Argentina/Buenos_Aires',1
union all select newid(),'America/Noronha',1
union all select newid(),'America/Argentina/Rio_Gallegos',1
union all select newid(),'America/Guatemala',1
union all select newid(),'America/Araguaina',1
union all select newid(),'America/Fortaleza',1
union all select newid(),'America/Argentina/Tucuman',1
union all select newid(),'Africa/Casablanca',1
union all select newid(),'America/Argentina/San_Juan',1
union all select newid(),'America/Argentina/Jujuy',1
union all select newid(),'Asia/Qyzylorda',1
union all select newid(),'Africa/Windhoek',1
union all select newid(),'Asia/Almaty',1
union all select newid(),'Asia/Aqtau',1
union all select newid(),'America/Argentina/Mendoza',1
union all select newid(),'America/Boa_Vista',1
union all select newid(),'Asia/Dhaka',1

INSERT INTO [dbo].[RefTimeZone] ([R2_PK] ,[R2_CivilianTimeZoneFullName] ,[R2_CivilianTimeZoneCode]
,[R2_MilitaryTimeZoneCode] ,[R2_OffsetMinutesFromUTC] ,[R2_R3_TimeZoneSet] ,[R2_Type])
select newid(),'','TEST1',''
,2,(select top 1 R3_PK from RefTimeZoneSet where R3_TimeZoneSetName = 'america/sao_paulo'),'DLS'
union all
select newid(),'','TEST2',''
,2,(select top 1 R3_PK from RefTimeZoneSet where R3_TimeZoneSetName = 'Africa/Cairo'),'DLS'
union all
select newid(),'','TEST3',''
,2,(select top 1 R3_PK from RefTimeZoneSet where R3_TimeZoneSetName = 'america/santiago'),'DLS'
union all select newid(),'','TEST4','',2,(select top 1 R3_PK from RefTimeZoneSet where R3_TimeZoneSetName = 'America/Recife'),'DLS'
union all select newid(),'','TEST4','',2,(select top 1 R3_PK from RefTimeZoneSet where R3_TimeZoneSetName = 'Pacific/Fiji'),'DLS'
union all select newid(),'','TEST4','',2,(select top 1 R3_PK from RefTimeZoneSet where R3_TimeZoneSetName = 'Asia/Bishkek'),'DLS'
union all select newid(),'','TEST4','',2,(select top 1 R3_PK from RefTimeZoneSet where R3_TimeZoneSetName = 'America/Argentina/La_Rioja'),'DLS'
union all select newid(),'','TEST4','',2,(select top 1 R3_PK from RefTimeZoneSet where R3_TimeZoneSetName = 'Asia/Oral'),'DLS'
union all select newid(),'','TEST4','',2,(select top 1 R3_PK from RefTimeZoneSet where R3_TimeZoneSetName = 'Pacific/Apia'),'DLS'
union all select newid(),'','TEST4','',2,(select top 1 R3_PK from RefTimeZoneSet where R3_TimeZoneSetName = 'America/Argentina/Ushuaia'),'DLS'
union all select newid(),'','TEST4','',2,(select top 1 R3_PK from RefTimeZoneSet where R3_TimeZoneSetName = 'Asia/Aqtobe'),'DLS'
union all select newid(),'','TEST4','',2,(select top 1 R3_PK from RefTimeZoneSet where R3_TimeZoneSetName = 'Africa/Tripoli'),'DLS'
union all select newid(),'','TEST4','',2,(select top 1 R3_PK from RefTimeZoneSet where R3_TimeZoneSetName = 'America/Bahia'),'DLS'
union all select newid(),'','TEST4','',2,(select top 1 R3_PK from RefTimeZoneSet where R3_TimeZoneSetName = 'Indian/Mauritius'),'DLS'
union all select newid(),'','TEST4','',2,(select top 1 R3_PK from RefTimeZoneSet where R3_TimeZoneSetName = 'America/Tegucigalpa'),'DLS'
union all select newid(),'','TEST4','',2,(select top 1 R3_PK from RefTimeZoneSet where R3_TimeZoneSetName = 'America/Maceio'),'DLS'
union all select newid(),'','TEST4','',2,(select top 1 R3_PK from RefTimeZoneSet where R3_TimeZoneSetName = 'America/Argentina/Catamarca'),'DLS'
union all select newid(),'','TEST4','',2,(select top 1 R3_PK from RefTimeZoneSet where R3_TimeZoneSetName = 'Asia/Tbilisi'),'DLS'
union all select newid(),'','TEST4','',2,(select top 1 R3_PK from RefTimeZoneSet where R3_TimeZoneSetName = 'Africa/El_Aaiun'),'DLS'
union all select newid(),'','TEST4','',2,(select top 1 R3_PK from RefTimeZoneSet where R3_TimeZoneSetName = 'Europe/Istanbul'),'DLS'
union all select newid(),'','TEST4','',2,(select top 1 R3_PK from RefTimeZoneSet where R3_TimeZoneSetName = 'America/Argentina/Cordoba'),'DLS'
union all select newid(),'','TEST4','',2,(select top 1 R3_PK from RefTimeZoneSet where R3_TimeZoneSetName = 'America/Argentina/Buenos_Aires'),'DLS'
union all select newid(),'','TEST4','',2,(select top 1 R3_PK from RefTimeZoneSet where R3_TimeZoneSetName = 'America/Noronha'),'DLS'
union all select newid(),'','TEST4','',2,(select top 1 R3_PK from RefTimeZoneSet where R3_TimeZoneSetName = 'America/Argentina/Rio_Gallegos'),'DLS'
union all select newid(),'','TEST4','',2,(select top 1 R3_PK from RefTimeZoneSet where R3_TimeZoneSetName = 'America/Guatemala'),'DLS'
union all select newid(),'','TEST4','',2,(select top 1 R3_PK from RefTimeZoneSet where R3_TimeZoneSetName = 'America/Araguaina'),'DLS'
union all select newid(),'','TEST4','',2,(select top 1 R3_PK from RefTimeZoneSet where R3_TimeZoneSetName = 'America/Fortaleza'),'DLS'
union all select newid(),'','TEST4','',2,(select top 1 R3_PK from RefTimeZoneSet where R3_TimeZoneSetName = 'America/Argentina/Tucuman'),'DLS'
union all select newid(),'','TEST4','',2,(select top 1 R3_PK from RefTimeZoneSet where R3_TimeZoneSetName = 'Africa/Casablanca'),'DLS'
union all select newid(),'','TEST4','',2,(select top 1 R3_PK from RefTimeZoneSet where R3_TimeZoneSetName = 'America/Argentina/San_Juan'),'DLS'
union all select newid(),'','TEST4','',2,(select top 1 R3_PK from RefTimeZoneSet where R3_TimeZoneSetName = 'America/Argentina/Jujuy'),'DLS'
union all select newid(),'','TEST4','',2,(select top 1 R3_PK from RefTimeZoneSet where R3_TimeZoneSetName = 'Asia/Qyzylorda'),'DLS'
union all select newid(),'','TEST4','',2,(select top 1 R3_PK from RefTimeZoneSet where R3_TimeZoneSetName = 'Africa/Windhoek'),'DLS'
union all select newid(),'','TEST4','',2,(select top 1 R3_PK from RefTimeZoneSet where R3_TimeZoneSetName = 'Asia/Almaty'),'DLS'
union all select newid(),'','TEST4','',2,(select top 1 R3_PK from RefTimeZoneSet where R3_TimeZoneSetName = 'Asia/Aqtau'),'DLS'
union all select newid(),'','TEST4','',2,(select top 1 R3_PK from RefTimeZoneSet where R3_TimeZoneSetName = 'America/Argentina/Mendoza'),'DLS'
union all select newid(),'','TEST4','',2,(select top 1 R3_PK from RefTimeZoneSet where R3_TimeZoneSetName = 'America/Boa_Vista'),'DLS'
union all select newid(),'','TEST4','',2,(select top 1 R3_PK from RefTimeZoneSet where R3_TimeZoneSetName = 'Asia/Dhaka'),'DLS'

Insert into RefTimeZoneRule
(R4_PK,R4_FromYear,R4_ToYear,R4_StartOrEndRule,R4_DaylightSavingDayWeekDate,R4_DaylightSavingDate
,R4_DaylightSavingDayCount,R4_DaylightSavingDayName,R4_DaylightSavingMonth,R4_TypeOfTime,R4_R2)
select top 1 newid(),2000,0,'STA','MON','1999-12-31 23:00:00',5,'FRI','SEP','LOC',R2_PK
from RefTimeZoneSet tzs
join RefTimeZone tz on tz.R2_R3_TimeZoneSet = tzs.R3_PK
where tzs.R3_TimeZoneSetName = 'Africa/Cairo'
union all
select top 1 newid(),2014,0,'STA','MON','1999-12-31 23:00:00',5,'FRI','SEP','LOC',R2_PK
from RefTimeZoneSet tzs
join RefTimeZone tz on tz.R2_R3_TimeZoneSet = tzs.R3_PK
where tzs.R3_TimeZoneSetName = 'Africa/Cairo'
union all
select top 1 newid(),2008,0,'END','MON','1999-12-31 23:00:00',5,'FRI','SEP','LOC',R2_PK
from RefTimeZoneSet tzs
join RefTimeZone tz on tz.R2_R3_TimeZoneSet = tzs.R3_PK
where tzs.R3_TimeZoneSetName = 'Africa/Cairo'
union all
select top 1 newid(),2010,0,'END','MON','1999-12-31 23:00:00',5,'FRI','SEP','LOC',R2_PK
from RefTimeZoneSet tzs
join RefTimeZone tz on tz.R2_R3_TimeZoneSet = tzs.R3_PK
where tzs.R3_TimeZoneSetName = 'Africa/Cairo'

union all
select top 1 newid(),0,0,'STA','MON','1999-12-31 23:00:00',5,'FRI','SEP','LOC',R2_PK
from RefTimeZoneSet tzs
join RefTimeZone tz on tz.R2_R3_TimeZoneSet = tzs.R3_PK
where tzs.R3_TimeZoneSetName = 'america/santiago'
union all
select top 1 newid(),0,0,'END','MON','1999-12-31 23:00:00',5,'FRI','SEP','LOC',R2_PK
from RefTimeZoneSet tzs
join RefTimeZone tz on tz.R2_R3_TimeZoneSet = tzs.R3_PK
where tzs.R3_TimeZoneSetName = 'america/santiago'
union all
select top 1 newid(),2016,2021,'STA','MON','1999-12-31 23:00:00',5,'FRI','SEP','LOC',R2_PK
from RefTimeZoneSet tzs
join RefTimeZone tz on tz.R2_R3_TimeZoneSet = tzs.R3_PK
where tzs.R3_TimeZoneSetName = 'america/santiago'
union all
select top 1 newid(),2017,2022,'END','MON','1999-12-31 23:00:00',5,'FRI','SEP','LOC',R2_PK
from RefTimeZoneSet tzs
join RefTimeZone tz on tz.R2_R3_TimeZoneSet = tzs.R3_PK
where tzs.R3_TimeZoneSetName = 'america/santiago'

union all
select top 1 newid(),2008,2018,'STA','MON','1999-12-31 23:00:00',5,'FRI','SEP','LOC',R2_PK
from RefTimeZoneSet tzs
join RefTimeZone tz on tz.R2_R3_TimeZoneSet = tzs.R3_PK
where tzs.R3_TimeZoneSetName = 'america/sao_paulo'
union all
select top 1 newid(),2016,2023,'END','MON','1999-12-31 23:00:00',5,'FRI','SEP','LOC',R2_PK
from RefTimeZoneSet tzs
join RefTimeZone tz on tz.R2_R3_TimeZoneSet = tzs.R3_PK
where tzs.R3_TimeZoneSetName = 'america/sao_paulo'
union all
select top 1 newid(),2018,0,'STA','MON','1999-12-31 23:00:00',5,'FRI','SEP','LOC',R2_PK
from RefTimeZoneSet tzs
join RefTimeZone tz on tz.R2_R3_TimeZoneSet = tzs.R3_PK
where tzs.R3_TimeZoneSetName = 'america/sao_paulo'

union all
select top 1 newid(),1999,0,'STA','MON','1999-12-31 23:00:00',5,'FRI','SEP','LOC',R2_PK
from RefTimeZoneSet tzs
join RefTimeZone tz on tz.R2_R3_TimeZoneSet = tzs.R3_PK
where tzs.R3_TimeZoneSetName = 'America/Recife'

union all
select top 1 newid(),2009,0,'STA','MON','1999-12-31 23:00:00',5,'FRI','SEP','LOC',R2_PK
from RefTimeZoneSet tzs
join RefTimeZone tz on tz.R2_R3_TimeZoneSet = tzs.R3_PK
where tzs.R3_TimeZoneSetName = 'Asia/Dhaka'
union all
select top 1 newid(),2010,0,'END','MON','1999-12-31 23:00:00',5,'FRI','SEP','LOC',R2_PK
from RefTimeZoneSet tzs
join RefTimeZone tz on tz.R2_R3_TimeZoneSet = tzs.R3_PK
where tzs.R3_TimeZoneSetName = 'Asia/Dhaka'
";
				cmd.Transaction = Transaction;
				cmd.ExecuteNonQuery();
			}
		}
	}
}
