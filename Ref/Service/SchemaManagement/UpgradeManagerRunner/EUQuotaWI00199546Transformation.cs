using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class EUQuotaWI00199546Transformation : DataTransformation, IDataTransformationTask
	{
		public EUQuotaWI00199546Transformation(int version) : base(version)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:ReviewSqlQueriesForSecurityVulnerabilities")]
		public void Run(IDbTransaction trans)
		{
			var sql = @"

--get needed pk's
declare @preferencePK uniqueidentifier
select top 1 @preferencePK = ZZS_PK from RefCusPreference where ZZS_Preference = '400' and ZZS_ZZZ_NKDataGrouping = 'ZA'
declare @ratecodePK uniqueidentifier
select top 1 @ratecodePK = ZY1_PK
From RefCusRateCode rc
Join RefCusRateType rt on rt.ZZR_PK = rc.ZY1_ZZR_RateType
where ZY1_RateCode = '1P1' And ZZr_ZZZ_NkDataGrouping = 'ZA'
declare @tradegroupPK uniqueidentifier
select top 1 @tradegroupPK = ZZA_PK from RefCusTradeGroup where ZZA_TradeGroup = 'EUQUOTA' and ZZA_ZZZ_NKDataGrouping = 'ZA'

--Change Rule calculations
Update r
Set ZZ2_RateFormula = '0'
From RefCustariffRule t
Join RefcusRateRule r
On t.ZZ1_PK = r.ZZ2_ZZ1_Tariff
Where ZZ1_TariffCode in (
'04069099'
,'040610'
,'040620'
,'040640')
and r.ZZ2_SelectorFormula = 'pp=''EUQUOTA'''
and t.ZZ1_ZZZ_NKDataGrouping = 'ZA'


--expire current euquota rates
Update r
Set ZZ2_EndDate = '2017-12-31 23:59'
From RefCusTariff t
Join RefCusRate r On t.ZZ1_PK = r.ZZ2_ZZ1_Tariff
Where ZZ1_TariffCode in (
'04069099'
,'040610'
,'040620'
,'040640')
And ZZ2_SelectorFormula = 'pp=''EUQUOTA'''
and r.ZZ2_ZZZ_NKDataGrouping = 'ZA'
and (ZZ2_EndDate >= '2018-01-01' and ZZ2_StartDate < '2017-12-31 23:59')

--get zz1_pk's that need new eu quotas
select  t.ZZ1_PK,d.RateFormula
,IIF(d.StartDate > t.ZZ1_StartDate,d.StartDate,t.ZZ1_StartDate) as StartDate
,IIF(d.EndDate < t.ZZ1_EndDate,d.EndDate,t.ZZ1_EndDate) as EndDate
into #Temp
from 
(
select '2018-01-01' as [StartDate],'2079-06-06 23:59' as [EndDate],'040610' as [Tariff],'0' as [RateFormula]
union all 
select '2018-01-01' as [StartDate],'2079-06-06 23:59' as [EndDate],'040620' as [Tariff],'0' as [RateFormula]
union all 
select '2018-01-01' as [StartDate],'2079-06-06 23:59' as [EndDate],'040640' as [Tariff],'0' as [RateFormula]
union all 
select '2018-01-01' as [StartDate],'2079-06-06 23:59' as [EndDate],'04069099' as [Tariff],'0' as [RateFormula]
) d
join RefcusTariff t on t.ZZ1_ZZZ_NKDataGrouping = 'ZA' and t.ZZ1_TariffCode like d.Tariff and t.ZZ1_IAmUnique = 0 and t.ZZ1_ZZF_NKTaxOrFeeCode = 'VAT'
left join RefCusRate r on t.ZZ1_PK = r.ZZ2_ZZ1_Tariff 
and ZZ2_SelectorFormula like '%EUQuota%'
and (
(ZZ2_EndDate >= d.StartDate and ZZ1_EndDate < d.EndDate)
or
(ZZ2_StartDate >= d.StartDate and ZZ2_StartDate < d.EndDate)
)
Where r.zz2_pk is null

INSERT INTO RefCusRate
(ZZ2_ZZ1_Tariff,ZZ2_StartDate,ZZ2_EndDate,ZZ2_ZY1_RateCode,ZZ2_RateFormula,ZZ2_ZZS_Preference,ZZ2_SelectorFormula,ZZ2_ZZZ_NKDataGrouping)
select ZZ1_PK,StartDate,EndDate,@ratecodePK,RateFormula,@preferencePK,'pp=''EUQUOTA''','ZA'
from #Temp

INSERT INTO RefCusApplicability
(ZZT_ZZ2_Rate,ZZT_StartDate,ZZT_EndDate,ZZT_ZZA_TradeGroup,ZZT_AdditionalCode,ZZT_OrderNumber)
select r.ZZ2_PK,StartDate,EndDate,@tradegroupPK,'',''
From #Temp t
join RefCusRate r on r.ZZ2_ZZ1_Tariff = t.ZZ1_PK 
and r.ZZ2_RateFormula = t.RateFormula 
and r.ZZ2_StartDate = t.StartDate 
and r.ZZ2_EndDate = t.EndDate
and r.ZZ2_SelectorFormula = 'pp=''EUQUOTA'''
and r.ZZ2_ZZZ_NKDataGrouping = 'ZA'

drop table #temp
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
