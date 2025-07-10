using CargoWise.Definitions;
using Enterprise.Packing.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transit.Business;
using static Enterprise.Integration.Customs;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal
{
	public class SHLEventProcessor : GoverningEventProcessor
	{
		public SHLEventProcessor(UniversalEvent eventDataObject, IXmlImportLogger logger) : base(eventDataObject, logger)
		{
		}

		#region Process

		public override void Process(WhsItemReceiveConsignment consignment)
		{
			AddOrUpdatePortReference(consignment.PortReferences, ExpectedPortReferenceType);
		}

		public override void Process(WhsItemDispatchConsignment consignment)
		{
			AddOrUpdatePortReference(consignment.PortReferences, TransitWarehousePortReferenceTypes.Codes.PortExport);
		}

		public override void Process(PkgPackage package)
		{
			AddOrUpdatePortReference(package.PortReferences, ExpectedPortReferenceType);
		}

		#endregion

		#region AddOrUpdatePortReference

		void AddOrUpdatePortReference(IPortReferenceCollection portReferences, string portReferenceType)
		{
			var customsReferenceNumber = GetEventParameteByCode(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.CustomsReferenceNumber);
			if (customsReferenceNumber != "")
			{
				CreateOrUpdatePortReference(portReferences, portReferenceType, customsReferenceNumber, string.Empty);
			}
		}

		#endregion
	}
}
