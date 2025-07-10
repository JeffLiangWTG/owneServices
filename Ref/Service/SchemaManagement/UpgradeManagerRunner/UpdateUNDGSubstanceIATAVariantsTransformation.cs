using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class UpdateUNDGSubstanceIATAVariantsTransformation : DataTransformation, IDataTransformationTask
	{
		public UpdateUNDGSubstanceIATAVariantsTransformation(int version) : base(version)
		{
		}

		public void Run(IDbTransaction trans)
		{
			var sql = @"
with dup as
(
	SELECT DG_UNNO
	FROM UNDGSubstance
	WHERE DG_Variant = '' and DG_Standard = 'IAT'
	GROUP BY DG_UNNO
	HAVING COUNT(DG_UNNO) > 1
),
unnosVariants as
(
	SELECT s.DG_PK, s.DG_UNNO, LOWER(Char(64 + ROW_NUMBER() OVER (PARTITION BY s.DG_UNNO ORDER BY CAST(DG_UniqueRecordId AS INT) ASC))) Variant
	FROM UNDGSubstance s
	JOIN dup d on s.DG_UNNO = d.DG_UNNO
	WHERE DG_Standard = 'IAT' AND DG_Variant = ''
	GROUP BY s.DG_UNNO, DG_UniqueRecordId, DG_PK
)

UPDATE s 
SET s.DG_Variant = Variant
FROM UNDGSubstance s
JOIN unnosVariants v on s.DG_PK = v.DG_PK";

			DbHelper.ExecuteNonQuery(trans, sql);
		}
	}
}
