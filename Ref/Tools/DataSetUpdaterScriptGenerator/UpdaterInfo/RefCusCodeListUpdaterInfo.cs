using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.Client.Common;
using CargoWise.RefDbRepo.Client.SqlServer;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public class RefCusCodeListUpdaterInfo<TStorage> : IDataSetUpdaterInfo
		where TStorage : IDataSetStorage
	{
		public RefCusCodeListUpdaterInfo()
		{
		}

		public string GetMergeSqlText(ISQLBuilder sQLBuilder, IDictionary<Type, ForeignKeyRelationship[]> fks, ISchemaInfo schemaInfo)
		{
			var result = new StringBuilder();
			result.AppendLine(ReplaceSQLBuilder.CreateReplaceSql<IRefCusCodeList>(sQLBuilder, string.Empty, fks, schemaInfo.GetAllUniqueIndexes<IRefCusCodeList>(null)));
			result.AppendLine(ReplaceSQLBuilder.CreateReplaceSqlForDependentTable<IRefCusCodeListAttribute>(string.Empty));
			result.AppendLine(ReplaceSQLBuilder.CreateReplaceSqlForDependentTable<IRefCusCodeListLanguage>(string.Empty));
			result.AppendLine(ReplaceSQLBuilder.CreateReplaceSqlForDependentTable<IRefCusCodeOrAttributeTransportMode>(string.Empty));
			return result.ToString();
		}

		public IEnumerable<Tuple<Type, Type>> GetReferenceTypeByInsertOrder()
		{
			return Enumerable.Empty<Tuple<Type, Type>>();
		}

		public IEnumerable<Tuple<Type, Type>> GetTypeByInsertOrder()
		{
			yield return Tuple.Create(typeof(IRefCusCodeList), typeof(IRefCusCodeList));
			yield return Tuple.Create(typeof(IRefCusCodeListAttribute), typeof(IRefCusCodeListAttribute));
			yield return Tuple.Create(typeof(IRefCusCodeListLanguage), typeof(IRefCusCodeListLanguage));
			yield return Tuple.Create(typeof(IRefCusCodeOrAttributeTransportMode), typeof(IRefCusCodeOrAttributeTransportMode));
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
