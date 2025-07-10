using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using CargoWise.RefDataRepo.Ent.Client.DataStorage;
using CargoWise.RefDbRepo.Client.Common;
using CargoWise.RefDbRepo.Client.SqlServer;
using CargoWise.RefDbRepo.Common.Contract_0_9;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.RefDataRepo.Ent.Client
{
	public class RefCurrencyUpdater : DataSetUpdater<RefCurrency, IRefCurrency>
	{
		public RefCurrencyUpdater(IServerProxy proxy, IDBHelper dbHelper, IRefVersionControlManager versionControlManager)
			: base(proxy, dbHelper, versionControlManager, new SqlServerSQLBuilder(), new ValueConverter())
		{
		}

		public override int UpdaterVersion => 2;

		public override IEnumerable<Tuple<Type, Type>> GetTypeByInsertOrder()
		{
			yield return Tuple.Create(typeof(IRefCurrency), typeof(RefCurrency));
			yield return Tuple.Create(typeof(IRefLanguageText), typeof(RefLanguageText));
		}

		protected override string GetMergeSqlTextCore(IDbTransaction transaction, IDictionary<Type, ForeignKeyRelationship[]> fks, string dataSetName)
		{
			//DO NOT USE Merge statement for update of RefCurrency as this causes performance issue due to link to AccTransactionHeader
			var result = new StringBuilder();
			var uniqueConstraintIndexColumnsArray = DBHelper.GetAllUniqueIndexes<IRefCurrency>(transaction);
			result.AppendLine(SharedDeleteSQLBuilder.SaveDeleteRecord<IRefCurrency>(sQLBuilder, dataSetName, fks, uniqueConstraintIndexColumnsArray));
			result.AppendLine(MergeSQLBuilder.CreateMergeOutputTableDeclarationSql(ParentPKChangesTable));
			result.AppendLine(MergeSQLBuilder.CreateUpdateSql<IRefCurrency>(dataSetName, uniqueConstraintIndexColumnsArray, $"OUTPUT inserted.RX_PK, s.RX_PK INTO {ParentPKChangesTable}"));
			result.AppendLine(MergeSQLBuilder.CreateInsertSql<IRefCurrency>(dataSetName, uniqueConstraintIndexColumnsArray, $"OUTPUT inserted.RX_PK, null INTO {ParentPKChangesTable}"));
			result.AppendLine(RefLanguageTextSQLBuilder.GetMergeSql<IRefCurrency, RefCurrency>(
				sQLBuilder,
				dataSetName,
				fks,
				DBHelper.GetAllUniqueIndexes<IRefLanguageText>(transaction),
				MergeSourceName,
				ParentPKChangesTable,
				ChildPKChangesTable,
				ParentTempCTEName));
			result.AppendLine(SharedDeleteSQLBuilder.SaveDeleteOrUpdateDependentRecord<IRefCurrency>(sQLBuilder, fks));
			result.AppendLine(SharedDeleteSQLBuilder.Delete<IRefCurrency>(sQLBuilder, fks));
			result.AppendLine(UpdateColumnsIfHaveChanges(dataSetName, uniqueConstraintIndexColumnsArray));
			return result.ToString();
		}

		string UpdateColumnsIfHaveChanges(string dataSetName, IEnumerable<IndexColumn[]> uniqueConstraintIndexColumnsArray)
		{
			var tempTableName = SQLBuilder.GetTemporaryTableName<IRefCurrency>(dataSetName);
			var pk = SharedSQLBuilder.GetPKColumn<IRefCurrency>();
			var subUnitRatioColumn = nameof(IRefCurrency.RX_SubUnitRatio);

			var mergeSearchConditions = SharedSQLBuilder.CreateUniqueConstraintColumnsCondition(uniqueConstraintIndexColumnsArray, (NoResString)"c", (NoResString)"t");

			return $@"DECLARE @TempCopyTable TABLE ({pk} UNIQUEIDENTIFIER, {subUnitRatioColumn} INT)

INSERT INTO @TempCopyTable({pk}, {subUnitRatioColumn})
SELECT c.{pk}, t.{subUnitRatioColumn}
FROM {SharedSQLBuilder.GetTableName<IRefCurrency>()} c
JOIN {tempTableName} t ON {mergeSearchConditions} AND Deleted = 0
WHERE
t.{subUnitRatioColumn} <> c.{subUnitRatioColumn}

IF EXISTS (SELECT * FROM @TempCopyTable)
BEGIN
	UPDATE c SET c.{subUnitRatioColumn} = t.{subUnitRatioColumn}
	FROM {SharedSQLBuilder.GetTableName<IRefCurrency>()} c
	JOIN @TempCopyTable t ON t.{pk} = c.{pk}
END
";
		}

		const string ParentPKChangesTable = "@ParentPKChangesTable";
		const string MergeSourceName = "TempChildTableCTE";
		const string ChildPKChangesTable = "@ChildPKChangesTable";
		const string ParentTempCTEName = "TempParentTableCTE";
	}
}
