using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Warehouse.Transit.Business.Common;
using Enterprise.Warehouse.Transit.Document.DocDataObjects;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Warehouse.Transit.Document
{
	public abstract class CIN750NotificationLogsCreator<TNotification> : IMessageLogCreator where TNotification : CIN750Notification
	{
		#region CreateMessageSentLog

		public bool CreateMessageSentLog(object logParent, IDynamicData data, string documentName, string recipient)
		{
			return CreateSentEvents(
				logParent,
				AutoEvents.MessageSent,
				data,
				documentName,
				recipient);
		}

		#endregion

		#region CreateWithdrawalSentLog

		public bool CreateWithdrawalSentLog(object logParent, IDynamicData data, string documentName, string recipient, object reasonForSending) => false;

		#endregion

		#region CreateResetToOriginalLog

		public bool CreateResetToOriginalLog(object logParent, IDynamicData data, string documentName) => false;

		#endregion

		#region CreateSentEvents

		protected virtual bool CreateSentEvents(object logParent, Event @event, IDynamicData data, string documentName, string recipient, string reasonForSending = null)
		{
			if (logParent is IStmALogProvider logProvider && data?.Value is TNotification notification)
			{
				var log = logProvider?.Logs.CreateRecreateOrUpdateEventLog(
					@event,
					EstimateActual.Actual,
					ZDateTimeOffset.Now,
					ZString.Empty,
					GetParametersForEvent(notification, documentName, recipient, reasonForSending));
				notification.SourceBusinessObject.PopulateAddOnValue($"{GetHistoryMessageIdPairAddOnValueType()}{GetCurrentHistoryMessageIdPairNumber(notification.SourceBusinessObject)}", "STR", $"{notification.MessageID}|{log.PK}");
				notification.SourceBusinessObject.Factory.Save();
				return true;
			}

			return false;
		}

		KeyValuePair<string, string>[] GetParametersForEvent(TNotification notification, string documentName, string recipient, string reasonForSending = null)
		{
			var result = new List<KeyValuePair<string, string>>
			{
				new KeyValuePair<string, string>(
				CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType,
				documentName)
			};

			foreach (var parameter in GetNotificationParametersForEvent(notification))
			{
				result.Add(parameter);
			}

			if (!string.IsNullOrWhiteSpace(reasonForSending))
			{
				result.Add(new KeyValuePair<string, string>(
					CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Reason,
					reasonForSending));
			}

			result.Add(new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Argument, ZDateTime.Now.ToString("yyMMddHHmmssfff")));

			return result.ToArray();
		}

		protected abstract IEnumerable<KeyValuePair<string, string>> GetNotificationParametersForEvent(TNotification notification);

		protected ZString FillWithHyphenIfEmpty(ZString source) => source.IsEmpty ? "-" : source;

		public abstract string GetHistoryMessageIdPairAddOnValueType();

		protected int GetCurrentHistoryMessageIdPairNumber(BusinessObject parent)
			=> parent.GetAddOnValues(a => a.XV_Name.StartsWith(GetHistoryMessageIdPairAddOnValueType())).Count + 1;

		#endregion
	}
}
