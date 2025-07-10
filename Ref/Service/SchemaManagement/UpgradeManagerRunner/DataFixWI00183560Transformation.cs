using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class DataFixWI00183560Transformation : DataTransformation, IDataTransformationTask
	{
		public DataFixWI00183560Transformation(int version) : base(version)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:ReviewSqlQueriesForSecurityVulnerabilities")]
		public void Run(IDbTransaction trans)
		{
			var sql = @"
IF ((select COUNT(*)
	FROM RefCusTariff t 
	LEFT JOIN refcusrate r ON r.zz2_zz1_tariff =t.zz1_pk 
	LEFT JOIN RefCusApplicability ap ON ap.zzt_zz2_rate = r.zz2_pk
	LEFT JOIN RefCusTariffRelationship re ON re.zzh_zz1_tariff = t.zz1_pk
	WHERE ZZ1_tariffcode = '213030208' AND re.ZZH_TariffCode = '70052925'
	AND ZZ1_ZZZ_NKDataGrouping = 'ZA' AND ZZ2_EndDate='2079-06-06 23:59:00'
	AND  ZZ2_StartDate = '2017-11-17 00:00:00'
	GROUP BY ZZ2_ZZ1_Tariff,ZZ2_StartDate,ZZ2_EndDate,ZZ2_ZY1_RateCode,ZZ2_RateFormula,ZZ2_SelectorFormula) > 1)
BEGIN

	DECLARE @ZZ2_PK uniqueidentifier

	select @ZZ2_PK = MAX(ZZ2_PK)
	FROM RefCusTariff t 
	LEFT JOIN refcusrate r ON r.zz2_zz1_tariff =t.zz1_pk 
	LEFT JOIN RefCusApplicability ap ON ap.zzt_zz2_rate = r.zz2_pk
	LEFT JOIN RefCusTariffRelationship re ON re.zzh_zz1_tariff = t.zz1_pk
	WHERE ZZ1_tariffcode = '213030208' AND re.ZZH_TariffCode = '70052925'
	AND ZZ1_ZZZ_NKDataGrouping = 'ZA' AND ZZ2_EndDate='2079-06-06 23:59:00'
	AND  ZZ2_StartDate = '2017-11-17 00:00:00'
	GROUP BY ZZ2_ZZ1_Tariff,ZZ2_StartDate,ZZ2_EndDate,ZZ2_ZY1_RateCode,ZZ2_RateFormula,ZZ2_SelectorFormula

	DELETE ap
	FROM RefCusTariff t 
	LEFT JOIN refcusrate r ON r.zz2_zz1_tariff =t.zz1_pk 
	LEFT JOIN RefCusApplicability ap ON ap.zzt_zz2_rate = r.zz2_pk
	LEFT JOIN RefCusTariffRelationship re ON re.zzh_zz1_tariff = t.zz1_pk
	WHERE ZZ1_tariffcode = '213030208' AND re.ZZH_TariffCode = '70052925'
	AND ZZ1_ZZZ_NKDataGrouping = 'ZA' AND ZZ2_EndDate='2079-06-06 23:59:00'
	AND  ZZ2_StartDate = '2017-11-17 00:00:00'
	AND ZZ2_PK <> @ZZ2_PK

	DELETE r
	FROM RefCusTariff t 
	LEFT JOIN refcusrate r ON r.zz2_zz1_tariff =t.zz1_pk 
	LEFT JOIN RefCusApplicability ap ON ap.zzt_zz2_rate = r.zz2_pk
	LEFT JOIN RefCusTariffRelationship re ON re.zzh_zz1_tariff = t.zz1_pk
	WHERE ZZ1_tariffcode = '213030208' AND re.ZZH_TariffCode = '70052925'
	AND ZZ1_ZZZ_NKDataGrouping = 'ZA' AND ZZ2_EndDate='2079-06-06 23:59:00'
	AND  ZZ2_StartDate = '2017-11-17 00:00:00'
	AND ZZ2_PK <> @ZZ2_PK

END

--populate ratecodes for the 5 tariffs missing ratecodes
UPDATE r
	SET r.ZZ2_ZY1_RateCode = rc.ZY1_PK
From RefCusTariff t 
join RefCusRate r on r.ZZ2_ZZ1_Tariff = t.ZZ1_PK
join RefCusTariffType tt on tt.ZZI_PK = t.ZZ1_ZZI_TariffType
join RefCusRateCode rc on rc.ZY1_RateCode = tt.ZZI_TariffType
where t.ZZ1_ZZZ_NKDataGrouping = 'ZA'
and r.ZZ2_ZY1_RateCode is null

--delete rates from refcusrate where preference is null and duplicate rate exists (85286910 and 85286990)
--delete all where duplicates exists for 20041091 and 20041099 tariffs
create table #RatesToDelete (ZZ2_PK_ToDelete uniqueidentifier)

insert into #RatesToDelete
select max(ZZ2_PK)
FROM refcustariff t
join refcusrate r on r.ZZ2_ZZ1_Tariff = t.ZZ1_PK
WHERE t.ZZ1_ZZZ_NKDataGrouping = 'ZA'
AND ((ZZ1_TariffCode = '85286990' AND ZZ1_StartDate = '2015-04-01 00:00:00' AND ZZ1_EndDate = '2079-06-06 23:59:00')
OR (ZZ1_TariffCode = '85286910' AND ZZ1_StartDate = '2015-04-01 00:00:00' AND ZZ1_EndDate = '2079-06-06 23:59:00'))
group by t.zz1_tariffcode,r.ZZ2_RateFormula,r.ZZ2_SelectorFormula,r.ZZ2_StartDate,r.ZZ2_EndDate
having count(*) > 1

insert into #RatesToDelete
select max(r.zz2_pk) as RateToDelete
FROM refcustariff t
join refcusrate r on r.ZZ2_ZZ1_Tariff = t.ZZ1_PK
WHERE t.ZZ1_ZZZ_NKDataGrouping = 'ZA'
AND ((ZZ1_TariffCode = '20041099' AND ZZ1_StartDate = '2016-05-27 00:00:00' AND ZZ1_EndDate = '2079-06-06 23:59:00')
OR (ZZ1_TariffCode = '20041091' AND ZZ1_StartDate = '2016-05-27 00:00:00' AND ZZ1_EndDate = '2079-06-06 23:59:00'))
group by t.zz1_tariffcode,r.ZZ2_RateFormula,r.ZZ2_SelectorFormula,r.ZZ2_StartDate,r.ZZ2_EndDate
having count(*) > 1

Delete from RefCusApplicability Where ZZT_ZZ2_Rate in (Select ZZ2_PK_ToDelete From #RatesToDelete) 
Delete from RefCusRate Where ZZ2_PK in (Select ZZ2_PK_ToDelete From #RatesToDelete) 

drop table #RatesToDelete

--delete all where duplicates exists for 311120304 and relationship is 5407
declare @RateToKeep uniqueidentifier
select @RateToKeep = max(zz2_pk) 
From RefCusRate r
join RefCusTariff t on ZZ2_ZZ1_Tariff = ZZ1_PK
join RefCusTariffRelationship on ZZH_ZZ1_Tariff = ZZ1_PK
where ZZ2_ZZZ_NKDataGrouping = 'ZA'
and (zz1_tariffcode = '311120304' and zzh_tariffcode = '5407' and ZZ2_StartDate = '1997-08-01 00:00:00' and ZZ2_EndDate = '2017-12-28 23:59:00')
group by t.zz1_pk,t.zz1_tariffcode,r.ZZ2_RateFormula,r.ZZ2_SelectorFormula,r.ZZ2_StartDate,r.ZZ2_EndDate,ZZH_TariffCode
having count(*) <> 1

If (@RateToKeep is not null)
Begin
	delete a
	From RefCusRate r
	join RefCusApplicability a on a.ZZT_ZZ2_Rate = r.ZZ2_PK
	join RefCusTariff t on ZZ2_ZZ1_Tariff = ZZ1_PK
	join RefCusTariffRelationship on ZZH_ZZ1_Tariff = ZZ1_PK
	where ZZ2_ZZZ_NKDataGrouping = 'ZA'
	and (zz1_tariffcode = '311120304' and zzh_tariffcode = '5407' and ZZ2_StartDate = '1997-08-01 00:00:00' and ZZ2_EndDate = '2017-12-28 23:59:00')
	and ZZ2_PK <> @RateToKeep

	delete r 
	From RefCusRate r
	join RefCusTariff t on ZZ2_ZZ1_Tariff = ZZ1_PK
	join RefCusTariffRelationship on ZZH_ZZ1_Tariff = ZZ1_PK
	where ZZ2_ZZZ_NKDataGrouping = 'ZA'
	and (zz1_tariffcode = '311120304' and zzh_tariffcode = '5407' and ZZ2_StartDate = '1997-08-01 00:00:00' and ZZ2_EndDate = '2017-12-28 23:59:00')
	and ZZ2_PK <> @RateToKeep
End

--tariff 213030208 and relationship is 70052925
--delete applicability records for tariff 213030208 and relationship is 70052925
delete a
From RefCusRate r
join refcusapplicability a on a.zzt_zz2_rate = r.zz2_pk
join RefCusTariff t on ZZ2_ZZ1_Tariff = ZZ1_PK
join RefCusTariffRelationship rh on ZZH_ZZ1_Tariff = ZZ1_PK
where ZZ2_ZZZ_NKDataGrouping = 'ZA'
and zz1_tariffcode = '213030208' 
and zzh_tariffcode = '70052925' 
and zz2_StartDate = '2017-11-17 00:00:00'
and zz2_EndDate = '2079-06-06 23:59:00'

--delete rates for tariff 213030208 and relationship is 70052925
delete r
From RefCusRate r
join RefCusTariff t on ZZ2_ZZ1_Tariff = ZZ1_PK
join RefCusTariffRelationship rh on ZZH_ZZ1_Tariff = ZZ1_PK
where ZZ2_ZZZ_NKDataGrouping = 'ZA'
and zz1_tariffcode = '213030208' 
and zzh_tariffcode = '70052925' 
and zz2_StartDate = '2017-11-17 00:00:00'
and zz2_EndDate = '2079-06-06 23:59:00'

--update rates for tariff 213030208 and relationship is 70052925 with correct enddate
update r set r.zz2_enddate = '2079-06-06 23:59:00'
From RefCusRate r
join RefCusTariff t on ZZ2_ZZ1_Tariff = ZZ1_PK
join RefCusTariffRelationship rh on ZZH_ZZ1_Tariff = ZZ1_PK
where ZZ2_ZZZ_NKDataGrouping = 'ZA'
and zz1_tariffcode = '213030208' 
and zzh_tariffcode = '70052925' 
and zz2_StartDate = '2015-01-01 00:00:00'
and zz2_EndDate = '2017-11-16 23:59:00'

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
