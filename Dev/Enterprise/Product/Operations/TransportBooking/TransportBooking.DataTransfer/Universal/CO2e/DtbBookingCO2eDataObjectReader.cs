using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.TransportBookings.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.TransportBookings.DataTransfer.Universal
{
	public class DtbBookingCO2eDataObjectReader : ShipmentDataObjectReader<DtbBooking>
	{
		public DtbBookingCO2eDataObjectReader(Shipment dataObject, IXmlImportLogger logger, UniversalObjectFactory factory, DtbBooking targetBooking) : base(dataObject, logger, factory)
		{
			Booking = targetBooking;
		}

		readonly DtbBooking Booking;

		public override DataContextType DataContextType => DataContextType.TransportBooking;

		protected override IMatchingBusinessEntityFinder<DtbBooking> GetCombinedReferenceMatcher() => null;

		protected override DtbBooking GetExistingBusinessObjectUsingModuleSpecificBusinessRules() => Booking;

		protected override void PopulateBusinessObject(DtbBooking booking)
		{
			var previousCO2eValue = (TotalCO2e: booking.GetTotalCO2e(),
									Transports: Enumerable.Empty<BusinessObject>());
			booking?.GetResponseImporter(booking.Factory, logger)?.ImportGreenHouseGasEmission(dataObject, booking, GetBusinessObjectHumanReadableName(booking), previousCO2eValue);
		}
	}
}
