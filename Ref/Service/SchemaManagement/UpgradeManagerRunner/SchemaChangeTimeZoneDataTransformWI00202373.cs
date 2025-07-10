using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class SchemaChangeTimeZoneDataTransformWI00202373 : DataTransformation, IDataTransformationTask
	{
		public SchemaChangeTimeZoneDataTransformWI00202373(int version) : base(version)
		{ }

		public void Run(IDbTransaction trans)
		{
			{
				var sql = @"
	DISABLE TRIGGER ALL ON RefTimeZone;

	IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE name = 'R2_R3_TimeZoneSet' AND object_id = OBJECT_ID('dbo.RefTimeZone'))
	BEGIN
		ALTER TABLE RefTimeZone Add [R2_R3_TimeZoneSet] uniqueidentifier NULL;
	END
	IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE name = 'R2_Type' AND object_id = OBJECT_ID('dbo.RefTimeZone'))
	BEGIN
		ALTER TABLE RefTimeZone Add [R2_Type] [varchar](3) CONSTRAINT [DF_RefTimeZone_R2_Type] DEFAULT '';
	END";
				using (var cmd = trans.Connection?.CreateCommand())
				{
					cmd.Transaction = trans;
					cmd.CommandText = sql;
					cmd.ExecuteNonQuery();
				}

				sql = @"UPDATE tz 
	SET
		R2_Type = case when tz.R2_PK = tzs.R3_R2_DaylightSavingZone Then 'DLS' else 'STD' end
		,R2_R3_TimeZoneSet = tzs.R3_PK
	FROM RefTimeZone tz
	JOIN RefTimeZoneSet tzs on tz.R2_PK in (tzs.R3_R2_DaylightSavingZone,tzs.R3_R2_StandardZone);

	ENABLE TRIGGER ALL ON RefTimeZone;";

				using (var cmd = trans.Connection?.CreateCommand())
				{
					cmd.Transaction = trans;
					cmd.CommandText = sql;
					cmd.ExecuteNonQuery();
				}
			}
		}
	}
}

