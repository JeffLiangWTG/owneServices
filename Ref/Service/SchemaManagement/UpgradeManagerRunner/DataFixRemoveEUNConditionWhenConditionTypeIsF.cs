using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class DataFixRemoveEUNConditionWhenConditionTypeIsF : DataTransformation, IDataTransformationTask
	{
		public DataFixRemoveEUNConditionWhenConditionTypeIsF(int version) : base(version)
		{
		}

		public void Run(IDbTransaction trans)
		{
			var sql = @"
select ZX1_PK
into #EUNConditions
from RefCusCondition
where 
ZX1_Comment like 'Condition F%'
and ZX1_ZZZ_NKDataGrouping = 'EUN'

DELETE cv 
FROM RefCusConditionValue cv
JOIN #EUNConditions on ZX1_PK = ZX3_ZX1_Condition

DELETE a
FROM RefCusApplicability a
JOIN #EUNConditions ON ZZT_ZX1_Conditions = ZX1_PK

DELETE c
FROM RefCusCondition c
JOIN #EUNConditions tc ON tc.ZX1_PK = c.ZX1_PK

DROP TABLE #EUNConditions"
;
			DbHelper.ExecuteNonQuery(trans, sql, 600);
		}
	}
}
