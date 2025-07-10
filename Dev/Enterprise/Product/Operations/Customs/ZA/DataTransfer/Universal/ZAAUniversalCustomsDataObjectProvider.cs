using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.ZA.DataTransfer.Universal
{
	public class ZAAUniversalCustomsDataObjectProvider : UniversalCustomsDataObjectProvider, IUniversalCustomsDataObjectProvider
	{
		ITopLevelDataObjectReader IUniversalCustomsDataObjectProvider.GetNewJobDeclarationDataObjectReader(UniversalShipment declarationDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, ForwardingShipment shipment)
		{
			return new JobDeclarationDataObjectReaderForWOT(declarationDataObject, logger, factory, shipment);
		}
	}
}
