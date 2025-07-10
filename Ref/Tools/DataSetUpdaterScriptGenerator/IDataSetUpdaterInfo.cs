using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Client.Common;
using CargoWise.RefDbRepo.Client.SqlServer;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public interface IDataSetUpdaterInfo : IDataSetInfo
	{
		Type GetStorageType();
		IEnumerable<Tuple<Type, Type>> GetReferenceTypeByInsertOrder();
		string GetMergeSqlText(ISQLBuilder sQLBuilder, IDictionary<Type, ForeignKeyRelationship[]> fks, ISchemaInfo schemaInfo);
		string GetOverriddenPrepareTemporaryTablesScripts();
	}
}
