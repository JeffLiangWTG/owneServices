using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.Client.Common;
using CargoWise.RefDbRepo.Client.SqlServer;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public class RefCusRulingUpdaterInfo<TStorage> : IDataSetUpdaterInfo
		where TStorage : IDataSetStorage
	{
		public RefCusRulingUpdaterInfo()
		{
		}

		public string GetMergeSqlText(ISQLBuilder sQLBuilder, IDictionary<Type, ForeignKeyRelationship[]> fks, ISchemaInfo schemaInfo)
		{
			var result = new StringBuilder();
			result.AppendLine(ReplaceSQLBuilder.CreateReplaceSql<IRefCusRuling>(sQLBuilder, string.Empty, fks, schemaInfo.GetAllUniqueIndexes<IRefCusRuling>(null)));
			result.AppendLine(ReplaceSQLBuilder.CreateReplaceSqlForDependentTable<IRefCusRulingConfig>(string.Empty));
			return result.ToString();
		}

		public IEnumerable<Tuple<Type, Type>> GetReferenceTypeByInsertOrder()
		{
			return Enumerable.Empty<Tuple<Type, Type>>();
		}

		public IEnumerable<Tuple<Type, Type>> GetTypeByInsertOrder()
		{
			yield return Tuple.Create(typeof(IRefCusRuling), typeof(IRefCusRuling));
			yield return Tuple.Create(typeof(IRefCusRulingConfig), typeof(IRefCusRulingConfig));
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
