using System;
using System.Collections.Generic;
using System.Text;
using CargoWise.RefDbRepo.Client.Common;
using CargoWise.RefDbRepo.Client.SqlServer;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public class RefCusProfileUpdaterInfo<TStorage> : IDataSetUpdaterInfo where TStorage : IDataSetStorage
	{
		public RefCusProfileUpdaterInfo()
		{
		}

		public string GetMergeSqlText(ISQLBuilder sQLBuilder, IDictionary<Type, ForeignKeyRelationship[]> fks, ISchemaInfo schemaInfo)
		{
			var result = new StringBuilder();
			result.AppendLine(SQLBuilder.UpdateReferenceFKStatement<IRefCusTariffType, IRefCusProfileType>(string.Empty, schemaInfo.GetAllUniqueIndexes<IRefCusTariffType>(null)));
			result.AppendLine(SQLBuilder.UpdateReferenceFKStatement<IRefCusProfileType, IRefCusProfile>(string.Empty, schemaInfo.GetAllUniqueIndexes<IRefCusProfileType>(null)));
			result.AppendLine(ReplaceSQLBuilder.CreateReplaceSql<IRefCusProfile>(sQLBuilder, string.Empty, fks, schemaInfo.GetAllUniqueIndexes<IRefCusProfile>(null)));
			result.AppendLine(ReplaceSQLBuilder.CreateReplaceSqlForDependentTable<IRefCusProfileAttribute>(string.Empty));
			return result.ToString();
		}

		public IEnumerable<Tuple<Type, Type>> GetReferenceTypeByInsertOrder()
		{
			yield return Tuple.Create(typeof(IRefCusTariffType), typeof(IRefCusTariffType));
			yield return Tuple.Create(typeof(IRefCusProfileType), typeof(IRefCusProfileType));
		}

		public IEnumerable<Tuple<Type, Type>> GetTypeByInsertOrder()
		{
			yield return Tuple.Create(typeof(IRefCusProfile), typeof(IRefCusProfile));
			yield return Tuple.Create(typeof(IRefCusProfileAttribute), typeof(IRefCusProfileAttribute));
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
