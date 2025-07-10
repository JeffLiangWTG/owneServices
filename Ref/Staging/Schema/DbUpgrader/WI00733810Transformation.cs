using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Staging.DbUpgrader
{
	public class WI00733810Transformation : DataTransformation, IDataTransformationTask
	{
		public WI00733810Transformation(int version) : base(version) { }

		public void Run(IDbTransaction trans)
		{
			var sql = @"IF ((SELECT COUNT(*) FROM RefApplicationAttributeType WHERE RAT_Type = 'Credential') = 0)
BEGIN
INSERT INTO RefApplicationAttributeType(RAT_PK, RAT_Type, RAT_Description)
VALUES
(NEWID(), 'Credential', 'credential type')
END
";
			DbHelper.ExecuteNonQuery(trans, sql, 0);
		}
	}
}
