using System;
using System.Collections.Generic;
using System.Data;

namespace CargoWise.RefDataRepo.Ent.Client.Exporter
{
	public interface IDBHelper
	{
		IEnumerable<IDataRow> GetRecords(Type storageType, string column, IEnumerable<Guid> values, IDbTransaction transaction = null);
	}
}
