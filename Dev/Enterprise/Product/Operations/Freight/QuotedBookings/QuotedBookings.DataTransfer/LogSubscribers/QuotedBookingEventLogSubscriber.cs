using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.Freight.DataTransfer;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using Event = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Freight.QuotedBookings.DataTransfer
{
	[Serializable]
	public class QuotedBookingEventLogSubscriber : ShipmentEventLogProcessor
	{
		protected override BusinessObject GetLogParent(IQueuedLog log)
		{
			var factory = new ReadOnlyBusinessObjectFactory();
			return factory.Load(ViewQuotedBookingSchema.Constants.Prefix, log.SJ_ParentID);
		}

		protected override BusinessObject GetLogParentForBusinessObject(BusinessObject logParent, IQueuedLog log)
		{
			return log.Factory.Load(ViewQuotedBookingSchema.Constants.Prefix, logParent.PK);
		}

		protected override bool ShouldProcess(BusinessObject logParent)
		{
			return logParent is QuotedBooking;
		}

		protected override bool IsLCLDatesOverrideConsol(BusinessObject logParent)
		{
			return false;
		}

		protected override string GetPackingMode(BusinessObject logParent)
		{
			return (logParent as QuotedBooking)?.Booking.JS_PackingMode;
		}

		protected override IEnumerable<BusinessObject> GetContainerShipmentParent(CommonContainer container)
		{
			var bookingLink = container.JC_JS_FCLBookingOnlyLink;

			if (bookingLink.IsEmpty)
			{
				return null;
			}

			var factory = new ReadOnlyBusinessObjectFactory();
			var booking = factory.Load<QuotedBooking>(bookingLink);
			return booking == null ? null : new[] { booking };
		}

		protected override IEnumerable<CommonContainer> GetContainers(BusinessObject logParent)
		{
			return (logParent as QuotedBooking)?.Booking.Containers;
		}

		protected override EventDataObjectWriter GetEventDataObjectWriter(IDataWritingManager dataWritingManager, BusinessObject logParent, string subscriptionReference, IDictionary<string, string> contextMappings)
		{
			var booking = logParent as QuotedBooking;
			return new ShipmentEventDataObjectWriter(dataWritingManager, booking?.Booking, contextMappings)
			{
				SubscriptionReference = subscriptionReference,
				VesselName = contextMappings[nameof(Event.ContextTypes.VesselName)] ?? booking?.ScheduleChooser?.Sailing?.Vessel.RV_Code,
				LloydsNumber = contextMappings[nameof(Event.ContextTypes.LloydsNumber)] ?? booking?.ScheduleChooser?.Sailing?.Vessel.RV_LloydsNumber,
				VoyageNumber = contextMappings[nameof(Event.ContextTypes.VoyageNumber)] ?? booking?.ScheduleChooser?.Sailing?.JX_JV_VoyageFlight,
				LegOriginUNLOCO = contextMappings[nameof(Event.ContextTypes.LegOriginUNLOCO)] ?? booking?.ScheduleChooser?.Sailing?.JX_JA_RL_NKPortOfLoading,
				LegDestinationUNLOCO = contextMappings[nameof(Event.ContextTypes.LegDestinationUNLOCO)] ?? booking?.ScheduleChooser?.Sailing?.JX_JB_RL_NKPortOfDischarge,
			};
		}
	}
}
