using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class DataFixesWI00175292Transformation : DataTransformation, IDataTransformationTask
	{
		public DataFixesWI00175292Transformation(int version) : base(version)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:ReviewSqlQueriesForSecurityVulnerabilities")]
		public void Run(IDbTransaction trans)
		{
			var sql = @"
--Missing UOM
--Create rule for 16025040 
DECLARE @id uniqueidentifier
IF (Select Count(1) from RefCusTariffRule t Join RefCusTariffUOMRule u on u.ZZ8_ZZ1_Tariff = t.ZZ1_PK Where t.ZZ1_TariffCode = '16025040' And t.ZZ1_ZZZ_NKDataGrouping = 'ZA' And u.ZZ8_Type = 'RU1' And u.ZZ8_UOM = 'NO') = 0
Begin
SELECT @id = NEWID(); 
INSERT INTO RefCusTariffRule (ZZ1_PK, ZZ1_TariffCode, ZZ1_ZZZ_NKDataGrouping) VALUES (@id, '16025040', 'ZA'); 
INSERT INTO RefCusTariffUOMRule (ZZ8_ZZ1_Tariff, ZZ8_Type, ZZ8_UOM) VALUES (@id, 'RU1','NO'); 
INSERT INTO RefCusTariffAttributeRule (ZZ3_ZZ1_Tariff, ZZ3_Name, ZZ3_Value) VALUES (@id, 'CheckDigit', '9')
End
--fix rules from KG to NO
UPDATE u SET u.ZZ8_UOM = 'NO'
FROM RefCusTariffRule t
JOIN RefCusTariffUOMRule u on u.ZZ8_ZZ1_Tariff = t.ZZ1_PK
WHERE t.ZZ1_TariffCode in ('27121010'
,'27121020'
,'34021110'
,'34021120'
,'35030010')
and ZZ1_ZZZ_NKDataGrouping = 'ZA'
and u.ZZ8_UOM <> 'NO'
--Fix Data
UPDATE u SET u.ZZ8_UOM = 'NO'
FROM RefCusTariff t
JOIN RefCusTariffUOM u on u.ZZ8_ZZ1_Tariff = t.ZZ1_PK
WHERE t.ZZ1_TariffCode in ('27121010'
,'27121020'
,'34021110'
,'34021120'
,'35030010')
and ZZ1_ZZZ_NKDataGrouping = 'ZA'
and u.ZZ8_UOM <> 'NO'
and u.ZZ8_Type = 'RU1'
 
--Effective Tariff
select @ID = ZZ1_PK from RefCusTariff Where ZZ1_TariffCode = '48114190' and ZZ1_ZZZ_NKDataGrouping = 'ZA' AND ZZ1_StartDate = '2015-01-01 00:00:00'
If @ID is not null
Begin
Update RefCusTariff Set ZZ1_EndDate = '2079-06-06 23:59' Where ZZ1_PK = @ID
Update RefCusRate Set ZZ2_EndDate = '2079-06-06 23:59' Where ZZ2_ZZ1_Tariff = @ID
End
 
--Expired Tariffs
update vc set RVC_Deleted = 1, RVC_LastUpdatedUTC = sysutcdatetime()
From RefDbVersionControl vc
join RefCusTariff t 
on t.ZZ1_PK = vc.RVC_ParentPK 
and vc.RVC_ParentCode = 'ZZ1' 
Where ZZ1_TariffCode = '03055990' 
And ZZ1_ZZZ_NKDataGrouping = 'ZA' 
and ZZ1_StartDate = '2015-01-01 00:00:00'

update vc set RVC_Deleted = 1, RVC_LastUpdatedUTC = sysutcdatetime()
From RefDbVersionControl vc
join RefCusTariff t 
on t.ZZ1_PK = vc.RVC_ParentPK 
and vc.RVC_ParentCode = 'ZZ1' 
Where ZZ1_TariffCode = '22029020' 
And ZZ1_ZZZ_NKDataGrouping = 'ZA' 
and ZZ1_StartDate = '2015-01-01 00:00:00'

update vc set RVC_Deleted = 1, RVC_LastUpdatedUTC = sysutcdatetime()
From RefDbVersionControl vc
join RefCusTariff t 
on t.ZZ1_PK = vc.RVC_ParentPK 
and vc.RVC_ParentCode = 'ZZ1' 
Where ZZ1_TariffCode = '22029090' 
And ZZ1_ZZZ_NKDataGrouping = 'ZA' 
and ZZ1_StartDate = '2015-01-01 00:00:00'

update vc set RVC_Deleted = 1, RVC_LastUpdatedUTC = sysutcdatetime()
From RefDbVersionControl vc
join RefCusTariff t 
on t.ZZ1_PK = vc.RVC_ParentPK 
and vc.RVC_ParentCode = 'ZZ1' 
Where ZZ1_TariffCode = '29399910' 
And ZZ1_ZZZ_NKDataGrouping = 'ZA' 
and ZZ1_StartDate = '2015-01-01 00:00:00'

update vc set RVC_Deleted = 1, RVC_LastUpdatedUTC = sysutcdatetime()
From RefDbVersionControl vc
join RefCusTariff t 
on t.ZZ1_PK = vc.RVC_ParentPK 
and vc.RVC_ParentCode = 'ZZ1' 
Where ZZ1_TariffCode = '30044010' 
And ZZ1_ZZZ_NKDataGrouping = 'ZA' 
and ZZ1_StartDate = '2015-01-01 00:00:00'

update vc set RVC_Deleted = 1, RVC_LastUpdatedUTC = sysutcdatetime()
From RefDbVersionControl vc
join RefCusTariff t 
on t.ZZ1_PK = vc.RVC_ParentPK 
and vc.RVC_ParentCode = 'ZZ1' 
Where ZZ1_TariffCode = '44187290' 
And ZZ1_ZZZ_NKDataGrouping = 'ZA' 
and ZZ1_StartDate = '2015-01-01 00:00:00'

update vc set RVC_Deleted = 1, RVC_LastUpdatedUTC = sysutcdatetime()
From RefDbVersionControl vc
join RefCusTariff t 
on t.ZZ1_PK = vc.RVC_ParentPK 
and vc.RVC_ParentCode = 'ZZ1' 
Where ZZ1_TariffCode = '441890' 
And ZZ1_ZZZ_NKDataGrouping = 'ZA' 
and ZZ1_StartDate = '2015-01-01 00:00:00'

update vc set RVC_Deleted = 1, RVC_LastUpdatedUTC = sysutcdatetime()
From RefDbVersionControl vc
join RefCusTariff t 
on t.ZZ1_PK = vc.RVC_ParentPK 
and vc.RVC_ParentCode = 'ZZ1' 
Where ZZ1_TariffCode = '441900' 
And ZZ1_ZZZ_NKDataGrouping = 'ZA' 
and ZZ1_StartDate = '2015-01-01 00:00:00'

update vc set RVC_Deleted = 1, RVC_LastUpdatedUTC = sysutcdatetime()
From RefDbVersionControl vc
join RefCusTariff t 
on t.ZZ1_PK = vc.RVC_ParentPK 
and vc.RVC_ParentCode = 'ZZ1' 
Where ZZ1_TariffCode = '44219005' 
And ZZ1_ZZZ_NKDataGrouping = 'ZA' 
and ZZ1_StartDate = '2015-01-01 00:00:00'

update vc set RVC_Deleted = 1, RVC_LastUpdatedUTC = sysutcdatetime()
From RefDbVersionControl vc
join RefCusTariff t 
on t.ZZ1_PK = vc.RVC_ParentPK 
and vc.RVC_ParentCode = 'ZZ1' 
Where ZZ1_TariffCode = '44219090' 
And ZZ1_ZZZ_NKDataGrouping = 'ZA' 
and ZZ1_StartDate = '2015-01-01 00:00:00'

update vc set RVC_Deleted = 1, RVC_LastUpdatedUTC = sysutcdatetime()
From RefDbVersionControl vc
join RefCusTariff t 
on t.ZZ1_PK = vc.RVC_ParentPK 
and vc.RVC_ParentCode = 'ZZ1' 
Where ZZ1_TariffCode = '690710' 
And ZZ1_ZZZ_NKDataGrouping = 'ZA' 
and ZZ1_StartDate = '2015-01-01 00:00:00'

update vc set RVC_Deleted = 1, RVC_LastUpdatedUTC = sysutcdatetime()
From RefDbVersionControl vc
join RefCusTariff t 
on t.ZZ1_PK = vc.RVC_ParentPK 
and vc.RVC_ParentCode = 'ZZ1' 
Where ZZ1_TariffCode = '690790' 
And ZZ1_ZZZ_NKDataGrouping = 'ZA' 
and ZZ1_StartDate = '2015-01-01 00:00:00'

update vc set RVC_Deleted = 1, RVC_LastUpdatedUTC = sysutcdatetime()
From RefDbVersionControl vc
join RefCusTariff t 
on t.ZZ1_PK = vc.RVC_ParentPK 
and vc.RVC_ParentCode = 'ZZ1' 
Where ZZ1_TariffCode = '690810' 
And ZZ1_ZZZ_NKDataGrouping = 'ZA' 
and ZZ1_StartDate = '2015-01-01 00:00:00'

update vc set RVC_Deleted = 1, RVC_LastUpdatedUTC = sysutcdatetime()
From RefDbVersionControl vc
join RefCusTariff t 
on t.ZZ1_PK = vc.RVC_ParentPK 
and vc.RVC_ParentCode = 'ZZ1' 
Where ZZ1_TariffCode = '690890' 
And ZZ1_ZZZ_NKDataGrouping = 'ZA' 
and ZZ1_StartDate = '2015-01-01 00:00:00'

update vc set RVC_Deleted = 1, RVC_LastUpdatedUTC = sysutcdatetime()
From RefDbVersionControl vc
join RefCusTariff t 
on t.ZZ1_PK = vc.RVC_ParentPK 
and vc.RVC_ParentCode = 'ZZ1' 
Where ZZ1_TariffCode = '85285190' 
And ZZ1_ZZZ_NKDataGrouping = 'ZA' 
and ZZ1_StartDate = '2015-01-01 00:00:00'

update vc set RVC_Deleted = 1, RVC_LastUpdatedUTC = sysutcdatetime()
From RefDbVersionControl vc
join RefCusTariff t 
on t.ZZ1_PK = vc.RVC_ParentPK 
and vc.RVC_ParentCode = 'ZZ1' 
Where ZZ1_TariffCode = '940151' 
And ZZ1_ZZZ_NKDataGrouping = 'ZA' 
and ZZ1_StartDate = '2012-01-01 00:00:00'

update vc set RVC_Deleted = 1, RVC_LastUpdatedUTC = sysutcdatetime()
From RefDbVersionControl vc
join RefCusTariff t 
on t.ZZ1_PK = vc.RVC_ParentPK 
and vc.RVC_ParentCode = 'ZZ1' 
Where ZZ1_TariffCode = '940381' 
And ZZ1_ZZZ_NKDataGrouping = 'ZA' 
and ZZ1_StartDate = '2015-01-01 00:00:00'

update vc set RVC_Deleted = 1, RVC_LastUpdatedUTC = sysutcdatetime()
From RefDbVersionControl vc
join RefCusTariff t 
on t.ZZ1_PK = vc.RVC_ParentPK 
and vc.RVC_ParentCode = 'ZZ1' 
Where ZZ1_TariffCode = '94054019' 
And ZZ1_ZZZ_NKDataGrouping = 'ZA' 
and ZZ1_StartDate = '2015-01-01 00:00:00'

Select @ID = null; 
Select @ID = ZZ1_PK From RefCusTariff Where ZZ1_TariffCode = '03055990' And ZZ1_ZZZ_NKDataGrouping = 'ZA' and ZZ1_StartDate = '2010-01-01 00:00:00' 
IF @ID is not null 
Begin 
Update t Set ZZ1_EndDate = '2016-12-31 23:59:00' From RefCusTariff t Where ZZ1_PK = @ID 
Update RefCusRate Set ZZ2_EndDate = '2016-12-31 23:59:00' Where ZZ2_ZZ1_Tariff = @ID and ZZ2_EndDate = (select max(ZZ2_EndDate) From RefCusRate Where ZZ2_ZZ1_Tariff = @ID)
end
Select @ID = null; Select @ID = ZZ1_PK From RefCusTariff Where ZZ1_TariffCode = '040510' And ZZ1_ZZZ_NKDataGrouping = 'ZA' and ZZ1_StartDate = '2008-01-01 00:00:00' IF @ID is not null Begin Update t Set ZZ1_EndDate = '2016-10-09 23:59:00' From RefCusTariff t Where ZZ1_PK = @ID Update RefCusRate Set ZZ2_EndDate = '2016-10-09 23:59:00' Where ZZ2_ZZ1_Tariff = @ID and ZZ2_EndDate = (select max(ZZ2_EndDate) From RefCusRate Where ZZ2_ZZ1_Tariff = @ID) end
Select @ID = null; Select @ID = ZZ1_PK From RefCusTariff Where ZZ1_TariffCode = '121120' And ZZ1_ZZZ_NKDataGrouping = 'ZA' and ZZ1_StartDate = '2008-01-01 00:00:00' IF @ID is not null Begin Update t Set ZZ1_EndDate = '2016-12-31 23:59:00' From RefCusTariff t Where ZZ1_PK = @ID Update RefCusRate Set ZZ2_EndDate = '2016-12-31 23:59:00' Where ZZ2_ZZ1_Tariff = @ID and ZZ2_EndDate = (select max(ZZ2_EndDate) From RefCusRate Where ZZ2_ZZ1_Tariff = @ID) end
Select @ID = null; Select @ID = ZZ1_PK From RefCusTariff Where ZZ1_TariffCode = '121130' And ZZ1_ZZZ_NKDataGrouping = 'ZA' and ZZ1_StartDate = '2008-01-01 00:00:00' IF @ID is not null Begin Update t Set ZZ1_EndDate = '2016-12-31 23:59:00' From RefCusTariff t Where ZZ1_PK = @ID Update RefCusRate Set ZZ2_EndDate = '2016-12-31 23:59:00' Where ZZ2_ZZ1_Tariff = @ID and ZZ2_EndDate = (select max(ZZ2_EndDate) From RefCusRate Where ZZ2_ZZ1_Tariff = @ID) end
Select @ID = null; Select @ID = ZZ1_PK From RefCusTariff Where ZZ1_TariffCode = '121140' And ZZ1_ZZZ_NKDataGrouping = 'ZA' and ZZ1_StartDate = '2008-01-01 00:00:00' IF @ID is not null Begin Update t Set ZZ1_EndDate = '2016-12-31 23:59:00' From RefCusTariff t Where ZZ1_PK = @ID Update RefCusRate Set ZZ2_EndDate = '2016-12-31 23:59:00' Where ZZ2_ZZ1_Tariff = @ID and ZZ2_EndDate = (select max(ZZ2_EndDate) From RefCusRate Where ZZ2_ZZ1_Tariff = @ID) end
Select @ID = null; Select @ID = ZZ1_PK From RefCusTariff Where ZZ1_TariffCode = '150710' And ZZ1_ZZZ_NKDataGrouping = 'ZA' and ZZ1_StartDate = '2008-01-01 00:00:00' IF @ID is not null Begin Update t Set ZZ1_EndDate = '2016-12-08 23:59:00' From RefCusTariff t Where ZZ1_PK = @ID Update RefCusRate Set ZZ2_EndDate = '2016-12-08 23:59:00' Where ZZ2_ZZ1_Tariff = @ID and ZZ2_EndDate = (select max(ZZ2_EndDate) From RefCusRate Where ZZ2_ZZ1_Tariff = @ID) end
Select @ID = null; Select @ID = ZZ1_PK From RefCusTariff Where ZZ1_TariffCode = '150790' And ZZ1_ZZZ_NKDataGrouping = 'ZA' and ZZ1_StartDate = '2008-01-01 00:00:00' IF @ID is not null Begin Update t Set ZZ1_EndDate = '2016-12-08 23:59:00' From RefCusTariff t Where ZZ1_PK = @ID Update RefCusRate Set ZZ2_EndDate = '2016-12-08 23:59:00' Where ZZ2_ZZ1_Tariff = @ID and ZZ2_EndDate = (select max(ZZ2_EndDate) From RefCusRate Where ZZ2_ZZ1_Tariff = @ID) end
Select @ID = null; Select @ID = ZZ1_PK From RefCusTariff Where ZZ1_TariffCode = '150810' And ZZ1_ZZZ_NKDataGrouping = 'ZA' and ZZ1_StartDate = '2008-01-01 00:00:00' IF @ID is not null Begin Update t Set ZZ1_EndDate = '2016-12-08 23:59:00' From RefCusTariff t Where ZZ1_PK = @ID Update RefCusRate Set ZZ2_EndDate = '2016-12-08 23:59:00' Where ZZ2_ZZ1_Tariff = @ID and ZZ2_EndDate = (select max(ZZ2_EndDate) From RefCusRate Where ZZ2_ZZ1_Tariff = @ID) end
Select @ID = null; Select @ID = ZZ1_PK From RefCusTariff Where ZZ1_TariffCode = '151110' And ZZ1_ZZZ_NKDataGrouping = 'ZA' and ZZ1_StartDate = '2008-01-01 00:00:00' IF @ID is not null Begin Update t Set ZZ1_EndDate = '2016-12-08 23:59:00' From RefCusTariff t Where ZZ1_PK = @ID Update RefCusRate Set ZZ2_EndDate = '2016-12-08 23:59:00' Where ZZ2_ZZ1_Tariff = @ID and ZZ2_EndDate = (select max(ZZ2_EndDate) From RefCusRate Where ZZ2_ZZ1_Tariff = @ID) end
Select @ID = null; Select @ID = ZZ1_PK From RefCusTariff Where ZZ1_TariffCode = '151190' And ZZ1_ZZZ_NKDataGrouping = 'ZA' and ZZ1_StartDate = '2008-01-01 00:00:00' IF @ID is not null Begin Update t Set ZZ1_EndDate = '2016-12-08 23:59:00' From RefCusTariff t Where ZZ1_PK = @ID Update RefCusRate Set ZZ2_EndDate = '2016-12-08 23:59:00' Where ZZ2_ZZ1_Tariff = @ID and ZZ2_EndDate = (select max(ZZ2_EndDate) From RefCusRate Where ZZ2_ZZ1_Tariff = @ID) end
Select @ID = null; Select @ID = ZZ1_PK From RefCusTariff Where ZZ1_TariffCode = '151211' And ZZ1_ZZZ_NKDataGrouping = 'ZA' and ZZ1_StartDate = '2008-01-01 00:00:00' IF @ID is not null Begin Update t Set ZZ1_EndDate = '2016-12-08 23:59:00' From RefCusTariff t Where ZZ1_PK = @ID Update RefCusRate Set ZZ2_EndDate = '2016-12-08 23:59:00' Where ZZ2_ZZ1_Tariff = @ID and ZZ2_EndDate = (select max(ZZ2_EndDate) From RefCusRate Where ZZ2_ZZ1_Tariff = @ID) end
Select @ID = null; Select @ID = ZZ1_PK From RefCusTariff Where ZZ1_TariffCode = '151221' And ZZ1_ZZZ_NKDataGrouping = 'ZA' and ZZ1_StartDate = '2008-01-01 00:00:00' IF @ID is not null Begin Update t Set ZZ1_EndDate = '2016-12-08 23:59:00' From RefCusTariff t Where ZZ1_PK = @ID Update RefCusRate Set ZZ2_EndDate = '2016-12-08 23:59:00' Where ZZ2_ZZ1_Tariff = @ID and ZZ2_EndDate = (select max(ZZ2_EndDate) From RefCusRate Where ZZ2_ZZ1_Tariff = @ID) end
Select @ID = null; Select @ID = ZZ1_PK From RefCusTariff Where ZZ1_TariffCode = '151311' And ZZ1_ZZZ_NKDataGrouping = 'ZA' and ZZ1_StartDate = '2008-01-01 00:00:00' IF @ID is not null Begin Update t Set ZZ1_EndDate = '2016-12-08 23:59:00' From RefCusTariff t Where ZZ1_PK = @ID Update RefCusRate Set ZZ2_EndDate = '2016-12-08 23:59:00' Where ZZ2_ZZ1_Tariff = @ID and ZZ2_EndDate = (select max(ZZ2_EndDate) From RefCusRate Where ZZ2_ZZ1_Tariff = @ID) end
Select @ID = null; Select @ID = ZZ1_PK From RefCusTariff Where ZZ1_TariffCode = '151319' And ZZ1_ZZZ_NKDataGrouping = 'ZA' and ZZ1_StartDate = '2008-01-01 00:00:00' IF @ID is not null Begin Update t Set ZZ1_EndDate = '2016-12-08 23:59:00' From RefCusTariff t Where ZZ1_PK = @ID Update RefCusRate Set ZZ2_EndDate = '2016-12-08 23:59:00' Where ZZ2_ZZ1_Tariff = @ID and ZZ2_EndDate = (select max(ZZ2_EndDate) From RefCusRate Where ZZ2_ZZ1_Tariff = @ID) end
Select @ID = null; Select @ID = ZZ1_PK From RefCusTariff Where ZZ1_TariffCode = '151321' And ZZ1_ZZZ_NKDataGrouping = 'ZA' and ZZ1_StartDate = '2008-01-01 00:00:00' IF @ID is not null Begin Update t Set ZZ1_EndDate = '2016-12-08 23:59:00' From RefCusTariff t Where ZZ1_PK = @ID Update RefCusRate Set ZZ2_EndDate = '2016-12-08 23:59:00' Where ZZ2_ZZ1_Tariff = @ID and ZZ2_EndDate = (select max(ZZ2_EndDate) From RefCusRate Where ZZ2_ZZ1_Tariff = @ID) end
Select @ID = null; Select @ID = ZZ1_PK From RefCusTariff Where ZZ1_TariffCode = '151329' And ZZ1_ZZZ_NKDataGrouping = 'ZA' and ZZ1_StartDate = '2008-01-01 00:00:00' IF @ID is not null Begin Update t Set ZZ1_EndDate = '2016-12-08 23:59:00' From RefCusTariff t Where ZZ1_PK = @ID Update RefCusRate Set ZZ2_EndDate = '2016-12-08 23:59:00' Where ZZ2_ZZ1_Tariff = @ID and ZZ2_EndDate = (select max(ZZ2_EndDate) From RefCusRate Where ZZ2_ZZ1_Tariff = @ID) end
Select @ID = null; Select @ID = ZZ1_PK From RefCusTariff Where ZZ1_TariffCode = '151411' And ZZ1_ZZZ_NKDataGrouping = 'ZA' and ZZ1_StartDate = '2008-01-01 00:00:00' IF @ID is not null Begin Update t Set ZZ1_EndDate = '2016-12-08 23:59:00' From RefCusTariff t Where ZZ1_PK = @ID Update RefCusRate Set ZZ2_EndDate = '2016-12-08 23:59:00' Where ZZ2_ZZ1_Tariff = @ID and ZZ2_EndDate = (select max(ZZ2_EndDate) From RefCusRate Where ZZ2_ZZ1_Tariff = @ID) end
Select @ID = null; Select @ID = ZZ1_PK From RefCusTariff Where ZZ1_TariffCode = '151491' And ZZ1_ZZZ_NKDataGrouping = 'ZA' and ZZ1_StartDate = '2008-01-01 00:00:00' IF @ID is not null Begin Update t Set ZZ1_EndDate = '2016-12-08 23:59:00' From RefCusTariff t Where ZZ1_PK = @ID Update RefCusRate Set ZZ2_EndDate = '2016-12-08 23:59:00' Where ZZ2_ZZ1_Tariff = @ID and ZZ2_EndDate = (select max(ZZ2_EndDate) From RefCusRate Where ZZ2_ZZ1_Tariff = @ID) end
Select @ID = null; Select @ID = ZZ1_PK From RefCusTariff Where ZZ1_TariffCode = '151511' And ZZ1_ZZZ_NKDataGrouping = 'ZA' and ZZ1_StartDate = '2008-01-01 00:00:00' IF @ID is not null Begin Update t Set ZZ1_EndDate = '2016-12-08 23:59:00' From RefCusTariff t Where ZZ1_PK = @ID Update RefCusRate Set ZZ2_EndDate = '2016-12-08 23:59:00' Where ZZ2_ZZ1_Tariff = @ID and ZZ2_EndDate = (select max(ZZ2_EndDate) From RefCusRate Where ZZ2_ZZ1_Tariff = @ID) end
Select @ID = null; Select @ID = ZZ1_PK From RefCusTariff Where ZZ1_TariffCode = '151519' And ZZ1_ZZZ_NKDataGrouping = 'ZA' and ZZ1_StartDate = '2008-01-01 00:00:00' IF @ID is not null Begin Update t Set ZZ1_EndDate = '2016-12-08 23:59:00' From RefCusTariff t Where ZZ1_PK = @ID Update RefCusRate Set ZZ2_EndDate = '2016-12-08 23:59:00' Where ZZ2_ZZ1_Tariff = @ID and ZZ2_EndDate = (select max(ZZ2_EndDate) From RefCusRate Where ZZ2_ZZ1_Tariff = @ID) end
Select @ID = null; Select @ID = ZZ1_PK From RefCusTariff Where ZZ1_TariffCode = '151521' And ZZ1_ZZZ_NKDataGrouping = 'ZA' and ZZ1_StartDate = '2008-01-01 00:00:00' IF @ID is not null Begin Update t Set ZZ1_EndDate = '2016-12-08 23:59:00' From RefCusTariff t Where ZZ1_PK = @ID Update RefCusRate Set ZZ2_EndDate = '2016-12-08 23:59:00' Where ZZ2_ZZ1_Tariff = @ID and ZZ2_EndDate = (select max(ZZ2_EndDate) From RefCusRate Where ZZ2_ZZ1_Tariff = @ID) end
Select @ID = null; Select @ID = ZZ1_PK From RefCusTariff Where ZZ1_TariffCode = '22029020' And ZZ1_ZZZ_NKDataGrouping = 'ZA' and ZZ1_StartDate = '2010-01-01 00:00:00' IF @ID is not null Begin Update t Set ZZ1_EndDate = '2016-12-31 23:59:00' From RefCusTariff t Where ZZ1_PK = @ID Update RefCusRate Set ZZ2_EndDate = '2016-12-31 23:59:00' Where ZZ2_ZZ1_Tariff = @ID and ZZ2_EndDate = (select max(ZZ2_EndDate) From RefCusRate Where ZZ2_ZZ1_Tariff = @ID) end
Select @ID = null; Select @ID = ZZ1_PK From RefCusTariff Where ZZ1_TariffCode = '22029090' And ZZ1_ZZZ_NKDataGrouping = 'ZA' and ZZ1_StartDate = '2010-01-01 00:00:00' IF @ID is not null Begin Update t Set ZZ1_EndDate = '2016-12-31 23:59:00' From RefCusTariff t Where ZZ1_PK = @ID Update RefCusRate Set ZZ2_EndDate = '2016-12-31 23:59:00' Where ZZ2_ZZ1_Tariff = @ID and ZZ2_EndDate = (select max(ZZ2_EndDate) From RefCusRate Where ZZ2_ZZ1_Tariff = @ID) end
Select @ID = null; Select @ID = ZZ1_PK From RefCusTariff Where ZZ1_TariffCode = '29399910' And ZZ1_ZZZ_NKDataGrouping = 'ZA' and ZZ1_StartDate = '2010-01-01 00:00:00' IF @ID is not null Begin Update t Set ZZ1_EndDate = '2016-12-31 23:59:00' From RefCusTariff t Where ZZ1_PK = @ID Update RefCusRate Set ZZ2_EndDate = '2016-12-31 23:59:00' Where ZZ2_ZZ1_Tariff = @ID and ZZ2_EndDate = (select max(ZZ2_EndDate) From RefCusRate Where ZZ2_ZZ1_Tariff = @ID) end
Select @ID = null; Select @ID = ZZ1_PK From RefCusTariff Where ZZ1_TariffCode = '30044010' And ZZ1_ZZZ_NKDataGrouping = 'ZA' and ZZ1_StartDate = '2010-01-01 00:00:00' IF @ID is not null Begin Update t Set ZZ1_EndDate = '2016-12-31 23:59:00' From RefCusTariff t Where ZZ1_PK = @ID Update RefCusRate Set ZZ2_EndDate = '2016-12-31 23:59:00' Where ZZ2_ZZ1_Tariff = @ID and ZZ2_EndDate = (select max(ZZ2_EndDate) From RefCusRate Where ZZ2_ZZ1_Tariff = @ID) end
Select @ID = null; Select @ID = ZZ1_PK From RefCusTariff Where ZZ1_TariffCode = '300510' And ZZ1_ZZZ_NKDataGrouping = 'ZA' and ZZ1_StartDate = '2008-01-01 00:00:00' IF @ID is not null Begin Update t Set ZZ1_EndDate = '2016-12-08 23:59:00' From RefCusTariff t Where ZZ1_PK = @ID Update RefCusRate Set ZZ2_EndDate = '2016-12-08 23:59:00' Where ZZ2_ZZ1_Tariff = @ID and ZZ2_EndDate = (select max(ZZ2_EndDate) From RefCusRate Where ZZ2_ZZ1_Tariff = @ID) end
Select @ID = null; Select @ID = ZZ1_PK From RefCusTariff Where ZZ1_TariffCode = '360300' And ZZ1_ZZZ_NKDataGrouping = 'ZA' and ZZ1_StartDate = '2008-01-01 00:00:00' IF @ID is not null Begin Update t Set ZZ1_EndDate = '2016-12-31 23:59:00' From RefCusTariff t Where ZZ1_PK = @ID Update RefCusRate Set ZZ2_EndDate = '2016-12-31 23:59:00' Where ZZ2_ZZ1_Tariff = @ID and ZZ2_EndDate = (select max(ZZ2_EndDate) From RefCusRate Where ZZ2_ZZ1_Tariff = @ID) end
Select @ID = null; Select @ID = ZZ1_PK From RefCusTariff Where ZZ1_TariffCode = '390761' And ZZ1_ZZZ_NKDataGrouping = 'ZA' and ZZ1_StartDate = '2017-01-01 00:00:00' IF @ID is not null Begin Update t Set ZZ1_EndDate = '2017-03-16 23:59:00' From RefCusTariff t Where ZZ1_PK = @ID Update RefCusRate Set ZZ2_EndDate = '2017-03-16 23:59:00' Where ZZ2_ZZ1_Tariff = @ID and ZZ2_EndDate = (select max(ZZ2_EndDate) From RefCusRate Where ZZ2_ZZ1_Tariff = @ID) end
Select @ID = null; Select @ID = ZZ1_PK From RefCusTariff Where ZZ1_TariffCode = '44187290' And ZZ1_ZZZ_NKDataGrouping = 'ZA' and ZZ1_StartDate = '2010-01-01 00:00:00' IF @ID is not null Begin Update t Set ZZ1_EndDate = '2016-12-31 23:59:00' From RefCusTariff t Where ZZ1_PK = @ID Update RefCusRate Set ZZ2_EndDate = '2016-12-31 23:59:00' Where ZZ2_ZZ1_Tariff = @ID and ZZ2_EndDate = (select max(ZZ2_EndDate) From RefCusRate Where ZZ2_ZZ1_Tariff = @ID) end
Select @ID = null; Select @ID = ZZ1_PK From RefCusTariff Where ZZ1_TariffCode = '441890' And ZZ1_ZZZ_NKDataGrouping = 'ZA' and ZZ1_StartDate = '2010-01-01 00:00:00' IF @ID is not null Begin Update t Set ZZ1_EndDate = '2016-12-31 23:59:00' From RefCusTariff t Where ZZ1_PK = @ID Update RefCusRate Set ZZ2_EndDate = '2016-12-31 23:59:00' Where ZZ2_ZZ1_Tariff = @ID and ZZ2_EndDate = (select max(ZZ2_EndDate) From RefCusRate Where ZZ2_ZZ1_Tariff = @ID) end
Select @ID = null; Select @ID = ZZ1_PK From RefCusTariff Where ZZ1_TariffCode = '441900' And ZZ1_ZZZ_NKDataGrouping = 'ZA' and ZZ1_StartDate = '2010-01-01 00:00:00' IF @ID is not null Begin Update t Set ZZ1_EndDate = '2016-12-31 23:59:00' From RefCusTariff t Where ZZ1_PK = @ID Update RefCusRate Set ZZ2_EndDate = '2016-12-31 23:59:00' Where ZZ2_ZZ1_Tariff = @ID and ZZ2_EndDate = (select max(ZZ2_EndDate) From RefCusRate Where ZZ2_ZZ1_Tariff = @ID) end
Select @ID = null; Select @ID = ZZ1_PK From RefCusTariff Where ZZ1_TariffCode = '44219005' And ZZ1_ZZZ_NKDataGrouping = 'ZA' and ZZ1_StartDate = '2010-01-01 00:00:00' IF @ID is not null Begin Update t Set ZZ1_EndDate = '2016-12-31 23:59:00' From RefCusTariff t Where ZZ1_PK = @ID Update RefCusRate Set ZZ2_EndDate = '2016-12-31 23:59:00' Where ZZ2_ZZ1_Tariff = @ID and ZZ2_EndDate = (select max(ZZ2_EndDate) From RefCusRate Where ZZ2_ZZ1_Tariff = @ID) end
Select @ID = null; Select @ID = ZZ1_PK From RefCusTariff Where ZZ1_TariffCode = '44219090' And ZZ1_ZZZ_NKDataGrouping = 'ZA' and ZZ1_StartDate = '2010-01-01 00:00:00' IF @ID is not null Begin Update t Set ZZ1_EndDate = '2016-12-31 23:59:00' From RefCusTariff t Where ZZ1_PK = @ID Update RefCusRate Set ZZ2_EndDate = '2016-12-31 23:59:00' Where ZZ2_ZZ1_Tariff = @ID and ZZ2_EndDate = (select max(ZZ2_EndDate) From RefCusRate Where ZZ2_ZZ1_Tariff = @ID) end
Select @ID = null; Select @ID = ZZ1_PK From RefCusTariff Where ZZ1_TariffCode = '690710' And ZZ1_ZZZ_NKDataGrouping = 'ZA' and ZZ1_StartDate = '2010-01-01 00:00:00' IF @ID is not null Begin Update t Set ZZ1_EndDate = '2016-12-31 23:59:00' From RefCusTariff t Where ZZ1_PK = @ID Update RefCusRate Set ZZ2_EndDate = '2016-12-31 23:59:00' Where ZZ2_ZZ1_Tariff = @ID and ZZ2_EndDate = (select max(ZZ2_EndDate) From RefCusRate Where ZZ2_ZZ1_Tariff = @ID) end
Select @ID = null; Select @ID = ZZ1_PK From RefCusTariff Where ZZ1_TariffCode = '690790' And ZZ1_ZZZ_NKDataGrouping = 'ZA' and ZZ1_StartDate = '2010-01-01 00:00:00' IF @ID is not null Begin Update t Set ZZ1_EndDate = '2016-12-31 23:59:00' From RefCusTariff t Where ZZ1_PK = @ID Update RefCusRate Set ZZ2_EndDate = '2016-12-31 23:59:00' Where ZZ2_ZZ1_Tariff = @ID and ZZ2_EndDate = (select max(ZZ2_EndDate) From RefCusRate Where ZZ2_ZZ1_Tariff = @ID) end
Select @ID = null; Select @ID = ZZ1_PK From RefCusTariff Where ZZ1_TariffCode = '690810' And ZZ1_ZZZ_NKDataGrouping = 'ZA' and ZZ1_StartDate = '2010-01-01 00:00:00' IF @ID is not null Begin Update t Set ZZ1_EndDate = '2016-12-31 23:59:00' From RefCusTariff t Where ZZ1_PK = @ID Update RefCusRate Set ZZ2_EndDate = '2016-12-31 23:59:00' Where ZZ2_ZZ1_Tariff = @ID and ZZ2_EndDate = (select max(ZZ2_EndDate) From RefCusRate Where ZZ2_ZZ1_Tariff = @ID) end
Select @ID = null; Select @ID = ZZ1_PK From RefCusTariff Where ZZ1_TariffCode = '690890' And ZZ1_ZZZ_NKDataGrouping = 'ZA' and ZZ1_StartDate = '2010-01-01 00:00:00' IF @ID is not null Begin Update t Set ZZ1_EndDate = '2016-12-31 23:59:00' From RefCusTariff t Where ZZ1_PK = @ID Update RefCusRate Set ZZ2_EndDate = '2016-12-31 23:59:00' Where ZZ2_ZZ1_Tariff = @ID and ZZ2_EndDate = (select max(ZZ2_EndDate) From RefCusRate Where ZZ2_ZZ1_Tariff = @ID) end
Select @ID = null; Select @ID = ZZ1_PK From RefCusTariff Where ZZ1_TariffCode = '85285190' And ZZ1_ZZZ_NKDataGrouping = 'ZA' and ZZ1_StartDate = '2011-04-01 00:00:00' IF @ID is not null Begin Update t Set ZZ1_EndDate = '2016-12-31 23:59:00' From RefCusTariff t Where ZZ1_PK = @ID Update RefCusRate Set ZZ2_EndDate = '2016-12-31 23:59:00' Where ZZ2_ZZ1_Tariff = @ID and ZZ2_EndDate = (select max(ZZ2_EndDate) From RefCusRate Where ZZ2_ZZ1_Tariff = @ID) end
Select @ID = null; Select @ID = ZZ1_PK From RefCusTariff Where ZZ1_TariffCode = '940151' And ZZ1_ZZZ_NKDataGrouping = 'ZA' and ZZ1_StartDate = '2010-01-01 00:00:00' IF @ID is not null Begin Update t Set ZZ1_EndDate = '2016-12-31 23:59:00' From RefCusTariff t Where ZZ1_PK = @ID Update RefCusRate Set ZZ2_EndDate = '2016-12-31 23:59:00' Where ZZ2_ZZ1_Tariff = @ID and ZZ2_EndDate = (select max(ZZ2_EndDate) From RefCusRate Where ZZ2_ZZ1_Tariff = @ID) end
Select @ID = null; Select @ID = ZZ1_PK From RefCusTariff Where ZZ1_TariffCode = '940381' And ZZ1_ZZZ_NKDataGrouping = 'ZA' and ZZ1_StartDate = '2010-01-01 00:00:00' IF @ID is not null Begin Update t Set ZZ1_EndDate = '2016-12-31 23:59:00' From RefCusTariff t Where ZZ1_PK = @ID Update RefCusRate Set ZZ2_EndDate = '2016-12-31 23:59:00' Where ZZ2_ZZ1_Tariff = @ID and ZZ2_EndDate = (select max(ZZ2_EndDate) From RefCusRate Where ZZ2_ZZ1_Tariff = @ID) end
Select @ID = null; Select @ID = ZZ1_PK From RefCusTariff Where ZZ1_TariffCode = '94054019' And ZZ1_ZZZ_NKDataGrouping = 'ZA' and ZZ1_StartDate = '2013-01-01 00:00:00' IF @ID is not null Begin Update t Set ZZ1_EndDate = '2016-12-31 23:59:00' From RefCusTariff t Where ZZ1_PK = @ID Update RefCusRate Set ZZ2_EndDate = '2016-12-31 23:59:00' Where ZZ2_ZZ1_Tariff = @ID and ZZ2_EndDate = (select max(ZZ2_EndDate) From RefCusRate Where ZZ2_ZZ1_Tariff = @ID) end

--Incorrect rate of duty
delete r
FROM RefCusTariff t
JOIN RefCusRate r on r.zz2_zz1_tariff = t.ZZ1_PK
JOIN RefDbVersionControl vc on vc.RVC_parentPK = t.ZZ1_PK
WHERE t.zz1_tariffcode= '1041605'
and ZZ2_RateFormula = '6.71 * [LI]'
and ZZ2_StartDate = '2017-02-22 00:00:00'

UPDATE r SET r.ZZ2_EndDate = '2079-06-06 23:59'
from refcustariff t
join refcusrate r on r.zz2_zz1_tariff = t.ZZ1_PK
where t.zz1_tariffcode= '1041605'
and ZZ2_RateFormula = '6.17 * [LI]'
and ZZ2_StartDate = '2017-02-22 00:00:00'
";

			using (var cmd = trans.Connection?.CreateCommand())
			{
				cmd.Connection = trans.Connection;
				cmd.Transaction = trans;
				cmd.CommandText = sql;
				cmd.ExecuteNonQuery();
			}
		}
	}
}
