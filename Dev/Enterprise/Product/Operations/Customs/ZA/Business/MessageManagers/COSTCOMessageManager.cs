using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.ZA.Business.MessageBuilders;
using Enterprise.Environment;
using Enterprise.Messaging.MessageBuilders;

namespace Enterprise.Customs.ZA.Business.MessageManagers
{
	public class COSTCOMessageManager : Customs.Business.MessageManagers.EDIFACTMessageManager
	{
		public COSTCOMessageManager(COSTCOHeader source, IMessageNotificationCollector notification) : base(source, new EDIFACTStatusCalculator(SARSEDIMessage.MessageTypeNames.COSTCO), notification)
		{
			this.source = Argument.NotNull(source, nameof(source));
		}

		public override string MessageFriendlyName => "COSTCO";

		protected override bool ShouldSendMessagesInTestMode => Env.Registry.ZACustoms.GetIsTestMode(Env.CurrentBranch);

		protected override IMessageBuilder GetMessageBuilder(MessageSubTypes actionCode)
		{
			return new COSTCOMessageBuilder(source, actionCode);
		}

		protected override void OnValidationIsBeingRun()
		{
			base.OnValidationIsBeingRun();
			BusinessObject.AMA_IssueDateInfo.AdditionalValidation -= AMA_IssueDateInfo_AdditionalValidation;
			BusinessObject.AMA_IssueDateInfo.AdditionalValidation += AMA_IssueDateInfo_AdditionalValidation;
		}

		void AMA_IssueDateInfo_AdditionalValidation()
		{
			MandatoryValidation.MessageErrorIfNotEntered(BusinessObject.AMA_IssueDateInfo);
		}

		protected override void OnValidationCompleted()
		{
			base.OnValidationCompleted();
			BusinessObject.AMA_IssueDateInfo.AdditionalValidation -= AMA_IssueDateInfo_AdditionalValidation;
		}

		public new AsycudaManifestHeader BusinessObject => (AsycudaManifestHeader)base.BusinessObject;

		readonly COSTCOHeader source;
	}
}
