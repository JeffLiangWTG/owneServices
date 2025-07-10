using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class ChangeRateCodeFrom160To125ForItalianTariffTransformation : DataTransformation, IDataTransformationTask
	{
		public ChangeRateCodeFrom160To125ForItalianTariffTransformation(int version) : base(version)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:ReviewSqlQueriesForSecurityVulnerabilities")]
		public void Run(IDbTransaction trans)
		{
			var sql = @"
DECLARE @RateCode125PK UNIQUEIDENTIFIER = (SELECT TOP 1 ZY1_PK FROM RefCusRateCode WHERE ZY1_RateCode = '125')
UPDATE RefCusRate
SET ZZ2_ZY1_RateCode = @RateCode125PK
FROM RefCusRate
JOIN RefCusRateCode ON ZZ2_ZY1_RateCode = ZY1_PK
JOIN RefCusTariff ON ZZ2_ZZ1_Tariff = ZZ1_PK
JOIN RefCusTariffType ON ZZ1_ZZI_TariffType = ZZI_PK
WHERE ZZI_TariffType = 'IMP' AND ZZI_ZZZ_NKDataGrouping = 'EUN' AND ZZ1_ZZZ_NKDataGrouping = 'EUN' AND ZZ2_ZZZ_NKDataGrouping = 'IT' AND ZY1_RateCode = '160'
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
