using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class RemoveDuplicatedMEURates : DataTransformation, IDataTransformationTask
	{
		public RemoveDuplicatedMEURates(int version) : base(version)
		{ }

		public void Run(IDbTransaction trans)
		{
			var sql = @"
if (SELECT top 1 count(*) FROM RefCusRate
	join RefCusTariff on ZZ2_ZZ1_Tariff = ZZ1_PK
	join RefCusTarifftype on ZZ1_ZZI_TariffType = ZZI_PK
	where ZZI_TariffType = 'MEU' and ZZI_ZZZ_NKDataGrouping = 'EUN'
	group by ZZ2_ZZ1_Tariff, ZZ2_RateFormula, ZZ2_StartDate, ZZ2_EndDate, ZZ2_ZY1_RateCode
	having count(*) > 1) IS NOT NULL
BEGIN

alter index IX_RefCusApplicability_Rate_StartDate_TradeGroup_AdditionalCode_OrderNumber_SecondTradeGroup on RefCusApplicability disable;

with dupRate as
( SELECT ZZ2_ZZ1_Tariff, ZZ2_RateFormula, ZZ2_StartDate, ZZ2_EndDate, ZZ2_ZY1_RateCode, MAX(ZZ2_PK) AS MaxRatePK FROM RefCusRate
join RefCusTariff on ZZ2_ZZ1_Tariff = ZZ1_PK
join RefCusTarifftype on ZZ1_ZZI_TariffType = ZZI_PK
where ZZI_TariffType = 'MEU' and ZZI_ZZZ_NKDataGrouping = 'EUN'
group by ZZ2_ZZ1_Tariff, ZZ2_RateFormula, ZZ2_StartDate, ZZ2_EndDate, ZZ2_ZY1_RateCode )

update app set ZZT_ZZ2_Rate = d.MaxRatePK
from RefCusApplicability app 
join RefCusRate r on ZZT_ZZ2_Rate = ZZ2_PK
join dupRate d on d.ZZ2_ZZ1_Tariff = r.ZZ2_ZZ1_Tariff and d.ZZ2_RateFormula = r.ZZ2_RateFormula
and d.ZZ2_StartDate = r.ZZ2_StartDate and d.ZZ2_EndDate = r.ZZ2_EndDate and d.ZZ2_ZY1_RateCode = r.ZZ2_ZY1_RateCode
where ZZT_ZZ2_Rate <> d.MaxRatePK;

with dupRate as
( SELECT ZZ2_ZZ1_Tariff, ZZ2_RateFormula, ZZ2_StartDate, ZZ2_EndDate, ZZ2_ZY1_RateCode, MAX(ZZ2_PK) AS MaxRatePK FROM RefCusRate
join RefCusTariff on ZZ2_ZZ1_Tariff = ZZ1_PK
join RefCusTarifftype on ZZ1_ZZI_TariffType = ZZI_PK
where ZZI_TariffType = 'MEU' and ZZI_ZZZ_NKDataGrouping = 'EUN'
group by ZZ2_ZZ1_Tariff, ZZ2_RateFormula, ZZ2_StartDate, ZZ2_EndDate, ZZ2_ZY1_RateCode )

delete r
from RefCusRate r
join dupRate on dupRate.ZZ2_ZZ1_Tariff = r.ZZ2_ZZ1_Tariff and dupRate.ZZ2_RateFormula = r.ZZ2_RateFormula
and dupRate.ZZ2_StartDate = r.ZZ2_StartDate and duprate.ZZ2_EndDate = r.ZZ2_EndDate and dupRate.ZZ2_ZY1_RateCode = r.ZZ2_ZY1_RateCode
where r.ZZ2_PK <> MaxRatePK;

with dupApp as
(
	select ZZT_ZZA_TradeGroup, ZZT_ZZ2_Rate, Max(ZZT_AdditionalCode) ZZT_AdditionalCode, MAX(ZZT_OrderNumber) ZZT_OrderNumber, 
		MAX(ZZT_StartDate) ZZT_StartDate, MAX(ZZT_EndDate) ZZT_EndDate from RefCusApplicability
	join RefCusRate on ZZ2_PK = ZZT_ZZ2_Rate
	join RefCusTariff on ZZ1_PK = ZZ2_ZZ1_Tariff
	join RefCusTariffType on ZZ1_ZZI_TariffType = ZZI_PK
	where ZZI_TariffType = 'MEU' and ZZI_ZZZ_NKDataGrouping = 'EUN'
	group by ZZT_ZZA_TradeGroup, ZZT_ZZ2_Rate
)
update app set ZZT_AdditionalCode = dupApp.ZZT_AdditionalCode, ZZT_OrderNumber = dupapp.ZZT_OrderNumber, ZZT_StartDate = dupapp.ZZT_StartDate, ZZT_EndDate = dupApp.ZZT_EndDate
from RefCusApplicability app
join dupApp on app.ZZT_ZZA_TradeGroup = dupApp.ZZT_ZZA_TradeGroup and app.ZZT_ZZ2_Rate = dupApp.ZZT_ZZ2_Rate;

with dupApp1 as
(
		select ZZT_ZZA_TradeGroup, ZZT_ZZ2_Rate, MAX(ZZT_PK) ZZT_PK from RefCusApplicability
	join RefCusRate on ZZ2_PK = ZZT_ZZ2_Rate
	join RefCusTariff on ZZ1_PK = ZZ2_ZZ1_Tariff
	join RefCusTariffType on ZZ1_ZZI_TariffType = ZZI_PK
	where ZZI_TariffType = 'MEU' and ZZI_ZZZ_NKDataGrouping = 'EUN'
	group by ZZT_ZZA_TradeGroup, ZZT_ZZ2_Rate
)

delete app 
from RefCusApplicability app
join dupApp1 on app.ZZT_ZZA_TradeGroup = dupApp1.ZZT_ZZA_TradeGroup and dupApp1.ZZT_ZZ2_Rate = app.ZZT_ZZ2_Rate
where app.ZZT_PK <> dupApp1.ZZT_PK

alter index IX_RefCusApplicability_Rate_StartDate_TradeGroup_AdditionalCode_OrderNumber_SecondTradeGroup on RefCusApplicability rebuild

END
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
