using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.DataTransfer.Universal.SeaManifest;
using Enterprise.Customs.NZ.Business.Express;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.NZ.Business.Data.Universal
{
	public class CusSCAOceanBillDataObjectReader : CusSCAOceanBillDataObjectReader<CusSCAOceanBill, CusSCAHouse, CusSCAContainer, CusSCAPackingLine>
	{
		public CusSCAOceanBillDataObjectReader(Shipment dataObject, Shipment hVLVShipperConsolidation, IXmlImportLogger logger, UniversalObjectFactory factory) : base(dataObject, hVLVShipperConsolidation, logger, factory)
		{
			hvlvShipmentDataObjectWrapper = hVLVShipperConsolidation != null ? new HVLVShipmentDataObjectWrapper(hVLVShipperConsolidation, dataObject, factory) : null;
		}

		readonly HVLVShipmentDataObjectWrapper hvlvShipmentDataObjectWrapper;

		public CusSCAOceanBillDataObjectReader(CusSCAOceanBill existingBill, IEnumerable<ZString> hbsToBeInserted, Shipment dataObject, Shipment hVLVShipperConsolidation, IXmlImportLogger logger, UniversalObjectFactory factory)
			: this(dataObject, hVLVShipperConsolidation, logger, factory)
		{
			shouldLookForExistingBill = false;
			this.existingBill = existingBill;
			this.hbsToBeInserted = hbsToBeInserted?.ToHashSet();
		}

		readonly bool shouldLookForExistingBill = true;

		readonly CusSCAOceanBill existingBill;

		readonly HashSet<ZString> hbsToBeInserted;

		public bool IsUpdatable(CusSCAOceanBill scaBill)
		{
			return GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(scaBill).IsEmpty;
		}

		protected override CusSCAOceanBill GetExistingBusinessObjectUsingModuleSpecificBusinessRules()
		{
			return shouldLookForExistingBill ? GetExistingCusSCAOceanBill() : existingBill;
		}

		CusSCAOceanBill GetExistingCusSCAOceanBill()
		{
			var result = base.GetExistingBusinessObjectUsingModuleSpecificBusinessRules();
			return result;
		}

		protected override ZString GetApplicationCode()
		{
			return JobApplicationCodeList.Codes.TSW;
		}

		protected override CusSCAHouseDataObjectReader<CusSCAHouse, CusSCAPackingLine> GetNewCusSCAHouseDataObjectReader(IColumnIndexer oceanBill, Shipment subShipmentDataObject)
		{
			var shipmentType = (string)subShipmentDataObject?.ShipmentType?.Code;

			var billReader = (shipmentType == Core.Constants.ShipmentTypes.HighVolumeLowValue) ? new ETailCusSCAHouseDataObjectReader(oceanBill, hvlvShipmentDataObjectWrapper, subShipmentDataObject, logger, factory)
				: new CusSCAHouseDataObjectReader(oceanBill, hvlvShipmentDataObjectWrapper, subShipmentDataObject, logger, factory);

			if (hbsToBeInserted != null && !hbsToBeInserted.Contains(subShipmentDataObject.WayBillNumber.GetValueOrDefault().ToUpper()) && billReader.GetExistingBusinessObject() == null)
			{
				return null;
			}

			return billReader;
		}

		protected override CusSCAContainerDataObjectReader<CusSCAContainer> GetNewCusSCAContainerDataObjectReader(IColumnIndexer oceanBill, Container containerDataObject)
		{
			return new CusSCAContainerDataObjectReader(oceanBill, hvlvShipmentDataObjectWrapper, containerDataObject, logger, factory);
		}

		protected override void PopulateBusinessObject(CusSCAOceanBill targetBO)
		{
			var generator = targetBO.LineNumberGenerator;

			using (generator.GetLineNumberSuspender())
			{
				base.PopulateBusinessObject(targetBO);
			}

			targetBO.HouseBills.Reload(false);
			generator.ReCalculateAll();
		}

		protected override TransportLeg GetArrivalVoyage()
		{
			return dataObject.GetArrivalTransportLeg();
		}

		protected override TransportLeg GetDepartureVoyage()
		{
			return dataObject.GetDepartureTransportLeg();
		}

		protected override bool ShouldDeleteUnprocessedHouseBill(BaseCusSCAHouse house)
		{
			if (dataObject.IsHVLV())
			{
				return house.ShouldDeleteUnprocessedHouseBill_HVLV(() => hvlvShipmentDataObjectWrapper?.ShipmentPK);
			}

			return base.ShouldDeleteUnprocessedHouseBill(house);
		}
	}
}
