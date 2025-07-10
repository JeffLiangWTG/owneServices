using CargoWise.Definitions;
using Enterprise.Packing.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transit.Business;
using static Enterprise.Integration.Customs;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal
{
	public class CENEventProcessor : GoverningEventProcessor
	{
		public CENEventProcessor(UniversalEvent eventDataObject, IXmlImportLogger logger, UniversalObjectFactory factory) : base(eventDataObject, logger, factory) { }

		#region Process

		public override void Process(WhsItemReceiveConsignment consignment)
		{
			AddCustomsReferenceIfNotExist(consignment.CustomsReferenceNumbers);
		}

		public override void Process(PkgPackage package)
		{
			AddCustomsReferenceIfNotExist(package.CustomsReferenceNumbers);
		}

		public override void Process(WhsItemDispatchConsignment consignment)
		{
		}

		#endregion

		#region AddCustomsReferenceIfNotExist

		void AddCustomsReferenceIfNotExist(ICustomsReferenceCollection customsReferences) => AddCustomsReferenceIfNotExist(customsReferences, CargoWise.EventReference.Constants.EventReferenceParameters.Codes.CustomsReferenceNumber, TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber);

		#endregion
	}
}
