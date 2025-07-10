using System;
using CargoWise.EntityFramework;
using Enterprise.Freight.DataTransfer.Universal;
using Enterprise.TransportBookings.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.TransportBookings.DataTransfer.Universal
{
	/// <summary>
	/// This is only used when the uXML has a data target of Transport Booking.
	/// Because we always require a Consolidation and it's our entry point, create the Consolidation Reader and pass in the targeted Booking.
	/// </summary>
	class DtbBookingDataObjectReader_DataTargetIsTransportBooking : ShipmentDataObjectReader<DtbBooking>,
		ITopLevelDataObjectReader
	{
		public DtbBookingDataObjectReader_DataTargetIsTransportBooking(Shipment shipment, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(shipment, logger, factory)
		{
		}

		public override DataContextType DataContextType
		{
			get { return DataContextType.TransportBooking; }
		}

		protected override IMatchingBusinessEntityFinder<DtbBooking> GetCombinedReferenceMatcher()
		{
			return null;
		}

		protected override DtbBooking GetExistingBusinessObjectUsingModuleSpecificBusinessRules()
		{
			return null;
		}

		protected override bool IsImportJobCostingAllowed(DtbBooking targetBO)
		{
			return false;
		}

		protected sealed override void PopulateBusinessObject(DtbBooking targetBO)
		{
			throw new InvalidOperationException("This reader should never be used to populate a business object, it should only be used to redirect to the consolidation reader.");
		}

		DtbBookingConsolidationDataObjectReader GetNewConsolidationReader(Shipment dataObject, IXmlImportLogger logger, UniversalObjectFactory factory, DtbBooking targetBO)
		{
			return new DtbBookingConsolidationDataObjectReader(dataObject, logger, factory, targetBO);
		}

		BusinessObject ITopLevelDataObjectReader.ReadIntoTopLevelBusinessObject()
		{
			if (dataObject.IsCO2eResponse())
			{
				return new DtbBookingCO2eDataObjectReader(dataObject, logger, factory, GetBusinessObjectFromContextKey()).ReadIntoBusinessObject();
			}

			var booking = GetBusinessObjectFromContextKey() ?? GetNewBusinessObject();

			var reader = GetNewConsolidationReader(dataObject, logger, factory, booking);
			reader.ReadIntoBusinessObject();

			return booking;
		}
	}
}
