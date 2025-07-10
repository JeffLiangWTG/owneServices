using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.CN
{
	sealed class EManifestLogsCreator : IMessageLogCreator
	{
		#region CreateMessageSentLog

		public bool CreateMessageSentLog(object logParent, IDynamicData data, string documentName, string recipient)
		{
			return CreateSentEvents(
				logParent,
				Events.MessageSent,
				data,
				documentName,
				recipient);
		}

		#endregion

		#region CreateWithdrawalSentLog

		public bool CreateWithdrawalSentLog(object logParent, IDynamicData data, string documentName, string recipient, object reasonForSending)
		{
			return CreateSentEvents(
				logParent,
				Events.MessageWithdrawCancelRequest,
				data,
				documentName,
				recipient,
				Convert.ToString(reasonForSending));
		}

		#endregion

		#region CreateResetToOriginalLog

		public bool CreateResetToOriginalLog(object logParent, IDynamicData data, string documentName)
		{
			if (logParent is IStmALogProvider logProvider
				&& data?.Value is EManifest eManifest)
			{
				foreach (var booking in eManifest.Bookings)
				{
					if (booking.Send)
					{
						logProvider?.Logs.CreateOrRecreateEventLog(
							Events.StatusUpdated,
							EstimateActual.Actual,
							ZDateTimeOffset.Now,
							ZString.Empty,
							GetParametersForStatusUpdatedEvent(documentName, booking.BookingNumber));
					}
				}

				return true;
			}

			return false;
		}

		KeyValuePair<string, string>[] GetParametersForStatusUpdatedEvent(string documentName, string referenceNumber)
		{
			return new[]
			{
				new KeyValuePair<string, string>(
					CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType,
					documentName),

				new KeyValuePair<string, string>(
					CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type,
					Enterprise.Core.Constants.EventReferenceMessageTypes.ResetToOriginal),

				new KeyValuePair<string, string>(
					CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department,
					Env.CurrentUser.FullName),

				new KeyValuePair<string, string>(
					CargoWise.EventReference.Constants.EventReferenceParameters.Codes.ReferenceNumber,
					referenceNumber)
			};
		}

		#endregion

		#region CreateSentEvents

		bool CreateSentEvents(object logParent, Event @event, IDynamicData data, string documentName, string recipient, string reasonForSending = null)
		{
			if (logParent is IStmALogProvider logProvider
				&& data?.Value is EManifest eManifest)
			{
				foreach (var booking in eManifest.Bookings)
				{
					if (booking.Send)
					{
						logProvider?.Logs.CreateOrRecreateEventLog(
							@event,
							EstimateActual.Actual,
							ZDateTimeOffset.Now,
							ZString.Empty,
							GetParametersForEvent(documentName, recipient, booking.BookingNumber, reasonForSending));
					}
				}

				return true;
			}

			return false;
		}

		KeyValuePair<string, string>[] GetParametersForEvent(string documentName, string recipient, string referenceNumber, string reasonForSending = null)
		{
			var result = new List<KeyValuePair<string, string>>();

			result.Add(new KeyValuePair<string, string>(
				CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType,
				documentName));

			result.Add(new KeyValuePair<string, string>(
				CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department,
				recipient));

			result.Add(new KeyValuePair<string, string>(
				CargoWise.EventReference.Constants.EventReferenceParameters.Codes.ReferenceNumber,
				referenceNumber));

			if (!string.IsNullOrWhiteSpace(reasonForSending))
			{
				result.Add(new KeyValuePair<string, string>(
					CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Reason,
					reasonForSending));
			}

			return result.ToArray();
		}

		#endregion
	}
}
