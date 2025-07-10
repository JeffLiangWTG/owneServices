using Enterprise.TransportBookings.Business;
using Enterprise.TransportCommon.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.TransportBookings.DataTransfer.Universal
{
	class AdditionalReferenceCollectionReaderHelper
	{
		public static void ReadAdditionalReferences(DtbBookingConsolidation consolidation, Shipment dataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
		{
			if (dataObject.AdditionalReferenceCollection != null)
			{
				var additionalReferenceCollectionReader = new TransportAdditionalReferenceCollectionReader<DtbBookingConsolidation>(dataObject.AdditionalReferenceCollection, logger, factory, consolidation);
				additionalReferenceCollectionReader.ReadIntoCollection();
			}
		}

		public static void ReadNewAdditionalReferences(DtbBookingConsolidation consolidation, Shipment dataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
		{
			if (dataObject.AdditionalReferenceCollection != null)
			{
				var additionalReferenceCollectionReader = new TransportAdditionalReferenceCollectionReader<DtbBookingConsolidation>(dataObject.AdditionalReferenceCollection, logger, factory, consolidation);
				additionalReferenceCollectionReader.ReadIntoCollectionUnmatchedElementsOnly();
			}
		}
	}
}
