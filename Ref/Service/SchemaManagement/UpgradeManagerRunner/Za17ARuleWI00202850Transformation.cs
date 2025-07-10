using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class Za17ARuleWI00202850Transformation : DataTransformation, IDataTransformationTask
	{
		public Za17ARuleWI00202850Transformation(int version) : base(version)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:ReviewSqlQueriesForSecurityVulnerabilities")]
		public void Run(IDbTransaction trans)
		{
			var sql = @"
Declare @TariffType uniqueidentifier
Select TOP 1 @TariffType = ZZI_PK From RefCusTariffType Where ZZI_TariffType = '17A' and ZZI_ZZZ_NKDataGrouping = 'ZA'
IF (select count(*) from RefCusTariffRule join RefCusTariffUOMRule on ZZ1_PK = ZZ8_ZZ1_Tariff Where ZZ1_TariffCode = '1910' and ZZ1_ZZI_TariffType = @TariffType and ZZ8_Type = 'RU1' and ZZ8_UOM = 'LI') < 1
BEGIN
	Declare @ZZ1_PK uniqueidentifier	
	Select @ZZ1_PK = newid()
	insert into RefCusTariffRule (ZZ1_PK,ZZ1_TariffCode,ZZ1_ZZZ_NKDataGrouping,ZZ1_ZZI_TariffType) Values (@ZZ1_PK,'1910','ZA',@TariffType)
	insert into RefCusTariffUOMRule (ZZ8_ZZ1_Tariff,ZZ8_Type,ZZ8_UOM,ZZ8_ZZZ_NKDataGrouping) Values (@ZZ1_PK,'RU1','LI','ZA')	
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
