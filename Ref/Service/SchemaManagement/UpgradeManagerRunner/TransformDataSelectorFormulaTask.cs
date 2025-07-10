using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class TransformDataSelectorFormulaTask : DataTransformation, IDataTransformationTask
	{
		public TransformDataSelectorFormulaTask(int version) : base(version)
		{
		}

		public void Run(IDbTransaction trans)
		{
			var sqlText = @"
--Populate ZZ2_ZZZDataGrouping from ZZ1_ZZZ_DataGrouping
update r set r.ZZ2_ZZZ_NKDataGrouping = ZZ1_ZZZ_NKDataGrouping
FROM RefCusTariff t
join RefCusRate r on r.ZZ2_ZZ1_Tariff = t.ZZ1_PK
where t.ZZ1_ZZZ_NKDataGrouping = 'ZA'

--RefCusRateCode
INSERT INTO RefCusRateCode([ZY1_PK],[ZY1_RateCode],[ZY1_ZZR_RateType],[ZY1_Description])
Select newid(),tt.ZZI_TariffType,tt.ZZI_ZZR_RateType,tt.ZZI_Description
FROM RefCusTariffType tt
left join RefCusRateCode rc on rc.ZY1_RateCode = tt.ZZI_TariffType
where tt.ZZI_ZZZ_NKDataGrouping = 'ZA'
and rc.ZY1_PK is null 

--Update RefCusRate with RateCode
UPDATE r
	SET r.ZZ2_ZY1_RateCode = rc.ZY1_PK
From RefCusTariff t 
join RefCusRate r on r.ZZ2_ZZ1_Tariff = t.ZZ1_PK
join RefCusTariffType tt on tt.ZZI_PK = t.ZZ1_ZZI_TariffType
join RefCusRateCode rc on rc.ZY1_RateCode = tt.ZZI_TariffType
where t.ZZ1_ZZZ_NKDataGrouping = 'ZA'

--Insert RefCusApplicability
insert into RefCusApplicability ([ZZT_PK],[ZZT_ZZ2_Rate],[ZZT_StartDate],[ZZT_EndDate],[ZZT_ZZA_TradeGroup],[ZZT_AdditionalCode],[ZZT_OrderNumber])
select newid(),ZZ2_PK,ZZ2_StartDate,ZZ2_EndDate,tg.ZZA_PK,'',''
from RefCusRate r
join RefCusTradeGroup tg on tg.ZZA_ZZZ_NKDataGrouping = 'ZA' and ZZA_TradeGroup = 'EFTA'
left join RefCusApplicability a on a.ZZT_ZZ2_Rate = r.ZZ2_PK
where ZZ2_SelectorFormula = 'pp=''EFTA'''
and r.ZZ2_ZZZ_NKDataGrouping = 'ZA'
and a.ZZT_ZZ2_Rate is null
union all
select newid(),ZZ2_PK,ZZ2_StartDate,ZZ2_EndDate,tg.ZZA_PK,'',''
from RefCusRate r
join RefCusTradeGroup tg on tg.ZZA_ZZZ_NKDataGrouping = 'ZA' and ZZA_TradeGroup = 'SADC'
left join RefCusApplicability a on a.ZZT_ZZ2_Rate = r.ZZ2_PK
where ZZ2_SelectorFormula = 'pp=''SADC'''
and r.ZZ2_ZZZ_NKDataGrouping = 'ZA'
and a.ZZT_ZZ2_Rate is null
union all
select newid(),ZZ2_PK,ZZ2_StartDate,ZZ2_EndDate,tg.ZZA_PK,'',''
from RefCusRate r
join RefCusTradeGroup tg on tg.ZZA_ZZZ_NKDataGrouping = 'ZA' and ZZA_TradeGroup = 'EUTRADE'
left join RefCusApplicability a on a.ZZT_ZZ2_Rate = r.ZZ2_PK
where ZZ2_SelectorFormula = 'pp=''EUTRADE'''
and r.ZZ2_ZZZ_NKDataGrouping = 'ZA'
and a.ZZT_ZZ2_Rate is null
union all
select newid(),ZZ2_PK,ZZ2_StartDate,ZZ2_EndDate,tg.ZZA_PK,'',''
from RefCusRate r
join RefCusTradeGroup tg on tg.ZZA_ZZZ_NKDataGrouping = 'ZA' and ZZA_TradeGroup = 'STANDARD'
left join RefCusApplicability a on a.ZZT_ZZ2_Rate = r.ZZ2_PK
where ZZ2_SelectorFormula = 'pp=''STANDARD'''
and r.ZZ2_ZZZ_NKDataGrouping = 'ZA'
and a.ZZT_ZZ2_Rate is null
union all
select newid(),ZZ2_PK,ZZ2_StartDate,ZZ2_EndDate,tg.ZZA_PK,'',''
from RefCusRate r
join RefCusTradeGroup tg on tg.ZZA_ZZZ_NKDataGrouping = 'ZA' and ZZA_TradeGroup = 'EFTAQUOTA'
left join RefCusApplicability a on a.ZZT_ZZ2_Rate = r.ZZ2_PK
where ZZ2_SelectorFormula = 'pp=''EFTAQUOTA'''
and r.ZZ2_ZZZ_NKDataGrouping = 'ZA'
and a.ZZT_ZZ2_Rate is null
union all
select newid(),ZZ2_PK,ZZ2_StartDate,ZZ2_EndDate,tg.ZZA_PK,'',''
from RefCusRate r
join RefCusTradeGroup tg on tg.ZZA_ZZZ_NKDataGrouping = 'ZA' and ZZA_TradeGroup = 'EUQUOTA'
left join RefCusApplicability a on a.ZZT_ZZ2_Rate = r.ZZ2_PK
where ZZ2_SelectorFormula = 'pp=''EUQUOTA'''
and r.ZZ2_ZZZ_NKDataGrouping = 'ZA'
and a.ZZT_ZZ2_Rate is null
union all
SELECT newid(),ZZ2_PK,ZZ2_StartDate,ZZ2_EndDate,null,'',''
FROM RefCusTariff t
JOIN RefCusRate r on r.ZZ2_ZZ1_Tariff = t.ZZ1_PK
JOIN RefCusTariffType tt on tt.ZZI_PK = t.ZZ1_ZZI_TariffType
WHERE r.ZZ2_SelectorFormula = '' 
and r.ZZ2_ZZZ_NKDataGrouping = 'ZA'
AND tt.ZZI_TariffType <> '1P1'
union all
SELECT newid(),ZZ2_PK,ZZ2_StartDate,ZZ2_EndDate,tg.ZZA_PK,'',''
FROM (
SELECT ZZ2_PK,ZZ2_StartDate,ZZ2_EndDate
,REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(zz2_selectorformula,'pp=''MERCOSUR'' & (',''),')',''),'CofO=''',''),'''',''),' ','') as TradeGroup
FROM RefCusRate
WHERE ZZ2_SelectorFormula LIKE '%CofO%'
and ZZ2_ZZZ_NKDataGrouping = 'ZA'
) r
JOIN RefCusTradeGroup tg ON tg.ZZA_ZZZ_NKDataGrouping = 'ZA' AND r.TradeGroup LIKE '%' + ZZA_TradeGroup + '%'
LEFT JOIN RefCusApplicability a ON a.ZZT_ZZ2_Rate = r.ZZ2_PK
WHERE a.ZZT_ZZ2_Rate IS NULL
union all
SELECT newid(),ZZ2_PK,ZZ2_StartDate,ZZ2_EndDate,tg.ZZA_PK,'',''
FROM (
SELECT ZZ2_PK,ZZ2_StartDate,ZZ2_EndDate,'MERCOSUR' as TradeGroup
FROM RefCusRate
where ZZ2_SelectorFormula = 'pp=''MERCOSUR'''
and ZZ2_ZZZ_NKDataGrouping = 'ZA'
) r
JOIN RefCusTradeGroup tg ON tg.ZZA_ZZZ_NKDataGrouping = 'ZA' AND r.TradeGroup = ZZA_TradeGroup
LEFT JOIN RefCusApplicability a ON a.ZZT_ZZ2_Rate = r.ZZ2_PK
WHERE a.ZZT_ZZ2_Rate IS NULL

UPDATE r
	SET r.ZZ2_ZZS_Preference = p.ZZS_PK
FROM RefCusRate r
JOIN RefCusApplicability a on a.ZZT_ZZ2_Rate = r.ZZ2_PK
LEFT JOIN RefCusTradeGroup tg on tg.ZZA_PK = a.ZZT_ZZA_TradeGroup
JOIN RefCusPreference p on p.ZZS_Preference =
CASE 
	WHEN tg.ZZA_TradeGroup = 'STANDARD' THEN '100'
	WHEN tg.ZZA_TradeGroup = 'EFTA' THEN '200'
	WHEN tg.ZZA_TradeGroup = 'EUTRADE' THEN '200'
	WHEN tg.ZZA_TradeGroup = 'SADC' THEN '200'
	WHEN r.ZZ2_SelectorFormula like 'pp=''MERCOSUR%' THEN '200'
	WHEN tg.ZZA_TradeGroup = 'EFTAQUOTA' THEN '400'
	WHEN tg.ZZA_TradeGroup = 'EUQUOTA' THEN '400'
END
AND p.ZZS_ZZZ_NKDataGrouping = 'ZA'
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
