using System;

namespace CargoWise.RefDataRepo.Ent.Client.Exporter
{
	public interface IXMLDataCreator
	{
		object Create(IDataRow data, Type storageType);
	}
}
