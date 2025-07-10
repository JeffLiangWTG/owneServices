using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Freight.Integration;
using Enterprise.Integration.Freight;
using Enterprise.ZArchitecture.Business;
using static Enterprise.Freight.Integration.Agency;
using EventConstants = CargoWise.EventReference.Constants;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;

namespace Enterprise.Freight.Business
{
	public static class RelatedAgencyBookingsEventLogHelper
	{
		public static void UpdateRelatedBookedAgencyBookingEvent(JobSailing[] sailings, bool onlyForMainSeaTransport = false)
		{
			var agencyRegistryWrapper = ObjectFactory.Get<IAgencyRegistry>();
			if (agencyRegistryWrapper.ElectronicBookingAndShippingInstructions)
			{
				foreach (var sailing in sailings)
				{
					var relatedBookedAgencyBookings = sailing.RelatedJobs.Where(x => x.JobType is AgencyBookingScheduleRelatedJobType && x.BizObj is IAgencyBooking)
						.Select(x => x.BizObj).Cast<CommonShipment>()
						.Where(x =>
						{
							if (x.JS_ShipmentStatus != ShipmentStatusList.Codes.Booked)
							{
								return false;
							}

							if (x.Logs.LogsNotInDB.OfType<StmALog>().Any(x => x.SL_SE_NKEvent == Events.StatusUpdatedCode
									&& x.Parameters.TryGetValue(Params.Type, out var type) && type == Core.Constants.EventReferenceMessageTypes.ShipmentStatus
									&& x.Parameters.TryGetValue(Params.New, out var newValue) && newValue == ShipmentStatusList.Codes.Booked))
							{
								return false;
							}

							if (!x.Numbers.Cast<CusEntryNumber>().Any(n => n.CE_EntryType == CusEntryNumLookups.HIR))
							{
								return false;
							}

							if (onlyForMainSeaTransport)
							{
								var mainSeaLeg = x.Transports.Cast<Transport>()
									.FirstOrDefault(transport => transport.JW_TransportType == Core.Constants.TransportPlanningType.MainVessel && transport.JW_TransportMode == Core.Constants.TransportModes.Sea);

								return mainSeaLeg != null && mainSeaLeg.JW_JX == sailing.PK && (mainSeaLeg.JW_ATD.IsEmpty || (!mainSeaLeg.JW_ATD.IsEmpty && agencyRegistryWrapper.IsDeliveryAgentAllowedSendingBookingConfirmationEDIAfterATD(x.JS_OH_DeliveryAgent)));
							}

							return true;
						});

					UpdateRelatedBookedAgencyBookingEvent(relatedBookedAgencyBookings);
				}
			}
		}

		public static void UpdateRelatedBookedAgencyBookingEvent(JobVoyage voyage)
		{
			if (voyage != null && ObjectFactory.Get<IAgencyRegistry>().ElectronicBookingAndShippingInstructions)
			{
				var relatedBookedAgencyBookings = voyage.RelatedJobs.Where(x => x.JobType is AgencyBookingScheduleRelatedJobType && x.BizObj is IAgencyBooking)
					.Select(x => x.BizObj).Cast<CommonShipment>()
					.Where(bizObj => bizObj.JS_ShipmentStatus == ShipmentStatusList.Codes.Booked
						&& bizObj.Numbers.Cast<CusEntryNumber>().Any(n => n.CE_EntryType == CusEntryNumLookups.HIR)
						&& !bizObj.Logs.LogsNotInDB.OfType<StmALog>().Any(x => x.SL_SE_NKEvent == Events.StatusUpdatedCode
							&& x.Parameters.TryGetValue(Params.Type, out var type) && type == Core.Constants.EventReferenceMessageTypes.ShipmentStatus
							&& x.Parameters.TryGetValue(Params.New, out var newValue) && newValue == ShipmentStatusList.Codes.Booked));

				UpdateRelatedBookedAgencyBookingEvent(relatedBookedAgencyBookings);
			}
		}

		static void UpdateRelatedBookedAgencyBookingEvent(IEnumerable<CommonShipment> relatedBookedAgencyBookings)
		{
			var parameters = new KeyValuePair<string, string>[]
			{
				new KeyValuePair<string, string>(EventConstants.EventReferenceParameters.Codes.LogSubscriber, FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber)
			};

			foreach (var booking in relatedBookedAgencyBookings)
			{
				booking.Logs.CreateRecreateOrUpdateEventLog(Events.EditedARecord, EstimateActual.Actual, ZDateTimeOffset.Now, ZString.Empty, parameters);
			}
		}
	}
}
