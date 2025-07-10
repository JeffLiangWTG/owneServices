using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Business
{
	internal class FlightMonitoringConsolProvider
	{
		public FlightMonitoringConsolProvider(CommonConsol consol)
		{
			this.consol = consol;
		}
		readonly CommonConsol consol;

		public void UpdateFlightSubscriptionEvent()
		{
			if (IsApplicableForFlightSubscription)
			{
				if (consol.IsInDatabase)
				{
					FlightMonitoringSystemManager.RemoveSubscripionLogIfExists(consol.Logs);
				}

				if (consol.IsAllDataValidForFlightTrackingSubscription)
				{
					consol.Logs.CreateRecreateOrUpdateEventLog(
						AutoEvents.SubscriptionRequested,
						EstimateActual.Actual,
						ZDateTimeOffset.Now,
						ZString.Empty,
						GetFlightSubscriptionEventParameters().ToArray());
				}
			}
		}

		bool IsApplicableForFlightSubscription
		{
			get
			{
				return !consol.IsInDatabase
					|| consol.JK_TransportModeInfo.HasChanges
					|| consol.JK_AgentTypeInfo.HasChanges
					|| consol.JK_OA_CreditorAddressInfo.HasChanges
					|| consol.JK_CoLoadMasterBillInfo.HasChanges
					|| consol.JK_CoLoadBookingReferenceInfo.HasChanges;
			}
		}

		Dictionary<string, string> GetFlightSubscriptionEventParameters()
		{
			var referenceNumber = !consol.JK_CoLoadMasterBill.IsEmpty ? consol.JK_CoLoadMasterBill : consol.JK_CoLoadBookingReference;

			return new Dictionary<string, string>
			{
				[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type] = Core.Constants.EventReferenceParameterTypes.AWBAutomation,
				[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.ReferenceNumber] = referenceNumber,
			};
		}
	}
}
