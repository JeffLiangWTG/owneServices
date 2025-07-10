using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using EventConstants = CargoWise.EventReference.Constants;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.CN
{
	sealed class MessageStatus : DocDataObject
	{
		public MessageStatus(ZString bookingNumber, IStmALogProvider logProvider)
		{
			this.bookingNumber = bookingNumber;
			this.logProvider = logProvider;

			Description = GetDescription();

			if (logProvider != null)
			{
				logProvider.Logs.GetAllLogs().CountChanged += (s, e) =>
				{
					logs = null;
					currentState = null;
					Description = GetDescription();
				};
			}
		}

		readonly ZString bookingNumber;
		readonly IStmALogProvider logProvider;

		#region Description

		[IgnoreChanges]
		public ZString Description
		{
			get => description;
			private set
			{
				if (SetNonPersistentPropertyValue(DescriptionInfo, ref description, value))
				{
					Validate(DescriptionInfo);
				}
			}
		}

		ZString description;

		public ZPropertyInfo DescriptionInfo => GetZPropertyInfo(nameof(Description));

		string GetDescription()
		{
			return Logs.LastOrDefault()?.DisplayEventReference ?? (NoResString)"Not Sent"; // non-translatable status
		}

		#endregion

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

		MessageState GetState(StmALog[] bookingLogs)
		{
			var state = MessageState.NotSent;

			foreach (var log in bookingLogs ?? Array.Empty<StmALog>())
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
			if (logProvider == null
				|| bookingNumber.IsEmpty)
			{
				return Array.Empty<StmALog>();
			}

			return logProvider
				.Logs
				.GetAllLogs()
				.OfType<StmALog>()
				.OrderBy(log => log.SL_PostedTimeUtc)
				.Where(log => IsApplicable(log))
				.ToArray();
		}

		bool IsApplicable(StmALog log)
		{
			switch (log.SL_SE_NKEvent)
			{
				case Events.MessageSentCode:
				case Events.InterchangeSentCode:
				case Events.InterchangeRejectedCode:
				case Events.StatusUpdatedCode:
				case Events.MessageWithdrawCancelRequestCode:
					return MatchesBookingNumber(log);

				default:
					return false;
			}
		}

		bool MatchesBookingNumber(StmALog log)
		{
			return log.Parameters.TryGetValue(EventConstants.EventReferenceParameters.Codes.ReferenceNumber, out string referenceNumber)
				&& string.Compare(bookingNumber, referenceNumber, StringComparison.OrdinalIgnoreCase) == 0;
		}

		#endregion

		#region Implementation

		public override string ToString() => Description;

		#endregion
	}
}
