using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class DataFixConditionTypeTransformation : DataTransformation, IDataTransformationTask
	{
		public DataFixConditionTypeTransformation(int version) : base(version)
		{
		}

		public void Run(IDbTransaction trans)
		{
			var sql = @"
declare @type uniqueidentifier
--update conditionvaluetypes to SUP 
select top 1 @type = ZX4_PK
from RefCusConditionValueType
Where ZX4_ValueType = 'SUP'

update cv
	set cv.ZX3_ZX4_ValueType = @type
from RefCusConditionValueType cvt
join RefCusConditionValue cv on cvt.ZX4_PK = cv.ZX3_ZX4_ValueType
join RefCusCondition c on c.ZX1_PK = cv.ZX3_ZX1_Condition
Where cvt.ZX4_ZZZ_NKDataGrouping = 'EUN'
and ZX4_ValueType not in ('FRM','SUP')
and (
	(Left(ZX3_Value,1) in ('A','B','C','E','N','H','L','Q','Z') and ZX3_Value not in ('L136', 'N380', 'N235', 'N271', 'N325', 'N750', 'N934', 'N935', 'N787', 'N864'))
	or ZX3_Value in ('Y022', 'Y023', 'Y024', 'Y025', 'Y026', 'Y027', 'Y028', 'Y029', 'Y031', 'Y040', 'Y041', 'Y042', 'Y915', 'Y919')
)

--update conditionvaluetypes to SNR

select top 1 @type = ZX4_PK
from RefCusConditionValueType
Where ZX4_ValueType = 'SNR'

update cv
	set cv.ZX3_ZX4_ValueType = @type
from RefCusConditionValueType cvt
join RefCusConditionValue cv on cvt.ZX4_PK = cv.ZX3_ZX4_ValueType
join RefCusCondition c on c.ZX1_PK = cv.ZX3_ZX1_Condition
Where cvt.ZX4_ZZZ_NKDataGrouping = 'EUN'
and ZX4_ValueType not in ('FRM','SNR')
and (
	ZX3_Value in ('L136', 'N380', 'N235', 'N271', 'N325', 'N750', 'N934', 'N935', 'N787', 'N864')
	or
	(Left(ZX3_Value,1) not in ('A','B','C','E','N','H','L','Q','Z')
	and ZX3_Value not in ('Y022', 'Y023', 'Y024', 'Y025', 'Y026', 'Y027', 'Y028', 'Y029', 'Y031', 'Y040', 'Y041', 'Y042', 'Y915', 'Y919'))
)
";
			DbHelper.ExecuteNonQuery(trans, sql);
		}
	}
}
