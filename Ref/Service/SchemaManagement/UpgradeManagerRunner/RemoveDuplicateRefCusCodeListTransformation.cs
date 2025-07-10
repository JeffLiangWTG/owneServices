using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class RemoveDuplicateRefCusCodeListTransformation : DataTransformation, IDataTransformationTask
	{
		public RemoveDuplicateRefCusCodeListTransformation(int version) : base(version)
		{
		}

		public void Run(IDbTransaction trans)
		{
			var sql = @"
DECLARE @CodeListToDelete TABLE (PK UNIQUEIDENTIFIER);

WITH data 
AS (
SELECT *, ItemNumber = ROW_NUMBER() 
	OVER (PARTITION BY ZZD_ZZZ_NKDataGrouping, ZZD_ZZK_NKCodeType, ZZD_Code
	ORDER BY RVC_Deleted ASC, ZZD_StartDate DESC)
	FROM
	dbo.RefCusCodeList
	JOIN RefDbVersionControl ON ZZD_PK = RVC_ParentPK AND RVC_ParentCode = 'ZZD'
)

INSERT INTO @CodeListToDelete(PK)
SELECT r.ZZD_PK FROM RefCusCodeList r
JOIN data ON data.ZZD_PK = r.ZZD_PK AND data.ItemNumber > 1

DELETE r FROM RefCusCodeOrAttributeTransportMode r
JOIN @CodeListToDelete t ON r.ZZU_DataSetPK = t.PK AND r.ZZU_DataSetCode = 'ZZD'

DELETE r FROM RefCusCodeListAttribute r
JOIN @CodeListToDelete t ON r.ZZE_ZZD_CodeList = t.PK

DELETE r FROM RefCusCodeListLanguage r
JOIN @CodeListToDelete t ON r.ZXA_ZZD_CodeList = t.PK

DELETE r FROM RefCusCodeList r
JOIN @CodeListToDelete t ON t.PK = r.ZZD_PK

DELETE d FROM RefDbVersionControl d
JOIN @CodeListToDelete t ON t.PK = d.RVC_ParentPK and RVC_ParentCode = 'ZZD'";

			DbHelper.ExecuteNonQuery(trans, sql);
		}
	}
}
