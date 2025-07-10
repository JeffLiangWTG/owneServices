using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EventReference;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	sealed class AirBookingLogsInterpreter
	{
		public AirBookingLogsInterpreter(IStmALogParent logParent)
		{
			this.logParent = Argument.NotNull(logParent, nameof(logParent));
		}

		readonly IStmALogParent logParent;

		public bool IsWaitingForResponseFromCarrier => isWaitingForResponseFromCarrier ?? (isWaitingForResponseFromCarrier = GetIsWaitingForResponseFromCarrier()).Value;
		bool? isWaitingForResponseFromCarrier;

		public bool MessageWasSentAndNotRejected => messageWasSentAndNotRejected ?? (messageWasSentAndNotRejected = GetMessageWasSentAndNotRejected()).Value;
		bool? messageWasSentAndNotRejected;

		StmALog[] LogsInDescendingOrder => logsInDescendingOrder ?? (logsInDescendingOrder = GetLogsInDescendingOrder().ToArray());
		StmALog[] logsInDescendingOrder;

		#region GetLogsInDescendingOrder

		IEnumerable<StmALog> GetLogsInDescendingOrder()
		{
			foreach (var log in logParent.Logs.GetAllLogs().OfType<StmALog>().OrderByDescending(log => log.SL_PostedTimeUtc))
			{
				var messageType = log.Parameters.GetValueSafe(Constants.EventReferenceParameters.Codes.MessageType);

				if (string.Compare(messageType, AirBookingLogConstants.MessageType, StringComparison.OrdinalIgnoreCase) == 0)
				{
					yield return log;
				}
			}
		}

		#endregion

		#region GetIsWaitingForResponseFromCarrier

		bool GetIsWaitingForResponseFromCarrier()
		{
			foreach (var log in LogsInDescendingOrder)
			{
				switch (log.SL_SE_NKEvent)
				{
					case Events.MessageSentCode:                    // sent message
					case Events.MessageWithdrawCancelRequestCode:   // sent message withdrawal
					case Events.InterchangeSentCode:                // acknowledged by WTG
					case Events.InterchangeReceiptAcknowledgedCode: // carrier acknowledged receiving but not responded yet
						return true;

					default:
						return false;
				}
			}

			return false;
		}

		#endregion

		#region GetMessageWasSentAndNotRejected

		bool GetMessageWasSentAndNotRejected()
		{
			var ignoreSendEvent = false;

			foreach (var log in LogsInDescendingOrder)
			{
				switch (log.SL_SE_NKEvent)
				{
					case Events.MessageSentCode:
						if (ignoreSendEvent)
						{
							ignoreSendEvent = false;
						}
						else
						{
							return true;
						}
						break;

					case Events.InterchangeRejectedCode:
						ignoreSendEvent = true;
						break;
				}
			}

			return false;
		}

		#endregion
	}
}
