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
	public class RefShippingLineUpdater : DataSetUpdater<RefShippingLine, IRefShippingLine>
	{
		public RefShippingLineUpdater(IServerProxy proxy, IDBHelper dbHelper, IRefVersionControlManager versionControlManager)
			: base(proxy, dbHelper, versionControlManager, new SqlServerSQLBuilder(), new ValueConverter())
		{
		}

		public override int UpdaterVersion => 11;

		public override IEnumerable<Tuple<Type, Type>> GetTypeByInsertOrder()
		{
			yield return Tuple.Create(typeof(IRefShippingLine), typeof(RefShippingLine));
			yield return Tuple.Create(typeof(IRefShippingLineMessagingRequirement), typeof(RefShippingLineMessagingRequirement));
			yield return Tuple.Create(typeof(IRefShippingLineEBLProvider), typeof(RefShippingLineEBLProvider));
		}

		protected override string GetMergeSqlTextCore(IDbTransaction transaction, IDictionary<Type, ForeignKeyRelationship[]> fks, string dataSetName)
		{
			var result = new StringBuilder();
			result.AppendLine(GetPreProcessSql(transaction, dataSetName));
			result.AppendLine(ReplaceSQLBuilder.CreateReplaceSql<IRefShippingLine>(sQLBuilder, dataSetName, fks, DBHelper.GetAllUniqueIndexes<IRefShippingLine>(transaction)));
			result.AppendLine(ReplaceSQLBuilder.CreateReplaceSqlForDependentTable<IRefShippingLineMessagingRequirement>(dataSetName));
			result.AppendLine(ReplaceSQLBuilder.CreateReplaceSqlForDependentTable<IRefShippingLineEBLProvider>(dataSetName));
			result.AppendLine(RestoreOrgLinkSql(dataSetName));
			return result.ToString();
		}

		string GetPreProcessSql(IDbTransaction transaction, string dataSetName)
		{
			var result = new StringBuilder();
			var tableName = SharedSQLBuilder.GetTableName(typeof(IRefShippingLine));
			var tempTableName = SQLBuilder.GetTemporaryTableName<IRefShippingLine>(dataSetName);
			var uniqueConstraintIndexColumnsArray = DBHelper.GetAllUniqueIndexes<IRefShippingLine>(transaction);
			var mergeConditions = SharedSQLBuilder.CreateUniqueConstraintColumnsCondition(uniqueConstraintIndexColumnsArray, (NoResString)"t", (NoResString)"s");
			result.AppendLine(FormattableString.Invariant($@"
IF Object_Id('tempdb..#TempOrgShippingLineLink', 'U') IS NOT NULL
DROP TABLE #TempOrgShippingLineLink;

SELECT {nameof(IOrgHeader.OH_PK)} OrgPK,
	t.{nameof(IRefShippingLine.RSL_CargoWiseOneCode)} CargoWiseOneCode,
	t.{nameof(IRefShippingLine.RSL_StandardCarrierAlphaCode)} StandardCarrierAlphaCode,
	t.{nameof(IRefShippingLine.RSL_CarrierName)} CarrierName
INTO #TempOrgShippingLineLink
FROM {SharedSQLBuilder.GetTableName<IOrgHeader>()}
JOIN {tableName} t ON {nameof(IOrgHeader.OH_RSL_ShippingLine)} = {nameof(IRefShippingLine.RSL_PK)}
JOIN {tempTableName} s ON {mergeConditions}
"));
			return result.ToString();
		}

		string RestoreOrgLinkSql(string dataSetName)
		{
			var result = new StringBuilder();
			var tempTableName = SQLBuilder.GetTemporaryTableName<IRefShippingLine>(dataSetName);
			var systemColumnsUpdateStatement = SharedSQLBuilder.GetUpdateSystemLastEditTimeAndUserStatement(typeof(IOrgHeader));
			result.AppendLine(FormattableString.Invariant($@"
DELETE FROM {tempTableName} WHERE Deleted = 1;

UPDATE org SET {nameof(IOrgHeader.OH_RSL_ShippingLine)} = {nameof(IRefShippingLine.RSL_PK)}, {systemColumnsUpdateStatement}
FROM {SharedSQLBuilder.GetTableName<IOrgHeader>()} org
JOIN #TempOrgShippingLineLink ON {nameof(IOrgHeader.OH_PK)} = OrgPK
JOIN {tempTableName} ON {nameof(IRefShippingLine.RSL_StandardCarrierAlphaCode)} = StandardCarrierAlphaCode
AND {nameof(IRefShippingLine.RSL_StandardCarrierAlphaCode)} <> ''
WHERE {nameof(IOrgHeader.OH_RSL_ShippingLine)} IS NULL

DELETE entity FROM {tempTableName} entity
JOIN #TempOrgShippingLineLink ON {nameof(IRefShippingLine.RSL_StandardCarrierAlphaCode)} = StandardCarrierAlphaCode
AND {nameof(IRefShippingLine.RSL_StandardCarrierAlphaCode)} <> ''

UPDATE org SET {nameof(IOrgHeader.OH_RSL_ShippingLine)} = {nameof(IRefShippingLine.RSL_PK)}, {systemColumnsUpdateStatement}
FROM {SharedSQLBuilder.GetTableName<IOrgHeader>()} org
JOIN #TempOrgShippingLineLink ON {nameof(IOrgHeader.OH_PK)} = OrgPK
JOIN {tempTableName} ON {nameof(IRefShippingLine.RSL_CarrierName)} = CarrierName
WHERE {nameof(IOrgHeader.OH_RSL_ShippingLine)} IS NULL

DELETE entity FROM {tempTableName} entity
JOIN #TempOrgShippingLineLink ON {nameof(IRefShippingLine.RSL_CarrierName)} = CarrierName

UPDATE org SET {nameof(IOrgHeader.OH_RSL_ShippingLine)} = {nameof(IRefShippingLine.RSL_PK)}, {systemColumnsUpdateStatement}
FROM {SharedSQLBuilder.GetTableName<IOrgHeader>()} org
JOIN #TempOrgShippingLineLink ON {nameof(IOrgHeader.OH_PK)} = OrgPK
JOIN {tempTableName} ON {nameof(IRefShippingLine.RSL_CargoWiseOneCode)} = CargoWiseOneCode
AND {nameof(IRefShippingLine.RSL_IsActive)} = 1
WHERE {nameof(IOrgHeader.OH_RSL_ShippingLine)} IS NULL
"));
			return result.ToString();
		}
	}
}
