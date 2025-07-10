using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class AddZZZ_NKDataGroupColumnTask : DataTransformation, IDataTransformationTask
	{
		public AddZZZ_NKDataGroupColumnTask(int version) : base(version)
		{
		}

		public void Run(IDbTransaction trans)
		{
			var sql = @"
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE name = 'ZZO_ZZZ_NKDataGrouping' AND object_id = OBJECT_ID('dbo.RefVesselZZ'))
BEGIN
	ALTER TABLE RefVesselZZ
	ADD ZZO_ZZZ_NKDataGrouping VARCHAR(3);

	DISABLE TRIGGER RefVesselZZ_Version_Update ON RefVesselZZ

	EXEC ('UPDATE RefVesselZZ SET ZZO_ZZZ_NKDataGrouping = ''ZA''');

	ENABLE TRIGGER RefVesselZZ_Version_Update ON RefVesselZZ
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
