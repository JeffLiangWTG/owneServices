using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Staging.DbUpgrader
{
	public class WI00470139Transformation : DataTransformation, IDataTransformationTask
	{
		public WI00470139Transformation(int version) : base(version) { }

		public void Run(IDbTransaction trans)
		{
			var sql = @"IF ((SELECT COUNT(*) FROM RefApplicationAttributeType WHERE RAT_Type = 'StatusFlag') = 0)
BEGIN
INSERT INTO RefApplicationAttributeType(RAT_PK, RAT_Type, RAT_Description)
VALUES
(newid(), 'StatusFlag', 'status flag')
END
";
			DbHelper.ExecuteNonQuery(trans, sql, 0);
		}
	}
}
