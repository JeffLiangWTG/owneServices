using CargoWise.EntityFramework;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;

namespace Enterprise.Customs.US.eManifest.Business
{
	class TripEventParentFinder : EventParentFinder
	{
		public TripEventParentFinder(TripDataContextManager manager, BusinessObjectFactory factory, IXmlImportLogger logger)
			: base(factory, manager, logger)
		{
		}

		protected override BusinessObject[] GetLogParentsForEventUsingContext(Event xmlEvent) => null;
	}
}
