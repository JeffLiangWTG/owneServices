using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.Client.Common;
using CargoWise.RefDbRepo.Client.SqlServer;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public class RefCusPreferenceUpdaterInfo<TStorage> : IDataSetUpdaterInfo
		where TStorage : IDataSetStorage
	{
		public string GetMergeSqlText(ISQLBuilder sQLBuilder, IDictionary<Type, ForeignKeyRelationship[]> fks, ISchemaInfo schemaInfo)
		{
			var result = new StringBuilder();
			var refCusPreferenceUniqueIndexes = schemaInfo.GetAllUniqueIndexes<IRefCusPreference>(null);
			var mergeSearchConditions = SharedSQLBuilder.CreateUniqueConstraintColumnsCondition(refCusPreferenceUniqueIndexes, "t", "s");

			result.AppendLine(SharedDeleteSQLBuilder.SaveDeleteRecord<IRefCusPreference>(sQLBuilder, string.Empty, fks, refCusPreferenceUniqueIndexes, false));
			result.AppendLine(MergeSQLBuilder.CreateMergeSql<IRefCusPreference>(string.Empty, refCusPreferenceUniqueIndexes));
			result.AppendLine(SaveDeletedRefCusPreferenceRecords(mergeSearchConditions));
			result.AppendLine(SaveDeleteOrUpdateDependencies(sQLBuilder, fks));
			result.AppendLine(SharedDeleteSQLBuilder.DeleteDependentRecords<IRefCusPreference>(sQLBuilder, fks));
			result.AppendLine(CultureInfo.InvariantCulture, $@"
DELETE t
FROM {SharedSQLBuilder.GetTableName<IRefCusPreference>()} AS t
JOIN {deletedPreferenceTempTableName} AS s ON t.{preferencePKColumn} = s.{preferencePKColumn};");

			result.AppendLine(SQLBuilder.UpdateReferenceFKStatement<IRefCusPreference, IRefCusCondition>(string.Empty, refCusPreferenceUniqueIndexes));
			result.AppendLine(SQLBuilder.UpdateReferenceFKStatement<IRefCusPreference, IRefCusPreferenceLanguage>(string.Empty, refCusPreferenceUniqueIndexes));
			result.AppendLine(SQLBuilder.UpdateReferenceFKStatement<IRefCusConditionType, IRefCusCondition>(string.Empty, schemaInfo.GetAllUniqueIndexes<IRefCusConditionType>(null)));
			result.AppendLine(SQLBuilder.UpdateReferenceFKStatement<IRefCusConditionValueType, IRefCusConditionValue>(string.Empty, schemaInfo.GetAllUniqueIndexes<IRefCusConditionValueType>(null)));
			result.AppendLine(SQLBuilder.UpdateReferenceFKStatement<IRefCusTradeGroup, IRefCusApplicability>(string.Empty, schemaInfo.GetAllUniqueIndexes<IRefCusTradeGroup>(null)));
			result.AppendLine(SQLBuilder.UpdateReferenceFKStatement<IRefCusTradeGroup, IRefCusExcludedTradeGroup>(string.Empty, schemaInfo.GetAllUniqueIndexes<IRefCusTradeGroup>(null)));

			foreach (var type in GetTypeByInsertOrder().Skip(1))
			{
				result.AppendLine((string)typeof(ReplaceSQLBuilder).InvokeStaticGenericMethod(nameof(ReplaceSQLBuilder.CreateReplaceSqlForDependentTable), type.Item1, string.Empty));
			}
			result.AppendLine(CultureInfo.InvariantCulture, $"TRUNCATE TABLE {deletedPreferenceTempTableName}");
			return result.ToString();
		}

		static string SaveDeletedRefCusPreferenceRecords(string mergeSearchConditions)
		{
			return $@"
SELECT t.{preferencePKColumn}, Deleted into {deletedPreferenceTempTableName}
FROM {SharedSQLBuilder.GetTableName<IRefCusPreference>()} AS t
JOIN {preferenceTempTableName} AS s ON {mergeSearchConditions} AND Deleted = 1;";
		}

		static string SaveDeleteOrUpdateDependencies(ISQLBuilder sQLBuilder, IDictionary<Type, ForeignKeyRelationship[]> fks)
		{
			var result = new StringBuilder();
			var preferenceLanguageFk = new Dictionary<Type, ForeignKeyRelationship[]>();
			var fkRelationshipPreferenceLanguage = fks[typeof(IRefCusPreference)].FirstOrDefault(o => o.ReferencedTable == typeof(IRefCusPreference) && o.Table == typeof(IRefCusPreferenceLanguage));
			preferenceLanguageFk.Add(typeof(IRefCusPreference), new ForeignKeyRelationship[] { fkRelationshipPreferenceLanguage });
			result.AppendLine(SharedDeleteSQLBuilder.SaveDeleteOrUpdateDependentRecord<IRefCusPreference>(sQLBuilder, preferenceLanguageFk));

			var fkRelationshipPreferenceRate = fks[typeof(IRefCusPreference)].FirstOrDefault(o => o.ReferencedTable == typeof(IRefCusPreference) && o.Table == typeof(IRefCusRate));
			result.AppendLine(SaveDeleteOrUpdateRefCusRate(fkRelationshipPreferenceRate));

			var fkRelationshipCondition = fks[typeof(IRefCusPreference)].FirstOrDefault(o => o.ReferencedTable == typeof(IRefCusPreference) && o.Table == typeof(IRefCusCondition));
			result.AppendLine(SaveDeleteOrUpdateRefCusCondition(fkRelationshipCondition));
			result.AppendLine(SharedDeleteSQLBuilder.SaveDeleteOrUpdateDependentRecord<IRefCusCondition>(sQLBuilder, fks));
			return result.ToString();
		}

		static string SaveDeleteOrUpdateRefCusRate(ForeignKeyRelationship fk)
		{
			var rateTableName = SharedSQLBuilder.GetTableName<IRefCusRate>();
			return $@"UPDATE t SET {fk.Column} = NULL FROM {rateTableName} t
JOIN {deletedPreferenceTempTableName} ON {fk.ReferencedColumn} = {fk.Column};";
		}

		static string SaveDeleteOrUpdateRefCusCondition(ForeignKeyRelationship fk)
		{
			var deleteTableName = DeleteSQLBuilder.GetDeleteTableName<IRefCusCondition>();
			var pkColumn = SharedSQLBuilder.GetPKColumn<IRefCusCondition>();
			var tableName = SharedSQLBuilder.GetTableName<IRefCusCondition>();
			var parentDeleteTableName = DeleteSQLBuilder.GetDeleteTableName<IRefCusPreference>();

			return FormattableString.Invariant($@"
INSERT INTO {deleteTableName}
SELECT {pkColumn}
FROM {tableName}
JOIN {parentDeleteTableName} ON {fk.Column} = {fk.ReferencedColumn}
WHERE {nameof(IRefCusCondition.ZX1_ZZ1_Tariff)} IS NULL AND {nameof(IRefCusCondition.ZX1_ZZ5_Nomenclature)} IS NULL;

UPDATE t SET {fk.Column} = NULL FROM {tableName} t 
JOIN {deletedPreferenceTempTableName} ON {fk.ReferencedColumn} = {fk.Column}
WHERE t.{nameof(IRefCusCondition.ZX1_ZZ1_Tariff)} IS NOT NULL OR t.{nameof(IRefCusCondition.ZX1_ZZ5_Nomenclature)} IS NOT NULL;"); // SuppressCodeSmell Reason = SQL Statement";
		}

		public IEnumerable<Tuple<Type, Type>> GetReferenceTypeByInsertOrder()
		{
			yield return Tuple.Create(typeof(IRefCusConditionType), typeof(IRefCusConditionType));
			yield return Tuple.Create(typeof(IRefCusConditionValueType), typeof(IRefCusConditionValueType));
			yield return Tuple.Create(typeof(IRefCusTradeGroup), typeof(IRefCusTradeGroup));
		}

		public IEnumerable<Tuple<Type, Type>> GetTypeByInsertOrder()
		{
			yield return Tuple.Create(typeof(IRefCusPreference), typeof(IRefCusPreference));
			yield return Tuple.Create(typeof(IRefCusPreferenceLanguage), typeof(IRefCusPreferenceLanguage));
			yield return Tuple.Create(typeof(IRefCusCondition), typeof(IRefCusCondition));
			yield return Tuple.Create(typeof(IRefCusConditionValue), typeof(IRefCusConditionValue));
			yield return Tuple.Create(typeof(IRefCusConditionLanguage), typeof(IRefCusConditionLanguage));
			yield return Tuple.Create(typeof(IRefCusApplicability), typeof(IRefCusApplicability));
			yield return Tuple.Create(typeof(IRefCusExcludedTradeGroup), typeof(IRefCusExcludedTradeGroup));
		}

		public Type GetStorageType()
		{
			return typeof(TStorage);
		}

		public string GetOverriddenPrepareTemporaryTablesScripts()
		{
			return null;
		}

		static readonly string preferenceTempTableName = SQLBuilder.GetTemporaryTableName<IRefCusPreference>(string.Empty);
		static readonly string deletedPreferenceTempTableName = preferenceTempTableName + "_DELETE";
		static readonly string preferencePKColumn = SharedSQLBuilder.GetPKColumn<IRefCusPreference>();
	}
}
