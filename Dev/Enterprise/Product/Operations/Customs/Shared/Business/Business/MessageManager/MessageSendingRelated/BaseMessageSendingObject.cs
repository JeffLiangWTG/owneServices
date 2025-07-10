using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public abstract class BaseMessageSendingObject : NonPersistentBusinessObject
	{
		public const string SchemaShouldSend = "ShouldSend";

		protected BaseMessageSendingObject() : base() { }

		protected BaseMessageSendingObject(BusinessObjectFactory factory) : base(factory)
		{
		}

		#region ShouldSend

		[ReadOnlyMember(nameof(ShouldSend_ReadOnly))]
		[CargoWiseOne.ResourceStrings.ResourceStringData("NPBO:Enterprise.Customs.Business.MessageSendingObject|ShouldSend", Caption = "Send?")]
		public virtual ZBool ShouldSend
		{
			[System.Diagnostics.DebuggerStepThrough]
			get
			{
				return shouldSend;
			}
			set
			{
				SetNonPersistentPropertyValue(ShouldSendInfo, ref shouldSend, value);
				if (IsValidationSuspended)
				{
				}
				else
				{
					ValidateShouldSend();
				}
			}
		}

		public virtual ZPropertyInfo ShouldSendInfo
		{
			get
			{
				return this.GetZPropertyInfo(SchemaShouldSend);
			}
		}

		protected virtual bool ShouldSend_ReadOnly
		{
			[System.Diagnostics.DebuggerStepThrough]
			get
			{
				return false;
			}
		}
		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		ZBool shouldSend;

		public virtual void ValidateShouldSend()
		{
		}

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			ValidateShouldSend();
			base.RunPreSaveValidationCore();
		}

		#endregion

		#region Overrides of Amend Members

		public bool IsAmend => IsAmendCore;
		protected virtual bool IsAmendCore => false;

		public bool IsCancel => IsCancelCore;
		protected virtual bool IsCancelCore => false;

		#endregion

		public delegate void MessagePreviewEventHandler(MessageEventArgs args);

		public event MessagePreviewEventHandler PreviewMessage;

		public string MessageCreated(string messageText)
		{
			string result = messageText;
			if (PreviewMessage != null)
			{
				var args = new MessageEventArgs(messageText);
				PreviewMessage(args);
				result = args.MessageText;
			}
			return result;
		}
	}
}
