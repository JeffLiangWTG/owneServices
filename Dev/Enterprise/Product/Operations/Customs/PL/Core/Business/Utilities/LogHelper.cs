using System;
using System.Text;
using CargoWise.Common;
using Enterprise.BatchProcessor;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.PL.Business;

public static class LogHelper
{
	public static void Log(Event eventType, string logMessage,
		ISimpleLogger serviceLog = null,
		EDIInterchange interchange = null,
		EnterpriseEDIMessage incomingMessage = null,
		EnterpriseEDIMessage transmitMessage = null,
		bool logToLinkedObject = true)
	{
		Argument.NotNullOrEmpty(logMessage, nameof(logMessage));

		interchange?.Logs.AddNew(eventType, logMessage);

		incomingMessage?.Logs.AddNew(eventType, logMessage);

		if (logToLinkedObject && transmitMessage == null)
		{
			incomingMessage?.EM_LinkedObject?.GetLogs().AddNew(eventType, logMessage);
		}

		if (transmitMessage == null && serviceLog == null)
		{
			return;
		}

		var logMessageWithMessageSourceInfoBuilder = new StringBuilder();
		logMessageWithMessageSourceInfoBuilder.Append(logMessage);
		if (interchange != null)
		{
			logMessageWithMessageSourceInfoBuilder.Append($" Interchange number: {interchange.EI_InterchangeNum}, {interchange.EI_ApplicationCode}/{interchange.EI_InterchangeType}.");
		}
		else if (incomingMessage != null)
		{
			logMessageWithMessageSourceInfoBuilder.Append($" Message number: {incomingMessage.EM_MessageNum}, {incomingMessage.EM_ApplicationCode}/{incomingMessage.EM_MessageType}");
			if (!incomingMessage.EM_MessageSubType.IsEmpty)
			{
				logMessageWithMessageSourceInfoBuilder.Append($":{incomingMessage.EM_MessageSubType}");
			}
			logMessageWithMessageSourceInfoBuilder.Append(".");
		}
		var logMessageWithMessageSourceInfo = logMessageWithMessageSourceInfoBuilder.ToString();

		transmitMessage?.Logs.AddNew(eventType, logMessageWithMessageSourceInfo);
		if (logToLinkedObject)
		{
			transmitMessage?.EM_LinkedObject?.GetLogs().AddNew(eventType, logMessageWithMessageSourceInfo);
		}

		serviceLog?.Log(GetLogType(eventType), logMessageWithMessageSourceInfo);
	}

	static LogType GetLogType(Event @event) => @event.Code switch
	{
		Events.InterchangeAcknowledgedCode => LogType.Information,
		Events.InterchangeReceivedCode => LogType.Information,
		Events.ErrorReportCode => LogType.Error,
		_ => throw new ArgumentOutOfRangeException(nameof(@event), @event.Code, $"Event {@event.Code} is unsupported!")
	};

	internal static void LogProcessingError(this LoggingInformation logger, BaseEDIMessage message, string error)
	{
		if (string.IsNullOrEmpty(error))
		{
			return;
		}

		logger.LogError(error);
		message.Logs.AddNew(Events.ErrorReport, error);
	}
}
