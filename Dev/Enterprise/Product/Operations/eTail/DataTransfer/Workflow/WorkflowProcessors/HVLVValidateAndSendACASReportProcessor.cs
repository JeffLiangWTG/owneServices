using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.eTail.Business;
using Enterprise.eTail.Documents.Business;
using Enterprise.eTail.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Business;
using static Enterprise.Core.Constants;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;

namespace Enterprise.eTail.DataTransfer
{
	public abstract class HVLVValidateAndSendACASReportProcessor : IProcessor
	{
		public HVLVValidateAndSendACASReportProcessor(ForwardingShipment shipment)
		{
			Shipment = shipment;
		}

		ForwardingShipment Shipment { get; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "app lock key")]
		public const string HVLVACASReportAppLockKey = "HVLV ACAS Report";

		public void Process(INotifications notifications, CancellationToken token = default)
		{
			if (Shipment.IsAir
				&& IsUSShipment(Shipment)
				&& Shipment.HVLVConsignments.Any(c => c.HVC_ACASMessageStatus == ACASStatusCanBeSend))
			{
				if (!Shipment.PK.TryAcquireApplicationLock<ForwardingShipment>(
					HVLVACASReportAppLockKey,
					SendACASReport,
					out var shipmentLockedErrorMessage))
				{
					notifications.AddError(shipmentLockedErrorMessage);
				}
			}
		}

		static bool IsUSShipment(ForwardingShipment shipment)
		{
			var countryCode = shipment.Destination?.Country?.Code;
			return countryCode.HasValue && countryCode.Value == CountryCodes.UnitedStates;
		}

		bool ValidateConsignments(HVLVAirCargoAdvanceScreeningMessageSender sender)
		{
			var errorMessage = sender.ValidateACASReportBasicRequirments();

			if (!string.IsNullOrEmpty(errorMessage))
			{
				return false;
			}

			var header = Shipment.GetOrCreateHVLVConsignmentHeader();
			var consignmentsToSend = header.Consignments.Where(x => x.HVC_ACASMessageStatus == ACASStatusCanBeSend && x.HVC_IsActive);
			var consignmentsForACASWrapperCollection = new HVLVConsignmentForACASWrapperCollection(consignmentsToSend);
			consignmentsForACASWrapperCollection.RunPreSaveValidation();

			if (consignmentsForACASWrapperCollection.HasMessageErrors())
			{
				return false;
			}

			return true;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "EDI008:LogReferenceValuesInEnglishOnly", Justification = "Baseline")]
		void SendACASReport()
		{
			var sender = new HVLVAirCargoAdvanceScreeningMessageSender(Shipment);

#if DEBUG
			if (ACASMessageSender_ForTesting != null)
			{
				sender = ACASMessageSender_ForTesting;
			}
#endif

			if (ValidateConsignments(sender))
			{
				var parameters = new[]
				{
					new KeyValuePair<string, string>(Params.MessageType, HVLVAdvancedAirCargoReportMessagetype),
					new KeyValuePair<string, string>(Params.Location, CountryCodes.UnitedStates)
				};

				Shipment.Logs.AddNew(AutoEvents.MessageValidationPassed, parameters);

				if (!sender.TrySendACASReports(AcasReportAction, out var message))
				{
					parameters = parameters.Append(new KeyValuePair<string, string>(Params.Reason, message)).ToArray();
					Shipment.Logs.AddNew(AutoEvents.InterchangeFailedToBeSent, parameters);
				}
			}
			else
			{
				var parameters = new[]
				{
					new KeyValuePair<string, string>(Params.MessageType, HVLVAdvancedAirCargoReportMessagetype),
					new KeyValuePair<string, string>(Params.Location, CountryCodes.UnitedStates),
					new KeyValuePair<string, string>(Params.Reason, Res.GetString("432d6819-b1c7-46a3-a6e4-ea57319cae78", "Check HVLV Advanced Air Cargo Report Form for errors."))
				};

				Shipment.Logs.AddNew(AutoEvents.MessageValidationFailed, parameters);
			}
		}

		protected abstract ACASReportAction AcasReportAction { get; }

		protected abstract string ACASStatusCanBeSend { get; }

		static string HVLVAdvancedAirCargoReportMessagetype => Res.GetString("a27113cc-bb8a-47e5-84f2-4cb383c9e9c9", "HVLV Advanced Air Cargo Report");

#if DEBUG
		public HVLVAirCargoAdvanceScreeningMessageSender ACASMessageSender_ForTesting { get; set; }
#endif
	}
}
