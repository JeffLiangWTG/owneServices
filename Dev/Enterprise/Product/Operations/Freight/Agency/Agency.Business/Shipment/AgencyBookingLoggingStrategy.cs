using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Freight.Business;
using Enterprise.Freight.Integration;
using Enterprise.ZArchitecture.Business;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;

namespace Enterprise.Freight.Agency.Business.Shipment
{
	class AgencyBookingLoggingStrategy : IBusinessObjectStrategy
	{
		public void BeforeSuccessfulDelete(BusinessObject businessObject) { }

		public DeleteDetails DeleteDetails(BusinessObject businessObject) => null;

		public void FetchForLoad(BusinessObject businessObject) { }

		public void OnDelete(BusinessObject businessObject) { }

		public void OnFactorySaved(BusinessObject businessObject, bool saveSucceeded)
		{
			if (saveSucceeded && businessObject is AgencyBooking agencyBooking)
			{
				agencyBooking.TransportsIncludingRelatedOriginalCount = agencyBooking.TransportsIncludingRelated.Count;
			}
		}

		public void OnFactorySaving(BusinessObject businessObject)
		{
			if (businessObject is AgencyBooking agencyBooking && ShouldAddAgencyBookingUpdatedLogSubscriberEditedARecordEvent(agencyBooking))
			{
				agencyBooking.Logs.CreateRecreateOrUpdateEventLog(Events.EditedARecord, EstimateActual.Actual, ZDateTimeOffset.Now, ZString.Empty, new[]
				{
					new KeyValuePair<string, string>(Params.LogSubscriber, FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber)
				});
			}
		}

		bool ShouldAddAgencyBookingUpdatedLogSubscriberEditedARecordEvent(AgencyBooking agencyBooking)
		{
			if (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.Value
				&& agencyBooking.IsInDatabase
				&& agencyBooking.JS_ShipmentStatus == ShipmentStatusList.Codes.Booked
				&& agencyBooking.Numbers.Cast<CusEntryNumber>().Any(n => n.CE_EntryType == CusEntryNumLookups.HIR)
				&& !agencyBooking.Logs.LogsNotInDB.OfType<StmALog>().Any(x => x.SL_SE_NKEvent == Events.StatusUpdatedCode
					&& x.Parameters.TryGetValue(Params.Type, out var type) && type == Core.Constants.EventReferenceMessageTypes.ShipmentStatus
					&& x.Parameters.TryGetValue(Params.New, out var newValue) && newValue == ShipmentStatusList.Codes.Booked))
			{
				var mainSeaTransportATDFilled = agencyBooking.TransportsIncludingRelated.Cast<Transport>()
					.Any(x => x.JW_TransportMode == Core.Constants.TransportModes.Sea
						&& x.JW_TransportType == Core.Constants.TransportPlanningType.MainVessel
						&& !x.JW_ATD.IsEmpty);

				return (!mainSeaTransportATDFilled || (mainSeaTransportATDFilled && AllowSendingBookingConfirmationRegistryHelper.IsAllowed(agencyBooking.JS_OH_DeliveryAgent)))
						 && (agencyBooking.JS_CFSReferenceInfo.HasChanges
							 || agencyBooking.JS_OA_BookedShippingLineAddressInfo.HasChanges
							 || agencyBooking.JS_OH_DeliveryAgentInfo.HasChanges
							 || agencyBooking.JS_JXInfo.HasChanges
							 || (agencyBooking.TransportsIncludingRelatedOriginalCount.HasValue && agencyBooking.TransportsIncludingRelatedOriginalCount.Value != agencyBooking.TransportsIncludingRelated.Count)
							 || agencyBooking.TransportsIncludingRelated.Cast<Transport>().Any(x => !x.IsInDatabase
								 || x.JW_TransportModeInfo.HasChanges
								 || x.JW_VesselHasChanges()
								 || x.JW_VoyageFlightHasChanges()
								 || x.JW_RL_NKLoadPortHasChanges()
								 || x.JW_RL_NKDiscPortHasChanges()
								 || x.JW_ETDHasChanges()
								 || x.JW_ETAHasChanges()
								 || x.JW_OA_CarrierAddressHasChanges()
								 || (x.JW_TransportMode == Core.Constants.TransportModes.Sea && x.JW_TransportType == Core.Constants.TransportPlanningType.MainVessel
									 && (x.JW_ATDInfo.HasChanges
										 || x.JW_TerminalReceivalCommencesHasChanges()
										 || x.JW_DepotReceivalCommencesHasChanges()
										 || x.JW_TerminalCutOffHasChanges()
										 || x.JW_DepotCutOffHasChanges()
										 || x.JW_DocumentaryCutOffHasChanges()
										 || x.JW_VGMCutOffHasChanges()))));
			}

			return false;
		}

		public void OnSaved(BusinessObject businessObject, bool saveSucceeded) { }

		public void OnSaveRollback(BusinessObject businessObject) { }

		public void OnSaving(BusinessObject businessObject) { }

		public void OnSavingInObjectsWithLateChanges(BusinessObject businessObject) { }
	}
}
