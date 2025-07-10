using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class DataFixWI00227004Transformation : DataTransformation, IDataTransformationTask
	{
		public DataFixWI00227004Transformation(int version) : base(version)
		{
		}

		public void Run(IDbTransaction trans)
		{
			var sqlText = @"
update a set a.ZZT_EndDate = '2079-06-06 23:59:00'
from dbo.refcustariff t
join dbo.refcusrate r on r.ZZ2_ZZ1_Tariff = t.ZZ1_PK
join RefCusApplicability a on a.ZZT_ZZ2_Rate = r.ZZ2_PK
where t.ZZ1_ZZZ_NKDataGrouping='za'
and t.ZZ1_TariffCode in 
(
'020910'
,'16010020'
,'19019040'
,'21050010'
,'21050020'
,'21050090'
)
and ZZ2_SelectorFormula = 'pp=''EUQUOTA'''
and r.ZZ2_EndDate <> '2079-06-06 23:59:00';

update r set r.ZZ2_EndDate = '2079-06-06 23:59:00'
from dbo.refcustariff t
join dbo.refcusrate r on r.ZZ2_ZZ1_Tariff = t.ZZ1_PK
where t.ZZ1_ZZZ_NKDataGrouping='za'
and t.ZZ1_TariffCode in 
(
'020910'
,'16010020'
,'19019040'
,'21050010'
,'21050020'
,'21050090'
)
and ZZ2_SelectorFormula = 'pp=''EUQUOTA'''
and r.ZZ2_EndDate <> '2079-06-06 23:59:00';

update a set a.ZZT_EndDate = '2018-12-31 23:59:00'
from dbo.refcustariff t
join dbo.refcusrate r on r.ZZ2_ZZ1_Tariff = t.ZZ1_PK
join RefCusApplicability a on a.ZZT_ZZ2_Rate = r.ZZ2_PK
where t.ZZ1_ZZZ_NKDataGrouping='za'
and t.ZZ1_TariffCode in
(
'020322'
,'02032990'
,'04051010'
,'040590'
)
and ZZ2_SelectorFormula = 'pp=''EUQUOTA'''
and r.ZZ2_EndDate = '2019-01-01';

update r set r.ZZ2_EndDate = '2018-12-31 23:59:00'
from dbo.refcustariff t
join dbo.refcusrate r on r.ZZ2_ZZ1_Tariff = t.ZZ1_PK
where t.ZZ1_ZZZ_NKDataGrouping='za'
and t.ZZ1_TariffCode in
(
'020322'
,'02032990'
,'04051010'
,'040590'
)
and ZZ2_SelectorFormula = 'pp=''EUQUOTA'''
and r.ZZ2_EndDate = '2019-01-01';

CREATE TABLE #ZaTariff2019(
	ZZ1_TariffCode varchar(35) not null,
	ZZ2_StartDate smalldatetime not null,
	ZZ2_EndDate smalldatetime NOT NULL,
	ZZ2_RateFormula varchar(500) not null,
	ZZ2_PK uniqueidentifier  not null
)

insert into #ZaTariff2019
select *
From (
select '020322' as ZZ1_TariffCode,'2019-01-01 00:00:00' as ZZ2_StartDate,'2019-12-31 23:59:00' as ZZ2_EndDate,'MAX(0.075 * VFD, 0.65 * [KG])' as ZZ2_RateFormula,newid() as ZZ2_PK
union all 
select '020322' as ZZ1_TariffCode,'2020-01-01 00:00:00' as ZZ2_StartDate,'2020-12-31 23:59:00' as ZZ2_EndDate,'MAX(0.05625 * VFD, 0.4875* [KG])' as ZZ2_RateFormula,newid() as ZZ2_PK
union all 
select '020322' as ZZ1_TariffCode,'2021-01-01 00:00:00' as ZZ2_StartDate,'2021-12-31 23:59:00' as ZZ2_EndDate,'MAX(0.0375 * VFD, 0. 325* [KG])' as ZZ2_RateFormula,newid() as ZZ2_PK
union all 
select '02032990' as ZZ1_TariffCode,'2019-01-01 00:00:00' as ZZ2_StartDate,'2019-12-31 23:59:00' as ZZ2_EndDate,'MAX(0.075 * VFD, 0.65 * [KG])' as ZZ2_RateFormula,newid() as ZZ2_PK
union all 
select '02032990' as ZZ1_TariffCode,'2020-01-01 00:00:00' as ZZ2_StartDate,'2020-12-31 23:59:00' as ZZ2_EndDate,'MAX(0.05625 * VFD, 0.4875* [KG])' as ZZ2_RateFormula,newid() as ZZ2_PK
union all 
select '02032990' as ZZ1_TariffCode,'2021-01-01 00:00:00' as ZZ2_StartDate,'2021-12-31 23:59:00' as ZZ2_EndDate,'MAX(0.0375 * VFD, 0. 325* [KG])' as ZZ2_RateFormula,newid() as ZZ2_PK
union all 
select '04051010' as ZZ1_TariffCode,'2019-01-01 00:00:00' as ZZ2_StartDate,'2019-12-31 23:59:00' as ZZ2_EndDate,'MIN(2.5 * [KG], 0.395 * VFD)' as ZZ2_RateFormula,newid() as ZZ2_PK
union all 
select '04051010' as ZZ1_TariffCode,'2020-01-01 00:00:00' as ZZ2_StartDate,'2020-12-31 23:59:00' as ZZ2_EndDate,'MIN(1.875 * [KG], 0.29625 * VFD)' as ZZ2_RateFormula,newid() as ZZ2_PK
union all 
select '04051010' as ZZ1_TariffCode,'2021-01-01 00:00:00' as ZZ2_StartDate,'2021-12-31 23:59:00' as ZZ2_EndDate,'MIN(1.25 * [KG], 0.1975 * VFD)' as ZZ2_RateFormula,newid() as ZZ2_PK
union all 
select '040590' as ZZ1_TariffCode,'2019-01-01 00:00:00' as ZZ2_StartDate,'2019-12-31 23:59:00' as ZZ2_EndDate,'MIN(2.5 * [KG], 0.395 * VFD)' as ZZ2_RateFormula,newid() as ZZ2_PK
union all 
select '040590' as ZZ1_TariffCode,'2020-01-01 00:00:00' as ZZ2_StartDate,'2020-12-31 23:59:00' as ZZ2_EndDate,'MIN(1.875 * [KG], 0.29625 * VFD)' as ZZ2_RateFormula,newid() as ZZ2_PK
union all 
select '040590' as ZZ1_TariffCode,'2021-01-01 00:00:00' as ZZ2_StartDate,'2021-12-31 23:59:00' as ZZ2_EndDate,'MIN(1.25 * [KG], 0.1975 * VFD)' as ZZ2_RateFormula,newid() as ZZ2_PK
) a

if (
select count(1)
from dbo.refcustariff t
join dbo.refcusrate r on r.ZZ2_ZZ1_Tariff = t.ZZ1_PK
join #ZaTariff2019 tempCheck
on tempCheck.ZZ1_TariffCode = t.ZZ1_TariffCode
and tempCheck.ZZ2_RateFormula = r.ZZ2_RateFormula
And tempCheck.ZZ2_StartDate = r.ZZ2_StartDate
And tempCheck.ZZ2_EndDate = r.ZZ2_EndDate
where t.ZZ1_ZZZ_NKDataGrouping='za'
and ZZ2_SelectorFormula = 'pp=''EUQUOTA'''
) = 0
BEGIN

INSERT INTO [dbo].[RefCusRate] (ZZ2_PK,[ZZ2_ZZ1_Tariff],[ZZ2_ZZW_TariffNationalCode],[ZZ2_StartDate],[ZZ2_EndDate],[ZZ2_ZY1_RateCode],[ZZ2_RateFormula],[ZZ2_ZZS_Preference],[ZZ2_SelectorFormula],[ZZ2_ZZZ_NKDataGrouping],[ZZ2_RateFormulaDerivedFrom],[ZZ2_RX_NKCurrencyOverride])
select temp.ZZ2_PK,t.ZZ1_PK,[ZZ2_ZZW_TariffNationalCode],temp.ZZ2_StartDate,temp.ZZ2_EndDate,[ZZ2_ZY1_RateCode],temp.ZZ2_RateFormula,[ZZ2_ZZS_Preference],[ZZ2_SelectorFormula],[ZZ2_ZZZ_NKDataGrouping],[ZZ2_RateFormulaDerivedFrom],[ZZ2_RX_NKCurrencyOverride]
from dbo.refcustariff t
join dbo.refcusrate r on r.ZZ2_ZZ1_Tariff = t.ZZ1_PK
join  #ZaTariff2019 temp on temp.ZZ1_TariffCode = t.ZZ1_TariffCode
where t.ZZ1_ZZZ_NKDataGrouping='za'
and ZZ2_SelectorFormula = 'pp=''EUQUOTA'''
and r.ZZ2_EndDate = '2018-12-31 23:59'

INSERT INTO [dbo].[RefCusApplicability] ([ZZT_ZZ2_Rate],[ZZT_ZX1_Conditions],[ZZT_StartDate],[ZZT_EndDate],[ZZT_ZZA_TradeGroup],[ZZT_AdditionalCode],[ZZT_OrderNumber])
select temp.ZZ2_PK,[ZZT_ZX1_Conditions],temp.ZZ2_StartDate,temp.ZZ2_EndDate,[ZZT_ZZA_TradeGroup],[ZZT_AdditionalCode],[ZZT_OrderNumber]
from dbo.refcustariff t
join dbo.refcusrate r on r.ZZ2_ZZ1_Tariff = t.ZZ1_PK
join dbo.refcusapplicability a on a.ZZT_ZZ2_Rate = r.ZZ2_PK
join  #ZaTariff2019 temp on temp.ZZ1_TariffCode = t.ZZ1_TariffCode
where t.ZZ1_ZZZ_NKDataGrouping='za'
and ZZ2_SelectorFormula = 'pp=''EUQUOTA'''
and r.ZZ2_EndDate = '2018-12-31 23:59'

END
drop table #ZaTariff2019
";
			using (var cmd = trans.Connection?.CreateCommand())
			{
				cmd.Transaction = trans;
				cmd.CommandText = sqlText;
				cmd.ExecuteNonQuery();
			}
		}
	}
}
