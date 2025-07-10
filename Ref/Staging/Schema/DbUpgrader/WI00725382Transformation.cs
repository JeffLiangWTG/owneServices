using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Staging.DbUpgrader
{
	public class WI00725382Transformation : DataTransformation, IDataTransformationTask
	{
		public WI00725382Transformation(int version) : base(version)
		{
		}

		public void Run(IDbTransaction trans)
		{
			const string tempIndexName = "IX_SourceData_SDA_SubSource_C2E6ACADB5D54DD58E1680A03B4F3EF2";
			var sql = $@"CREATE NONCLUSTERED INDEX {tempIndexName} ON dbo.SourceData (SDA_SubSource)
DELETE dbo.SourceData WHERE SDA_SubSource = 'UNKNOWN'
DROP INDEX {tempIndexName} ON dbo.SourceData";
			DbHelper.ExecuteNonQuery(trans, sql, 0);
		}
	}
}
