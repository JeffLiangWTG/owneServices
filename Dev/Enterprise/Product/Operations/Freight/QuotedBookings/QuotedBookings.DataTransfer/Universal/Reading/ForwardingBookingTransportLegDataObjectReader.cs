using System;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.QuotedBookings.DataTransfer.Universal
{
	public class ForwardingBookingTransportLegDataObjectReader : DataObjectReader<TransportLeg, QuotedBooking>
	{
		public ForwardingBookingTransportLegDataObjectReader(TransportLeg transportLegDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, QuotedBooking booking, UniversalShipment parentDataObject)
			: base(transportLegDataObject, logger, factory)
		{
			ParentBooking = booking;
			ParentDataObject = parentDataObject;
		}

		QuotedBooking ParentBooking { get; }

		UniversalShipment ParentDataObject { get; }

		protected override QuotedBooking GetExistingBusinessObject()
		{
			return ParentBooking;
		}

		protected override void PopulateBusinessObject(QuotedBooking targetBO)
		{
			logger.Log(LogType.Information, FormattableString.Invariant($@"Finding Schedule for Transport Leg: Origin: {dataObject.PortOfLoading?.Code} Destination: {dataObject.PortOfDischarge?.Code} to link to QuotedBooking"));

			var scheduleFinder = new ForwardingBookingSailingScheduleFinder(factory, dataObject, logger);
			var sailingManager = scheduleFinder.GetTransportSailingFromSailingManager();
			var sailing = sailingManager.Sailing;

			if (sailing == null)
			{
				logger.Log(LogType.Information, "An existing Schedule could not be found. The QuotedBooking will not be linked");
				sailingManager.LogReasonForSailingNotGenerated(logger);
				PopulateLoadAndDischarge(targetBO);
				return;
			}

			if (IsArchivedSailingSchedule(sailing))
			{
				logger.Log(LogType.Information, "The Schedule found is archived. Discarding.");
				return;
			}

			logger.Log(LogType.Information, FormattableString.Invariant($"Schedule with Origin: {sailing.JX_JA_RL_NKPortOfLoading} Destination: {sailing.JX_JB_RL_NKPortOfDischarge} has been found and linked to QuotedBooking."));
			targetBO.Booking.JS_JX = sailing.PK;

			if (targetBO.TransportMode == Core.Constants.TransportModes.Sea)
			{
				targetBO.Booking.JS_OA_BookedShippingLineAddress = sailing.Voyage?.Line?.MainAddress.PK ?? ZGuid.Empty;
			}

			if (AllowUpdateSailing || !sailing.IsInDatabase)
			{
				ImportSailingDetails(sailing);
				ImportSailingOriginDetails(sailing.Origin);
				ImportSailingDestinationDetails(sailing.Destination);
			}
		}

		void PopulateLoadAndDischarge(QuotedBooking targetBO)
		{
			var parentPortOfLoading = ParentDataObject.PortOfLoading.GetUNLOCOAsUpperCase(factory.BOFactory);
			var parentPortOfDischarge = ParentDataObject.PortOfDischarge.GetUNLOCOAsUpperCase(factory.BOFactory);

			var portOfLoading = dataObject.PortOfLoading.GetUNLOCOAsUpperCase(factory.BOFactory);
			var portOfDischarge = dataObject.PortOfDischarge.GetUNLOCOAsUpperCase(factory.BOFactory);

			if (parentPortOfLoading.IsEmpty && parentPortOfDischarge.IsEmpty
				&& (!portOfLoading.IsEmpty || !portOfDischarge.IsEmpty))
			{
				targetBO.LoadPort = portOfLoading;
				targetBO.DischargePort = portOfDischarge;
			}
		}

		bool IsArchivedSailingSchedule(JobSailing sailing)
		{
			return sailing?.Voyage != null && sailing.Voyage.IsArchived;
		}

		void ImportSailingDetails(JobSailing sailingBO)
		{
			if (sailingBO == null)
			{
				return;
			}

			SetValue(sailingBO, JobSailingSchema.JX_DepotAvailabilityDate, dataObject.LCLAvailability);
			SetValue(sailingBO, JobSailingSchema.JX_DepotCutOff, dataObject.LCLCutOff);
			SetValue(sailingBO, JobSailingSchema.JX_DepotReceivalCommences, dataObject.LCLReceivalCommences);
			SetValue(sailingBO, JobSailingSchema.JX_DepotStorageDate, dataObject.LCLStorageDate);
			SetValue(sailingBO, JobSailingSchema.JX_DeparturePortRouteId, dataObject.DepartureReference);
			SetValue(sailingBO, JobSailingSchema.JX_ArrivalPortRouteId, dataObject.ArrivalReference);
		}

		void ImportSailingOriginDetails(VoyageOrigin origin)
		{
			if (origin == null)
			{
				return;
			}

			SetValue(origin, JobVoyOriginSchema.JA_E_ARV, dataObject.EstimatedArrivalInPortOfLoading);
			SetValue(origin, JobVoyOriginSchema.JA_A_ARV, dataObject.ActualArrivalInPortOfLoading);
			SetValue(origin, JobVoyOriginSchema.JA_Berth, dataObject.DepartureBerth);
			SetValue(origin, JobVoyOriginSchema.JA_DocumentaryCutoff, dataObject.DocumentCutOff);
			SetValue(origin, JobVoyOriginSchema.JA_CutOff, dataObject.FCLCutOff);
			SetValue(origin, JobVoyOriginSchema.JA_VGMCutOff, dataObject.VGMCutOff);
			SetValue(origin, JobVoyOriginSchema.JA_ReceivalCommences, dataObject.FCLReceivalCommences);
			SetValue(origin, JobVoyOriginSchema.JA_DepartReference, dataObject.DepartureReference);

			if (dataObject.DepartureFrom == null && dataObject.DepartureCTO != null)
			{
				var orgDataReader = new OrganisationDataObjectReader(dataObject.DepartureCTO, logger, factory);
				var address = orgDataReader.GetMatched();
				if (address != null)
				{
					origin.JA_OA_DepartureCTOAddress = address.PK;
				}
			}
		}

		void ImportSailingDestinationDetails(VoyageDestination destination)
		{
			if (destination == null)
			{
				return;
			}

			SetValue(destination, JobVoyDestinationSchema.JB_Berth, dataObject.ArrivalBerth);
			SetValue(destination, JobVoyDestinationSchema.JB_AvailabilityDate, dataObject.FCLAvailability);
			SetValue(destination, JobVoyDestinationSchema.JB_StorageDate, dataObject.FCLStorage);
			SetValue(destination, JobVoyDestinationSchema.JB_ArrivalReference, dataObject.ArrivalReference);

			if (dataObject.ArrivalAt == null && dataObject.ArrivalCTO != null)
			{
				var orgDataReader = new OrganisationDataObjectReader(dataObject.ArrivalCTO, logger, factory);
				var address = orgDataReader.GetMatched();
				if (address != null)
				{
					destination.JB_OA_ArrivalCTOAddress = address.PK;
				}
			}
		}

		bool AllowUpdateSailing => SystemDataRegistry.Instance.UpdateSailingSchedulesDuringAutomaticImport.Value;
	}
}
