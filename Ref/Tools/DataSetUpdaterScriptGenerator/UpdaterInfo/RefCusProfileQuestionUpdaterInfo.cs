using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.Client.Common;
using CargoWise.RefDbRepo.Client.SqlServer;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public class RefCusProfileQuestionUpdaterInfo<TStorage> : IDataSetUpdaterInfo where TStorage : IDataSetStorage
	{
		public RefCusProfileQuestionUpdaterInfo()
		{
		}

		public string GetMergeSqlText(ISQLBuilder sQLBuilder, IDictionary<Type, ForeignKeyRelationship[]> fks, ISchemaInfo schemaInfo)
		{
			var uniqueIndexes = schemaInfo.GetAllUniqueIndexes<IRefCusProfileQuestion>(null);
			var result = new StringBuilder();
			result.AppendLine(SQLBuilder.UpdateReferenceFKStatement<IRefCusTariffType, IRefCusProfileType>(string.Empty, schemaInfo.GetAllUniqueIndexes<IRefCusTariffType>(null)));
			result.AppendLine(SQLBuilder.UpdateReferenceFKStatement<IRefCusProfileType, IRefCusProfileQuestion>(string.Empty, schemaInfo.GetAllUniqueIndexes<IRefCusProfileType>(null)));
			result.AppendLine(SharedDeleteSQLBuilder.SaveDeleteRecord<IRefCusProfileQuestion>(sQLBuilder, string.Empty, fks, uniqueIndexes));
			result.AppendLine(MergeSQLBuilder.CreateMergeSql<IRefCusProfileQuestion>(string.Empty, uniqueIndexes, QuestionPKChangesTable));
			result.AppendLine(SharedDeleteSQLBuilder.SaveDeleteOrUpdateDependentRecord<IRefCusProfileQuestion>(sQLBuilder, fks));

			var fkRelationship = fks[typeof(IRefCusProfileQuestion)].Where(o => o.Table == typeof(IRefCusProfileQuestionAnswerList));
			result.AppendLine(SharedMergeSQLBuilder.CreateSourceTableForMergeSql<IRefCusProfileQuestionAnswerList, IRefCusProfileQuestion>(sQLBuilder, string.Empty, QuestionAnswerListMergeSourceName, QuestionPKChangesTable, QuestionMergeSourceName, fkRelationship));
			result.AppendLine(ReplaceSQLBuilder.CreateReplaceSqlForDependentTableWhenParentMerges<IRefCusProfileQuestionAnswerList>(sQLBuilder, string.Empty, fkRelationship, QuestionMergeSourceName, uniqueIndexes, schemaInfo.GetAllUniqueIndexes<IRefCusProfileQuestionAnswerList>(null), fks, QuestionAnswerListMergeSourceName));
			result.AppendLine(ReplaceSQLBuilder.CreateReplaceSqlForDependentTable<IRefCusProfileQuestionAnswerListLanguage>(string.Empty));

			fkRelationship = fks[typeof(IRefCusProfileQuestion)].Where(o => o.Table == typeof(IRefCusProfileQuestionAttribute));
			result.AppendLine(SharedMergeSQLBuilder.CreateSourceTableForMergeSql<IRefCusProfileQuestionAttribute, IRefCusProfileQuestion>(sQLBuilder, string.Empty, QuestionAttributeMergeSourceName, QuestionPKChangesTable, QuestionMergeSourceName, fkRelationship));
			result.AppendLine(ReplaceSQLBuilder.CreateReplaceSqlForDependentTableWhenParentMerges<IRefCusProfileQuestionAttribute>(sQLBuilder, string.Empty, fkRelationship, QuestionMergeSourceName, uniqueIndexes, schemaInfo.GetAllUniqueIndexes<IRefCusProfileQuestionAttribute>(null), fks, QuestionAttributeMergeSourceName));

			fkRelationship = fks[typeof(IRefCusProfileQuestion)].Where(o => o.Table == typeof(IRefCusProfileQuestionLanguage));
			result.AppendLine(SharedMergeSQLBuilder.CreateSourceTableForMergeSql<IRefCusProfileQuestionLanguage, IRefCusProfileQuestion>(sQLBuilder, string.Empty, QuestionLanguageMergeSourceName, QuestionPKChangesTable, QuestionMergeSourceName, fkRelationship));
			result.AppendLine(ReplaceSQLBuilder.CreateReplaceSqlForDependentTableWhenParentMerges<IRefCusProfileQuestionLanguage>(sQLBuilder, string.Empty, fkRelationship, QuestionMergeSourceName, uniqueIndexes, schemaInfo.GetAllUniqueIndexes<IRefCusProfileQuestionLanguage>(null), fks, QuestionLanguageMergeSourceName));

			result.AppendLine(SharedDeleteSQLBuilder.Delete<IRefCusProfileQuestion>(sQLBuilder, fks));
			return result.ToString();
		}

		public IEnumerable<Tuple<Type, Type>> GetReferenceTypeByInsertOrder()
		{
			yield return Tuple.Create(typeof(IRefCusTariffType), typeof(IRefCusTariffType));
			yield return Tuple.Create(typeof(IRefCusProfileType), typeof(IRefCusProfileType));
		}

		public IEnumerable<Tuple<Type, Type>> GetTypeByInsertOrder()
		{
			yield return Tuple.Create(typeof(IRefCusProfileQuestion), typeof(IRefCusProfileQuestion));
			yield return Tuple.Create(typeof(IRefCusProfileQuestionAnswerList), typeof(IRefCusProfileQuestionAnswerList));
			yield return Tuple.Create(typeof(IRefCusProfileQuestionAnswerListLanguage), typeof(IRefCusProfileQuestionAnswerListLanguage));
			yield return Tuple.Create(typeof(IRefCusProfileQuestionAttribute), typeof(IRefCusProfileQuestionAttribute));
			yield return Tuple.Create(typeof(IRefCusProfileQuestionLanguage), typeof(IRefCusProfileQuestionLanguage));
		}

		public Type GetStorageType()
		{
			return typeof(TStorage);
		}

		public string GetOverriddenPrepareTemporaryTablesScripts()
		{
			return null;
		}

		const string QuestionPKChangesTable = "@QuestionPKChangesTable";
		const string QuestionMergeSourceName = "TempQuestionCTE";
		const string QuestionAnswerListMergeSourceName = "TempQuestionAnswerListCTE";
		const string QuestionAttributeMergeSourceName = "TempQuestionAttributeCTE";
		const string QuestionLanguageMergeSourceName = "TempQuestionLanguageCTE";
	}
}
