using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.Client.Common;
using CargoWise.RefDbRepo.Client.SqlServer;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public class OneTableDataSetUpdaterInfo<TStorage> : IDataSetUpdaterInfo
		where TStorage : IDataSetStorage
	{
		public OneTableDataSetUpdaterInfo(bool useReplace)
		{
			this.useReplace = useReplace;
		}
		bool useReplace;

		public string GetMergeSqlText(ISQLBuilder sQLBuilder, IDictionary<Type, ForeignKeyRelationship[]> fks, ISchemaInfo schemaInfo)
		{
			var uniqueIndexes = schemaInfo.GetAllUniqueIndexes<TStorage>(null);
			if (useReplace)
			{
				return ReplaceSQLBuilder.CreateReplaceSql<TStorage>(sQLBuilder, string.Empty, fks, uniqueIndexes);
			}
			var result = new StringBuilder();
			result.AppendLine(SharedDeleteSQLBuilder.SaveDeleteRecord<TStorage>(sQLBuilder, string.Empty, fks, uniqueIndexes));
			result.AppendLine(MergeSQLBuilder.CreateMergeSql<TStorage>(string.Empty, uniqueIndexes));
			result.AppendLine(SharedDeleteSQLBuilder.SaveDeleteOrUpdateDependentRecord<TStorage>(sQLBuilder, fks));
			result.AppendLine(SharedDeleteSQLBuilder.Delete<TStorage>(sQLBuilder, fks));
			return result.ToString();
		}

		public IEnumerable<Tuple<Type, Type>> GetReferenceTypeByInsertOrder()
		{
			return Enumerable.Empty<Tuple<Type,Type>>();
		}

		public IEnumerable<Tuple<Type, Type>> GetTypeByInsertOrder()
		{
			yield return Tuple.Create(typeof(TStorage), typeof(TStorage));
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
