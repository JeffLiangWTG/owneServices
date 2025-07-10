using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Warehouse.Integration
{
	public interface IFTZWhsOrderLinesDataProvider
	{
		IEnumerable<IFTZWhsOrderLineData> GetFTZWhsOrderLinesData(IOrgHeader importer, IOrgAddress warehouseAddress, ZString outwardEntryNumber);
	}
}
