using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Warehouse.Yard.DataTransfer.Universal
{
	public abstract class ContainerYardDataObjectReader<T>(Shipment dataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
		: ShipmentDataObjectReader<T>(dataObject, logger, factory)
		where T : BusinessObject
	{
		protected IOrgHeader Client = ContainerYardUniversalHelper.GetClient(factory, logger, dataObject);
		protected override LogType LogTypeForReasonNotAbleToUpdate => LogType.Warning;
	}
}
