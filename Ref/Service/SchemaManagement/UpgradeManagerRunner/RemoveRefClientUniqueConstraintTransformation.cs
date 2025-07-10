using System.Data;
using System.Globalization;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class RemoveRefClientUniqueConstraintTransformation : DataTransformation, IDataTransformationTask
	{
		public RemoveRefClientUniqueConstraintTransformation(int version) : base(version)
		{
		}

		public void Run(IDbTransaction trans)
		{
			var sql = string.Format(CultureInfo.InvariantCulture, @"
DECLARE @constraint_name NVARCHAR(MAX)
SELECT @constraint_name = ckc.name
	FROM sys.tables tab
	INNER JOIN sys.key_constraints ckc ON tab.object_id=ckc.parent_object_id
	WHERE tab.name = '{0}' AND ckc.type = 'UQ'
IF @constraint_name IS NOT NULL
BEGIN
	EXEC('ALTER TABLE {0} DROP CONSTRAINT ' + @constraint_name);
END;", "RefClient");
			DbHelper.ExecuteNonQuery(trans, sql);
		}
	}
}
