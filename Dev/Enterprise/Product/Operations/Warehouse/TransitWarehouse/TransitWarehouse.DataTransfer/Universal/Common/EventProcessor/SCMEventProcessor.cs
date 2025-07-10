using CargoWise.Definitions;
using Enterprise.Packing.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Common;
using static Enterprise.Integration.Customs;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal
{
	public class SCMEventProcessor : GoverningEventProcessor
	{
		public SCMEventProcessor(UniversalEvent eventDataObject, IXmlImportLogger logger) : base(eventDataObject, logger)
		{
		}

		#region Process

		public override void Process(WhsItemReceiveConsignment consignment)
		{
			if (TryClearPortReference(consignment.PortReferences, ExpectedPortReferenceType) && consignment.Warehouse.WW_IsPortAuthorityControlled)
			{
				consignment.WRC_CompleteTime = TransitWarehouseHelper.GetNowInCurrentWarehouse(consignment.Warehouse);
			}
		}

		public override void Process(WhsItemDispatchConsignment consignment)
		{
			if (TryClearPortReference(consignment.PortReferences, TransitWarehousePortReferenceTypes.Codes.PortExport) && consignment.Warehouse.WW_IsPortAuthorityControlled)
			{
				consignment.WDC_CompleteTime = TransitWarehouseHelper.GetNowInCurrentWarehouse(consignment.Warehouse);
			}
		}

		public override void Process(PkgPackage package)
		{
			TryClearPortReference(package.PortReferences, ExpectedPortReferenceType);
		}

		#endregion

		#region AddOrUpdatePortReference

		bool TryClearPortReference(IPortReferenceCollection portReferences, string portReferenceType)
		{
			var customsReferenceNumber = GetEventParameteByCode(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.CustomsReferenceNumber);
			if (customsReferenceNumber != "")
			{
				CreateOrUpdatePortReference(portReferences, portReferenceType, customsReferenceNumber, TransitWarehouseReferenceStatus.Codes.Cleared);
			}

			return customsReferenceNumber != "";
		}

		#endregion
	}
}
