using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;

namespace Enterprise.Customs.US.Business
{
	public class ConcurrenceDeliveryMessageSender : FTZRelatedMessageSender
	{
		public ConcurrenceDeliveryMessageSender(JobDeclaration declaration, FZEventType eventType)
			: base(declaration)
		{
			this.eventType = eventType;
		}
		readonly FZEventType eventType;

		protected override ZString MessageTypeDescription
		{
			get
			{
				var result = ZString.Empty;
				if (eventType == FZEventType.Concur)
				{
					result = EM_MessageSubTypeList.Descriptions.FZConcurrence;
				}
				else if (eventType == FZEventType.Unconcur)
				{
					result = EM_MessageSubTypeList.Descriptions.FZUnconcurrence;
				}
				else if (eventType == FZEventType.Delivery)
				{
					result = EM_MessageSubTypeList.Descriptions.FZDelivery;
				}
				return result;
			}
		}

		protected override bool Prepare()
		{
			var okToSend = base.Prepare();
			if (okToSend && OnPrepare != null)
			{
				return OnPrepare(Action);
			}
			return okToSend;
		}
		public event PrepareEventHandler OnPrepare;
		public delegate bool PrepareEventHandler(FZEventAction action);

		protected override ZBool CanSendThisMessageCore()
		{
			if (eventType == FZEventType.Unconcur)
			{
				return !Job.ConcurrenceMsgLastAcceptedDate.IsEmpty;
			}
			else
			{
				IFZEventHeader header = Job;
				return header?.HasBeenLodgedAtCustoms ?? false;
			}
		}

		protected override string CannotSendMessageConfirmationMessage => eventType == FZEventType.Unconcur ? ValidationConstants.FTZ.CannotSendFTZUnconcurrenceMessageConfirmation : ValidationConstants.FTZ.CannotSendFTZMessageConfirmation;

		protected override string CannotSendMessageInformationMessage => eventType == FZEventType.Unconcur ? ValidationConstants.FTZ.CannotSendFTZUnconcurrenceMessageInformation : ValidationConstants.FTZ.CannotSendFTZMessageInformation;

		protected override bool GenerateMessage()
		{
			var result = false;
			IFZEventHeader header = Job;
			MQEDIMessage message = null;
			var subType = ZString.Empty;
			if (eventType == FZEventType.Concur)
			{
				var builder = new FZConcurrenceMessageBuilder(Action);
				message = builder.PopulateMessage();
				header.AddMessage(message);
				subType = EM_MessageSubTypeList.Codes.FZConcurrence;
				Action.UpdateFTZConcurrenceQty();
				result = true;
			}
			else if (eventType == FZEventType.Delivery)
			{
				var builder = new FZDeliveryMessageBuilder(Action);
				message = builder.PopulateMessage();
				header.AddMessage(message);
				subType = EM_MessageSubTypeList.Codes.FZDelivery;
				result = true;
			}
			else if (eventType == FZEventType.Unconcur)
			{
				var builder = new FZUnconcurrenceMessageBuilder(Action);
				message = builder.PopulateMessage();
				header.AddMessage(message);
				subType = EM_MessageSubTypeList.Codes.FZUnconcurrence;
				result = true;
			}

			if (message != null)
			{
				message.EM_MessageSubType = subType;
				header.SetMessageStatus(subType, new FTZMessageStatusCalculator().Calculate(message, false, false));
			}
			return result;
		}

		protected override ValidationModes ValidationMode
		{
			get { return ValidationModes.FTZEventsValidationMode; }
		}

		FZEventAction Action
		{
			get { return action ?? (action = new FZEventAction(Job, eventType)); }
		}
		FZEventAction action;
	}
}
