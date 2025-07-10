using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Staging.DbUpgrader
{
	public class WI00843852Transformation : DataTransformation, IDataTransformationTask
	{
		public WI00843852Transformation(int version) : base(version) { }

		public void Run(IDbTransaction trans)
		{
			var sql = @"
DELETE FROM dbo.DataProcessingInformation
WHERE DPI_ParentPK = '00000000-0000-0000-0000-000000000000'
";
			DbHelper.ExecuteNonQuery(trans, sql, 0);
		}
	}
}
