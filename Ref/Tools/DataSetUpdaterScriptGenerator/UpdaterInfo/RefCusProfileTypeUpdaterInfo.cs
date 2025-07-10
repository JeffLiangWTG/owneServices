using System;
using System.Collections.Generic;
using System.Text;
using CargoWise.RefDbRepo.Client.Common;
using CargoWise.RefDbRepo.Client.SqlServer;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public class RefCusProfileTypeUpdaterInfo<TStorage> : IDataSetUpdaterInfo
		where TStorage : IDataSetStorage
	{
		public RefCusProfileTypeUpdaterInfo()
		{
		}

		public string GetMergeSqlText(ISQLBuilder sQLBuilder, IDictionary<Type, ForeignKeyRelationship[]> fks, ISchemaInfo schemaInfo)
		{
			var uniqueIndexes = schemaInfo.GetAllUniqueIndexes<TStorage>(null);
			var result = new StringBuilder();
			result.AppendLine(SQLBuilder.UpdateReferenceFKStatement<IRefCusTariffType, IRefCusProfileType>(string.Empty, schemaInfo.GetAllUniqueIndexes<IRefCusTariffType>(null)));
			result.AppendLine(SharedDeleteSQLBuilder.SaveDeleteRecord<IRefCusProfileType>(sQLBuilder, string.Empty, fks, uniqueIndexes));
			result.AppendLine(MergeSQLBuilder.CreateMergeSql<IRefCusProfileType>(string.Empty, uniqueIndexes));
			result.AppendLine(SharedDeleteSQLBuilder.SaveDeleteOrUpdateDependentRecord<IRefCusProfileType>(sQLBuilder, fks));
			result.AppendLine(SharedDeleteSQLBuilder.Delete<IRefCusProfileType>(sQLBuilder, fks));
			return result.ToString();
		}

		public IEnumerable<Tuple<Type, Type>> GetReferenceTypeByInsertOrder()
		{
			yield return Tuple.Create(typeof(IRefCusTariffType), typeof(IRefCusTariffType));
		}

		public IEnumerable<Tuple<Type, Type>> GetTypeByInsertOrder()
		{
			yield return Tuple.Create(typeof(IRefCusProfileType), typeof(IRefCusProfileType));
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
