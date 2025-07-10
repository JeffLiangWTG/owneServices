using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Staging.DbUpgrader
{
	public class WI00879039Transformation : DataTransformation, IDataTransformationTask
	{
		public WI00879039Transformation(int version) : base(version) { }

		public void Run(IDbTransaction trans)
		{
			var sql = @"IF ((SELECT COUNT(*) FROM RefApplicationAttributeType WHERE RAT_Type = 'SecretFile') = 0)
BEGIN
INSERT INTO RefApplicationAttributeType(RAT_PK, RAT_Type, RAT_Description)
VALUES
(NEWID(), 'SecretFile', 'secret file')
END
";
			DbHelper.ExecuteNonQuery(trans, sql, 0);
		}
	}
}
