using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal
{
	public interface ITransitDataObjectReaderHandler
	{
		void Execute(UniversalObjectFactory factory, UniversalShipment dataObject, IXmlImportLogger logger);
	}
}
