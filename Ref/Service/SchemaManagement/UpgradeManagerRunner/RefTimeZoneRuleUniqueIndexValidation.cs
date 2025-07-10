using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class RefTimeZoneRuleUniqueIndexValidation : DataTransformation, IDataTransformationTask
	{
		public RefTimeZoneRuleUniqueIndexValidation(int version) : base(version)
		{
		}

		public void Run(IDbTransaction trans)
		{
			var sql = @"
WITH TimeZoneRuleRecords AS
(
SELECT R4_PK,
ROW_NUMBER() OVER (PARTITION BY R4_R2, R4_StartOrEndRule, R4_FromYear, R4_DaylightSavingMonth ORDER BY R4_FromYear) row_n  
FROM RefTimeZoneRule
)

DELETE t
FROM RefTimeZoneRule t
JOIN TimeZoneRuleRecords r on t.R4_PK = r.R4_PK and r.row_n > 1
";

			DbHelper.ExecuteNonQuery(trans, sql);
		}
	}
}
