using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class FixMeursingApplicationDataSetPKTransformation : DataTransformation, IDataTransformationTask
	{
		public FixMeursingApplicationDataSetPKTransformation(int version) : base(version)
		{ }

		public void Run(IDbTransaction trans)
		{
			var sqlText = @"
UPDATE app SET ZZT_DataSetPK = ZZ2_DataSetPK
FROM RefCusApplicability app
JOIN RefCusRate ON ZZT_ZZ2_Rate = ZZ2_PK
JOIN RefCusTariff ON ZZ2_ZZ1_Tariff = ZZ1_PK
JOIN RefCusTariffType ON ZZ1_ZZI_TariffType = ZZI_PK
WHERE ZZI_TariffType = 'MEU' AND ZZI_ZZZ_NKDataGrouping = 'EUN' AND ZZT_DataSetPK <> ZZ2_DataSetPK
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
