using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.Client.Common;
using CargoWise.RefDbRepo.Client.SqlServer;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public class RefMessagingBussPackageInfoUpdaterInfo<TStorage> : IDataSetUpdaterInfo
		where TStorage : IDataSetStorage
	{
		public RefMessagingBussPackageInfoUpdaterInfo()
		{
		}

		public string GetMergeSqlText(ISQLBuilder sQLBuilder, IDictionary<Type, ForeignKeyRelationship[]> fks, ISchemaInfo schemaInfo)
		{
			var result = new StringBuilder();
			result.AppendLine(ReplaceSQLBuilder.CreateReplaceSql<IRefMessagingBussPackageInfo>(sQLBuilder, string.Empty, fks, schemaInfo.GetAllUniqueIndexes<IRefMessagingBussPackageInfo>(null)));
			result.AppendLine(ReplaceSQLBuilder.CreateReplaceSqlForDependentTable<IRefMessagingBussPackageVersion>(string.Empty));
			result.AppendLine(ReplaceSQLBuilder.CreateReplaceSqlForDependentTable<IRefMessagingBussCarrierInfo>(string.Empty));
			return result.ToString();
		}

		public IEnumerable<Tuple<Type, Type>> GetReferenceTypeByInsertOrder()
		{
			return Enumerable.Empty<Tuple<Type, Type>>();
		}

		public IEnumerable<Tuple<Type, Type>> GetTypeByInsertOrder()
		{
			yield return Tuple.Create(typeof(IRefMessagingBussPackageInfo), typeof(IRefMessagingBussPackageInfo));
			yield return Tuple.Create(typeof(IRefMessagingBussPackageVersion), typeof(IRefMessagingBussPackageVersion));
			yield return Tuple.Create(typeof(IRefMessagingBussCarrierInfo), typeof(IRefMessagingBussCarrierInfo));
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
