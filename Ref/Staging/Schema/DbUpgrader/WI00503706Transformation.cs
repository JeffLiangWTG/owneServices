using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Staging.DbUpgrader
{
	public class WI00503706Transformation : DataTransformation, IDataTransformationTask
	{
		public WI00503706Transformation(int version) : base(version) { }

		public void Run(IDbTransaction trans)
		{
			var sql = @"
DELETE FROM RefCusTariffUOM WHERE ZZ8_Type NOT IN ('CU1', 'RU1', 'AD1', 'CU2', 'CU3', 'CU4', 'CU5');
UPDATE RefCusApplicability SET ZZT_AdditionalCode = '' WHERE ZZT_AdditionalCode IS NULL;
UPDATE RefCusApplicability SET ZZT_OrderNumber = '' WHERE ZZT_OrderNumber IS NULL;
UPDATE RefCusConditionValueTypeLanguage SET ZXX_Description = '' WHERE ZXX_Description IS NULL;
";
			DbHelper.ExecuteNonQuery(trans, sql);
		}
	}
}
