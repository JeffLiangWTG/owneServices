using System;
using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class RemoveDuplicateRefAccTaxRateTransformation : DataTransformation, IDataTransformationTask
	{
		public RemoveDuplicateRefAccTaxRateTransformation(int version) : base(version)
		{
		}
		public void Run(IDbTransaction trans)
		{
			var sql = @"
DECLARE @TempTable TABLE (PK UNIQUEIDENTIFIER);

WITH TempData
AS	(
	SELECT	*, ItemNumber = ROW_NUMBER()
		OVER (PARTITION BY ZAT_RN_NKCountry, ZAT_ReferenceRateType, ZAT_StartDate
		ORDER BY RVC_Deleted ASC, ZAT_EndDate DESC)
	FROM	RefAccTaxRate
	JOIN	RefDbVersionControl
	ON		ZAT_PK = RVC_ParentPK AND RVC_ParentCode = 'ZAT'

)

INSERT INTO @TempTable(PK)
SELECT	RAT.ZAT_PK
FROM	RefAccTaxRate RAT
JOIN	TempData
ON		TempData.ZAT_PK = RAT.ZAT_PK
AND		ItemNumber > 1

DELETE	RAT
FROM	RefAccTaxRate RAT
JOIN	@TempTable TT
ON		RAT.ZAT_PK = TT.PK

DELETE	RDVC
FROM	RefDbVersionControl RDVC
JOIN	@TempTable TT
ON		RDVC.RVC_ParentPK = TT.PK
AND		RDVC.RVC_ParentCode = 'ZAT'";

			DbHelper.ExecuteNonQuery(trans, sql);
		}
	}
}
