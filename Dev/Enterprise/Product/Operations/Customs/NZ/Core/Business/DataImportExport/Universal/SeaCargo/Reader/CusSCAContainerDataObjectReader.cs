using CargoWise.EntityFramework;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.DataTransfer.Universal.SeaManifest;
using Enterprise.Customs.NZ.Business.Express;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.NZ.Business.Data.Universal
{
	public class CusSCAContainerDataObjectReader : CusSCAContainerDataObjectReader<CusSCAContainer>
	{
		public CusSCAContainerDataObjectReader(IColumnIndexer oceanBill, HVLVShipmentDataObjectWrapper hvlvConsolidatorShipmentWrapper, Container data, IXmlImportLogger logger, UniversalObjectFactory factory) : base(oceanBill, hvlvConsolidatorShipmentWrapper, data, logger, factory)
		{
		}
	}
}
