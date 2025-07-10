using System;
using System.Collections.Generic;
using System.Text;
using CargoWise.RefDbRepo.Client.Common;
using CargoWise.RefDbRepo.Client.SqlServer;
namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public class RefCusProfileQuestionPathwayUpdaterInfo<TStorage> : IDataSetUpdaterInfo
		where TStorage : IDataSetStorage
	{
		public RefCusProfileQuestionPathwayUpdaterInfo()
		{
		}
		public string GetMergeSqlText(ISQLBuilder sQLBuilder, IDictionary<Type, ForeignKeyRelationship[]> fks, ISchemaInfo schemaInfo)
		{
			var uniqueIndexes = schemaInfo.GetAllUniqueIndexes<TStorage>(null);
			var result = new StringBuilder();
			result.AppendLine(SQLBuilder.UpdateReferenceFKStatement<IRefCusTariffType, IRefCusProfileType>(string.Empty, schemaInfo.GetAllUniqueIndexes<IRefCusTariffType>(null)));
			result.AppendLine(SQLBuilder.UpdateReferenceFKStatement<IRefCusProfileType, IRefCusProfileQuestion>(string.Empty, schemaInfo.GetAllUniqueIndexes<IRefCusProfileType>(null)));
			result.AppendLine(SQLBuilder.UpdateReferenceFKStatement<IRefCusProfileQuestion, IRefCusProfileQuestionPathway>(string.Empty, schemaInfo.GetAllUniqueIndexes<IRefCusProfileQuestion>(null)));
			result.AppendLine(SharedDeleteSQLBuilder.SaveDeleteRecord<IRefCusProfileQuestionPathway>(sQLBuilder, string.Empty, fks, uniqueIndexes));
			result.AppendLine(MergeSQLBuilder.CreateMergeSql<IRefCusProfileQuestionPathway>(string.Empty, uniqueIndexes));
			result.AppendLine(SharedDeleteSQLBuilder.SaveDeleteOrUpdateDependentRecord<IRefCusProfileQuestionPathway>(sQLBuilder, fks));
			result.AppendLine(SharedDeleteSQLBuilder.Delete<IRefCusProfileQuestionPathway>(sQLBuilder, fks));
			return result.ToString();
		}
		public IEnumerable<Tuple<Type, Type>> GetReferenceTypeByInsertOrder()
		{
			yield return Tuple.Create(typeof(IRefCusTariffType), typeof(IRefCusTariffType));
			yield return Tuple.Create(typeof(IRefCusProfileType), typeof(IRefCusProfileType));
			yield return Tuple.Create(typeof(IRefCusProfileQuestion), typeof(IRefCusProfileQuestion));
		}
		public IEnumerable<Tuple<Type, Type>> GetTypeByInsertOrder()
		{
			yield return Tuple.Create(typeof(IRefCusProfileQuestionPathway), typeof(IRefCusProfileQuestionPathway));
		}
		public Type GetStorageType()
		{
			return typeof(TStorage);
		}
		public string GetOverriddenPrepareTemporaryTablesScripts()
		{
			return null;
		}
	}
}
