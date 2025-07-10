using System;
using System.Collections.Generic;

namespace CargoWise.RefDataRepo.Ent.Client.Exporter
{
	public interface IValueRetrieval
	{
		IEnumerable<IDataRow> GetRelatedEntities(IDataRow data, Type storageType, Type storageRelatedType);
		object GetValue(IDataRow data, Type storageTypeName, Type valueType, string propertyName);
		Type GetStorageTypeFromName(string name);
	}
}
