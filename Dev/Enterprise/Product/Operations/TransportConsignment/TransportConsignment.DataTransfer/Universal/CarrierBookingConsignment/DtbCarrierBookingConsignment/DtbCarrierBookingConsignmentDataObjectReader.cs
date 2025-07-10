using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.TransportConsignment.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.TransportConsignment.DataTransfer.Universal
{
	class DtbCarrierBookingConsignmentDataObjectReader : ShipmentDataObjectReader<DtbCarrierBookingConsignment>
	{
		public DtbCarrierBookingConsignmentDataObjectReader(UniversalShipment consignmentDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, UniversalShipment topLevelDataObject, Instruction instruction, DtbCarrierBookingConsignment consignment, IColumnIndexer pickUpInstructionToCopy = null, IColumnIndexer depotConfirmationToCopy = null, Instruction additionalInstruction = null)
			: this(consignmentDataObject, logger, factory, consignment, topLevelDataObject)
		{
		}

		public DtbCarrierBookingConsignmentDataObjectReader(UniversalShipment consignmentDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, DtbCarrierBookingConsignment consignment, UniversalShipment topLevelDataObject)
			: base(consignmentDataObject, logger, factory)
		{
			Consignment = consignment;
		}

		readonly Instruction DeliveryAddress;

		readonly DtbCarrierBookingConsignment Consignment;

		#region DataContextType

		public override DataContextType DataContextType
		{
			get { return DataContextType.CarrierBookingConsignment; }
		}

		#endregion

		#region LogChildTopLevelObjectsOnImport

		protected override bool LogChildTopLevelObjectsOnImport
		{
			get { return Consignment == null || DeliveryAddress != null; }
		}

		#endregion

		#region GetCombinedReferenceMatcher

		protected override IMatchingBusinessEntityFinder<DtbCarrierBookingConsignment> GetCombinedReferenceMatcher()
		{
			// No references to match a consignment
			return null;
		}

		#endregion

		#region GetExistingBusinessObjectUsingModuleSpecificBusinessRules

		protected override DtbCarrierBookingConsignment GetExistingBusinessObjectUsingModuleSpecificBusinessRules()
		{
			return Consignment;
		}

		#endregion

		#region PopulateBusinessObject

		protected override void PopulateBusinessObject(DtbCarrierBookingConsignment targetBO)
		{
			// Minimum implementation
			PopulateData(targetBO);
		}

		#endregion

		#region PopulateData

		void PopulateData(DtbConsignment consignment)
		{
			var row = GetColumnIndexerFromRow(consignment);
			SetValue(row, DtbConsignmentSchema.LTC_RS_NKServiceLevel, dataObject.ServiceLevel);
			SetValue(row, DtbConsignmentSchema.LTC_Direction, Constants.CartageDirection.Local);
		}

		#endregion

	}
}



