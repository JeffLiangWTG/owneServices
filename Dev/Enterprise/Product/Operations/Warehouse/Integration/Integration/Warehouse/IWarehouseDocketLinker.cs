using CargoWise.EntityFramework;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Warehouse.Integration
{
	public interface IWarehouseDocketLinker
	{
		bool LinkDocket(IColumnIndexer parent, IEntityID parentEntityID, IXmlImportLogger logger);
	}
}
