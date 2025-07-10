using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class OffsetFromUTCToOffsetMinutesFromUTC : DataTransformation, IDataTransformationTask
	{
		public OffsetFromUTCToOffsetMinutesFromUTC(int version) : base(version)
		{
		}

		public void Run(IDbTransaction trans)
		{
			DbHelper.SetSystemVersioningOff(trans, "RefUNLOCOUTCOffset");
			DbHelper.SetSystemVersioningOff(trans, "RefTimeZone");

			var sql = @"
IF NOT EXISTS(SELECT * FROM sys.tables t JOIN sys.all_columns c on c.object_id = t.object_id where c.name = 'R2_OffsetMinutesFromUTC')
	AND NOT EXISTS(SELECT * FROM sys.tables t JOIN sys.all_columns c on c.object_id = t.object_id where c.name = 'RLO_OffsetMinutesFromUTC')
BEGIN
	ALTER TABLE RefTimeZone ADD R2_OffsetMinutesFromUTC SMALLINT;
	ALTER TABLE RefTimeZoneHistory ADD R2_OffsetMinutesFromUTC SMALLINT;
	ALTER TABLE RefUNLOCOUTCOffset ADD RLO_OffsetMinutesFromUTC SMALLINT;
	ALTER TABLE RefUNLOCOUTCOffsetHistory ADD RLO_OffsetMinutesFromUTC SMALLINT;
END
";
			DbHelper.ExecuteNonQuery(trans, sql);

			sql = @"
IF EXISTS(SELECT * FROM sys.tables t JOIN sys.all_columns c on c.object_id = t.object_id where c.name = 'R2_OffsetFromUTC')
	AND EXISTS(SELECT * FROM sys.tables t JOIN sys.all_columns c on c.object_id = t.object_id where c.name = 'RLO_OffsetFromUtc')
BEGIN
	UPDATE RefTimeZone SET R2_OffsetMinutesFromUTC = CAST(R2_OffsetFromUtc * 60.0 AS SMALLINT);
	UPDATE RefTimeZoneHistory SET R2_OffsetMinutesFromUTC = CAST(R2_OffsetFromUtc * 60.0 AS SMALLINT);
	UPDATE RefUNLOCOUTCOffset SET RLO_OffsetMinutesFromUTC = CAST(RLO_OffsetFromUTC * 60.0 AS SMALLINT);
	UPDATE RefUNLOCOUTCOffsetHistory SET RLO_OffsetMinutesFromUTC = CAST(RLO_OffsetFromUTC * 60.0 AS SMALLINT);
END";

			DbHelper.ExecuteNonQuery(trans, sql, 600);

			DbHelper.SetSystemVersioningOn(trans, "RefUNLOCOUTCOffset");
			DbHelper.SetSystemVersioningOn(trans, "RefTimeZone");
		}
	}
}
