using CargoWise.EntityFramework;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.eTail.DataTransfer.Universal
{
	class HVLVBookingHeaderEventParentFinder : EventParentFinder
	{
		public HVLVBookingHeaderEventParentFinder(HVLVBookingHeaderDataContextManager manager, BusinessObjectFactory factory, IXmlImportLogger logger)
			: base(factory, manager, logger)
		{
		}

		protected override BusinessObject[] GetLogParentsForEventUsingContext(UniversalEvent xmlEvent) => null;
	}
}
