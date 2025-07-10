using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CN;
using Enterprise.ZArchitecture.Business;
using EventConstants = CargoWise.EventReference.Constants;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	class VGMMessageStatus
	{
		public VGMMessageStatus(IStmALogProvider logProvider)
		{
			this.logProvider = logProvider;

			if (logProvider != null)
			{
				logProvider.Logs.GetAllLogs().CountChanged += (s, e) =>
				{
					logs = null;
					currentState = null;
				};
			}
		}

		readonly IStmALogProvider logProvider;

		#region State

		public MessageState CurrentState => currentState ?? (currentState = GetState(Logs)).Value;
		MessageState? currentState;

		public bool AllowSendOriginal => CurrentState == MessageState.NotSent
			|| CurrentState == MessageState.OriginalRejected
			|| CurrentState == MessageState.WithdrawalAccepted;

		public bool AllowSendAmendment => CurrentState == MessageState.OriginalAccepted
			|| CurrentState == MessageState.AmendmentAccepted
			|| CurrentState == MessageState.AmendmentRejected
			|| CurrentState == MessageState.WithdrawalRejected;

		public bool AllowSendWithdrawal => CurrentState == MessageState.OriginalAccepted
			|| CurrentState == MessageState.AmendmentAccepted
			|| CurrentState == MessageState.AmendmentRejected
			|| CurrentState == MessageState.WithdrawalRejected;

		public bool AllowResetToOriginal => !AllowSendOriginal;

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		MessageState GetState(StmALog[] containerLogs)
		{
			var state = MessageState.NotSent;

			foreach (var log in containerLogs ?? Array.Empty<StmALog>())
			{
				switch (log.SL_SE_NKEvent)
				{
					case Events.MessageSentCode:
						switch (state)
						{
							case MessageState.NotSent:
							case MessageState.OriginalRejected:
							case MessageState.WithdrawalAccepted:
								state = MessageState.OriginalSent;
								break;

							case MessageState.OriginalAccepted:
							case MessageState.AmendmentAccepted:
							case MessageState.AmendmentRejected:
								state = MessageState.AmendmentSent;
								break;
						}
						break;

					case Events.InterchangeSentCode:
					case Events.MessageAcceptedCode:
						switch (state)
						{
							case MessageState.OriginalSent:
								state = MessageState.OriginalAccepted;
								break;

							case MessageState.AmendmentSent:
								state = MessageState.AmendmentAccepted;
								break;

							case MessageState.WithdrawalSent:
								state = MessageState.WithdrawalAccepted;
								break;
						}
						break;

					case Events.InterchangeRejectedCode:
					case Events.MessageRejectedCode:
						switch (state)
						{
							case MessageState.OriginalSent:
								state = MessageState.OriginalRejected;
								break;

							case MessageState.AmendmentSent:
								state = MessageState.AmendmentRejected;
								break;

							case MessageState.WithdrawalSent:
								state = MessageState.WithdrawalRejected;
								break;
						}
						break;

					case Events.StatusUpdatedCode:
						state = MessageState.NotSent;
						break;

					case Events.MessageWithdrawCancelRequestCode:
						state = MessageState.WithdrawalSent;
						break;
				}
			}

			return state;
		}

		#endregion

		#region Logs

		StmALog[] Logs => logs ?? (logs = GetLogs());
		StmALog[] logs;

		StmALog[] GetLogs()
		{
			if (logProvider == null)
			{
				return Array.Empty<StmALog>();
			}

			var validEventTypes = new string[]
			{
				Events.MessageSentCode,
				Events.InterchangeSentCode,
				Events.InterchangeRejectedCode,
				Events.StatusUpdatedCode,
				Events.MessageWithdrawCancelRequestCode
			};

			return logProvider
				.Logs
				.GetAllLogs()
				.OfType<StmALog>()
				.OrderBy(log => log.SL_PostedTimeUtc)
				.Where(log => !log.IsCancelled
							&& log.Parameters.TryGetValue(EventConstants.EventReferenceParameters.Codes.MessageType, out string messageType)
							&& messageType == Core.Constants.EventReferenceMessageTypes.VerifiedGrossContainerWeight
							&& validEventTypes.Contains(log.SL_SE_NKEvent.ToString()))
				.ToArray();
		}

		#endregion
	}
}
