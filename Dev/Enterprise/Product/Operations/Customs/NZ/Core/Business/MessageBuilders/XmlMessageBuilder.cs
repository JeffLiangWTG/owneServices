using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.NZ.Business.MessageBuilders
{
	/// <summary>
	/// TSW xml message generation
	/// </summary>
	public class XmlMessageBuilder : MessageBuilder
	{
		public XmlMessageBuilder(CusEntryHeader entryHeader)
		{
			EntryHeader = entryHeader;
			Declaration = EntryHeader.Declaration;
		}
		protected readonly CusEntryHeader EntryHeader;
		protected readonly JobDeclaration Declaration;

		public XmlMessageBuilder(ForwardingConsol consol)
		{
			this.consol = consol;
		}
		readonly ForwardingConsol consol;

		public virtual ZBool DeclarantPinRequired
		{
			get { return false; } // implement in inheriting classes....
		}

		public virtual ZString DeclarantPinEncrypted
		{
			get { return ZString.Empty; } // implement in inheriting classes....
		}

		protected override EDIMessage GetNewMessage()
		{
			EDIMessage message = null;
			if (EntryHeader != null)
			{
				message = EntryHeader.Messages.AddNew();
			}
			else
			{
				message = consol.Messages.AddNew();
			}

			return message;
		}

		protected override void GenerateIfNotAlreadyGenerated()
		{
			if (!generated)
			{
				generated = true;
			}
		}

		protected override void SetMessageType()
		{
			// implement in inheriting classes....
		}

		public override string GetMessageText()
		{
			return ""; // implement in inheriting classes....
		}

		protected override void SetMessageSubType()
		{
			// implement in inheriting classes....
		}

		protected override void SetParentMessagingStatusAfterMessagePosting()
		{
			var manifestingEntryHeader = EntryHeader as Declaration.ECIWriteOff.Manifesting.CusEntryHeader;
			if (manifestingEntryHeader != null)
			{
				manifestingEntryHeader.CH_EntryStatus = LowValueManifestStatusList.Codes.SentToCustoms;
				foreach (JobDeclaration jobDeclaration in manifestingEntryHeader.Declarations)
				{
					if (jobDeclaration.IsECIWriteoff)
					{
						jobDeclaration.JE_EntryStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;
						jobDeclaration.JE_EDITransmitDate = manifestingEntryHeader.Declaration.CachedTodaysDate;
						jobDeclaration.JE_EntrySubmittedDate = manifestingEntryHeader.Declaration.CachedTodaysDate;
						jobDeclaration.LogCustomsCommencedIfNeeded();
					}
				}
			}
			else
			{
				using (new Customs.Business.MergeManager.ChangingMergedDeclarationInAWayThatDoesNotRequireReMerge(EntryHeader.Declaration))
				{
					var newEntryStatus = message.EM_HeldUntilDate.IsEmpty ? FormalEntryStatusList.Codes.SentToCustoms : FormalEntryStatusList.Codes.QueuedForSending;
					EntryHeader.CH_EntryStatus = newEntryStatus;
					EntryHeader.CH_LastEntryStyle = Declaration.JE_MessageSubType;
					Declaration.JE_EntryStatus = newEntryStatus;
					Declaration.JE_EntrySubmittedDate = message.EM_HeldUntilDate.IsEmpty ? Declaration.CachedTodaysDate : ZDateTime.Empty;
					Declaration.JE_MessageStatus = ZString.Empty;
					Declaration.LogCustomsCommencedIfNeeded();
				}
			}
		}

		public override string GetEncryptedPassword()
		{
			// for eHub team, EI_Footer will contain the MAC, (message authentication code), needed for the Authentication element in the 
			// Manifest that is sent in the SOAP message, when required by message type and function. 
			// To be able to generate that MAC at the time of creating the interchange, we need to indicate if the MAC is required.
			// We do that by populating EM_MessageOwner with the a portion of the Declarant's encrypted pin.
			var result = ZString.Empty;
			if (DeclarantPinRequired)
			{
				result = DeclarantPinEncrypted;
			}

			return result;
		}

		protected override ZDateTime GetHeldUntilDate() => CalculateMessageHeldDate(Declaration);

		protected void SetPDOFlagIfCurrentSendIsAmendment()
		{
			if (!EnteredPDOFlag)
			{
				EnteredPDOFlag = true;
			}
		}

		public ZBool EnteredPDOFlag
		{
			get { return EntryHeader.Declaration.JE_PDOOtherInfoValue; }
			set { EntryHeader.Declaration.JE_PDOOtherInfoValue = value; }
		}
	}
}
