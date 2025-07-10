using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.EConversation.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ProcessManagement.DataTransfer
{
	static class UniversalEConversationHelper
	{
		internal static void PopulateConversation(BusinessObject source, Activity destination, IDataWritingManager writeManager, bool shouldIncludeInternalMessages)
		{
			if (source is IConversationProvider provider && JobConversation.GetConversation(source) != null)
			{
				var conversation = provider.eConversation;
				var messages = GetMessages(conversation, shouldIncludeInternalMessages);
				var staffParticipants = GetStaffParticipantsForWriting(conversation);
				var groupParticipants = GetGroupParticipantsForWriting(conversation);
				var relatedPartyParticipants = GetRelatedPartiesForWriting(conversation, writeManager);

				var shouldWriteConversation =
					(messages != null && messages.Any()) ||
					(staffParticipants != null && staffParticipants.Any()) ||
					(groupParticipants != null && groupParticipants.Any()) ||
					(relatedPartyParticipants != null && relatedPartyParticipants.Any());

				if (shouldWriteConversation)
				{
					destination.Conversation = new Conversation(writeManager.WriterStrategy);
					destination.Conversation.SetConversationMessageCollection(() => messages);
					destination.Conversation.SetParticipantStaffCollection(() => staffParticipants);
					destination.Conversation.SetParticipantGroupCollection(() => groupParticipants);
					destination.Conversation.SetParticipantRelatedPartyCollection(() => relatedPartyParticipants);
				}
			}
		}

		static List<ConversationMessage> GetMessages(JobConversation conversation, bool shouldIncludeInternalMessages)
		{
			var messagesToInclude = conversation.Messages?.Where(x => shouldIncludeInternalMessages || !x.JCM_IsInternal);

			return messagesToInclude?.Select(message => new ConversationMessage
			{
				ParticipantName = message.SenderDisplayName,
				Text = message.JCM_Body,
				IsInternal = message.JCM_IsInternal,
				IsSystem = message.JCM_IsSystem,
				CreatedTime = message.JCM_PostedTimeUtc,
			}).ToList();
		}

		static List<ParticipantStaff> GetStaffParticipantsForWriting(JobConversation conversation)
		{
			var participants = conversation.Staff;

			return participants.Select(participant =>
			{
				var parent = participant.Parent;

				return new ParticipantStaff
				{
					Staff = new Staff { Code = parent.Code, Name = parent.Name },
					JobTitle = parent.JobTitle,
					Location = GetLocation(participant.Factory, parent.Location),
					IsActive = parent.IsActive,
					IsSubscribed = participant.JCP_IsSubscribed,
				};
			}).ToList();
		}

		static List<ParticipantGroup> GetGroupParticipantsForWriting(JobConversation conversation)
		{
			var participants = conversation.Groups;

			return participants.Select(participant =>
			{
				var parent = participant.Parent;

				return new ParticipantGroup
				{
					Group = new Group { Code = parent.Code, Name = parent.Name },
					Location = GetLocation(participant.Factory, parent.Location),
					IsActive = parent.IsActive,
					IsSubscribed = participant.JCP_IsSubscribed,
				};
			}).ToList();
		}

		static List<ParticipantRelatedParty> GetRelatedPartiesForWriting(JobConversation conversation, IDataWritingManager writeManager)
		{
			var participants = conversation.RelatedParties;

			return participants.Select(participant =>
			{
				var parent = participant.Parent;

				var (type, party) = GetPartyInfo(participant, writeManager);

				return new ParticipantRelatedParty
				{
					RelatedPartyType = type,
					Party = party,
					Location = GetLocation(participant.Factory, parent.Location),
					Relation = participant.JCP_Relation,
					IsActive = parent.IsActive,
					IsSubscribed = participant.JCP_IsSubscribed,
				};
			}).ToList();
		}

		static (RelatedPartyType, OrganizationAddress) GetPartyInfo(JobConversationParticipant participant, IDataWritingManager writeManager)
		{
			if (participant.IsEmailParticipant)
			{
				var emailOnlyAddress = participant.Factory.New<OrgAddress>();
				emailOnlyAddress.OA_Email = participant.EmailAddress;
				return (RelatedPartyType.Email, ActivityOrganizationHelper.GetAddress(emailOnlyAddress, writeManager, ActivityOrganizationAddressType.None, null));
			}

			var parent = participant.Parent;
			var contact = parent as OrgContact;
			var org = contact?.Header ?? parent as OrgHeader;
			var type = contact == null ? RelatedPartyType.Organization : RelatedPartyType.Contact;

			return (type, ActivityOrganizationHelper.GetAddress(org, writeManager, ActivityOrganizationAddressType.None, contact));
		}

		static UNLOCO GetLocation(BusinessObjectFactory factory, ZString locationCode)
		{
			var unloco = factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, locationCode);
			return unloco == null ? null : UNLOCO.New(unloco);
		}
	}
}
