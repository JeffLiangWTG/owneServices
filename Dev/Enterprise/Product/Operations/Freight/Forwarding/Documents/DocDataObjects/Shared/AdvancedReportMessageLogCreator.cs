using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	class AdvancedReportMessageLogCreator : IMessageLogCreator
	{
		public AdvancedReportMessageLogCreator(string messageType, string countryCode)
		{
			this.messageType = messageType;
			this.countryCode = countryCode;
		}

		readonly string countryCode;
		readonly string messageType;

		public bool CreateMessageSentLog(object logParent, IDynamicData data, string documentName, string recipient)
		{
			if (logParent is IStmALogProvider logProvider)
			{
				logProvider.Logs.CreateOrRecreateEventLog(
				Events.MessageSent,
				EstimateActual.Actual,
				ZDateTimeOffset.Now,
				ZString.Empty,
					GetParametersForEvent(data, recipient));

				return true;
			}

			return false;
		}

		protected virtual KeyValuePair<string, string>[] GetParametersForEvent(IDynamicData data, string recipient)
		{
			var result = new List<KeyValuePair<string, string>>();

			result.Add(new KeyValuePair<string, string>(
				CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType,
				messageType));

			result.Add(new KeyValuePair<string, string>(
				CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Location,
				countryCode));

			result.Add(new KeyValuePair<string, string>(
				CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department,
				recipient));

			return result.ToArray();
		}

		KeyValuePair<string, string>[] GetParametersForStatusUpdatedEvent()
		{
			var result = new List<KeyValuePair<string, string>>();

			result.Add(new KeyValuePair<string, string>(
				CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType,
				messageType));

			result.Add(new KeyValuePair<string, string>(
				CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Location,
				countryCode));

			result.Add(new KeyValuePair<string, string>(
				CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department,
				Env.CurrentUser.FullName));

			result.Add(new KeyValuePair<string, string>(
				CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type,
				Core.Constants.EventReferenceMessageTypes.ResetToOriginal));

			return result.ToArray();
		}

		public bool CreateWithdrawalSentLog(object logParent, IDynamicData data, string documentName, string recipient, object reasonForSending)
		{
			return false;
		}

		public bool CreateResetToOriginalLog(object logParent, IDynamicData data, string documentName)
		{
			if (logParent is IStmALogProvider logProvider)
			{
				logProvider?.Logs.CreateOrRecreateEventLog(
					Events.StatusUpdated,
					EstimateActual.Actual,
							ZDateTimeOffset.Now,
							ZString.Empty,
							GetParametersForStatusUpdatedEvent());

				return true;
			}

			return false;
		}
	}
}
