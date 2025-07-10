using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.DataTransfer.Universal.SeaManifest;
using Enterprise.Customs.NZ.Business.Express;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.NZ.Business.Data.Universal
{
	public class CusSCAHouseDataObjectReader : CusSCAHouseDataObjectReader<CusSCAHouse, CusSCAPackingLine>
	{
		public CusSCAHouseDataObjectReader(IColumnIndexer oceanBill, HVLVShipmentDataObjectWrapper hvlvConsolidatorShipmentWrapper, Shipment dataObject, IXmlImportLogger logger, UniversalObjectFactory factory) : base(oceanBill, hvlvConsolidatorShipmentWrapper, dataObject, logger, factory)
		{
		}

		public new CusSCAHouse GetExistingBusinessObject()
		{
			return base.GetExistingBusinessObject();
		}

		protected override CusSCAPivotDataObjectReader<CusSCAPackingLine> GetNewCusSCAPivotDataObjectReader(ZInt lineNo, IColumnIndexer houseBill, PackingLine packageDataObject)
		{
			return new CusSCAPivotDataObjectReader(lineNo, houseBill, hvlvConsolidatorShipmentWrapper, packageDataObject, logger, factory);
		}

		protected override void PopulateCountrySpecificDetails(IColumnIndexer houseBill)
		{
		}
	}
}
