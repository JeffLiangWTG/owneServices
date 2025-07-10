using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class DeactivateUNDGSubstanceWithIncorrectVariantInformation : DataTransformation, IDataTransformationTask
	{
		public DeactivateUNDGSubstanceWithIncorrectVariantInformation(int version) : base(version)
		{
		}

		public void Run(IDbTransaction trans)
		{
			var cmd = $@"
SELECT s.DG_PK, s.DG_UNNO
INTO #substances
FROM UNDGSubstance s
LEFT JOIN UNDGSubstance t ON t.DG_UNNO = s.DG_UNNO AND t.DG_Variant = 'b' AND t.DG_Standard = s.DG_Standard
WHERE
s.DG_Standard = 'IMO' AND
t.DG_PK IS NULL

UPDATE UNDGSubstance SET DG_IsActive = 0
WHERE DG_PK IN (SELECT DG_PK FROM #substances)

UPDATE u SET u.DG_IsActive = 1
FROM UNDGSubstance u
JOIN #substances s ON s.DG_UNNO = u.DG_UNNO AND u.DG_Variant = ''

DROP TABLE #substances
";

			DbHelper.ExecuteNonQuery(trans, cmd);
		}
	}
}
