using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class DeleteEUNConditionsWithMoreThanOneConditionValueType : DataTransformation, IDataTransformationTask
	{
		public DeleteEUNConditionsWithMoreThanOneConditionValueType(int version) : base(version)
		{
		}

		public void Run(IDbTransaction trans)
		{
			var sql = @"IF EXISTS (SELECT TOP 1 ZX3_ZX1_Condition
FROM (SELECT DISTINCT ZX3_ZX1_Condition, ZX3_ZX4_ValueType 
	FROM RefCusConditionValue 
	JOIN RefCusCondition on ZX3_ZX1_Condition = ZX1_PK
	WHERE ZX1_ZZZ_NKDataGrouping = 'EUN'
) ValueTypesPerCondition
GROUP BY ZX3_ZX1_Condition
HAVING COUNT(*) > 1)
BEGIN
	DELETE RefCusConditionValue WHERE 
	ZX3_ZX1_Condition IN (
		SELECT ZX3_ZX1_Condition
		FROM (SELECT DISTINCT ZX3_ZX1_Condition, ZX3_ZX4_ValueType 
			FROM RefCusConditionValue 
			JOIN RefCusCondition on ZX3_ZX1_Condition = ZX1_PK
			WHERE ZX1_ZZZ_NKDataGrouping = 'EUN'
		) ValueTypesPerCondition
		GROUP BY ZX3_ZX1_Condition
		HAVING COUNT(*) > 1
	)
END
";
			DbHelper.ExecuteNonQuery(trans, sql);
		}
	}
}
