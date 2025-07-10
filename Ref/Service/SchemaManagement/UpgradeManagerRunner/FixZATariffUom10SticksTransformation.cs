using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class FixZATariffUom10SticksTransformation : DataTransformation, IDataTransformationTask
	{
		public FixZATariffUom10SticksTransformation(int version) : base(version)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:ReviewSqlQueriesForSecurityVulnerabilities")]
		public void Run(IDbTransaction trans)
		{
			var sql = @"
insert into RefCusTariffUOM (ZZ8_PK,ZZ8_Type,ZZ8_UOM,ZZ8_ZZ1_Tariff,ZZ8_DataSetPK,ZZ8_DataSetCode)
select newid(),'CU1','NO',ZZ2_ZZ1_Tariff,ZZ2_ZZ1_Tariff,'ZZ1'
from RefCusRate r
join RefCusTariff t on t.ZZ1_PK = ZZ2_ZZ1_Tariff
left join RefCusTariffUOM uom on uom.ZZ8_ZZ1_Tariff = ZZ1_PK and uom.ZZ8_Type = 'CU1' and uom.ZZ8_UOM = 'NO'
where ZZ2_ZZZ_NKDataGrouping = 'ZA'
and ZZ2_RateFormula like '%[[STICKS]]'
and uom.ZZ8_PK is null
and t.ZZ1_TariffCode in ('1043506','1043510','1043514');

update r
	set ZZ2_RateFormula = REPLACE(ZZ2_RateFormula,'[STICKS]','[NO]')
from RefCusRate r
join RefCusTariff t on t.ZZ1_PK = ZZ2_ZZ1_Tariff
where ZZ2_ZZZ_NKDataGrouping = 'ZA'
and ZZ2_RateFormula like '%[[STICKS]]'
and t.ZZ1_TariffCode in ('1043506','1043510','1043514');
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
