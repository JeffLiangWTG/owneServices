using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class PGACorrectionMessageSendingAction : NonPersistentBusinessObject, IObsoleteValidation
	{
		public PGACorrectionMessageSendingAction(CusEntryHeader entry)
			: base(entry.Factory)
		{
			this.entry = entry;
			Validation.ValidateAll();
		}
		readonly CusEntryHeader entry;

		public CusEntryHeader Entry
		{
			get { return entry; }
		}

		#region Properties

		[BusinessObjectTestExclude]
		public ZBool US_SendMessage
		{
			get { return fUS_SendMessage; }
			set
			{
				SetNonPersistentPropertyValue(US_SendMessageInfo, ref fUS_SendMessage, value);
				ReGenerateMessageContents();
				Validation.CheckUS_SendMessage();
			}
		}
		ZBool fUS_SendMessage = true;

		public ZPropertyInfo US_SendMessageInfo
		{
			get { return GetZPropertyInfo(nameof(US_SendMessage)); }
		}

		public ZString US_MessageContents
		{
			get { return GetSerialiseMessageContents(); }
		}

		ZString GetSerialiseMessageContents()
		{
			if (!messageContentsCached.HasValue)
			{
				messageContentsCached = new PGACorrectionMessageBuilder(Entry, ACEEntrySummaryMessageSendingOption.New(), false).GetSerialiseMessageContents();
			}

			return messageContentsCached.Value;
		}
		ZString? messageContentsCached;

		void ReGenerateMessageContents()
		{
			var oldMessageContents = ZString.Empty;
			if (messageContentsCached.HasValue)
			{
				oldMessageContents = messageContentsCached.Value;
				messageContentsCached = null;
			}
			US_MessageContentsInfo.RefreshBinding(oldMessageContents);
		}

		public ZPropertyInfo US_MessageContentsInfo
		{
			get { return GetZPropertyInfo(nameof(US_MessageContents)); }
		}
		public bool ShouldSendMessage
		{
			get { return fShouldSendMessage; }
			set { fShouldSendMessage = value; }
		}
		bool fShouldSendMessage;

		public void GeneratePGACorrectionMessages()
		{
			new PGACorrectionMessageBuilder(Entry, ACEEntrySummaryMessageSendingOption.New(), true).GenerateMessage();
			Entry.Declaration.US_PGACorrectionStatus = PGACorrectionStatusList.Codes.AwaitingPGADataCorrection;
		}

		#endregion

		#region Validation

		public PGACorrectionMessageSendingActionValidation Validation => new PGACorrectionMessageSendingActionValidation(this);

		#endregion
	}
}
