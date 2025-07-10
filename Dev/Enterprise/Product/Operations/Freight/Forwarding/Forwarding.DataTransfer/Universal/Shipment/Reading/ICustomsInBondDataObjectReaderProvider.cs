namespace Enterprise.Freight.Forwarding.DataTransfer
{
	using Enterprise.UniversalDataBuss.DataObjects.Core;
	using Enterprise.UniversalDataBuss.DataObjects.Universal;
	using Enterprise.UniversalDataBuss.Integration;

	public interface ICustomsInBondDataObjectReaderProvider
	{
		ITopLevelDataObjectReader GetReader(Shipment universalShipment, IXmlImportLogger logger, UniversalObjectFactory factory, Integration.ICusInBondParent parentBO);
	}
}
