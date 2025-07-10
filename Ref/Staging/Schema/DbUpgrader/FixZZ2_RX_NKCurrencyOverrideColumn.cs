using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Staging.DbUpgrader
{
	public class FixZZ2_RX_NKCurrencyOverrideColumn : DataTransformation, IDataTransformationTask
	{
		public FixZZ2_RX_NKCurrencyOverrideColumn(int version) : base(version)
		{
		}

		public void Run(IDbTransaction trans)
		{
			var sqlText = @"
IF EXISTS(SELECT 1 FROM RefCusRate WHERE ZZ2_RX_NKCurrencyOverride IS NULL)
BEGIN
	UPDATE RefCusRate SET ZZ2_RX_NKCurrencyOverride = '' WHERE ZZ2_RX_NKCurrencyOverride IS NULL
END
";
			DbHelper.ExecuteNonQuery(trans, sqlText, 0);
		}
	}
}
