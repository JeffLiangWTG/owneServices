using CargoWise.EntityFramework;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.TransportConsignment.DataTransfer.Universal
{
	class DtbBookingConsignmentEventParentFinder : EventParentFinder
	{
		public DtbBookingConsignmentEventParentFinder(DtbBookingConsignmentDataContextManager manager, BusinessObjectFactory factory, IXmlImportLogger logger)
			: base(factory, manager, logger)
		{
		}

		protected override BusinessObject[] GetLogParentsForEventUsingContext(UniversalEvent xmlEvent)
		{
			return null;
		}
	}
}
