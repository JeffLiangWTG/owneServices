using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class RuleAddWI00176734Transformation : DataTransformation, IDataTransformationTask
	{
		public RuleAddWI00176734Transformation(int version) : base(version)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:ReviewSqlQueriesForSecurityVulnerabilities")]
		public void Run(IDbTransaction trans)
		{
			var sql = @"
DECLARE @id uniqueidentifier
IF (Select Count(1) from RefCusTariffRule t Join RefCusTariffUOMRule u on u.ZZ8_ZZ1_Tariff = t.ZZ1_PK Where t.ZZ1_TariffCode = '761210' And t.ZZ1_ZZZ_NKDataGrouping = 'ZA' And u.ZZ8_Type = 'CU2' And u.ZZ8_UOM = 'LI') = 0
Begin
SELECT @id = NEWID(); 
INSERT INTO RefCusTariffRule (ZZ1_PK, ZZ1_TariffCode, ZZ1_ZZZ_NKDataGrouping) VALUES (@id, '761210', 'ZA'); 
INSERT INTO RefCusTariffUOMRule (ZZ8_ZZ1_Tariff, ZZ8_Type, ZZ8_UOM) VALUES (@id, 'CU2','LI'); 
INSERT INTO RefCusTariffAttributeRule (ZZ3_ZZ1_Tariff, ZZ3_Name, ZZ3_Value) VALUES (@id, 'CheckDigit', '3')
End
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
