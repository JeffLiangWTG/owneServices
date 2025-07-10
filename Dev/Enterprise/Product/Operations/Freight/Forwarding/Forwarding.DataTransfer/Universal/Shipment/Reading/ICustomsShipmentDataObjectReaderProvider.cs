namespace Enterprise.Freight.Forwarding.DataTransfer
{
	using Enterprise.Freight.Forwarding.Business;
	using Enterprise.UniversalDataBuss.DataObjects.Core;
	using Enterprise.UniversalDataBuss.DataObjects.Universal;
	using Enterprise.UniversalDataBuss.Integration;

	public interface ICustomsShipmentDataObjectReaderProvider
	{
		ITopLevelDataObjectReader GetReader(Shipment universalShipment, IXmlImportLogger logger, UniversalObjectFactory factory, ForwardingShipment forwardingShipment);
	}
}
