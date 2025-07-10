using Enterprise.Customs.Common;
using Enterprise.Freight.DataTransfer.Universal;
using Enterprise.Rating.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.QuotedBookings.DataTransfer.Universal
{
	internal class RateOneOffShipmentAdditionalReferenceCollectionReader : CusEntryAdditionalReferenceCollectionReader<RateOneOffShipment>
	{
		public RateOneOffShipmentAdditionalReferenceCollectionReader(DataObjectList<AdditionalReference> additionalReferenceDataObjects, IXmlImportLogger logger, UniversalObjectFactory factory, RateOneOffShipment parent) : base(additionalReferenceDataObjects, logger, factory, parent)
		{
		}

		protected override CusEntryNumAdditionalReferenceCollection Numbers => Parent.Numbers;

		protected override Notes ParentNotes => Parent.Notes;
	}
}
