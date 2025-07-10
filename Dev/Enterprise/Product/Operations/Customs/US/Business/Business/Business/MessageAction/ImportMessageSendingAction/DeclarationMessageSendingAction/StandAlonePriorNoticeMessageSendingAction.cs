using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Business.MessageBuilders;

namespace Enterprise.Customs.US.Business
{
	public class StandAlonePriorNoticeMessageSendingAction : ImportMessageSendingAction
	{
		public StandAlonePriorNoticeMessageSendingAction(JobDeclaration declaration, ImportMessageSendingActionCollection actions)
			: base(declaration, ImportMessageStatusList.MessageType.Undefined, actions)
		{
		}

		public StandAlonePriorNoticeMessageSendingAction(IStandAlonePriorNoticeHeader priorNoticeHeader, JobDeclaration declaration, ImportMessageSendingActionCollection actions)
			: this(declaration, actions)
		{
			this.priorNoticeHeader = priorNoticeHeader;
		}
		readonly IStandAlonePriorNoticeHeader priorNoticeHeader;

		internal IStandAlonePriorNoticeHeader PriorNoticeHeader
		{
			get { return priorNoticeHeader; }
		}

		public override ZString US_MessageDescription
		{
			get { return "Standalone Prior Notice: " + (priorNoticeHeader != null ? priorNoticeHeader.HumanReadableName : Declaration.HumanReadableName); }
		}

		public override ZString US_MessageContents
		{
			get
			{
				if (!messageContentsCached.HasValue)
				{
					if (priorNoticeHeader != null)
					{
						messageContentsCached = new ACEPriorNoticeMessageBuilder(priorNoticeHeader, US_PNActionCode).GenerateHumanReadableMessageContent();
					}
					else
					{
						messageContentsCached = "This message is no longer supported by CBP.";
					}
				}
				return messageContentsCached.Value;
			}
		}
		ZString? messageContentsCached;

		[BusinessObjectTestExclude]
		public override ZString US_PNActionCode
		{
			get { return base.US_PNActionCode.IsEmpty ? (ZString)ACEPNActionCodeList.Codes.A : base.US_PNActionCode; }
			set
			{
				var oldValue = US_PNActionCode;
				base.US_PNActionCode = value;
				if (oldValue != value)
				{
					var oldMessageContents = ZString.Empty;
					if (messageContentsCached.HasValue)
					{
						oldMessageContents = messageContentsCached.Value;
						messageContentsCached = null;
					}
					US_MessageContentsInfo.RefreshBinding(oldMessageContents);
				}
				US_PNActionCodeInfo.RefreshBinding(oldValue);
				Validation.ValidateUS_PNActionCode();
			}
		}

		public override ZBool US_SendMessage
		{
			get { return base.US_SendMessage; }
			set
			{
				base.US_SendMessage = value;
				Validation.ValidateUS_PNActionCode();
			}
		}

		protected override Customs.Business.SingleMessageManager GetMessageManager()
		{
			return new StandAlonePriorNoticeMessageManager(this);
		}

		protected override USImportMessageSendingActionValidation GetNewValidation()
		{
			return new StandAlonePriorNoticeMessageSendingActionValidation(this);
		}

		protected new StandAlonePriorNoticeMessageSendingActionValidation Validation
		{
			get { return (StandAlonePriorNoticeMessageSendingActionValidation)base.Validation; }
		}
	}
}
