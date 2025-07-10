using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;

namespace Enterprise.Customs.US.Business
{
	public abstract class FTZRelatedMessageSender : MessageSender
	{
		protected FTZRelatedMessageSender(JobDeclaration declaration)
			: base(declaration)
		{
		}

		protected abstract ZString MessageTypeDescription { get; }

		protected override bool Prepare()
		{
			var okToSend = CanSendThisMessageCore();
			if (!okToSend)
			{
				okToSend = DoesSendWithMessageErrorsCore(MessageTypeDescription);
			}
			return okToSend;
		}

		protected virtual ZBool CanSendThisMessageCore()
		{
			IFZEventHeader header = Job;
			return header?.HasBeenLodgedAtCustoms ?? false;
		}

		protected virtual ZBool DoesSendWithMessageErrorsCore(ZString messageType)
		{
			var result = false;
			if (Environment.Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed)
			{
				result = Job.MessageInitiator.YesNoQuery(ZString.Format(CannotSendMessageConfirmationMessage, messageType), "Continue to send?");
			}
			else
			{
				Job.MessageInitiator.WarnUserAboutSomething(ZString.Format(CannotSendMessageInformationMessage, messageType), "Warning");
			}
			return result;
		}

		protected virtual string CannotSendMessageConfirmationMessage => ValidationConstants.FTZ.CannotSendFTZMessageConfirmation;

		protected virtual string CannotSendMessageInformationMessage => ValidationConstants.FTZ.CannotSendFTZMessageInformation;

		protected override string SuccessfulSendNotification
		{
			get { return ZString.Format(MessageSent, MessageTypeDescription); }
		}
		internal const string MessageSent = "{0} Message sent.";

		protected sealed override bool ShouldValidateJob
		{
			get { return true; }
		}

		protected abstract ValidationModes ValidationMode { get; }

		protected override bool ValidateJob()
		{
			oldValidationMode = Job.ValidationModes;
			Job.ValidationModes = ValidationMode;

			return base.ValidateJob();
		}
		ValidationModes oldValidationMode;

		protected override bool SaveMessage()
		{
			var result = base.SaveMessage();
			if (result)
			{
				Job.ValidationModes = oldValidationMode;
			}
			return result;
		}
	}
}
