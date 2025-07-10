using CargoWise.Definitions;
using Enterprise.Packing.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transit.Business;
using static Enterprise.Integration.Customs;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal
{
	public class MAAEventProcessor : GoverningEventProcessor
	{
		public MAAEventProcessor(UniversalEvent eventDataObject, IXmlImportLogger logger) : base(eventDataObject, logger)
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
			var ecvReference = GetEventParameteByCode(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.CustomsReferenceNumber);
			var cresaReference = GetEventParameteByCode(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.ReferenceNumber);
			if (ecvReference != "" && cresaReference != "")
			{
				var portReference = CreateOrUpdatePortReference(portReferences, portReferenceType, ecvReference, string.Empty);
				portReference.CE_EntryLineReference = cresaReference;
			}
		}

		#endregion
	}
}
