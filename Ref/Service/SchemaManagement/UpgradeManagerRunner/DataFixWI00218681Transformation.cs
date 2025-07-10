using System.Data;
using System.Text;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class DataFixWI00218681Transformation : DataTransformation, IDataTransformationTask
	{
		public DataFixWI00218681Transformation(int version) : base(version)
		{
		}

		public void Run(IDbTransaction trans)
		{
			var sql = new StringBuilder(GetUNDGAttributeSql());
			sql.AppendLine(GetUNDGCommonDataSql());

			using (var cmd = DbHelper.CreateCommand(trans, sql.ToString()))
			{
				cmd.ExecuteNonQuery();
			}
		}

		static string GetUNDGAttributeSql()
		{
			return @"
UPDATE UNDGAttribute
SET DA_Language = 'EN'
WHERE DA_Language = 'ENG';";
		}

		static string GetUNDGCommonDataSql()
		{
			return @"
INSERT INTO UNDGCommonData ([DC_PK],[DC_Language],[DC_Type],[DC_Index],[DC_Descriptor])
SELECT NEWID(), 'EN', DC_Type, DC_Index, DC_Descriptor FROM UNDGCommonData
WHERE DC_PK NOT IN (SELECT RVC_ParentPK FROM RefDbVersionControl WHERE RVC_Deleted = 1 AND RVC_ParentCode = 'DC')
AND DC_Language = 'ENG';

UPDATE r SET RVC_Deleted = 1, RVC_LastUpdatedUTC = SYSUTCDATETIME()
FROM RefDbVersionControl r
JOIN UNDGCommonData ON RVC_ParentPK = DC_PK AND RVC_ParentCode = 'DC'
WHERE
DC_Language = 'ENG';";
		}
	}
}
