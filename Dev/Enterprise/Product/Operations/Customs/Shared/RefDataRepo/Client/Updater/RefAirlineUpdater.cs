using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using CargoWise.RefDataRepo.Ent.Client.DataStorage;
using CargoWise.RefDbRepo.Client.Common;
using CargoWise.RefDbRepo.Client.SqlServer;
using CargoWise.RefDbRepo.Common.Contract_0_9;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.RefDataRepo.Ent.Client
{
	public class RefAirlineUpdater : DataSetUpdater<RefAirline, IRefAirline>
	{
		public RefAirlineUpdater(IServerProxy proxy, IDBHelper dbHelper, IRefVersionControlManager versionControlManager, ISQLBuilder sQLBuilder, IValueConverter valueConverter)
		   : base(proxy, dbHelper, versionControlManager, sQLBuilder, valueConverter)
		{
		}

		public override IEnumerable<Tuple<Type, Type>> GetTypeByInsertOrder()
		{
			yield return Tuple.Create(typeof(IRefAirline), typeof(RefAirline));
			yield return Tuple.Create(typeof(IStmNote), typeof(StmNote));
		}

		public override int UpdaterVersion => 5;

		protected override string GetMergeSqlTextCore(IDbTransaction transaction, IDictionary<Type, ForeignKeyRelationship[]> fks, string dataSetName)
		{
			var result = new StringBuilder();
			var uniqueConstraintIndexColumnsArray = DBHelper.GetAllUniqueIndexes<IRefAirline>(transaction);
			var mergeConditions = SharedSQLBuilder.CreateUniqueConstraintColumnsCondition(uniqueConstraintIndexColumnsArray, (NoResString)"t", (NoResString)"s");

			result.AppendLine(GetMergeSqlForInactive(dataSetName));
			result.AppendLine(GetPreProcessSql(dataSetName, mergeConditions));

			result.AppendLine(SharedDeleteSQLBuilder.SaveDeleteRecord<IRefAirline>(sQLBuilder, dataSetName, fks, uniqueConstraintIndexColumnsArray));
			result.AppendLine(SQLBuilder.UpdateReferenceFKStatement<IRefAirline, IStmNote>(dataSetName, uniqueConstraintIndexColumnsArray));

			result.AppendLine(MergeSQLBuilder.CreateMergeOutputTableDeclarationSql(ParentPKChangesTable));

			result.AppendLine(SharedReplaceSQLBuilder.CreateSqlToHandleNonUpdatableRecords<IRefAirline>(sQLBuilder, dataSetName, uniqueConstraintIndexColumnsArray, ParentPKChangesTable));

			result.AppendLine(sQLBuilder.CreateMergeSql<IRefAirline>(dataSetName, uniqueConstraintIndexColumnsArray, ParentPKChangesTable, false));
			var fkRelationshipStmNote = fks[typeof(IRefAirline)].Where(o => o.Table == typeof(IStmNote));
			result.AppendLine(SharedDeleteSQLBuilder.SaveDeleteOrUpdateDependentRecord<IRefAirline>(sQLBuilder, fks));
			result.AppendLine(SharedMergeSQLBuilder.CreateSourceTableForMergeSql<IStmNote, IRefAirline>(sQLBuilder, dataSetName, MergeSourceName, ParentPKChangesTable, ParentTempCTEName, fkRelationshipStmNote));
			result.AppendLine(ReplaceSQLBuilder.CreateReplaceSqlForDependentTableWhenParentMerges<IStmNote>(sQLBuilder, dataSetName, fkRelationshipStmNote, ParentTempCTEName, uniqueConstraintIndexColumnsArray, DBHelper.GetAllUniqueIndexes<IStmNote>(transaction), fks, MergeSourceName));

			return result.ToString();
		}

		string GetPreProcessSql(string dataSetName, string mergeConditions)
		{
			var result = new StringBuilder();
			var tableName = SharedSQLBuilder.GetTableName(typeof(IRefAirline));
			var tempTableName = SQLBuilder.GetTemporaryTableName<IRefAirline>(dataSetName);
			var threeColumnIndexCondition = "t.RM_EagleAddedAirlinePrefixOrAccountingCode = s.RM_EagleAddedAirlinePrefixOrAccountingCode AND t.RM_ThreeLetterCode = s.RM_ThreeLetterCode AND t.RM_AirlineName1 = s.RM_AirlineName1";
			result.AppendLine(FormattableString.Invariant($@"
IF Object_Id('tempdb..#TempMatchedRefAirline', 'U') IS NOT NULL
DROP TABLE #TempMatchedRefAirline;

CREATE TABLE #TempMatchedRefAirline
(
	TargetPK UNIQUEIDENTIFIER NOT NULL,
	SourcePK UNIQUEIDENTIFIER NOT NULL
);

INSERT INTO #TempMatchedRefAirline
	SELECT t.RM_PK, s.RM_PK FROM {tableName} t JOIN {tempTableName} s
	ON {mergeConditions} WHERE t.RM_IsUpdatable = 1;

/****** we do not activate an inactive airline ******/
DELETE s FROM {tempTableName} s	JOIN {tableName} t
	ON {threeColumnIndexCondition}
	WHERE t.RM_IsActive = 0 AND s.RM_PK IN (SELECT SourcePK FROM #TempMatchedRefAirline GROUP BY SourcePK HAVING COUNT(SourcePK) = 1);

/****** we change AirlineName1 of inactive airlines to resolve conflict, then we delete them from the #TempMatchedRefAirline table ******/
CREATE TABLE #TempInactiveAirlineChangeName(RM_PK UNIQUEIDENTIFIER NOT NULL);
INSERT INTO #TempInactiveAirlineChangeName
	SELECT t.RM_PK FROM {tableName} t JOIN {tempTableName} s
	ON {threeColumnIndexCondition} WHERE t.RM_IsActive = 0 AND s.RM_PK IN (SELECT SourcePK FROM #TempMatchedRefAirline GROUP BY SourcePK HAVING COUNT(SourcePK) > 1);
UPDATE t SET t.RM_AirlineName1 = LEFT(t.RM_AirlineName1, 30) + CONVERT(varchar(10), GETDATE(), 23) FROM {tableName} t
	WHERE t.RM_PK IN (SELECT RM_PK FROM #TempInactiveAirlineChangeName);
DELETE FROM #TempMatchedRefAirline WHERE TargetPK IN (SELECT RM_PK FROM #TempInactiveAirlineChangeName);

/****** resolve conflict for two-client-match-on-server cases ******/
UPDATE t SET t.RM_IsActive = 0 FROM {tableName} t JOIN {tempTableName} s
	ON t.RM_EagleAddedAirlinePrefixOrAccountingCode <> s.RM_EagleAddedAirlinePrefixOrAccountingCode AND t.RM_ThreeLetterCode = s.RM_ThreeLetterCode AND t.RM_ThreeLetterCode <> ''
	WHERE s.RM_PK IN (SELECT SourcePK FROM #TempMatchedRefAirline GROUP BY SourcePK HAVING COUNT(SourcePK) > 1) AND t.RM_IsUpdatable = 1;

/****** resolve conflict for one-client-match-two-server cases ******/
UPDATE t SET t.RM_ThreeLetterCode = s.RM_ThreeLetterCode FROM {tableName} t JOIN {tempTableName} s
	ON t.RM_EagleAddedAirlinePrefixOrAccountingCode = s.RM_EagleAddedAirlinePrefixOrAccountingCode
	WHERE t.RM_PK In (SELECT TargetPK FROM #TempMatchedRefAirline GROUP BY TargetPK HAVING COUNT(TargetPK) > 1) AND t.RM_IsUpdatable = 1;

DROP TABLE #TempMatchedRefAirline;
DROP TABLE #TempInactiveAirlineChangeName;
"));
			return result.ToString();
		}

		string GetMergeSqlForInactive(string dataSetName)
		{
			var result = new StringBuilder();
			var tableName = SharedSQLBuilder.GetTableName(typeof(IRefAirline));
			var tempTableName = SQLBuilder.GetTemporaryTableName<IRefAirline>(dataSetName);

			result.AppendLine(FormattableString.Invariant($@"
UPDATE t SET t.RM_IsActive = 0 FROM {tableName} t JOIN {tempTableName} s
	ON t.RM_EagleAddedAirlinePrefixOrAccountingCode = s.RM_EagleAddedAirlinePrefixOrAccountingCode AND t.RM_ThreeLetterCode = s.RM_ThreeLetterCode AND t.RM_AirlineName1 = s.RM_AirlineName1
	WHERE t.RM_IsActive = 1 AND t.RM_IsUpdatable = 1 AND s.RM_IsActive = 0;
DELETE s FROM {tempTableName} s WHERE s.RM_IsActive = 0;
"));
			return result.ToString();
		}

		const string ParentPKChangesTable = "@ParentPKChangesTable";
		const string MergeSourceName = "TempChildTableCTE";
		const string ParentTempCTEName = "TempParentTableCTE";
	}
}
