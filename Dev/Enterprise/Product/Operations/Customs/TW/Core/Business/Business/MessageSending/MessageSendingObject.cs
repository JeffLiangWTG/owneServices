using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Business
{
	public abstract class MessageSendingObject : JobDeclarationMessageSendingObject
	{
		protected MessageSendingObject(CusEntryHeader header)
			: base(header)
		{
			entryInstruction = Argument.NotNull(header.EntryInstruction, "EntryInstruction");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Schema")]
		public static class TWSchema
		{
			public const string MessageStatus = "MessageStatus";
			public const string EntryNumber = "EntryNumber";
			public const string Action = "Action";
		}

		public new CusEntryHeader Header => (CusEntryHeader)base.Header;

		protected CusEntryInstruction entryInstruction;

		protected override JobDeclarationMessageSendingObjectValidation GetNewValidation()
		{
			return new MessageSendingObjectValidation(this);
		}

		public new MessageSendingObjectValidation Validation => (MessageSendingObjectValidation)base.Validation;

		#region MessageStatus

		[ResourceStringData("NPBO:Enterprise.Customs.TW.Business.MessageSendingObject|MessageStatus", Caption = "Status")]
		public ZString MessageStatus => Header.CH_Status;

		public ZPropertyInfo MessageStatusInfo => GetZPropertyInfo(TWSchema.MessageStatus);

		#endregion

		#region EntryStatus

		[ResourceStringData("NPBO:Enterprise.Customs.TW.Business.MessageSendingObject|EntryStatus", Caption = "Entry Status")]
		public override ZString EntryStatus => Header.CH_EntryStatus;
		#endregion

		#region EntryNumber

		[ResourceStringData("NPBO:Enterprise.Customs.TW.Business.MessageSendingObject|EntryNumber", Caption = "Entry Number")]
		public ZString EntryNumber => Header.DeclarationNumber;

		public ZPropertyInfo EntryNumberInfo => GetZPropertyInfo(TWSchema.EntryNumber);
		#endregion

		#region Action

		ZString action;
		[List(nameof(ActionList))]
		[ResourceStringData("NPBO:Enterprise.Customs.TW.Business.MessageSendingObject|Action", Caption = "Action")]
		public ZString Action
		{
			get => action;
			set
			{
				CheckMaximumLength(ActionInfo, value);
				SetNonPersistentPropertyValue(ActionInfo, ref action, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateAction();
				}
			}
		}

		public ZPropertyInfo ActionInfo => GetZPropertyInfo(TWSchema.Action);

		public virtual CodeDescriptionPairList ActionList => Factory.GetCachedValue<ActionCodeList>();

		#endregion

		public abstract ZString GetMessageOwner();

		public virtual ZString FriendlyNameForMessageManager { get => ZString.Empty; }

		public virtual ZString SerializeToMessageString()
		{
			return ZString.Empty;
		}
	}
}
