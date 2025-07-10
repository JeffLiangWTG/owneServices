using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class FixEUNTariffPreferenceRateMappingTransformation : DataTransformation, IDataTransformationTask
	{
		public FixEUNTariffPreferenceRateMappingTransformation(int version) : base(version)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:ReviewSqlQueriesForSecurityVulnerabilities")]
		public void Run(IDbTransaction trans)
		{
			var createIndexSql = @"
IF EXISTS(SELECT * FROM
sys.indexes
WHERE name = 'TempIndexWI00334606Rate')
BEGIN
	DROP INDEX TempIndexWI00334606Applicability ON RefCusApplicability
	DROP INDEX TempIndexWI00334606Rate ON RefCusRate
END
CREATE NONCLUSTERED INDEX TempIndexWI00334606Applicability ON dbo.RefCusApplicability (ZZT_ZZ2_Rate) INCLUDE (ZZT_ZZA_TradeGroup)
CREATE NONCLUSTERED INDEX TempIndexWI00334606Rate ON RefCusRate (ZZ2_ZZZ_NKDataGrouping) INCLUDE (ZZ2_ZZS_Preference)";
			var sql = @"
DECLARE @incorrectPK TABLE
(
	PK UNIQUEIDENTIFIER,
	TablePrefix VARCHAR(3)
)
DECLARE @incorrectApplicability TABLE
(
	ApplicabilityPK UNIQUEIDENTIFIER,
	ParentPK UNIQUEIDENTIFIER,
	ParentPrefix VARCHAR(3)
)
DECLARE @gspTradeGroup TABLE
(
	TradeGroupPK UNIQUEIDENTIFIER
)
INSERT INTO @gspTradeGroup
SELECT ZZA_PK FROM RefCusTradeGroup WHERE ZZA_TradeGroup IN ('2005', '2020','2027')

INSERT INTO @incorrectApplicability
SELECT ZZT_PK, ZZT_ZZ2_Rate, 'ZZ2'
FROM RefCusApplicability INNER JOIN
RefCusRate ON ZZ2_PK = ZZT_ZZ2_Rate INNER JOIN
RefCusPreference ON ZZ2_ZZS_Preference = ZZS_PK
WHERE ZZ2_ZZZ_NKDataGrouping = 'EUN' AND ((ZZS_Preference LIKE '3%' AND ZZT_ZZA_TradeGroup IN (SELECT TradeGroupPK FROM @gspTradeGroup)) OR (ZZS_Preference LIKE '2%' AND ZZT_ZZA_TradeGroup NOT IN (SELECT TradeGroupPK FROM @gspTradeGroup)))

INSERT INTO @incorrectApplicability
SELECT ZZT_PK, ZZT_ZX1_Conditions, 'ZX1'
FROM RefCusApplicability INNER JOIN
RefCusCondition ON ZX1_PK = ZZT_ZX1_Conditions INNER JOIN
RefCusPreference ON ZX1_ZZS_Preference = ZZS_PK
WHERE ZX1_ZZZ_NKDataGrouping = 'EUN' AND ((ZZS_Preference LIKE '3%' AND ZZT_ZZA_TradeGroup IN (SELECT TradeGroupPK FROM @gspTradeGroup)) OR (ZZS_Preference LIKE '2%' AND ZZT_ZZA_TradeGroup NOT IN (SELECT TradeGroupPK FROM @gspTradeGroup)))

DELETE a FROM RefCusExcludedTradeGroup a
JOIN @incorrectApplicability ON ApplicabilityPK = ZZC_ZZT_Applicability
DELETE a FROM RefCusApplicability a
JOIN @incorrectApplicability t ON a.ZZT_PK = t.ApplicabilityPK

INSERT INTO @incorrectPK
SELECT ZZ2_PK, 'ZZ2' FROM RefCusRate 
LEFT JOIN RefCusApplicability ON ZZ2_PK = ZZT_ZZ2_Rate
WHERE ZZ2_PK IN (SELECT DISTINCT ParentPK FROM @incorrectApplicability WHERE ParentPrefix = 'ZZ2') AND ZZT_PK IS NULL

INSERT INTO @incorrectPK
SELECT ZX1_PK, 'ZX1' FROM RefCusCondition 
LEFT JOIN RefCusApplicability ON ZX1_PK = ZZT_ZX1_Conditions
WHERE ZX1_PK IN (SELECT DISTINCT ParentPK FROM @incorrectApplicability WHERE ParentPrefix = 'ZX1') AND ZZT_PK IS NULL

DELETE a FROM RefCusRateUOM a JOIN @incorrectPK ON ZXG_ZZ2_Rate = PK AND TablePrefix = 'ZZ2'
DELETE a FROM RefCusRate a
JOIN @incorrectPK ON a.ZZ2_PK = PK AND TablePrefix = 'ZZ2'

DELETE a FROM RefCusConditionValue a JOIN @incorrectPK ON  a.ZX3_ZX1_Condition = PK AND TablePrefix = 'ZX1'
DELETE a FROM RefCusCondition a
JOIN @incorrectPK ON a.ZX1_PK = PK AND TablePrefix = 'ZX1'

DROP INDEX TempIndexWI00334606Applicability ON RefCusApplicability
DROP INDEX TempIndexWI00334606Rate ON RefCusRate
";

			using (var cmd = trans.Connection?.CreateCommand())
			{
				cmd.Connection = trans.Connection;
				cmd.Transaction = trans;
				cmd.CommandText = createIndexSql;
				cmd.CommandTimeout = 300;
				cmd.ExecuteNonQuery();
			}

			using (var cmd = trans.Connection?.CreateCommand())
			{
				cmd.Connection = trans.Connection;
				cmd.Transaction = trans;
				cmd.CommandText = sql;
				cmd.CommandTimeout = 300;
				cmd.ExecuteNonQuery();
			}
		}
	}
}
