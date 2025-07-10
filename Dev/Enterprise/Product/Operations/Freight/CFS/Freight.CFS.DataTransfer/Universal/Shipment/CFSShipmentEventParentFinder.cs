using CargoWise.EntityFramework;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;

namespace Enterprise.Freight.CFS.DataTransfer.Universal
{
	internal class CFSShipmentEventParentFinder : ConsolAndShipmentEventParentFinder<CFSLoadListConsol, CFSShipment, CFSContainer>
	{
		public CFSShipmentEventParentFinder(BusinessObjectFactory factory, IEventDataContextManager manager, IXmlImportLogger logger)
			: base(factory, manager, logger, new UniversalCFSHelper())
		{
		}

		protected override BusinessObject[] GetLogParentsForEventUsingContext(Event xmlEvent)
		{
			return xmlEvent.HasMatchingDataTarget(DataContextType.CFSShipment)
				&& !xmlEvent.HasMatchingDataTarget(DataContextType.CFSLoadListConsol)
				? base.GetLogParentsForEventUsingContext(xmlEvent)
				: null;
		}
	}
}
