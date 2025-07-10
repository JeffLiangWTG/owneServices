using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.Client.Common;
using CargoWise.RefDbRepo.Client.SqlServer;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public class RefCusCodeListAttributeNameUpdaterInfo : IDataSetUpdaterInfo
	{
		public string GetMergeSqlText(ISQLBuilder sQLBuilder, IDictionary<Type, ForeignKeyRelationship[]> fks, ISchemaInfo schemaInfo)
		{
			var result = new StringBuilder();
			result.AppendLine(ReplaceSQLBuilder.CreateReplaceSql<IRefCusCodeListAttributeName>(sQLBuilder, string.Empty, fks, schemaInfo.GetAllUniqueIndexes<IRefCusCodeListAttributeName>(null)));
			result.AppendLine(ReplaceSQLBuilder.CreateReplaceSqlForDependentTable<IRefCusCodeListAttributeNameLanguage>(string.Empty));
			return result.ToString();
		}

		public string GetOverriddenPrepareTemporaryTablesScripts()
		{
			return null;
		}

		public IEnumerable<Tuple<Type, Type>> GetReferenceTypeByInsertOrder()
		{
			return Enumerable.Empty<Tuple<Type, Type>>();
		}

		public Type GetStorageType()
		{
			return typeof(IRefCusCodeListAttributeName);
		}

		public IEnumerable<Tuple<Type, Type>> GetTypeByInsertOrder()
		{
			yield return Tuple.Create(typeof(IRefCusCodeListAttributeName), typeof(IRefCusCodeListAttributeName));
			yield return Tuple.Create(typeof(IRefCusCodeListAttributeNameLanguage), typeof(IRefCusCodeListAttributeNameLanguage));
		}
	}
}
