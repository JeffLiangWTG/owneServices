using System;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business.BatchProcessor;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.NZ.Business.BatchProcessor
{
	public class NZInterchangeProvider : OneInterchangeToOneMessageInterchangeProvider
	{
		public NZInterchangeProvider(LoggingInformation logger, NonDependentEDIMessageCollection messages)
			: base(messages)
		{
			this.logger = logger;
		}

		readonly LoggingInformation logger;

		#region Overrides

		protected override Type InterchangeType
		{
			get { return typeof(NZCInterchange); }
		}

		protected override void PopulateInterchange(NonDependentEDIMessageCollection messages, EDIInterchange interchange)
		{
			// Legacy CUSMOD edifact message are no longer used for NZ Customs. Class remains as TSWInterchangeProvider relies on this class.
			logger.LogWarning("Trying to generate legacy edifact message.");
			interchange.ContainedMessages.RemoveAll();
			interchange.Delete();
		}

		protected ZString GetMessageText(NonDependentEDIMessageCollection messages)
		{
			ZStringBuilder builder = new ZStringBuilder();
			foreach (EDIMessage message in messages)
			{
				builder.Append(message.EM_MessageText);
				if (builder.Length > 100) // MessageProcessingException only use the first 100 characters
				{
					break;
				}
			}

			return builder.ToString();
		}
		protected override string InstructionHowToSetInterchangeSenderID
		{
			get { return "Please set a value in Config > System > Registry > Customs > New Zealand > Brokerage ID."; }
		}

		protected override string GetCollationKey(EDIMessage message)
		{
			return DoNotCollateType;
		}

		#endregion

		#region Implementation

		protected internal ZString GetNZCRecipientID(EDIMessage message)
		{
			if (message.EM_IsTestMessage)
			{
				return EDIInterchange.InterchangePartyIDs.NZCustomsTestMailbox;
			}
			else
			{
				return EDIInterchange.InterchangePartyIDs.NZCustomsLiveMailbox;
			}
		}

		#endregion

		#region DEBUG only property accessors for testing
#if DEBUG
		internal ZDateTime PreparedTimeForTesting
		{
			get { return PreparedTime.ToZDateTime(); }
		}
#endif
		#endregion
	}
}
