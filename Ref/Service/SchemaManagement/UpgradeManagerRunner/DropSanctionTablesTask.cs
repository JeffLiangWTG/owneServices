using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class DropSanctionTablesTask : DataTransformation, IDataTransformationTask
	{
		public DropSanctionTablesTask(int version) : base(version)
		{
		}

		public void Run(IDbTransaction trans)
		{
			var sql = @"
IF EXISTS(SELECT NULL FROM sys.tables where name like 'RefSanctionType')
BEGIN
DROP TABLE RefSanctionType
END
IF EXISTS(SELECT NULL FROM sys.tables where name like 'RefSanctionCountry')
BEGIN
DROP TABLE RefSanctionCountry
END
";
			using (var cmd = trans.Connection?.CreateCommand())
			{
				cmd.Transaction = trans;
				cmd.CommandText = sql;
				cmd.ExecuteNonQuery();
			}
		}
	}
}
