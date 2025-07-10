using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.DataTransformations
{
	public class FixEUNConditionsWithIncorrectSupUOMnFormulanValueType : DataTransformation, IDataTransformationTask
	{
		public FixEUNConditionsWithIncorrectSupUOMnFormulanValueType(int version) : base(version)
		{ }

		public void Run(IDbTransaction trans)
		{
			DbHelper.ExecuteNonQuery(trans, @"
UPDATE val SET ZX3_Value = REPLACE(ZX3_Value, 'NAR', ZZ8_UOM)
FROM RefCusTariff
JOIN RefCusTariffType ON ZZ1_ZZI_TariffType = ZZI_PK
JOIN RefCusTariffUOM on ZZ8_ZZ1_Tariff = ZZ1_PK
JOIN RefCusCondition on ZX1_ZZ1_Tariff = ZZ1_PK
JOIN RefCusConditionValue val on ZX3_ZX1_Condition = ZX1_PK
JOIN RefCusApplicability on ZZT_ZX1_Conditions = ZX1_PK and ZZT_ZZA_TradeGroup = ZZ8_ZZA_TradeGroup
WHERE ZZ1_ZZZ_NKDataGrouping = 'EUN' AND ZZI_TariffType = 'IMP'
AND ZZ8_ZZZ_NKDataGrouping = 'EUN' AND ZZ8_Type = 'CU2' AND ZX3_Value LIKE '%NAR%' AND ZZ8_UOM <> 'NAR'");

			DbHelper.ExecuteNonQuery(trans, @"
DELETE val
FROM RefCusConditionValue val
JOIN RefCusCondition ON ZX3_ZX1_Condition = ZX1_PK
WHERE ZX3_Value LIKE '\[\] <=' ESCAPE '\' AND ZX1_ZZZ_NKDataGrouping = 'EUN'");

			DbHelper.ExecuteNonQuery(trans, @"
DELETE val
FROM RefCusConditionValue val
JOIN RefCusCondition ON ZX3_ZX1_Condition = ZX1_PK
JOIN RefCusConditionValueType ON ZX3_ZX4_ValueType = ZX4_PK
WHERE ZX3_Value LIKE '%[<>=]%' and ZX4_ValueType != 'FRM' and ZX1_ZZZ_NKDataGrouping = 'EUN'");
		}
	}
}
