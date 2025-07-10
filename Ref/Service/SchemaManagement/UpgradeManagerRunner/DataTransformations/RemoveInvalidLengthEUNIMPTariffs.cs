using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class RemoveInvalidLengthEUNIMPTariffs : DataTransformation, IDataTransformationTask
	{
		public RemoveInvalidLengthEUNIMPTariffs(int version) : base(version)
		{
		}

		public void Run(IDbTransaction trans)
		{
			var sql = @"UPDATE v SET RVC_Deleted = 1, RVC_IsPublished = 0
FROM RefDbVersionControl v
JOIN RefCusTariff ON RVC_ParentPK = ZZ1_PK
JOIN RefCusTariffType ON ZZ1_ZZI_TariffType = ZZI_PK
WHERE ZZI_TariffType = 'IMP' AND ZZ1_ZZZ_NKDataGrouping = 'EUN'
AND ZZI_ZZZ_NKDataGrouping = 'EUN' AND LEN(ZZ1_TariffCode) < 10";
			DbHelper.ExecuteNonQuery(trans, sql);
		}
	}
}
