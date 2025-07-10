using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Edifact;
using Enterprise.Freight.Agency.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.InterchangeProviders;

namespace Enterprise.Freight.Agency.ServiceTasks
{
	internal sealed class EIDOInterchangeProvider : InterchangeProviderBase
	{
		public EIDOInterchangeProvider(NonDependentEDIMessageCollection messages, FailedMessageList failures)
			: base(messages)
		{
			this.failures = failures;
		}

		protected override string InstructionHowToSetInterchangeSenderID
		{
			get { return Res.GetString("37f9f1a4-b828-403c-a457-71ae7f79573a", "You need to enable E-IDO messaging from the registry item: Liner & Agency -> E-IDO Messaging -> Enable"); }
		}

		protected override void PopulateInterchange(NonDependentEDIMessageCollection messages, EDIInterchange interchange)
		{
			try
			{
				EIDOMessagingIdentity identity = MessagingHelper.GetEIDOIdentityFromMessage(MessagingDetails, messages[0]);
				SetInterchangeValuesForTransmit(interchange, messages, messages[0].EM_MessageType, EIDOMessage.ThemKeyPart, EIDOMessage.UsKeyPart);
				interchange.EI_HeaderText = GetUNB(PreparedTime.ToZDateTime(), identity.SenderID, identity.RecipientID, MessagingDetails.Testing);
			}
			catch (MessageProcessingException ex)
			{
				if (ex.ShouldSendDeveloperInformation || !ex.ShouldSendEmailToUsers)
				{
					throw;
				}
				else
				{
					foreach (EDIMessage message in messages)
					{
						message.EM_Status = EDIMessage.Status.Failed;
					}

					failures.AddRange(ex.Message, messages.ToArray<EDIMessage>());
					messages.RemoveAll();
				}
			}
		}

		protected override UNCharacterSet CurrentUNCharacterSet
		{
			get { return new UNOACharacterSet(); }
		}

		protected override void BuildInterchangeBatchesCore(NonDependentEDIMessageCollection messages)
		{
			Dictionary<ZGuid, NonDependentEDIMessageCollection> messagesByPrincipal = new Dictionary<ZGuid, NonDependentEDIMessageCollection>();

			foreach (EDIMessage message in messages)
			{
				BillOfLadingContainer container = message.Factory.Load<BillOfLadingContainer>(message.EM_LinkUniqueID);

				if (container == null)
				{
					message.EM_Status = EIDOMessage.Status.Failed;
				}
				else
				{
					BillOfLading bill = message.Factory.Load<BillOfLading>(container.JC_JS_FCLBookingOnlyLink);
					NonDependentEDIMessageCollection list;

					if (!messagesByPrincipal.TryGetValue(bill.JS_OH_DeliveryAgent, out list))
					{
						list = new NonDependentEDIMessageCollection(messages.Factory);
						messagesByPrincipal.Add(bill.JS_OH_DeliveryAgent, list);
					}

					list.Add(message);
				}
			}

			foreach (KeyValuePair<ZGuid, NonDependentEDIMessageCollection> pair in messagesByPrincipal)
			{
				base.BuildInterchangeBatchesCore(pair.Value);
			}
		}

		ZString GetUNB(ZDateTime preparedTime, ZString senderID, ZString recipientID, bool testing)
		{
			return GetUNB(preparedTime,
				recipientID, ZString.Empty, senderID, ZString.Empty,
				ZString.Empty, "UNOA", "4", ZString.Empty, false, testing, ZString.Empty
				).ToString(new UNOACharacterSet());
		}

		EIDOMessagingHeader MessagingDetails
		{
			get { return messagingDetails ?? (messagingDetails = MessagingHelper.GetEIDOMessagingDetail()); }
		}
		EIDOMessagingHeader messagingDetails;

		readonly FailedMessageList failures;
	}
}


