using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MessagingProcess;
using Enterprise.Customs.Common;
using Enterprise.Customs.TR.Messaging;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.TR.Business.MessagingProcess
{
	public sealed class TRCustomsMessenger : ITRCustomsMessenger, ICustomsMessenger, ISupportMessageSigning
	{
		public TRCustomsMessenger(IEDIMessageCollectionOwner owner, ITRCustomsMessageGenerator generator, TRMessageSigner signer = null)
		{
			this.owner = owner;
			this.generator = generator;
			this.signer = signer ?? TRMessageSigner.New();
		}
		readonly IEDIMessageCollectionOwner owner;
		readonly TRMessageSigner signer;
		readonly ITRCustomsMessageGenerator generator;

		ICustomsMessenger ITRCustomsMessenger.Messenger => this;
		bool ITRCustomsMessenger.IsMessageSigningRequired => generator.IsMessageSigningRequired;

		IEDIMessageCollectionOwner ICustomsMessenger.Owner => owner;
		ICustomsMessageGenerator ICustomsMessenger.MessageGenerator => generator;

		bool ICustomsMessenger.ProcessUpdates(ActionResult previousResult)
		{
			if (previousResult.Success && owner.MessageOwner is IMessageAttachee attachee)
			{
				if (attachee is CusEntryHeader cusEntryHeader)
				{
					var msg = previousResult.EDIMessages.OrderByDescending(x => x.EM_SystemCreateTimeUtc).FirstOrDefault();
					if (msg != null)
					{
						attachee.MessageStatus = TRMessageStatusCodeList.Codes.Sent;
						attachee.CustomsStatus = attachee.GetEntryStatus(msg.EM_MessageType);
					}
				}
				else
				{
					attachee.MessageStatus = TRMessageStatusCodeList.Codes.Awaiting;
					attachee.CustomsStatus = TRMessageStatusCodeList.Codes.Awaiting;
				}
			}

			return true;
		}

		bool ICustomsMessenger.ShouldCreateMessage(ActionResult previousResult) => true;

		string ISupportMessageSigning.SignMessages(IReadOnlyCollection<EDIMessage> messages, ActionResult previousResult)
		{
			var result = string.Empty;
			if (generator.IsMessageSigningRequired)
			{
				if (previousResult.DataSource is MessageSendAcknowledgeAndSign sendAndSign)
				{
					try
					{
						var signer = this.signer;
						foreach (var msg in messages)
						{
							signer.SignEdiMessage(msg, sendAndSign.CurrentUserExternalPasswordInfo, sendAndSign.PINCode, msg.EM_FormattedMessageText);
						}
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						result = Res.GetString("BF65A2CD-154A-4E5A-8C8D-7997E98992CE", "The following error was encountered while signing the message: {0}", ex.Message);
					}
				}
				else
				{
					result = Res.GetString("A7E87498-62D7-4CD9-80B2-9E074B38C90F", "PIN information is missing from Preview Result");
				}
			}

			return result;
		}
	}
}
