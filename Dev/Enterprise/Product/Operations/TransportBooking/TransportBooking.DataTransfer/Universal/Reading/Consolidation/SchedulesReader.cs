using CargoWise.Application;
using Enterprise.Integration.Schedule;
using Enterprise.TransportBookings.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.TransportBookings.DataTransfer.Universal
{
	public static class SchedulesReader
	{
		public static void ReadIntoCollection(Shipment sourceDO, IXmlImportLogger logger, UniversalObjectFactory factory, DtbBookingConsolidation consolidation)
		{
			var collection = sourceDO.TransportLegCollection;
			var legReader = ObjectFactory.Get<ITransportLegCollectionReader>("ITransportLegCollectionReader", collection, logger, factory, consolidation);
			legReader.ReadIntoCollection();
		}
	}
}
