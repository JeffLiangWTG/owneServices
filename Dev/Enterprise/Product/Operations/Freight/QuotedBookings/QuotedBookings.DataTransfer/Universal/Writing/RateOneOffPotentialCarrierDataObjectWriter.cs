using Enterprise.Rating.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.QuotedBookings.DataTransfer.Universal
{
	internal class RateOneOffPotentialCarrierDataObjectWriter : DataObjectWriter<RateOneOffCarrier, PotentialCarrier>
	{
		internal RateOneOffPotentialCarrierDataObjectWriter(IDataWritingManager manager)
			: base(manager) { }

		protected override PotentialCarrier PopulateDataObject(RateOneOffCarrier rateOneOffCarrier) =>
			new (rateOneOffCarrier.Carrier, rateOneOffCarrier.Creditor);
	}
}
