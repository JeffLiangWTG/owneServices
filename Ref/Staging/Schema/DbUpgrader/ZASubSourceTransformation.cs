using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Staging.DbUpgrader
{
	public class ZASubSourceTransformation : DataTransformation, IDataTransformationTask
	{
		public ZASubSourceTransformation(int version) : base(version)
		{ }

		public void Run(IDbTransaction trans)
		{
			var sql = @"
IF EXISTS (SELECT 1 FROM SourceData WHERE SDA_SubSource = '' AND SDA_Source = 'ZAA' AND SDA_ContentType IN ('MES', 'PRO'))
BEGIN
	UPDATE SourceData SET SDA_SubSource = CASE WHEN SDA_ContentType = 'PRO' THEN 'ZA Tariffs' ELSE 'ZA Exchange Rates' END
	WHERE SDA_Source = 'ZAA' AND SDA_ContentType IN ('MES', 'PRO')
END
";
			DbHelper.ExecuteNonQuery(trans, sql, 0);
		}
	}
}
