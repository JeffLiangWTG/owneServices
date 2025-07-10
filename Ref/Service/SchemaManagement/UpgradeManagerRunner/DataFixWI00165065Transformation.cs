using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class DataFixWI00165065Transformation : DataTransformation, IDataTransformationTask
	{
		public DataFixWI00165065Transformation(int version) : base(version)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:ReviewSqlQueriesForSecurityVulnerabilities")]
		public void Run(IDbTransaction trans)
		{
			var sql = @"
--fix 0 rate formulas
update r
	set ZZ2_RateFormula = '0'
from dbo.RefCusRate r
where ZZ2_ZZZ_NKDataGrouping = 'IT'
and ZZ2_RateFormula like '0 * [[]%]'

--fix rate code on partial tariffcode match
update r
	set r.ZZ2_ZY1_RateCode = newRatePK.ZY1_PK
From dbo.RefCusTariff t
Join dbo.RefCusRate r
	On t.ZZ1_PK = r.ZZ2_ZZ1_Tariff
Join dbo.[RefCusRateCode] rc
	on r.[ZZ2_ZY1_RateCode] = rc.ZY1_PK
Left Join (
	Select '1302' as PartialTariff, '116' as newRateCode
	union all Select '2103' as PartialTariff, '116' as newRateCode
	union all Select '2203' as PartialTariff, '110' as newRateCode
	union all Select '2204' as PartialTariff, '116' as newRateCode
	union all Select '2205' as PartialTariff, '116' as newRateCode
	union all Select '2206' as PartialTariff, '116' as newRateCode
	union all Select '2207' as PartialTariff, '116' as newRateCode
	union all Select '2208' as PartialTariff, '116' as newRateCode
	union all Select '2710' as PartialTariff, '933' as newRateCode
	union all Select '2711' as PartialTariff, '933' as newRateCode
	union all Select '3302' as PartialTariff, '116' as newRateCode
	union all Select '3303' as PartialTariff, '116' as newRateCode
) newRateCode 
	on t.ZZ1_TariffCode like newRateCode.PartialTariff + '%'
Left Join dbo.[RefCusRateCode] newRatePK
	on newRateCode.newRateCode = newRatePK.ZY1_RateCode
Where ZZ2_ZZZ_NKDataGrouping = 'IT'
and newRateCode.newRateCode is not null
and newRateCode.newRateCode <> rc.ZY1_RateCode
and newRatePK.ZY1_PK is not null

--Fix vfd formulas
Update r
	set ZZ2_RateFormula = Convert(varchar(100),(Cast(ZZ2_RateFormula as float)/100),128) + ' * VFD'
From dbo.RefCusRate r
Where ZZ2_ZZZ_NKDataGrouping = 'IT'
And ZZ2_RateFormula not like '% * %'
And ZZ2_RateFormula <> '0'

--Add RefCusTariffUOM records
INSERT INTO RefCusTariffUOM (ZZ8_ZZ1_Tariff,ZZ8_ZZZ_NKDataGrouping,ZZ8_Type,ZZ8_UOM)
SELECT Distinct ZZ2_ZZ1_Tariff,'IT','AD1',
SUBSTRING(ZZ2_RateFormula
, CHARINDEX('[', ZZ2_RateFormula)+1
, CHARINDEX(']', ZZ2_RateFormula)- 1 - CHARINDEX('[', ZZ2_RateFormula))
From RefCusTariff t
Join dbo.RefCusRate r
	On t.ZZ1_PK = r.ZZ2_ZZ1_Tariff
Left Join RefCusTariffUOM uom
	On uom.ZZ8_ZZ1_Tariff = t.ZZ1_PK
	And uom.ZZ8_Type = 'AD1'
	And uom.ZZ8_UOM = SUBSTRING(ZZ2_RateFormula
		, CHARINDEX('[', ZZ2_RateFormula)+1
		, CHARINDEX(']', ZZ2_RateFormula)- 1 - CHARINDEX('[', ZZ2_RateFormula))
Where ZZ2_ZZZ_NKDataGrouping = 'IT'
And ZZ2_RateFormula like '% * [[]%]'
And uom.ZZ8_PK is null
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
