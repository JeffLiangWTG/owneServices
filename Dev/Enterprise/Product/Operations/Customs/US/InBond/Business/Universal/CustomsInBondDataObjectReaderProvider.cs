using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.UniversalDataBuss.Integration;
using UniversalXml = Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Customs.US.InBond.Business.Universal
{
	public class CustomsInBondDataObjectReaderProvider : ICustomsInBondDataObjectReaderProvider
	{
		#region ICustomsInBondDataObjectReaderProvider Members

		ITopLevelDataObjectReader ICustomsInBondDataObjectReaderProvider.GetReader(UniversalXml.Shipment universalShipment, IXmlImportLogger logger, UniversalDataBuss.DataObjects.Core.UniversalObjectFactory factory, Freight.Integration.ICusInBondParent parentBO)
		{
			return new CusInBondHeaderDataObjectReader(universalShipment, logger, factory, parentBO);
		}

		#endregion
	}
}
