using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class MergeMeursingTariffsTransformation : DataTransformation, IDataTransformationTask
	{
		public MergeMeursingTariffsTransformation(int version) : base(version)
		{
		}

		public void Run(IDbTransaction trans)
		{
			var sql = @"
DECLARE @meursingTypePK uniqueidentifier
SELECT @meursingTypePK = ZZI_PK FROM RefCusTariffType
WHERE ZZI_TariffType = 'MEU' AND ZZI_ZZZ_NKDataGrouping = 'EUN'

IF @meursingTypePK IS NOT NULL
BEGIN
	UPDATE rate SET ZZ2_StartDate = ZZ1_StartDate
	FROM RefCusRate rate
	JOIN RefCusTariff ON ZZ2_ZZ1_Tariff = ZZ1_PK
	JOIN RefDbVersionControl ON RVC_ParentPK = ZZ1_PK
	WHERE ZZ1_ZZZ_NKDataGrouping = 'EUN' AND ZZ1_ZZI_TariffType = @meursingTypePK AND RVC_Deleted = 0 AND ZZ2_StartDate = '1900-01-01 00:00:00';

	WITH Duplicate AS ( 
		SELECT ZZ1_PK, ZZ1_TariffCode, ItemNumber = ROW_NUMBER() OVER (PARTITION BY ZZ1_TariffCode ORDER BY ZZ1_StartDate)
		FROM RefCusTariff
		JOIN RefDbVersionControl ON RVC_ParentPK = ZZ1_PK
		WHERE ZZ1_ZZZ_NKDataGrouping = 'EUN' AND ZZ1_ZZI_TariffType = @meursingTypePK AND RVC_Deleted = 0
	)

	UPDATE rate SET ZZ2_ZZ1_Tariff = dup2.ZZ1_PK, ZZ2_DataSetPK = dup2.ZZ1_PK
	FROM RefCusRate rate
	JOIN Duplicate dup1 ON ZZ2_ZZ1_Tariff = dup1.ZZ1_PK AND ItemNumber > 1
	JOIN Duplicate dup2 ON dup1.ZZ1_TariffCode = dup2.ZZ1_TariffCode AND dup2.ItemNumber = 1;

	WITH Duplicate AS ( 
		SELECT ZZ1_PK, ItemNumber = ROW_NUMBER() OVER (PARTITION BY ZZ1_TariffCode ORDER BY ZZ1_StartDate)
		FROM RefCusTariff
		JOIN RefDbVersionControl ON RVC_ParentPK = ZZ1_PK
		WHERE ZZ1_ZZZ_NKDataGrouping = 'EUN' AND ZZ1_ZZI_TariffType = @meursingTypePK AND RVC_Deleted = 0
	)

	UPDATE ver SET RVC_Deleted = 1, RVC_LastUpdatedUTC = SYSUTCDATETIME()
	FROM RefDbVersionControl ver
	JOIN Duplicate ON RVC_ParentPK = ZZ1_PK AND ItemNumber > 1
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
