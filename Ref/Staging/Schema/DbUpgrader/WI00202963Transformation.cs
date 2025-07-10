using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Staging.DbUpgrader
{
	public class WI00202963Transformation : DataTransformation, IDataTransformationTask
	{
		public WI00202963Transformation(int version) : base(version) { }

		public void Run(IDbTransaction trans)
		{
			var sql = @"
IF ((SELECT COUNT(*) FROM RefCusTariff WHERE ZZ1_ZZI_ZZZ_NKDataGrouping <> '') = 0)
BEGIN
	UPDATE RefCusTariff SET ZZ1_ZZI_ZZZ_NKDataGrouping = ZZ1_ZZZ_NKDataGrouping
END
";
			DbHelper.ExecuteNonQuery(trans, sql, 0);
		}
	}
}
